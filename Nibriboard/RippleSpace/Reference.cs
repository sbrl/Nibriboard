﻿using System;
using System.Security.Policy;
namespace Nibriboard.RippleSpace
{
	/// <summary>
	/// An abstract class representing a coordinate reference to a location.
	/// </summary>
	public abstract class Reference
	{
		public readonly Plane Plane;

		public int X { get; set; }
		public int Y { get; set; }

		public Reference(Plane inPlane, int inX, int inY)
		{
			Plane = inPlane;
		}
	}
}
