using System;
using System.Drawing;

namespace RippleSpace
{
	/// <summary>
	/// Represents a client's state at a particular point in time.
	/// </summary>
	public class ClientState
	{
		/// <summary>
		/// The id of the client.
		/// </summary>
		public int Id;

		/// <summary>
		/// The date and time at which this client state snapshot was captured.
		/// </summary>
		public DateTime TimeCaptured = DateTime.Now;

		/// <summary>
		/// The name the client chose to identify themselves with.
		/// </summary>
		public string Name;

		/// <summary>
		/// The size and position of the client's viewport.
		/// </summary>
		public Rectangle Viewport = Rectangle.Empty;
		/// <summary>
		/// The absolute position of the client's cursor.
		/// </summary>
		public Point AbsCursorPosition = Point.Empty;

		public ClientState()
		{
		}
	}
}

