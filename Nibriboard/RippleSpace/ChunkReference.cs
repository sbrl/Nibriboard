using System;
using System.Security.Policy;
using System.IO;
namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// References the location of a chunk.
	/// </summary>
	/// <remarks>
	/// Defaults to chunk-space, but absolute plane-space can also be calculated
	/// and obtained (A <see cref="Nibriboard.RippleSpace.LocationReference" />
	/// is returned).
	/// </remarks>
	public class ChunkReference : Reference
	{
		public ChunkReference(Plane inPlane, int inX, int inY) : base(inPlane, inX, inY)
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
				X / Plane.ChunkSize,
				Y / Plane.ChunkSize
			);
		}

		public string AsFilename()
		{
			return $"{Plane.Name}-{X},{Y}.chunk";
		}

		public static ChunkReference Parse(Plane plane, string source)
		{
			if (!source.StartsWith("ChunkReference:"))
				throw new InvalidDataException($"Error: That isn't a valid chunk reference. Chunk references start with 'ChunkReference:'.");

			// Trim the extras off the reference
			source = source.Substring("ChunkReference:".Length);
			source = source.Trim("() \v\t\r\n".ToCharArray());

			int x = source.Substring(0, source.IndexOf(","));
			int y = source.Substring(source.IndexOf(",") + 1);
			return new ChunkReference(
				plane,
				x,
				y
			);
		}
	}
}
