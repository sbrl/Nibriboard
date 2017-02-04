using System;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using IotWeb.Common.Http;
using SBRL.Utilities;
using Nibriboard.Client.Messages;
using Newtonsoft.Json;
using System.Reflection;
using RippleSpace;

namespace Nibriboard.Client
{
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
			["handshakeRequest"] = typeof(HandshakeRequestMessage)
		};

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
		public Rectangle CurrentViewPort { get; private set; } = Rectangle.Empty;
		/// <summary>
		/// The absolute position in plane-space of this client's cursor.
		/// </summary>
		/// <value>The absolute cursor position.</value>
		public Point AbsoluteCursorPosition { get; private set; } = Point.Empty;

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
			MethodInfo handlerInfo = clientType.GetMethod(handlerMethodName);
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
			client.Send(Encoding.UTF8.GetBytes(message));
		}

		/// <summary>
		/// Generates a new ClientState object representing this client's state at the current time.
		/// </summary>
		public ClientState GenerateStateSnapshot()
		{
			ClientState result = new ClientState();
			result.Id = Id;
			result.Name = Name;
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
			Send(GenerateClientStateUpdate());

			return Task.CompletedTask;
		}

		/// <summary>
		/// Handles an incoming cursor position message from the client..
		/// </summary>
		protected Task handleCursorPositionMessage(CursorPositionMessage message) {
			AbsoluteCursorPosition = message.AbsCursorPosition;

			// Send the update to the other clients
			ClientStateMessage updateMessage = new ClientStateMessage();
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
		protected ClientStateMessage GenerateClientStateUpdate()
		{
			ClientStateMessage result = new ClientStateMessage();
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
