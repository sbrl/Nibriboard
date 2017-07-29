using System;
using System.IO;

using SBRL.Utilities;

namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// References the location of a chunk.
	/// </summary>
	/// <remarks>
	/// Defaults to chunk-space, but absolute plane-space can also be calculated
	/// and obtained (A <see cref="NibriboardServer.RippleSpace.LocationReference" />
	/// is returned).
	/// </remarks>
	public class ChunkReference : Reference
	{
		public ChunkReference(Plane inPlane, int inX, int inY) : base(inPlane, inX, inY)
		{
			
		}
		/// <summary>
		/// Creates a new blank <see cref="Nibriboard.RippleSpace.ChunkReference" />.
		/// Don't use this yourself! This is only for Newtonsoft.Json to use when deserialising references.
		/// </summary>
		public ChunkReference() : base()
		{

		}

		/// <summary>
		/// Converts this chunk-space reference to plane-space.
		/// </summary>
		/// <returns>This chunk-space reference in plane-space.</returns>
		public LocationReference InPlanespace()
		{
			return new LocationReference(
				Plane,
				X * Plane.ChunkSize,
				Y * Plane.ChunkSize
			);
		}
		/// <summary>
		/// Returns a rectangle representing the area of the chunk that this ChunkReference references.
		/// </summary>
		/// <returns>A Rectangle representing this ChunkReference's chunk's area.</returns>
		public Rectangle InPlanespaceRectangle()
		{
			return new Rectangle(
				X * Plane.ChunkSize,
				Y * Plane.ChunkSize,
				(X * Plane.ChunkSize) + Plane.ChunkSize,
				(Y * Plane.ChunkSize) + Plane.ChunkSize
			);
		}

		public string AsFilename()
		{
			return $"{Plane.Name}-{X},{Y}.chunk";
		}

		public override int GetHashCode ()
		{
			return $"({Plane.Name})+{X}+{Y}".GetHashCode();
		}
		public override bool Equals(object obj)
		{
			ChunkReference otherChunkReference = obj as ChunkReference;
			if (otherChunkReference == null)
				return false;

			if (X == otherChunkReference.X && Y == otherChunkReference.Y &&
			   Plane == otherChunkReference.Plane)
			{
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return $"ChunkReference: {base.ToString()}";
		}

		public static ChunkReference Parse(Plane plane, string source)
		{
			if (!source.StartsWith("ChunkReference:"))
				throw new InvalidDataException($"Error: That isn't a valid chunk reference. Chunk references start with 'ChunkReference:'.");

			// Trim the extras off the reference
			source = source.Substring("ChunkReference:".Length);
			source = source.Trim("() \v\t\r\n".ToCharArray());

			int x = int.Parse(source.Substring(0, source.IndexOf(",")));
			int y = int.Parse(source.Substring(source.IndexOf(",") + 1));
			return new ChunkReference(
				plane,
				x,
				y
			);
		}

		/// <summary>
		/// Returns a clone of this ChunkReference.
		/// </summary>
		/// <returns>The newly-cloned instance.</returns>
		public override object Clone()
		{
			return new ChunkReference(Plane, X, Y);
		}
	}
}
