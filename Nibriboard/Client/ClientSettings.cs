using System;
using Newtonsoft.Json;

namespace Nibriboard.Client
{
	public class ClientSettings 
	{
		/// <summary>
		/// Whether the WebSocket is secure.
		/// </summary>
		public bool SecureWebSocket = false;

		/// <summary>
		/// The protocol to use over the WebSocket.
		/// </summary>
		public string WebsocketProtocol = "RippleLink";

		public int WebSocketPort;
		public string WebSocketHost = "localhost";
		public string WebSocketPath;

		public string WebsocketUri {
			get {
				return (SecureWebSocket ? "wss" : "ws") + $"://{WebSocketHost}:{WebSocketPort}{WebSocketPath}";
			}
		}

		public ClientSettings()
		{

		}
	}
}
