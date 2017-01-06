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
	}
}
