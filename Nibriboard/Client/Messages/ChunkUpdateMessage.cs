using System;
using System.Collections.Generic;

using Nibriboard.RippleSpace;

namespace Nibriboard.Client.Messages
{
	public class ChunkUpdateMessage : Message
	{
		List<Chunk> Chunks = new List<Chunk>();

		public ChunkUpdateMessage()
		{
		}
		/// <summary>
		/// Creates a new ChunkUpdateMessaage containing the given list of chunks.
		/// </summary>
		/// <param name="chunks">Chunks.</param>
		public ChunkUpdateMessage(params Chunk[] chunks)
		{
			Chunks = new List<Chunk>(chunks);
		}
	}
}

