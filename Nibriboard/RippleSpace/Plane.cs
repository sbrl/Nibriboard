using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents an infinite plane.
	/// </summary>
	public class Plane
	{
		/// <summary>
		/// The name of this plane.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The size of the chunks on this plane.
		/// </summary>
		public readonly int ChunkSize;

		/// <summary>
		/// The chunkspace that holds the currently loaded and active chunks.
		/// </summary>
		protected Dictionary<ChunkReference, Chunk> loadedChunkspace = new Dictionary<ChunkReference, Chunk>();

		public Plane(string inName, int inChunkSize)
		{
			Name = inName;
			ChunkSize = inChunkSize;
		}

		public async Task<Chunk> FetchChunk(ChunkReference chunkLocation)
		{
			// If the chunk is in the loaded chunk-space, then return it immediately
			if(loadedChunkspace.ContainsKey(chunkLocation))
			{
				return loadedChunkspace[chunkLocation];
			}

			// Uh-oh! The chunk isn't loaded at moment. Load it quick & then
			// return it fast.
			Chunk loadedChunk = await Chunk.FromFile(this, chunkLocation.AsFilename());
			loadedChunkspace.Add(chunkLocation, loadedChunk);

			return loadedChunk;
		}
	}
}
