using System;
using System.Collections.Generic;
using System.IO;
using Nibriboard.RippleSpace;

namespace Nibriboard.Client.Messages
{
	public class ChunkUpdateRequestMessage : Message
	{
		/// <summary>
		/// A list of chunks that the client has intentionally forgotten about, and will need
		/// to be resent to the client.
		/// </summary>
		public List<RawChunkReference> ForgottenChunks = new List<RawChunkReference>();

		public ChunkUpdateRequestMessage()
		{
		}

		public List<ChunkReference> ForgottenChunksAsReferences(Plane plane)
		{
			List<ChunkReference> result = new List<ChunkReference>();
			foreach(RawChunkReference rawRef in ForgottenChunks)
			{
				if(rawRef.planeName as string != plane.Name)
					throw new InvalidDataException($"Error: A raw reference was for the plane " +
					                               "'{rawRef.planeName}', but the plane '{plane.Name}' " +
					                               "was specified as the plane to lay the chunk references onto!");

				result.Add(new ChunkReference(plane, rawRef.x, rawRef.y));
			}

			return result;
		}
	}
}
