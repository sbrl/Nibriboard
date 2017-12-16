using System;
using System.Drawing;
using Newtonsoft.Json;

namespace SBRL.Utilities
{
	/// <summary>
	/// Represents a rectangle in 2D space.
	/// </summary>
	/// <version>v0.1</version>
	/// <changelog>
	/// v0.1 - 1st April 2017
	/// 	 - Added this changelog!
	/// v0.2 - 4th May 2017
	/// 	 - Fixed Overlap(Rectangle otherRectangle) method
	/// </changelog>
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
		public double X { get; set; }
		/// <summary>
		/// The Ycoordinateof the rectangle.
		/// </summary>
		/// <value>The y.</value>
		public double Y { get; set; }
		/// <summary>
		/// The width of the rectangle.
		/// </summary>
		public double Width { get; set; }
		/// <summary>
		/// The height of the rectangle.
		/// </summary>
		public double Height { get; set; }
		#endregion

		#region Corners
		/// <summary>
		/// The top-left corner of the rectangle.
		/// </summary>
		[JsonIgnore]
		public Vector2 TopLeft {
			get {
				return new Vector2(X, Y);
			}
		}
		/// <summary>
		/// The top-right corner of the rectangle.
		/// </summary>
		[JsonIgnore]
		public Vector2 TopRight {
			get {
				return new Vector2(X + Width, Y);
			}
		}
		/// <summary>
		/// The bottom-left corner of the rectangle.
		/// </summary>
		[JsonIgnore]
		public Vector2 BottomLeft {
			get {
				return new Vector2(X, Y + Height);
			}
		}
		/// <summary>
		/// The bottom-right corner of the rectangle.
		/// </summary>
		[JsonIgnore]
		public Vector2 BottomRight {
			get {
				return new Vector2(X + Width, Y + Height);
			}
		}
		#endregion

		#region Edges
		/// <summary>
		/// The Y coordinate of the top of the rectangle.
		/// </summary>
		[JsonIgnore]
		public double Top {
			get {
				return Y;
			}
			set {
				Y = value;
			}
		}
		/// <summary>
		/// The Y coordinate of the bottom of the rectangle.
		/// </summary>
		[JsonIgnore]
		public double Bottom {
			get {
				return Y + Height;
			}
			set {
				Height = value - Y;
			}
		}
		/// <summary>
		/// The X coordinate of the left side of the rectangle.
		/// </summary>
		[JsonIgnore]
		public double Left {
			get {
				return X;
			}
			set {
				X = value;
			}
		}
		/// <summary>
		/// The X coordinate of the right side of the rectangle.
		/// </summary>
		[JsonIgnore]
		public double Right {
			get {
				return X + Width;
			}
			set {
				Width = value - X;
			}
		}
		#endregion

		public Rectangle(double x, double y, double width, double height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Figures out whether this rectangle overlaps another rectangle.
		/// </summary>
		/// <param name="otherRectangle">The other rectangle to check the overlap of.</param>
		/// <returns>Whether this rectangle overlaps another rectangle.</returns>
		public bool Overlap(Rectangle otherRectangle)
		{
			if(Top > otherRectangle.Bottom ||
			   Bottom < otherRectangle.Top ||
			   Left > otherRectangle.Right ||
			   Right < otherRectangle.Left)
				return false;
			
			return true;
		}

		/// <summary>
		/// Returns a Rectangle representing the area that this rectangle overlaps with another.
		/// Returns an empty rectangle if the two don't overlap at all.
		/// </summary>
		/// <param name="otherRectangle">The other rectangle that overlaps this one.</param>
		/// <returns>The area that this rectanagle overlaps with another.</returns>
		public Rectangle OverlappingArea(Rectangle otherRectangle)
		{
			if(!Overlap(otherRectangle))
				return Rectangle.Zero;

			Rectangle result = new Rectangle();
			result.Top = Math.Max(Top, otherRectangle.Top);
			result.Left = Math.Max(Left, otherRectangle.Left);
			result.Bottom = Math.Max(Bottom, otherRectangle.Bottom);
			result.Right = Math.Max(Right, otherRectangle.Right);

			return result;
		}

		public override string ToString()
		{
			return string.Format($"{Width}x{Height} @ ({X}, {Y})");
		}
	}
}

