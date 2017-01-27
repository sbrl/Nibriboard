using System;
using System.Drawing;

namespace SBRL.Utilities
{
	/// <summary>
	/// A selection of extensions to the System.Drawing.Point class to make things easier :)
	/// </summary>
	public static class PointExtensions
	{
		public static Point Add(this Point a, Point b)
		{
			return new Point(
				a.X + b.X,
				a.Y + b.X
			);
		}
		public static Point Subtract(this Point a, Point b)
		{
			return new Point(
				a.X - b.X,
				a.Y - b.X
			);
		}
		public static Point Divide(this Point a, int b)
		{
			return new Point(
				a.X / b,
				a.Y / b
			);
		}
		public static Point Multiply(this Point a, int b)
		{
			return new Point(
				a.X * b,
				a.Y * b
			);
		}
	}
}
