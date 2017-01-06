using System;
namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents a single chunk of an infinite <see cref="Nibriboard.RippleSpace.Plane" />.
	/// </summary>
	public class Chunk
	{
		/// <summary>
		/// The plane that this chunk is located on.
		/// </summary>
		public readonly Plane Plane;

		/// <summary>
		/// The size of this chunk.
		/// </summary>
		public readonly int Size;

		public Chunk(Plane inPlane, int inSize)
		{
			Plane = inPlane;
			Size = inSize;
		}
	}
}
