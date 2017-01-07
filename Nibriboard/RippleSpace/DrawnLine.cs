using System;
using System.Collections.Generic;
namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents a line drawn across the plane.
	/// </summary>
	public class DrawnLine
	{
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
					if (point.ContainingChunk != containingChunk)
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

		public DrawnLine()
		{
			
		}
	}
}
