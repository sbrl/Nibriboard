using System;
namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// Represents a location in absolute plane-space.
	/// </summary>
	public class LocationReference : Reference
	{
		public LocationReference(Plane inPlane, int inX, int inY) : base(inPlane, inX, inY)
		{
			
		}

		public static LocationReference Parse(Plane plane, string source)
		{
			// TODO: Decide if this is the format that we want to use for location references
			if (!source.StartsWith("LocationReference:"))
				throw new InvalidDataException($"Error: That isn't a valid location reference. Location references start with 'ChunkReference:'.");

			// Trim the extras off the reference
			source = source.Substring("LocationReference:".Length);
			source = source.Trim("() \v\t\r\n".ToCharArray());

			int x = source.Substring(0, source.IndexOf(","));
			int y = source.Substring(source.IndexOf(",") + 1);
			return new LocationReference(
				plane,
				x,
				y
			);
		}
	}
}
