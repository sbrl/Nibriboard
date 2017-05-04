"use strict";

import ChunkReference from './ChunkReference';
import Chunk from './Chunk';
import Rectangle from './Utilities/Rectangle';

class ChunkCache
{
	constructor(inBoardWindow)
	{
		this.boardWindow = inBoardWindow;
		this.cache = new Map();
		
		this.boardWindow.rippleLink.on("ChunkUpdate", this.handleChunkUpdate.bind(this));
	}
	
	/**
	 * Adds the given chunk to the chunk cache.
	 * @param {Chunk} chunkData The chunk to add to the cache.
	 */
	add(chunkData)
	{
		if(this.cache.contains(chunkData.chunkRef.toString()))
			throw new Error("Error: We already have a chunk at that location stored.");
		
		this.cache.set(chunkData.chunkRef.toString(), chunkData);
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
			(Math.floor((visibleArea.x + visibleArea.width) / chunkSize) * chunkSize),
			(Math.floor((visibleArea.y + visibleArea.height) / chunkSize) * chunkSize)
		);
		
		context.translate(
			-Math.abs(visibleArea.x - chunkArea.x),
			-Math.abs(visibleArea.y - chunkArea.y)
		);
		
		for(let cx = chunkArea.x; cx <= chunkArea.x + chunkArea.width; cx += chunkSize)
		{
			for(let cy = chunkArea.y; cy <= chunkArea.y + chunkArea.height; cy += chunkSize)
			{
				let cChunk = new ChunkReference(
					this.boardWindow.currentPlaneName,
					cx, cy
				);
				let chunk = this.cache.get(cChunk.toString());
				if(typeof chunk != "undefined")
					chunk.render(canvas, context);
			}
		}
		
		context.restore();
	}
	
	handleChunkUpdate(message)
	{
		for (let chunk of message.Chunks) {
			//let newChunkRef = new ChunkReference();
			let newChunk = new Chunk(chunk.Location);
			
		}
	}
}

export default ChunkCache;
