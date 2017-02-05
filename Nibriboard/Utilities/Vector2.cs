using System;

namespace SBRL.Utilities
{
	/// <summary>
	/// Represents a single point in 2D space.
	/// May also be used to represent a direction with a magnitude.
	/// </summary>
	public struct Vector2
	{
		/// <summary>
		/// A Vector 2 with all it's proeprties initialised to zero.
		/// </summary>
		public static Vector2 Zero = new Vector2() { X = 0, Y = 0 };

		/// <summary>
		/// The X coordinate.
		/// </summary>
		public int X { get; set; }
		/// <summary>
		/// The Y coordinate.
		/// </summary>
		public int Y { get; set; }


		public Vector2(int x, int y)
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
		public Vector2 Divide(int b)
		{
			return new Vector2(
				X / b,
				Y / b
			);
		}
		public Vector2 Multiply(int b)
		{
			return new Vector2(
				X * b,
				Y * b
			);
		}
	}
}

