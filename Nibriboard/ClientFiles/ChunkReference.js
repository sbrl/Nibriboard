"use strict";

import Vector from './Utilities/Vector.js';

class ChunkReference extends Vector
{
	constructor(inPlaneName, inX, inY)
	{
		super(inX, inY);
		this.planeName = inPlaneName;
	}
	
	toString()
	{
		return `ChunkReference: (${this.x}, ${this.y}, ${this.planeName})`;
	}
}
