using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

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
		/// The path to the directory that the plane's information will be stored in.
		/// </summary>
		public readonly string StorageDirectory;

		/// <summary>
		/// The number of milliseconds that should pass since a chunk's last
		/// access in order for it to be considered inactive.
		/// </summary>
		public int InactiveMillisecs = 60 * 1000;

		/// <summary>
		/// The number of chunks in a square around (0, 0) that should always be
		/// loaded.
		/// </summary>
		public int PrimaryChunkAreaSize = 10;

		/// <summary>
		/// The chunkspace that holds the currently loaded and active chunks.
		/// </summary>
		protected Dictionary<ChunkReference, Chunk> loadedChunkspace = new Dictionary<ChunkReference, Chunk>();

		public Plane(string inName, int inChunkSize)
		{
			Name = inName;
			ChunkSize = inChunkSize;

			StorageDirectory = $"./Planes/{Name}";
		}
		/// <summary>
		/// Fetches a list of chunks by a list of chunk refererences.
		/// </summary>
		/// <param name="chunkRefs">The chunk references to fetch the attached chunks for.</param>
		/// <returns>The chunks attached to the specified chunk references.</returns>
		public async Task<List<Chunk>> FetchChunks(List<ChunkReference> chunkRefs)
		{
			List<Chunk> chunks = new List<Chunk>();
			foreach(ChunkReference chunkRef in chunkRefs)
				chunks.Add(await FetchChunk(chunkRef));
			return chunks;
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
			string chunkFilePath = Path.Combine(StorageDirectory, chunkLocation.AsFilename());
			Chunk loadedChunk = await Chunk.FromFile(this, chunkFilePath);
			loadedChunkspace.Add(chunkLocation, loadedChunk);

			return loadedChunk;
		}

		public async Task AddLine(DrawnLine newLine)
		{
			List<DrawnLine> chunkedLine;
			// Split the line up into chunked pieces if neccessary
			if(newLine.SpansMultipleChunks)
				chunkedLine = newLine.SplitOnChunks(ChunkSize);
			else
				chunkedLine = new List<DrawnLine>() { newLine };

			// Add each segment to the appropriate chunk
			foreach(DrawnLine newLineSegment in chunkedLine)
			{
				Chunk containingChunk = await FetchChunk(newLineSegment.ContainingChunk);
				containingChunk.Add(newLineSegment);
			}
		}

		public async Task PerformMaintenance()
		{
			// TODO: Perform maintenance here
		}
	}
}
