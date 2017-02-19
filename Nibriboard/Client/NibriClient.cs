using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Reflection;

using IotWeb.Common.Http;
using Newtonsoft.Json;
using SBRL.Utilities;
using Nibriboard.Client.Messages;
using RippleSpace;

namespace Nibriboard.Client
{
	/// <summary>
	/// A delegate that is used in the event that is fired when a nibri client disconnects.
	/// </summary>
	public delegate void NibriDisconnectedEvent(NibriClient disconnectedClient);

	/// <summary>
	/// Represents a single client connected to the ripple-space on this Nibriboard server.
	/// </summary>
	public class NibriClient
	{
		private static int nextId = 1;
		private static int getNextId() { return nextId++; }

		/// <summary>
		/// This client's unique id.
		/// </summary>
		public readonly int Id = getNextId();
		/// <summary>
		/// The nibri client manager
		/// </summary>
		private readonly NibriClientManager manager;
		/// <summary>
		/// The underlying websocket connection to the client.
		/// Please try not to call the send method on here - use the NibriClient Send() method instead.
		/// </summary>
		private readonly WebSocket client;

		private static readonly Dictionary<string, Type> messageEventTypes = new Dictionary<string, Type>()
		{
			["HandshakeRequest"] = typeof(HandshakeRequestMessage),
			["CursorPosition"] = typeof(CursorPositionMessage)
		};

		/// <summary>
		/// Whether this nibri client is still connected.
		/// </summary>
		public bool Connected = true;
		public event NibriDisconnectedEvent Disconnected;

		/// <summary>
		/// Whether this client has completed the handshake yet or not.
		/// </summary>
		public bool HandshakeCompleted = false;
		/// <summary>
		/// The name this client has assignedto themselves.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; private set; }
		/// <summary>
		/// The current area that this client is looking at.
		/// </summary>
		/// <value>The current view port.</value>
		public Rectangle CurrentViewPort { get; private set; } = Rectangle.Zero;
		/// <summary>
		/// The absolute position in plane-space of this client's cursor.
		/// </summary>
		/// <value>The absolute cursor position.</value>
		public Vector2 AbsoluteCursorPosition { get; private set; } = Vector2.Zero;
		/// <summary>
		/// This client's colour. Used to tell multiple clients apart visually.
		/// </summary>
		public readonly ColourHSL Colour = ColourHSL.RandomSaturated();

		#region Core Setup & Message Routing Logic

		public NibriClient(NibriClientManager inManager, WebSocket inClient)
		{
			manager = inManager;
			client = inClient;

			client.DataReceived += async (WebSocket clientSocket, string frame) => {
				try
				{
					await handleMessage(frame);
				}
				catch (Exception error)
				{
					await Console.Error.WriteLineAsync(error.ToString());
					throw;
				}

				//Task.Run(async () => await onMessage(frame)).Wait();
			};
			// Store whether this NibriClient is still connected or not
			client.ConnectionClosed += (WebSocket socket) => {
				Connected = false;
				Disconnected(this);
			};
		}

		private async Task handleMessage(string frame)
		{
			string eventName = JsonUtilities.DeserializeProperty<string>(frame, "event");

			if(eventName == null) {
				Log.WriteLine("[NibriClient#{0}] Received message that didn't have an event.", Id);
				return;
			}

			if (!messageEventTypes.ContainsKey(eventName)) {
				Log.WriteLine("[NibriClient#{0}] Received message with invalid event {1}.", Id, eventName);
				return;
			}

			Type messageType = messageEventTypes[eventName];
			Type jsonNet = typeof(JsonConvert);
			MethodInfo deserialiserInfo = jsonNet.GetMethods().First(method => method.Name == "DeserializeObject" && method.IsGenericMethod);
			MethodInfo genericInfo = deserialiserInfo.MakeGenericMethod(messageType);
			var decodedMessage = genericInfo.Invoke(null, new object[] { frame });

			string handlerMethodName = "handle" + decodedMessage.GetType().Name;
			Type clientType = this.GetType();
			MethodInfo handlerInfo = clientType.GetMethod(handlerMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
			await (Task)handlerInfo.Invoke(this, new object[] { decodedMessage });
		}

		#endregion

		/// <summary>
		/// Sends a <see cref="Nibriboard.Client.Messages.Message"/> to the client.
		/// If you *really* need to send a raw message to the client, you can do so with the SendRawa() method.
		/// </summary>
		/// <param name="message">The message to send.</param>
		public void Send(Message message)
		{
			SendRaw(JsonConvert.SerializeObject(message));
		}
		/// <summary>
		/// Sends a raw string to the client. Don't use unnless you know what you're doing!
		/// Use the regular Send() method if you can possibly help it.
		/// </summary>
		/// <param name="message">The message to send.</param>
		public void SendRaw(string message)
		{
			if (!Connected)
				throw new InvalidOperationException($"[NibriClient]{Id}] Can't send a message as the client has disconnected.");
			
			client.Send(message);
		}

		/// <summary>
		/// Generates a new ClientState object representing this client's state at the current time.
		/// </summary>
		public ClientState GenerateStateSnapshot()
		{
			ClientState result = new ClientState();
			result.Id = Id;
			result.Name = Name;
			result.Colour = Colour;
			result.AbsCursorPosition = AbsoluteCursorPosition;
			result.Viewport = CurrentViewPort;

			return result;
		}

		#region Message Handlers
		/// <summary>
		/// Handles an incoming handshake request. We should only receive one of these!
		/// </summary>
		protected Task handleHandshakeRequestMessage(HandshakeRequestMessage message)
		{
			CurrentViewPort = message.InitialViewport;
			AbsoluteCursorPosition = message.InitialAbsCursorPosition;

			// Tell everyone else about the new client
			ClientStatesMessage newClientNotification = new ClientStatesMessage();
			newClientNotification.ClientStates.Add(GenerateStateSnapshot());
			manager.Broadcast(this, newClientNotification);

			// Send the new client a response to their handshake request
			HandshakeResponseMessage handshakeResponse = new HandshakeResponseMessage();
			handshakeResponse.Id = Id;
			handshakeResponse.Colour = Colour;
			Send(handshakeResponse);

			// Tell the new client about everyone else who's connected
			// FUTURE: If we need to handle a large number of connections, we should generate this message based on the chunks surrounding the client
			Send(GenerateClientStateUpdate());

			return Task.CompletedTask;
		}

		/// <summary>
		/// Handles an incoming cursor position message from the client..
		/// </summary>
		protected Task handleCursorPositionMessage(CursorPositionMessage message) {
			AbsoluteCursorPosition = message.AbsCursorPosition;

			// Send the update to the other clients
			// TODO: Buffer these updates and send them about 5 times a second
			ClientStatesMessage updateMessage = new ClientStatesMessage();
			updateMessage.ClientStates.Add(this.GenerateStateSnapshot());
			manager.Broadcast(this, updateMessage);

			return Task.CompletedTask;
		}
		#endregion

		/// <summary>
		/// Generates an update message that contains information about the locations and states of all connected clients.
		/// Automatically omits information about the current client.
		/// </summary>
		/// <returns>The client state update message.</returns>
		protected ClientStatesMessage GenerateClientStateUpdate()
		{
			ClientStatesMessage result = new ClientStatesMessage();
			foreach (NibriClient client in manager.Clients)
			{
				// Don't include ourselves in the update message!
				if (client == this)
					continue;
				
				result.ClientStates.Add(client.GenerateStateSnapshot());
			}
			return result;
		}
	}
}
