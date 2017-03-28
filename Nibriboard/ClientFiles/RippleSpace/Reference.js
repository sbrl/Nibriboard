"use strict";

/**
 * Represents a point in 2d space that's not tied to a particular plane.
 */
class LocationReference
{
	/**
	 * Creates a new location reference
	 * @param  {number} inX The x coordinate
	 * @param  {number} inY The y coordinate
	 * @return {LocationReference}	The new location reference
	 */
	constructor(inX, inY)
	{
		this.X = inX;
		this.Y = inY;
	}
	
	/**
	 * Returns this LocationReference as a string.
	 * @return {string} This LocationReference as a string.
	 */
	toString()
	{
		return `LocationReference: (${this.X}, ${this.Y})`;
	}
}

/**
 * References a chunk in 2d space, that's not tied to a specific plane.
 */
class ChunkReference
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
		this.ChunkSize = inChunkSize;
		this.X = inX;
		this.Y = inY;
	}
	
	/**
	 * Converts ths ChunkReference into a LocationReference.
	 * @return	{LocationReference}	This ChunkReference as a LocationReference.
	 */
	AsLocationReference()
	{
		return new LocationReference(
			this.X * this.ChunkSize,
			this.Y * this.ChunkSize
		);
	}
	
	/**
	 * This LocationReference represented as a string.
	 * @return {string} This LocationReference as a string.
	 */
	toString()
	{
		return `ChunkReference: ${this.ChunkSize} x (${this.X}, ${this.Y})`;
	}
}

export { LocationReference, ChunkReference };
