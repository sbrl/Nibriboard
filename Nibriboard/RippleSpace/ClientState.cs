using System;

using SBRL.Utilities;

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
		/// The colour associated with the client.
		/// </summary>
		public ColourHSL Colour;

		/// <summary>
		/// The size and position of the client's viewport.
		/// </summary>
		public Rectangle Viewport = Rectangle.Zero;
		/// <summary>
		/// The absolute position of the client's cursor.
		/// </summary>
		public Vector2 AbsCursorPosition = Vector2.Zero;

		public ClientState()
		{
		}
	}
}

