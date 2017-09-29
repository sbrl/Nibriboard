using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents a line drawn across the plane.
	/// </summary>
	[Serializable]
	[JsonObject(MemberSerialization.OptIn)]
	public class DrawnLine
	{
		/// <summary>
		/// This line (fragment?)'s unique id. Should be globally unique - please blow up
		/// if it isn't!
		/// </summary>
		[JsonProperty]
		public readonly string UniqueId = NCuid.Cuid.Generate();

		/// <summary>
		/// The id of line that this <see cref="DrawnLine" /> is part of.
		/// Note that this id may not be unique - several lines that were all
		/// drawn at once may also have the same id. This is such that a single
		/// line that was split across multiple chunks can still be referenced and
		/// joined together.
		/// </summary>
		[JsonProperty]
		public readonly string LineId;

		/// <summary>
		/// The width of the line.
		/// </summary>
		[JsonProperty]
		public int Width;
		/// <summary>
		/// The colour of the line.
		/// </summary>
		[JsonProperty]
		public string Colour;
		/// <summary>
		/// The points that represent the line.
		/// </summary>
		[JsonProperty]
		public List<LocationReference> Points = new List<LocationReference>();

		/// <summary>
		/// Whether this line spans multiple chunks.
		/// </summary>
		[JsonProperty]
		public bool SpansMultipleChunks {
			get {
				// TODO: Make this more intelligent such that connecting lines
				// can be stored that connect lines that span multiple chunks
				if (Points.Count == 0)
					return false;
				ChunkReference containingChunk = Points[0].ContainingChunk;
				foreach(LocationReference point in Points)
				{
					if (!point.ContainingChunk.Equals(containingChunk))
						return true;
				}
				return false;
			}
		}

		/// <summary>
		/// The chunk reference of the next chunk that this line continues in.
		/// A value of null is present when this line doesn't continue into another chunk.
		/// </summary>
		[JsonProperty]
		public ChunkReference ContinuesIn = null;
        /// <summary>
        /// The id of the line that this line fragment is continued by.
        /// </summary>
        [JsonProperty]
        public string ContinuesWithId = null;
		/// <summary>
		/// The chunk reference of the previous chunk that contains the line fragment that
		/// this line continues from. Is null when this line either doesn't continue from
		/// another line fragment or doesn't span multiple chunks.
		/// </summary>
		[JsonProperty]
		public ChunkReference ContinuesFrom = null;
        /// <summary>
        /// The id of the line fragment that this line continues from.
        /// </summary>
        [JsonProperty]
        public string ContinuesFromId = null;

		/// <summary>
		/// Gets a reference in chunk-space ot the chunk that this line starts in.
        /// </summary>
        [JsonProperty]
		public ChunkReference ContainingChunk {
			get {
				if (Points.Count == 0)
					throw new InvalidOperationException("Error: This line doesn't contain any points yet!");
				return Points[0].ContainingChunk;
			}
		}

		public DrawnLine() : this(Guid.NewGuid().ToString("N"))
		{
		}
		public DrawnLine(string inLineId)
		{
			LineId = inLineId;
		}

		/// <summary>
		/// Splits this line into a list of lines that don't cross chunk boundaries.
		/// </summary>
		/// <returns>A list of lines, that, when stitched together, will produce this line.</returns>
		public List<DrawnLine> SplitOnChunks()
		{
			List<DrawnLine> results = new List<DrawnLine>();

			// Don't bother splitting the line up if it all falls in the same chunk
			if (!SpansMultipleChunks)
			{
				results.Add(this);
				return results;
			}

            DrawnLine nextLine = new DrawnLine(LineId);
			ChunkReference currentChunk = null;
			foreach(LocationReference point in Points)
			{
				if(currentChunk != null && !point.ContainingChunk.Equals(currentChunk))
				{
                    // We're heading into a new chunk! Split the line up here.
                    // TODO: Add connecting lines to each DrawnLine instance to prevent gaps
                    nextLine.Colour = Colour;
                    nextLine.Width = Width;
					if(nextLine.Points.Count > 0)
						results.Add(nextLine);
					nextLine = new DrawnLine(LineId);
				}

				nextLine.Points.Add(point);
				if(!point.ContainingChunk.Equals(currentChunk))
					currentChunk = point.ContainingChunk;
			}

			if(nextLine.Points.Count > 0)
			{
				nextLine.Colour = Colour;
				nextLine.Width = Width;
				results.Add(nextLine);
            }

			// Set the ContinuesIn and ContinuesFrom properties
			// so that clients can find the next / previous chunk line fragmentss
			for(int i = 0; i < results.Count - 1; i++)
			{
				// Set the ContinuesFrom reference, but not on the first fragment in the list
                if(i > 0) {
					results[i].ContinuesFrom = results[i - 1].ContainingChunk;
                    results[i].ContinuesFromId = results[i - 1].UniqueId;
                }
				
				// Set the ContinuesIn reference, but not on the last fragment in the list
                if(i < results.Count - 1) {
					results[i].ContinuesIn = results[i + 1].ContainingChunk;
                    results[i].ContinuesWithId = results[i + 1].UniqueId;
                }
			}

			return results;
		}
	}
}
