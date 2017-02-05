using System;
using System.Drawing;
using Newtonsoft.Json;

namespace SBRL.Utilities
{
	/// <summary>
	/// Represents a rectangle in 2D space.
	/// </summary>
	public struct Rectangle
	{
		/// <summary>
		/// A rectangle with all it's properties initialised to zero.
		/// </summary>
		public static Rectangle Zero = new Rectangle() { X = 0, Y = 0, Width = 0, Height = 0 };

		#region Core Data
		/// <summary>
		/// The X coordinate of the rectangle.
		/// </summary>
		public int X { get; set; }
		/// <summary>
		/// The Ycoordinateof the rectangle.
		/// </summary>
		/// <value>The y.</value>
		public int Y { get; set; }
		/// <summary>
		/// The width of the rectangle.
		/// </summary>
		public int Width { get; set; }
		/// <summary>
		/// The height of the rectangle.
		/// </summary>
		public int Height { get; set; }
		#endregion

		#region Corners
		/// <summary>
		/// The top-left corner of the rectangle.
		/// </summary>
		[JsonIgnore]
		public Point TopLeft {
			get {
				return new Point(X, Y);
			}
		}
		/// <summary>
		/// The top-right corner of the rectangle.
		/// </summary>
		[JsonIgnore]
		public Point TopRight {
			get {
				return new Point(X + Width, Y);
			}
		}
		/// <summary>
		/// The bottom-left corner of the rectangle.
		/// </summary>
		[JsonIgnore]
		public Point BottomLeft {
			get {
				return new Point(X, Y + Height);
			}
		}
		/// <summary>
		/// The bottom-right corner of the rectangle.
		/// </summary>
		[JsonIgnore]
		public Point BottomRight {
			get {
				return new Point(X + Width, Y + Height);
			}
		}
		#endregion

		#region Edges
		/// <summary>
		/// The Y coordinate of the top of the rectangle.
		/// </summary>
		[JsonIgnore]
		public int Top {
			get {
				return Y;
			}
		}
		/// <summary>
		/// The Y coordinate of the bottom of the rectangle.
		/// </summary>
		[JsonIgnore]
		public int Bottom {
			get {
				return Y + Width;
			}
		}
		/// <summary>
		/// The X coordinate of the left side of the rectangle.
		/// </summary>
		[JsonIgnore]
		public int Left {
			get {
				return X;
			}
		}
		/// <summary>
		/// The X coordinate of the right side of the rectangle.
		/// </summary>
		[JsonIgnore]
		public int Right {
			get {
				return X + Width;
			}
		}
		#endregion

		public Rectangle(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
	}
}

