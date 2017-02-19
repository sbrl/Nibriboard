using System;

namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Sent by the server to an idle client who's not responding to heartbeats just before the client disconnects them.
	/// </summary>
	public class IdleDisconnectMessage : Message
	{
		public IdleDisconnectMessage()
		{
		}
	}
}

