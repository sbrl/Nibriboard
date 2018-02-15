using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;

using Newtonsoft.Json;
using SBRL.Utilities;
using Nibriboard.Client.Messages;
using Nibriboard.RippleSpace;

using SBRL.GlidingSquirrel.Websocket;
using System.Net;
using Nibriboard.Userspace;

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
		#region Id Generation Logic

		private static int nextId = 1;
		private static int getNextId() { return nextId++; }

		/// <summary>
		/// This client's unique id.
		/// </summary>
		public readonly int Id = getNextId();

		#endregion

		/// <summary>
		/// The nibri client manager
		/// </summary>
		private readonly NibriboardApp manager;
		/// <summary>
		/// The plane that this client is currently on.
		/// </summary>
		public Plane CurrentPlane;

		/// <summary>
		/// The underlying websocket connection to the client.
		/// Please try not to call the send method on here - use the NibriClient Send() method instead.
		/// </summary>
		private readonly WebsocketClient connection;

		private static readonly Dictionary<string, Type> messageEventTypes = new Dictionary<string, Type>() {
			["HandshakeRequest"] = typeof(HandshakeRequestMessage),
			["CursorPosition"] = typeof(CursorPositionMessage),
			["PlaneChange"] = typeof(PlaneChangeMessage),
			["ChunkUpdateRequest"] = typeof(ChunkUpdateRequestMessage),
			["LineStart"] = typeof(LineStartMessage),
			["LinePart"] = typeof(LinePartMessage),
			["LineComplete"] = typeof(LineCompleteMessage),
			["LineRemove"] = typeof(LineRemoveMessage),
			["ViewportUpdate"] = typeof(ViewportUpdateMessage),
			["PlaneListRequest"] = typeof(PlaneListRequestMessage)
		};

		/// <summary>
		/// Whether this nibri client is still connected.
		/// </summary>
		public bool Connected {
			get {
				return !connection.IsClosing;
			}
		}
		public IPEndPoint RemoteEndpoint {
			get {
				return connection.RemoteEndpoint;
			}
		}
		/// <summary>
		/// The user account of the currently connected client.
		/// </summary>
		public User ConnectedUser {
			get {
				if (connection.HandshakeRequest.BasicAuthCredentials == null)
					return null;

				return manager.NibriServer.AccountManager.GetByName(
					connection.HandshakeRequest.BasicAuthCredentials.Username
				);
			}
		}

		/// <summary>
		/// Fires when this nibri client disconnects.
		/// </summary>
		public event NibriDisconnectedEvent Disconnected;

		/// <summary>
		/// The date and time at which the last message was received from this client.
		/// </summary>
		public DateTime LastMessageTime = DateTime.Now;
		/// <summary>
		/// The number of milliseconds since we last received a message from this client.
		/// </summary>
		public int MillisecondsSinceLastMessage {
			get {
				return (int)((DateTime.Now - LastMessageTime).TotalMilliseconds);
			}
		}

		/// <summary>
		/// Whether this client has completed the handshake yet or not.
		/// </summary>
		public bool HandshakeCompleted = false;


		/// <summary>
		/// The name this client has assigned to themselves.
		/// </summary>
		public string Name {
			get {
				return connection.HandshakeRequest.BasicAuthCredentials.Username;
			}
		}
		/// <summary>
		/// The current area that this client is looking at.
		/// </summary>
		public Rectangle CurrentViewPort { get; private set; } = Rectangle.Zero;
		/// <summary>
		/// The absolute position in plane-space of this client's 
		public Vector2 AbsoluteCursorPosition { get; private set; } = Vector2.Zero;
		/// <summary>
		/// This client's colour. Used to tell multiple clients apart visually.
		/// </summary>
		public readonly ColourHSL Colour = ColourHSL.RandomSaturated();
		/// <summary>
		/// The chunk cache. Keeps track of which chunks this client currently has.
		/// </summary>
		protected ChunkCache chunkCache = new ChunkCache();


		#region Core Setup & Message Routing Logic

		public NibriClient(NibriboardApp inManager, WebsocketClient inClient)
		{
			manager = inManager;
			connection = inClient;

			// Attach a few events
			connection.OnDisconnection += handleDisconnection;
			connection.OnTextMessage += handleMessage;

			sendPlaneList(false).Wait();

            Log.WriteLine(
                "[Nibriboard/WebSocket] New NibriClient connected with id #{0} for user {1}.",
                Id,
                ConnectedUser != null ? ConnectedUser.Username : "Anonymous"
            );
		}

		private async Task handleMessage(object sender, TextMessageEventArgs eventArgs)
		{
			// Update the last time we received a message from the client
			LastMessageTime = DateTime.Now;

			// Extract the event name from the message that the client sent.
			string eventName = JsonUtilities.DeserializeProperty<string>(eventArgs.Payload, "Event");

			if(eventName == null) {
				Log.WriteLine("[NibriClient#{0}] Received message that didn't have an event.", Id);
				return;
			}
			if (!messageEventTypes.ContainsKey(eventName)) {
				Log.WriteLine("[NibriClient#{0}] Received message with invalid event {1}.", Id, eventName);
				return;
			}
			if(eventName != "CursorPosition")
				Log.WriteLine("[NibriClient#{0}] Recieved message with event {1}.", Id, eventName);
			
			try
			{
				Type messageType = messageEventTypes[eventName];
				Type jsonNet = typeof(JsonConvert);
				MethodInfo deserialiserInfo = jsonNet.GetMethods().First(method => method.Name == "DeserializeObject" && method.IsGenericMethod);
				MethodInfo genericInfo = deserialiserInfo.MakeGenericMethod(messageType);
				var decodedMessage = genericInfo.Invoke(null, new object[] { eventArgs.Payload });

				string handlerMethodName = "handle" + decodedMessage.GetType().Name;
				Type clientType = this.GetType();
				MethodInfo handlerInfo = clientType.GetMethod(handlerMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
				await (Task)handlerInfo.Invoke(this, new object[] { decodedMessage });
			}
			catch(Exception error)
			{
				Log.WriteLine("[NibriClient#{0}] Error decoding and / or handling message.", Id);
				Log.WriteLine("[NibriClient#{0}] Raw frame content: {1}", Id, eventArgs.Payload);
				Log.WriteLine("[NibriClient#{0}] Exception details: {1}", Id, error);
			}
		}

		private Task handleDisconnection(object sender, ClientDisconnectedEventArgs eventArgs)
		{
			Disconnected?.Invoke(this);
			Log.WriteLine("[NibriClient] Client #{0} disconnected.", Id);

			return Task.CompletedTask;
		}

		#endregion


		#region Message Sending

		/// <summary>
		/// Sends a <see cref="Message"/> to the client.
		/// If you *really* need to send a raw message to the client, you can do so with the SendRaw() method.
		/// </summary>
		/// <param name="message">The message to send.</param>
		public async Task Send(Message message)
		{
			try
			{
				string payload = JsonConvert.SerializeObject(message);
				await SendRaw(payload);
			}
			catch(Exception error)
			{
				Log.WriteLine("[NibriClient/#{0}] Error serialising message!", Id);
				Log.WriteLine("[NibriClient/#{0}] {1}", Id, error);
			}
		}
		/// <summary>
		/// Sends a raw string to the client. Don't use unless you know what you're doing!
		/// Use the regular Send() method if you can possibly help it.
		/// </summary>
		/// <param name="message">The message to send.</param>
		public async Task<bool> SendRaw(string message)
		{
            if (!Connected) {
				Log.WriteLine($"[NibriClient#{Id}] Can't send a message as the client has disconnected.");
                return false;
            }

			Log.WriteLine("[NibriClient/#{0}] Sending message with length {1}.", Id, message.Length);
			
			await connection.Send(message);
            return true;
		}

		/// <summary>
		/// Sends a heartbeat message to this client.
		/// </summary>
		public async Task SendHeartbeat()
		{
			await Send(new HeartbeatMessage());
		}

		#endregion

		/// <summary>
		/// Closes the connection to the client gracefully.
		/// </summary>
		public async Task CloseConnection(Message lastMessage)
		{
			if (!Connected)
				return;
			
			// Tell the client that we're shutting down
			await Send(lastMessage);

			await connection.Close(WebsocketCloseReason.Normal, "Goodbye!");
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
			result.CursorPosition = AbsoluteCursorPosition;
			result.Viewport = CurrentViewPort;

			return result;
		}

		/// <summary>
		/// Determines whether this client can see the chunk at the specified chunk reference.
		/// </summary>
		/// <param name="chunkRef">The chunk reference to check the visibility of.</param>
		/// <returns>Whether this client can see the chunk located at the specified chunk reference</returns>
		public bool CanSee(ChunkReference chunkRef)
		{
			if(chunkRef.Plane != CurrentPlane)
				return false;

			Rectangle chunkArea = chunkRef.InPlanespaceRectangle();

			return chunkArea.Overlap(CurrentViewPort);
		}


		#region Message Handlers
		protected async Task handlePlaneListRequestMessage(PlaneListRequestMessage message)
		{
			await sendPlaneList(message.IsCreator);
		}
		/// <summary>
		/// Handles an incoming handshake request. We should only receive one of these!
		/// </summary>
		protected async Task handleHandshakeRequestMessage(HandshakeRequestMessage message)
		{
			CurrentViewPort = message.InitialViewport;
			AbsoluteCursorPosition = message.InitialAbsCursorPosition;

			// Tell everyone else about the new client
			ClientStatesMessage newClientNotification = new ClientStatesMessage();
			newClientNotification.ClientStates.Add(GenerateStateSnapshot());
			await manager.Broadcast(this, newClientNotification);

			// Send the new client a response to their handshake request
			HandshakeResponseMessage handshakeResponse = new HandshakeResponseMessage();
			handshakeResponse.Id = Id;
			handshakeResponse.Colour = Colour;
			handshakeResponse.Name = Name;
			foreach(Plane plane in manager.NibriServer.PlaneManager.Planes)
				handshakeResponse.Planes.Add(plane.Name);
			
			await Send(handshakeResponse);

			// Tell the new client about everyone else who's connected
			// FUTURE: If we need to handle a large number of connections, we should generate this message based on the chunks surrounding the client
			await Send(GenerateClientStateUpdate());
		}
		/// <summary>
		/// Handles an incoming plane change request.
		/// </summary>
		protected async Task handlePlaneChangeMessage(PlaneChangeMessage message)
		{
			Log.WriteLine("[NibriClient#{0}] Changing to plane {1}.", Id, message.NewPlaneName);

			// Create a new plane with the specified name if it doesn't exist already
			// Makes sure that the uesr has permission to do so
			// future: we might want to allow the user to specify the chunk size
			if (manager.NibriServer.PlaneManager[message.NewPlaneName] == default(Plane))
			{
				if (ConnectedUser.HasPermission("create-plane"))
					manager.NibriServer.PlaneManager.CreatePlane(new PlaneInfo(message.NewPlaneName));
				else {
					await Send(new ExceptionMessage(
						402, "Error: That plane doesn't exist, but you don't have permission to create it."
					));
					return;
				}
			}

			Plane newPlane = manager.NibriServer.PlaneManager[message.NewPlaneName];
			if (!newPlane.HasMember(ConnectedUser.Username)) {
				await Send(new ExceptionMessage(
					403, "Error: You don't have permission to view that plane. Try contacting it's owner!"
				));
				return;
			}

			// Remove the event listener from the old plane if there is indeed an old plane to remove it from
			if(CurrentPlane != null)
				CurrentPlane.OnChunkUpdates -= handleChunkUpdateEvent;
			// Swap out the current plane
			CurrentPlane = manager.NibriServer.PlaneManager[message.NewPlaneName];
			// Attach a listener to the new plane
			CurrentPlane.OnChunkUpdates += handleChunkUpdateEvent;

			// Tell the client that the switch over all went according to plan
			await Send(new PlaneChangeOkMessage() {
				NewPlaneName = message.NewPlaneName,
				GridSize = CurrentPlane.ChunkSize
			});

			// Reset the position to (0, 0) since we've just changed planes
			Rectangle workingViewport = CurrentViewPort;
			workingViewport.X = 0;
			workingViewport.Y = 0;
			CurrentViewPort = workingViewport;

			List<ChunkReference> initialChunks = new List<ChunkReference>();
			ChunkReference currentChunkRef = new ChunkReference(
				CurrentPlane,
				(int)Math.Floor(CurrentViewPort.X / CurrentPlane.ChunkSize),
				(int)Math.Floor(CurrentViewPort.Y / CurrentPlane.ChunkSize)
			);
			while(CanSee(currentChunkRef))
			{
				while(CanSee(currentChunkRef))
				{
					initialChunks.Add(currentChunkRef);
					currentChunkRef = currentChunkRef.Clone() as ChunkReference;
					currentChunkRef.X++;
				}
				currentChunkRef.X = (int)Math.Floor(CurrentViewPort.X / CurrentPlane.ChunkSize);
				currentChunkRef.Y++;
			}

			await SendChunks(initialChunks);
		}
		/// <summary>
		/// Handles requests from clients for chunk updates.
		/// </summary>
		/// <param name="message">The message to process.</param>
		protected async Task handleChunkUpdateRequestMessage(ChunkUpdateRequestMessage message)
		{
			List<ChunkReference> requestedChunkRefs = message.ForgottenChunksAsReferences(this.CurrentPlane);
			chunkCache.Remove(requestedChunkRefs);

			await SendChunks(requestedChunkRefs);
		}

		/// <summary>
		/// Handles an incoming cursor position message from the client..
		/// </summary>
		/// <param name="message">The message to process.</param>
		protected async Task handleCursorPositionMessage(CursorPositionMessage message) {
			AbsoluteCursorPosition = message.AbsCursorPosition;

			// Send the update to the other clients
			// TODO: Buffer these updates and send them about 5 times a second
			// This will improve bandwidth & server resource usage when many clients are connected
			ClientStatesMessage updateMessage = new ClientStatesMessage();
			updateMessage.ClientStates.Add(this.GenerateStateSnapshot());

			await manager.BroadcastPlane(this, updateMessage);
		}

		/// <summary>
		/// Handles viewport updates from the remote client.
		/// </summary>
		/// <param name="message">The viewport update message to handle.</param>
		protected Task handleViewportUpdateMessage(ViewportUpdateMessage message) {
			CurrentViewPort = message.NewViewport;

			return Task.CompletedTask;
		}

		/// <summary>
		/// Handles line start events from the client.
		/// These messages are currently only required to let other clients know about
		/// lines that are being drawn and their properties for live display.
		/// </summary>
		/// <param name="message">The LineStartMessage to process.</param>
		protected async Task handleLineStartMessage(LineStartMessage message)
		{
			// Create a new line
			manager.LineIncubator.CreateLine(
				message.LineId,
				message.LineColour,
				message.LineWidth
			);

			await manager.BroadcastPlane(this, new LineStartReflectionMessage() {
				OtherClientId = Id,
				LineId = message.LineId,
				LineColour = message.LineColour,
				LineWidth = message.LineWidth
			});
		}

		/// <summary>
		/// Handles messages containing a fragment of a line from the client.
		/// </summary>
		/// <param name="message">The message to process.</param>
		protected async Task handleLinePartMessage(LinePartMessage message)
		{
			// Forward the line part to everyone on this plane
			await manager.BroadcastPlane(this, message);

			List<LocationReference> linePoints = new List<LocationReference>(message.Points.Count);
			foreach(Vector2 point in message.Points)
				linePoints.Add(new LocationReference(CurrentPlane, point.X, point.Y));
			
			manager.LineIncubator.AddBit(message.LineId, linePoints);

			await manager.BroadcastPlane(this, new LinePartReflectionMessage() {
				OtherClientId = Id,
				LineId = message.LineId,
				Points = message.Points
			});
		}

		/// <summary>
		/// Handles notifications from clients telling us that they've finished drawing a line.
		/// </summary>
		/// <param name="message">The message to handle.</param>
		protected async Task handleLineCompleteMessage(LineCompleteMessage message)
		{
			// If the line doesn't exist, then ignore it
			if(!manager.LineIncubator.LineExists(message.LineId))
			{
				Log.WriteLine("[NibriClient#{0}/handlers] Ignoring LineComplete event for line that doesn't exist", Id);
				return;
			}
			DrawnLine line = manager.LineIncubator.CompleteLine(message.LineId);

			if(CurrentPlane == null)
			{
				Log.WriteLine("[NibriClient#{0}] Attempt to complete a line before selecting a plane - ignoring");
				await Send(new ExceptionMessage(
					401, "Error: You can't complete a line until you've selected a plane to draw it on!"
				));
				return;
			}

			Log.WriteLine("[NibriClient#{0}] Adding {1}px {2} line", Id, line.Width, line.Colour);
			await Task.WhenAll(
				manager.BroadcastPlane(this, new LineCompleteReflectionMessage() {
					OtherClientId = Id,
					LineId = line.LineId
				}),
				CurrentPlane.AddLine(line)
			);
		}

		/// <summary>
		/// Handles messages requesting that a line be removed from a chunk.
		/// </summary>
		/// <param name="message">The message to handle.</param>
		protected async Task handleLineRemoveMessage(LineRemoveMessage message)
		{
			bool removeSuccess = await CurrentPlane.RemoveLineSegment(
				message.ConvertedContainingChunk(CurrentPlane),
				message.UniqueId
			);
			Log.WriteLine("[NibriClient#{1}] " + (removeSuccess ? "Removed" : "Failed to remove") + " line segment with unique id {0} from {1}", message.UniqueId, message.ConvertedContainingChunk(CurrentPlane));
		}

		#endregion

		#region RippleSpace Event Handlers

		protected void handleChunkUpdateEvent(object sender, MultiChunkUpdateEventArgs eventArgs)
		{
			Log.WriteLine(
				"[NibriClient#{0}] Sending {0} chunk updates (for {1})",
				Id,
				eventArgs.UpdatedChunks.Count(),
				string.Join(", ", eventArgs.UpdatedChunks.Select((Chunk chunk) => chunk.Location))
			);
			ChunkUpdateMessage clientNotification = new ChunkUpdateMessage() {
				Chunks = new List<Chunk>(eventArgs.UpdatedChunks)
			};
			Send(clientNotification).Wait();
		}

		#endregion

		protected async Task sendPlaneList(bool isCreator)
		{
			IEnumerable<Plane> planes = manager.NibriServer.PlaneManager.GetByUser(ConnectedUser, isCreator);
			PlaneListResponseMessage message = new PlaneListResponseMessage();
			foreach (Plane nextPlane in planes)
				message.Planes.Add(new PlaneListItem(nextPlane, nextPlane.HasCreator(ConnectedUser.Username) ? "Creator" : "Member"));

			await Send(message);
		}

		/// <summary>
		/// Generates an update message that contains information about the locations and states of all connected clients.
		/// Automatically omits information about the current client, and clients on other planes.
		/// </summary>
		/// <returns>The client state update message.</returns>
		protected ClientStatesMessage GenerateClientStateUpdate()
		{
			ClientStatesMessage result = new ClientStatesMessage();
			foreach (NibriClient otherClient in manager.NibriClients)
			{
				// Don't include ourselves in the update message!
				if (otherClient == this)
					continue;
				// Only include other nibri clients on our plane
				if(otherClient.CurrentPlane != CurrentPlane)
					continue;
				
				result.ClientStates.Add(otherClient.GenerateStateSnapshot());
			}
			return result;
		}

		/// <summary>
		/// Sends variable list of chunks to this client.
		/// Automatically fetches the chunks by reference from the current plane.
		/// </summary>
		/// <param name="chunkRefs">The references of the chunks to send.</param>
		protected async Task SendChunks(IEnumerable<ChunkReference> chunkRefs)
		{
			if(CurrentPlane == default(Plane))
			{
				await Send(new ExceptionMessage(1, "You're not on a plane yet, so you can't request chunks. " +
										  "Try joining a plane and sending that request again."));
				return;
			}

			if(chunkRefs.Count() == 0) {
				Log.WriteLine("[NibriClient#{0}/SendChunks] Can't send 0 chunks!", Id);
				return;
			}

			// Keep track of the fact that we've sent the client a bunch of chunks
			chunkCache.Add(chunkRefs);

			ChunkUpdateMessage updateMessage = new ChunkUpdateMessage();
			foreach(ChunkReference chunkRef in chunkRefs)
				updateMessage.Chunks.Add(await CurrentPlane.FetchChunk(chunkRef));

			Log.WriteLine("[NibriClient#{0}/SendChunks] Sending {1} chunks", Id, updateMessage.Chunks.Count);
			await Send(updateMessage);
		}
	}
}
