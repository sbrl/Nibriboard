using System;

namespace SBRL.Utilities
{
	/// <summary>
	/// Represents a single podouble in 2D space.
	/// May also be used to represent a direction with a magnitude.
	/// </summary>
	/// <version>v0.1</version>
	/// <changelog>
	/// v0.1 - 1st April 2017
	/// 	 - Added this changelog
	/// </changelog>
	public struct Vector2
	{
		/// <summary>
		/// A Vector 2 with all it's proeprties initialised to zero.
		/// </summary>
		public static Vector2 Zero = new Vector2() { X = 0, Y = 0 };

		/// <summary>
		/// The X coordinate.
		/// </summary>
		public double X { get; set; }
		/// <summary>
		/// The Y coordinate.
		/// </summary>
		public double Y { get; set; }


		public Vector2(double x, double y)
		{
			X = x;
			Y = y;
		}


		public Vector2 Add(Vector2 b)
		{
			return new Vector2(
				X + b.X,
				Y + b.X
			);
		}
		public Vector2 Subtract(Vector2 b)
		{
			return new Vector2(
				X - b.X,
				Y - b.X
			);
		}
		public Vector2 Divide(double b)
		{
			return new Vector2(
				X / b,
				Y / b
			);
		}
		public Vector2 Multiply(double b)
		{
			return new Vector2(
				X * b,
				Y * b
			);
		}
	}
}

