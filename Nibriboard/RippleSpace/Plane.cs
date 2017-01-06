using System;
using System.Collections.Generic;
namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents an infinite plane.
	/// </summary>
	public class Plane
	{
		/// <summary>
		/// The size of the chunks on this plane.
		/// </summary>
		public readonly int ChunkSize;

		/// <summary>
		/// The chunkspace that holds the currently loaded and active chunks.
		/// </summary>
		protected Dictionary<ChunkReference, Chunk> loadedChunkspace = new Dictionary<ChunkReference, Chunk>();

		public Plane(int inChunkSize)
		{
			ChunkSize = inChunkSize;
		}
	}
}
