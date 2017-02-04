using System;
using System.Collections.Generic;

using RippleSpace;

namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Represents an update of where a group of connected clients are.
	/// </summary>
	public class ClientStateMessage : Message
	{
		public List<ClientState> ClientStates = new List<ClientState>();

		public ClientStateMessage()
		{
		}
	}
}

