using System;
using System.Collections.Generic;
using System.Linq;
namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents a line drawn across the plane.
	/// </summary>
	public class DrawnLine
	{
		/// <summary>
		/// The id of line that this <see cref="NibriboardServer.RippleSpace.DrawnLine" /> is part of.
		/// Note that this id may not be unique - several lines that were all
		/// drawn at once may also have the same id. This is such that a single
        /// line that was split across multiple chunks can still be referenced and
        /// joined together.
		/// </summary>
		public readonly string LineId;

		/// <summary>
		/// The width of the line.
		/// </summary>
		public int Width;
		/// <summary>
		/// The colour of the line.
		/// </summary>
		public string Colour;
		/// <summary>
		/// The points that represent the line.
		/// </summary>
		public List<LocationReference> Points = new List<LocationReference>();

		/// <summary>
		/// Whether this line spans multiple chunks.
		/// </summary>
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
		/// Gets a reference in chunk-space ot the chunk that this line starts in.
		/// </summary>
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
		protected DrawnLine(string inLineId)
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

			return results;
		}
	}
}
