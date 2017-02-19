using System;

namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// A message that's sent by the server to a client to tell them that the server is shutting down.
	/// </summary>
	public class ShutdownMessage : Message
	{
		public ShutdownMessage()
		{
		}
	}
}

