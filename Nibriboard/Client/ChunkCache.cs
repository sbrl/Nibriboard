using System;
using System.Collections.Generic;
using Nibriboard.RippleSpace;

namespace Nibriboard.Client
{
	/// <summary>
	/// Represents a cache of chunk references. Useful for keeping track which chunks
	/// a remote party is currently keeping in memory.
	/// </summary>
	public class ChunkCache
	{
		List<ChunkReference> cache = new List<ChunkReference>();

		/// <summary>
		/// Creates a new empty chunk cache.
		/// </summary>
		public ChunkCache()
		{
		}

		/// <summary>
		/// Adds a chunk reference to the cache.
		/// If the chunk is already in the cache, then it won't be added again.
		/// </summary>
		/// <param name="chunkRef">The chunk reference to add.</param>
		public void Add(ChunkReference chunkRef)
		{
			// If this cache already contains the specified chunk reference, then we
			// probably shouldn't add it to the cache twice
			if(cache.Contains(chunkRef))
				return;

			cache.Add(chunkRef);
		}

		/// <summary>
		/// Adds the given chunk references to the cache.
		/// Quietly skips over duplicate chunk references.
		/// </summary>
		/// <param name="chunkRefs">The chunk references to add.</param>
		public void Add(IEnumerable<ChunkReference> chunkRefs)
		{
			foreach(ChunkReference chunkRef in chunkRefs)
				Add(chunkRef);
		}

		/// <summary>
		/// Remvoes a chunk reference from the cache.
		/// </summary>
		/// <param name="chunkRef">The chunk reference to remove.</param>
		public void Remove(ChunkReference chunkRef)
		{
			cache.Remove(chunkRef);
		}
		/// <summary>
		/// Removes a list of chunk references from the cache.
		/// </summary>
		/// <param name="chunkRefs">The chunk references to remove.</param>
		public void Remove(IEnumerable<ChunkReference> chunkRefs)
		{
			foreach(ChunkReference chunkRef in chunkRefs)
				Remove(chunkRef);
		}

		/// <summary>
		/// Returns whether this cache contains the specified chunk reference.
		/// </summary>
		/// <param name="chunkRef">The chunk reference to check for.</param>
		/// <returns>Whether this cache contaisn  the specified chunk reference..</returns>
		public bool Contains(ChunkReference chunkRef)
		{
			return cache.Contains(chunkRef);
		}

		/// <summary>
		/// Given a list of chunk references, return another list of chunk references
		/// that aren't in this chunk cache.
		/// </summary>
		/// <param name="sourceChunkRefs">The list of chunk references to check against this chunk cache.</param>
		/// <returns>The chunk references missing from this chunk cache.</returns>
		public List<ChunkReference> FindMissing(IEnumerable<ChunkReference> sourceChunkRefs)
		{
			List<ChunkReference> result = new List<ChunkReference>();
			foreach(ChunkReference sourceChunkRef in sourceChunkRefs)
			{
				if(!Contains(sourceChunkRef))
					result.Add(sourceChunkRef);
			}
			return result;
		}
	}
}
