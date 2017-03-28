"use strict";

import Vector from '../Utilities/Vector';

/**
 * Represents a point in 2d space that's not tied to a particular plane.
 */
class LocationReference extends Vector
{
	
	/**
	 * Fetches a reference to the chunk that this LocationReference
	 * falls inside.
	 * @param {number} chunkSize The chunk size.
	 */
	ContainingChunk(chunkSize) {
		return new ChunkReference(
			chunkSize,
			this.x /= chunkSize,
			this.y /= chunkSize
		);
	}
	
	is(otherLocationRef)
	{
		return this.x === otherLocationRef.x &&
			this.y === otherLocationRef.y;
	}
	
	/**
	 * Returns this LocationReference as a string.
	 * @return {string} This LocationReference as a string.
	 */
	toString()
	{
		return `LocationReference: (${this.x}, ${this.y})`;
	}
}

/**
 * References a chunk in 2d space, that's not tied to a specific plane.
 */
class ChunkReference extends Vector
{
	/**
	 * Creates a new ChunkReference instance.
	 * @param	{number}	inChunkSize	The size of the chunk.
	 * @param	{number}	inX			The x position of the chunk in chunk-space.
	 * @param	{number}	inY			The y position of the chunk in chunk-space.
	 * @return	{ChunkReference}		The new chunk reference.
	 */
	constructor(inChunkSize, inX, inY)
	{
		super(inX, inY);
		this.ChunkSize = inChunkSize;
	}
	
	/**
	 * Converts ths ChunkReference into a LocationReference.
	 * @return	{LocationReference}	This ChunkReference as a LocationReference.
	 */
	AsLocationReference()
	{
		return new LocationReference(
			this.x * this.ChunkSize,
			this.y * this.ChunkSize
		);
	}
	
	/**
	 * Whether this chunk reference references the same chunk as another
	 * chunk reference.
	 * @param  {ChunkReference}  otherChunkRef The other chunk reference to
	 *                                         compare this chunk reference to.
	 * @return {boolean}
	 */
	is(otherChunkRef)
	{
		return this.x === otherChunkRef.x &&
			this.y === otherChunkRef.y &&
			this.ChunkSize === otherChunkRef.ChunkSize; // The chunk size should always be the same, but you never know :P
	}
	
	/**
	 * This LocationReference represented as a string.
	 * @return {string} This LocationReference as a string.
	 */
	toString()
	{
		return `ChunkReference: ${this.ChunkSize} x (${this.x}, ${this.y})`;
	}
}

export { LocationReference, ChunkReference };
