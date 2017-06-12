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
		this.showRenderedChunks = true;
		
		this.boardWindow.rippleLink.on("ChunkUpdate", this.handleChunkUpdate.bind(this));
	}
	
	/**
	 * Adds the given chunk to the chunk cache.
	 * @param	{Chunk}	chunkData	The chunk to add to the cache.
	 * @returns	{bool}	Whether we actually updated an existing chunk in the	
	 * 					cache instead of adding a new one.
	 */
	add(chunkData, override = false)
	{
		var hasChunk = this.cache.has(chunkData.chunkRef.toString());
		if(!override && hasChunk)
			throw new Error("Error: We already have a chunk at that location stored.");
		
		this.cache.set(chunkData.chunkRef.toString(), chunkData);
		
		return !hasChunk;
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
		let chunkArea = new Rectangle(
			Math.floor(visibleArea.x / chunkSize) * chunkSize,
			Math.floor(visibleArea.y / chunkSize) * chunkSize,
			(Math.floor((visibleArea.x + (visibleArea.width / visibleArea.zoomLevel)) / chunkSize) * chunkSize),
			(Math.floor((visibleArea.y + (visibleArea.height / visibleArea.zoomLevel)) / chunkSize) * chunkSize)
		);
		
		/*context.translate(
			-Math.abs(visibleArea.x - chunkArea.x),
			-Math.abs(visibleArea.y - chunkArea.y)
		);*/
		context.translate(
			-visibleArea.x,
			-visibleArea.y
		);
		
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
				
				if(this.showRenderedChunks) {
					context.beginPath();
					context.fillStyle = "hsla(270.5, 79.6%, 55.9%, 0.5)";
					context.fillRect(cx, cy, chunkSize, chunkSize);
				}
				
				let chunk = this.cache.get(cChunk.toString());
				if(typeof chunk != "undefined")
					chunk.render(canvas, context);
			}
		}
		
		context.restore();
	}
	
	handleChunkUpdate(message)
	{
		for (let chunkData of message.Chunks) {
			let newChunkRef = new ChunkReference(
				chunkData.Location.PlaneName,
				chunkData.Location.X,
				chunkData.Location.Y
			);
			
			console.info(`Chunk Update @ ${newChunkRef}`)
			
			let newChunk = new Chunk(newChunkRef, chunkData.Size);
			let newLines = chunkData.lines.map((line) => {
				line.Points = line.Points.map((raw) => new Vector(raw.X, raw.Y));
				return line;
			});
			newChunk.lines = newChunk.lines.concat(newLines);
			
			this.add(newChunk, true);
		}
	}
}

export default ChunkCache;
