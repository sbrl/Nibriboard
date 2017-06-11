"use strict";

import Vector from './Utilities/Vector.js';

class ChunkReference extends Vector
{
	constructor(inPlaneName, inX, inY)
	{
		super(inX, inY);
		this.planeName = inPlaneName;
	}
	
	/**
	 * Returns a plain Vector of this chunk reference in plane space.
	 * @param  	{number} chunkSize The size of the chunk this ChunkReference refers to.
	 * @returns	{Vector}           This ChunkReference in plane space.
	 */
	inPlaneSpace(chunkSize)
	{
		return this.clone().multiply(chunkSize);
	}
	
	toString()
	{
		return `ChunkReference: (${this.x}, ${this.y}, ${this.planeName})`;
	}
}

export default ChunkReference;
