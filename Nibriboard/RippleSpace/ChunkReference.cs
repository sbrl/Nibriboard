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
	public class ChunkReference : Reference<int>
	{
		/// <summary>
		/// The region size to use.
		/// Regions are used when saving and loading to avoid too many files being stored in a 
		/// single directory.
		/// </summary>
		public static int RegionSize = 32;

		/// <summary>
		/// Converts this ChunkReference into a RegionReference.
		/// A RegionReference is used when saving, to work out which folder it should go in.
		/// The size of the regions used is determined by the <see cref="RegionSize" /> property.
		/// </summary>
		public ChunkReference RegionReference {
			get {
				return new ChunkReference(
					Plane,
					(int)Math.Floor((float)X / (float)RegionSize),
					(int)Math.Floor((float)Y / (float)RegionSize)
				);
			}
		}

		public ChunkReference(Plane inPlane, int inX, int inY) : base(inPlane, inX, inY)
		{
			
		}
		/// <summary>
		/// Creates a new blank <see cref="ChunkReference" />.
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

		public string AsFilepath()
		{
			return Path.Combine($"Region_{RegionReference.X},{RegionReference.Y}", $"{X},{Y}.chunk");
		}

		public override int GetHashCode()
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
