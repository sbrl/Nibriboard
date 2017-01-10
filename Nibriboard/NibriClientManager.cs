using System;
using IotWeb.Common.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
namespace Nibriboard
{
	public class NibriClientManager : IWebSocketRequestHandler
	{
		List<NibriClient> clients = new List<NibriClient>();

		public NibriClientManager()
		{
		}

		/// <summary>
		/// Whether we will accept a given new WebSocket connection or not.
		/// </summary>
		/// <param name="uri">The uri the user connected to.</param>
		/// <param name="protocol">The protocol the user is connecting with.</param>
		/// <returns>Whether we want to accept the WebSocket connection attempt or not.</returns>
		public bool WillAcceptRequest(string uri, string protocol)
		{
			Log.WriteLine("[Nibriboard/Websocket] Accepting {0} via {1}.", uri, protocol);
			return true;
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
	}
}
