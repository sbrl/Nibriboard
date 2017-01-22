using System;
using IotWeb.Common.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace Nibriboard.Client
{
	public class NibriClientManager : IWebSocketRequestHandler
	{
		private ClientSettings clientSettings;
		private List<NibriClient> clients = new List<NibriClient>();

		/// <summary>
		/// The number of clients currently connected to this Nibriboard.
		/// </summary>
		public int ClientCount {
			get {
				return clients.Count;
			}
		}

		public NibriClientManager(ClientSettings inClientSettings)
		{
			clientSettings = inClientSettings;
		}

		/// <summary>
		/// Whether we will accept a given new WebSocket connection or not.
		/// </summary>
		/// <param name="uri">The uri the user connected to.</param>
		/// <param name="protocol">The protocol the user is connecting with.</param>
		/// <returns>Whether we want to accept the WebSocket connection attempt or not.</returns>
		public bool WillAcceptRequest(string uri, string protocol)
		{
			Log.WriteLine("[Nibriboard/Websocket] Accepting new {0} connection.", protocol);
			return clientSettings.WebsocketProtocol == protocol;
		}
		/// <summary>
		/// Handles WebSocket clients when they first connect, wrapping them in
		/// a <see cref="Nibriboard.NibriClient" /> instance and adding them to
		/// the client list.
		/// </summary>
		/// <param name="newSocket">New socket.</param>
		public void Connected(WebSocket newSocket)
		{
			NibriClient client = new NibriClient(this, newSocket);
			clients.Add(client);
		}

		public void Broadcast(NibriClient sendingClient, string message)
		{
			foreach(NibriClient client in clients)
			{
				// Don't send the message to the sender
				if (client == sendingClient)
					continue;
				
				client.Send(message);
			}
		}
	}
}
