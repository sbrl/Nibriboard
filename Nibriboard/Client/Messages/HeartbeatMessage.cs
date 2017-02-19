using System;

namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Represents a heartbeat message. The server sends this periodically to the client,
	/// to which the client should respond with an identical message.
	/// </summary>
	public class HeartbeatMessage : Message
	{
		public HeartbeatMessage()
		{
		}
	}
}

