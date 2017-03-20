using System;
using IotWeb.Common.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using Nibriboard.Client.Messages;
using System.Threading;
using Nibriboard.RippleSpace;

namespace Nibriboard.Client
{
	/// <summary>
	/// Manages a group of <see cref="Nibriboard.Client.NibriClient"/>s.
	/// </summary>
	public class NibriClientManager : IWebSocketRequestHandler
	{
		/// <summary>
		/// The ripple space manager that this client manager is connected to.
		/// </summary>
		public RippleSpaceManager SpaceManager { get; private set; }

		private ClientSettings clientSettings;
		public List<NibriClient> Clients = new List<NibriClient>();

		/// <summary>
		/// The cancellation token that's used by the main server to tell us when we should shut down.
		/// </summary>
		protected CancellationToken canceller;

		/// <summary>
		/// The interval at which heatbeats should be sent to the client.
		/// </summary>
		public readonly int HeatbeatInterval = 5000;

		/// <summary>
		/// The number of clients currently connected to this Nibriboard.
		/// </summary>
		public int ClientCount {
			get {
				return Clients.Count;
			}
		}

		public NibriClientManager(ClientSettings inClientSettings, RippleSpaceManager inSpaceManager, CancellationToken inCancellationToken)
		{
			clientSettings = inClientSettings;
			canceller = inCancellationToken;

			SpaceManager = inSpaceManager;
		}

		/// <summary>
		/// Whether we will accept a given new WebSocket connection or not.
		/// </summary>
		/// <param name="uri">The uri the user connected to.</param>
		/// <param name="protocol">The protocol the user is connecting with.</param>
		/// <returns>Whether we want to accept the WebSocket connection attempt or not.</returns>
		public bool WillAcceptRequest(string uri, string protocol)
		{
			//Log.WriteLine("[Nibriboard/Websocket] Accepting new {0} connection.", protocol);
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
			client.Disconnected += handleDisconnection; // Clean up when the client disconnects

			Clients.Add(client);
		}

		/// <summary>
		/// Sends a message to all the connected clients, except the one who's sending it.
		/// </summary>
		/// <param name="sendingClient">The client sending the message.</param>
		/// <param name="message">The message that is to bee sent.</param>
		public void Broadcast(NibriClient sendingClient, Message message)
		{
			foreach(NibriClient client in Clients)
			{
				// Don't send the message to the sender
				if (client == sendingClient)
					continue;
				
				client.Send(message);
			}
		}
		/// <summary>
		/// Sends a message to everyone on the same plane as the sender, except the sender themselves.
		/// </summary>
		/// <param name="sendingClient">The sending client.</param>
		/// <param name="message">The message to send.</param>
		public void BroadcastPlane(NibriClient sendingClient, Message message)
		{
			foreach(NibriClient client in Clients)
			{
				// Don't send the message to the sender
				if(client == sendingClient)
					continue;
				// Only send the message to others on the same plane
				if(client.CurrentPlane != sendingClient.CurrentPlane)
					continue;

				client.Send(message);
			}
		}

		/// <summary>
		/// Periodically tidies up the client list, disconnecting old clients.
		/// </summary>
		private async Task ClientMaintenanceMonkey()
		{
			while (true) {
				// Exit if we've been asked to shut down
				if (canceller.IsCancellationRequested) {
					close();
					return;
				}

				// Disconnect unresponsive clients.
				foreach (NibriClient client in Clients) {
					// If we haven't heard from this client in a little while, send a heartbeat message
					if(client.MillisecondsSinceLastMessage > HeatbeatInterval)
						client.SendHeartbeat();

					// If the client hasn't sent us a message in a while (even though we sent
					// them a heartbeat to check on them on the last loop), disconnect them
					if (client.MillisecondsSinceLastMessage > HeatbeatInterval * 2)
						client.CloseConnection(new IdleDisconnectMessage());
				}

				await Task.Delay(HeatbeatInterval);
			}
		}

		/// <summary>
		/// Cleans up this NibriClient manager ready for shutdown.
		/// </summary>
		private void close()
		{
			// Close the connection to all the remaining nibri clients, telling them that the server is about to shut down
			foreach (NibriClient client in Clients)
				client.CloseConnection(new ShutdownMessage());
		}

		/// <summary>
		/// Clean up after a client disconnects from the server.
		/// </summary>
		/// <param name="disconnectedClient">The client that has disconnected.</param>
		private void handleDisconnection(NibriClient disconnectedClient)
		{
			Clients.Remove(disconnectedClient);
		}
	}
}
