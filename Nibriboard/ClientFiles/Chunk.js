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
	
	update(dt)
	{
		
	}
	
	render(canvas, context)
	{
		context.save();
		
		for(let line of this.lines)
		{
			context.beginPath();
			context.moveTo(
				line.Points[0].x - this.chunkRef.x,
				line.Points[0].y - this.chunkRef.y
			);
			for(let i = 1; i < line.Points.length; i++)
			{
				context.lineTo(
					line.Points[i].x - this.chunkRef.x,
					line.Points[i].y - this.chunkRef.y
				);
			}
			
			context.lineWidth = line.Width;
			context.strokeStyle = line.Colour;
			context.stroke();
		}
		
		context.restore();
	}
}

export default Chunk;
