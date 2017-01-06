using System;
using System.Security.Policy;
namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// An abstract class representing a coordinate reference to a location.
	/// </summary>
	public abstract class Reference
	{
		public int X { get; set; }
		public int Y { get; set; }
	}
}
