using System;
using System.Drawing;

namespace Nibriboard.Client.Messages
{
	public class HandshakeRequestMessage : Message
	{
		/// <summary>
		/// The initial visible area on the client's screen.
		/// Very useful for determining which chunks we should send a client when they first connect.
		/// </summary>
		public Rectangle InitialViewport = new Rectangle(
			0, 0,
			1366, 768
		);

		public HandshakeRequestMessage()
		{
		}
	}
}
