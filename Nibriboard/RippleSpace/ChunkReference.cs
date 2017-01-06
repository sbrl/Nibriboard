using System;
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
		public ChunkReference()
		{
		}
	}
}
