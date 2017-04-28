using System;

using Newtonsoft.Json;

using SBRL.Utilities;

namespace Nibriboard.Client.Messages
{
	public class HandshakeRequestMessage : Message
	{
			/// <summary>
		/// The initial visible area on the client's screen.
		/// Very useful for determining which chunks we should send a client when they first connect.
		/// </summary>
		public Rectangle InitialViewport = Rectangle.Zero;

		/// <summary>
		/// The initial position of the user's cursor.
		/// </summary>
		public Vector2 InitialAbsCursorPosition = Vector2.Zero;

		public HandshakeRequestMessage()
		{
		}
	}
}
