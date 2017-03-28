"use strict";

/**
 * Manages a local cache of chunks.
 */
class ChunkCache
{
	constructor()
	{
		this.chunks = new Map();
	}
	
	/**
	 * Adds the specified chunks to the chunk cache.
	 * @param {Chunk[]} chunks The chunks to add.
	 */
	addChunks(...chunks)
	{
		for (let { reference, chunk } of chunks) {
			this.chunks.set(reference.toString(), chunk);
		}
	}
	
	/**
	 * Figures out whether a chunk with the specified chunk reference is
	 * currently present in this chunk cache.
	 * @param	{ChunkReference}	chunkRef	The chunk reference to search for.
	 * @return	{Boolean}			Whether the specified chunk reference is
	 * 								currently in this chunk cache.
	 */
	hasChunk(chunkRef)
	{
		return this.chunks.has(chunkRef.toString());
	}
	
	/**
	 * Forgets the specified chunk references.
	 * @param  {ChunkReference[]} chunkRefs References to the chunks to forget.
	 */
	forgetChunk(...chunkRefs)
	{
		for (let chunkRef of chunkRefs)
			this.chunks.delete(chunkRef.toString());
	}
	
	/**
	 * Forgets all the chunks currently in the chunk cache.
	 */
	forgetAll()
	{
		this.chunks = new Map();
	}
	
	/**
	 * Returns the number of chunks currently in the chunk cache.
	 */
	get CacheSize()
	{
		return this.chunks.size;
	}
	
	[Symbol.iterator]()
	{
		return this.chunks[Symbol.iterator];
	}
}

export default ChunkCache;
