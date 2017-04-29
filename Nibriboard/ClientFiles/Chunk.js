"use strict";

/**
 * Represents a single chunk on a plane.
 * Note that this is the client's representation of the chunk, so it's likely
 * to be a little different to the server's representation.
 */
class Chunk
{
	/**
	 * Creates a new chunk.
	 * @param  {ChunkReference} inChunkRef The location of the new chunk.
	 */
	constructor(inChunkRef)
	{
		this.chunkRef = inChunkRef;
		this.lines = [];
	}
	
	/**
	 * Whether this chunk is located at the specified chunk reference.
	 * @param	{ChunkReference}	otherChunkRef	The chunk reference to check
	 *                                       		ourselves against.
	 * @return	{bool}				Whether this chunk is located at the
	 *                           	specified chunk reference.
	 */
	isAt(otherChunkRef)
	{
		if(this.chunkRef.toString() == otherChunkPos.toString())
			return true;
		return false;
	}
}

export default Chunk;
