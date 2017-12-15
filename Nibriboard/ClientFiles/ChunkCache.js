"use strict";

import ChunkReference from './ChunkReference';
import Chunk from './Chunk';
import Rectangle from './Utilities/Rectangle';
import Vector from './Utilities/Vector';

class ChunkCache
{
	constructor(inBoardWindow)
	{
		this.boardWindow = inBoardWindow;
		this.cache = new Map();
		
		// Whether to highlight rendered chunks. Useful for debugging purposes.
		this.showRenderedChunks = false;
		
		this.boardWindow.rippleLink.on("ChunkUpdate", this.handleChunkUpdate.bind(this));
	}
	
	/**
	 * Fetches the chunk with the specified chunk reference.
	 * @param	{ChunkReference}	chunkRef	The chunk reference of the chunk to fetch.
	 * @param	{bool}				quiet		Whether to be quiet if the chunk wasn't found.
	 * @return	{Chunk|null}					The requested chunk, or null if it isn't present in the cache.
	 */
	fetchChunk(chunkRef, quiet)
	{
		// Return null if we don't currently have that chunk in storage
		if(!this.cache.has(chunkRef.toString()))
			return null;
		
		let requestedChunk = this.cache.get(chunkRef.toString());
		if(!(requestedChunk instanceof Chunk)) {
			if(!quiet)
				console.warn(`Attempt to access a chunk at ${chunkRef} that's not loaded yet.`);
			return null;
		}
		return requestedChunk;
	}
	
	/**
	 * Walk the currently cached chunks to find all the line fragments for the
	 * specified line id, starting at the specified chunk reference.
	 * @param	{ChunkReference}	startingChunkRef	The reference of hte chunk we should start walking at.
	 * @param	{string}	lineId				The unique id of the first line fragment we should include in the list.
	 * @return	{object[]}	A list of line fragments found.
	 */
	fetchLineFragments(startingChunkRef, lineUniqueId)
	{
		let lineFragments = [];
		let currentChunk = this.fetchChunk(startingChunkRef);
		let nextUniqueId = lineUniqueId;
		
		while(currentChunk instanceof Chunk)
		{
			let nextLineFragment = currentChunk.getLineByUniqueId(nextUniqueId);
			if(nextLineFragment == null)
				break;
			
			lineFragments.push(nextLineFragment);
			
			if(nextLineFragment.ContinuesIn == null)
				break;
			
			currentChunk = this.fetchChunk(nextLineFragment.ContinuesIn);
			nextUniqueId = nextLineFragment.ContinuesWithId;
		}
		
		return lineFragments;
	}
	
	clearRequestedChunks()
	{
		for(let [chunkRefStr, chunk] of this.cache.entries()) {
			if(!(chunk instanceof Chunk))
				this.cache.delete(chunkRefStr);
		}
	}
	
	/**
	 * Adds the given chunk to the chunk cache.
	 * @param	{Chunk}	chunkData	The chunk to add to the cache.
	 * @returns	{bool}	Whether we actually updated an existing chunk in the	
	 * 					cache instead of adding a new one.
	 */
	add(chunkData, override = false)
	{
		var hasChunk = this.cache.has(chunkData.chunkRef.toString()) &&
			this.cache.get(chunkData.chunkRef.toString()) instanceof Chunk;
		
		if(!override && hasChunk)
			throw new Error("Error: We already have a chunk at that location stored.");
		
		this.cache.set(chunkData.chunkRef.toString(), chunkData);
		
		return !hasChunk;
	}
	
	/**
	 * Updates the chunk cache ready for the next frame.
	 * @param	{number}			dt			The amount of time, in milliseconds, since that last frame was rendered.
	 * @param	{viewport_object}	visibleArea	The area that's currently visible on-screen.
	 */
	update(dt, visibleArea)
	{
		let chunkSize = this.boardWindow.gridSize;
		let chunkArea = ChunkCache.CalculateChunkArea(visibleArea, chunkSize);
		
		// Collect a list of missing chunks
		let missingChunks = [];
		for(let cx = chunkArea.x; cx <= chunkArea.x + chunkArea.width; cx += chunkSize)
		{
			for(let cy = chunkArea.y; cy <= chunkArea.y + chunkArea.height; cy += chunkSize)
			{
				let cChunk = new ChunkReference(
					this.boardWindow.currentPlaneName,
					cx / chunkSize, cy / chunkSize
				);
				if(!this.cache.has(cChunk.toString())) {
					console.info(`Requesting ${cChunk}`);
					missingChunks.push(cChunk);
					this.cache.set(cChunk.toString(), { requestedFromServer: true });
				}
			}
		}
		
		if(missingChunks.length > 0) {
			// Make sure that the server knows our current viewport
			this.boardWindow.sendViewport();
			// Asynchronously request the missing chunks from the server
			this.boardWindow.rippleLink.send({
				"Event": "ChunkUpdateRequest",
				"ForgottenChunks": missingChunks
			});
		}
	}
	
