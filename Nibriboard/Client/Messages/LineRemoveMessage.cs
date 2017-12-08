using System;
using System.Collections.Generic;
using System.IO;
using Nibriboard.RippleSpace;

namespace Nibriboard.Client.Messages
{
	public class LineRemoveMessage : Message
	{
		/// <summary>
		/// A reference to the chunk that contains the line to remove.
		/// </summary>
		public RawChunkReference ContainingChunk;

		/// <summary>
		/// The unique id of the line segment to remove.
		/// </summary>
		public string UniqueId;

		public LineRemoveMessage()
		{
		}

		/// <summary>
		/// Returns the raw ContainingChunk as a ChunkReference.
		/// </summary>
		/// <param name="plane">The plane to put the chunk reference on.</param>
		/// <returns>The containing chunk as a regular chunk reference.</returns>
		public ChunkReference ConvertedContainingChunk(Plane plane)
		{
			if(ContainingChunk.planeName as string != plane.Name)
				throw new InvalidDataException($"Error: A raw reference was for the plane " +
					"'{rawRef.planeName}', but the plane '{plane.Name}' " +
					"was specified as the plane to lay the chunk references onto!");
			
			return new ChunkReference(plane, ContainingChunk.x, ContainingChunk.y);
		}
	}
}
