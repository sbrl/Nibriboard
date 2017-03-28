"use strict";

class Chunk
{
	constructor(inSize, inLocation)
	{
		this.Size = inSize;
		this.Location = inLocation;
		
		this.lines = [];
	}
}

export default Chunk;