	/**
	 * Renders the specified area to the given canvas with the given context.
	 * @param	{Rectangle}					visibleArea	The area to render.
	 * @param	{number}					chunkSize	The size of the chunks on the current plane.
	 * @param	{HTMLCanvasElement}			canvas		The canvas to draw on.
	 * @param	{CanvasRenderingContext2D}	context		The rendering context to
	 * 												 	use to draw on the canvas.
	 */
	renderVisible(visibleArea, canvas, context)
	{
		context.save();
		let chunkSize = this.boardWindow.gridSize;
		let chunkArea = ChunkCache.CalculateChunkArea(visibleArea, chunkSize);
		
		for(let cx = chunkArea.x; cx <= chunkArea.x + chunkArea.width; cx += chunkSize)
		{
			for(let cy = chunkArea.y; cy <= chunkArea.y + chunkArea.height; cy += chunkSize)
			{
				let cChunk = new ChunkReference(
					this.boardWindow.currentPlaneName,
					// Translate from plane space to chunk space when creating
					// a _chunk_ reference
					cx / chunkSize, cy / chunkSize
				);
				
				let chunk = this.fetchChunk(cChunk, true);
				if(chunk != null)
					chunk.render(canvas, context, this, chunkArea);
				
				if(this.showRenderedChunks) {
					context.beginPath();
					context.fillStyle = `hsla(270.5, 79.6%, 55.9%, ${typeof chunk != "undefined" ? 0.3 : 0.1})`;
					context.fillRect(cx, cy, chunkSize, chunkSize);
				}
			}
		}
		
		context.restore();
	}
	
	handleChunkUpdate(message)
	{
		for (let chunkData of message.Chunks) {
			let newChunkRef = new ChunkReference(
				this.boardWindow.currentPlaneName,
				chunkData.Location.X,
				chunkData.Location.Y
			);
			
			console.info(`Chunk Update @ ${newChunkRef}`)
			
			let newChunk = new Chunk(newChunkRef, chunkData.Size);
			let newLines = chunkData.lines.map((line) => {
				line.Points = line.Points.map((raw) => new Vector(raw.X, raw.Y));
				if(line.ContinuesIn != null) {
					line.ContinuesIn = new ChunkReference(
						this.boardWindow.currentPlaneName,
						line.ContinuesIn.X, line.ContinuesIn.Y
					);
				}
				if(line.ContinuesFrom != null) {
					line.ContinuesFrom = new ChunkReference(
						this.boardWindow.currentPlaneName,
						line.ContinuesFrom.X, line.ContinuesFrom.Y
					);
				}
				line.ContainingChunk = new ChunkReference(
					this.boardWindow.currentPlaneName,
					line.ContainingChunk.X,
					line.ContainingChunk.Y
				);
				return line;
			});
			newChunk.lines = newChunk.lines.concat(newLines);
			
			this.add(newChunk, true);
		}
	}
}

/**
 * Calculates the area of the chunks that cover a specified box.
 * @param	{Rectangle}	visibleArea	The box to calculate the covering chunks for.
 * @param	{number}	chunkSize	The size of the chunks that cover the box.
 * @return	{Rectangle}	The area of the chunks that cover the box.
 */
ChunkCache.CalculateChunkArea = function(visibleArea, chunkSize) {
	return new Rectangle(
		Math.floor(visibleArea.x / chunkSize) * chunkSize,
		Math.floor(visibleArea.y / chunkSize) * chunkSize,
		(Math.ceil((Math.abs(visibleArea.x) + (visibleArea.width)) / chunkSize) * chunkSize),
		(Math.ceil((Math.abs(visibleArea.y) + (visibleArea.height)) / chunkSize) * chunkSize)
	);
	
}

export default ChunkCache;
