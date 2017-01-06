using System;
using System.Security.Policy;
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
	}
}
