using System;
using System.Drawing;

using Newtonsoft.Json;
// TODO: In C# you can either have namespaces or types in a namespace - not both.
using Nibriboard.Utilities.JsonConverters;

namespace Nibriboard.Client.Messages
{
	public class HandshakeRequestMessage : Message
	{
		/// <summary>
		/// The initial visible area on the client's screen.
		/// Very useful for determining which chunks we should send a client when they first connect.
		/// </summary>
		[JsonConverter(typeof(RectangleConverter))]
		public Rectangle InitialViewport = Rectangle.Empty;
		/// <summary>
		/// The initial position of the user's cursor.
		/// </summary>
		[JsonConverter(typeof(RectangleConverter))]
		public Point InitialAbsCursorPosition = Point.Empty;

		public HandshakeRequestMessage()
		{
		}
	}
}
