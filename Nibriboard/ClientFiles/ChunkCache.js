"use strict";

class ChunkCache
{
	constructor()
	{
		this.cache = new Map();
	}
	
	/**
	 * Adds the given chunk to the chunk cache.
	 * @param {Chunk} chunkData The chunk to add to the cache.
	 */
	add(chunkData)
	{
		if(this.cache.contains(chunkData.chunkRef.toString()))
			throw new Error("Error: We already have a chunk at that location stored.");
		
		this.cache.set(chunkData.toString(), chunkData);
	}
}

export default ChunkCache;
