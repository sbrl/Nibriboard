using System;
using Newtonsoft.Json;

namespace Nibriboard.Client
{
	public class ClientSettings 
	{
		[JsonIgnore]
		public bool SecureWebsocket = false;
		[JsonIgnore]
		public int WebsocketPort;
		[JsonIgnore]
		public string WebsocketHost = "localhost";
		[JsonIgnore]
		public string WebsocketPath;

		public string WebsocketUri {
			get {
				return (SecureWebsocket ? "wss" : "ws") + $"://{WebsocketHost}:{WebsocketPort}{WebsocketPath}";
			}
		}

		public string WebsocketProtocol = "RippleLink";

		public ClientSettings()
		{

		}
	}
}
