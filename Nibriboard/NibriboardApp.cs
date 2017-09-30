using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nibriboard.Client;
using Nibriboard.Client.Messages;
using Nibriboard.RippleSpace;
using SBRL.GlidingSquirrel.Http;
using SBRL.GlidingSquirrel.Websocket;
using SBRL.Utilities;

namespace Nibriboard
{
	public class NibriboardAppStartInfo
	{
		public string FilePrefix { get; set; }

		public ClientSettings ClientSettings { get; set; }
		public RippleSpaceManager SpaceManager { get; set; }
	}

	public class NibriboardApp : WebsocketServer
	{
		private string filePrefix;
		private List<string> embeddedFiles = new List<string>(EmbeddedFiles.ResourceList);

		/// <summary>
		/// The ripple space manager that this client manager is connected to.
		/// </summary>
		public RippleSpaceManager SpaceManager { get; private set; }

		public LineIncubator LineIncubator = new LineIncubator();

		private ClientSettings clientSettings;

		public List<NibriClient> NibriClients = new List<NibriClient>();

		/// <summary>
		/// The number of clients currently connected to this Nibriboard.
		/// </summary>
		public int ClientCount {
			get {
				return Clients.Count;
			}
		}

		public NibriboardApp(NibriboardAppStartInfo startInfo, IPAddress inBindAddress, int inPort) : base(inBindAddress, inPort)
		{
			clientSettings = startInfo.ClientSettings;
			SpaceManager = startInfo.SpaceManager;

			filePrefix = startInfo.FilePrefix;
			MimeTypeOverrides.Add(".ico", "image/x-icon");
		}


		public override Task HandleClientConnected(object sender, ClientConnectedEventArgs eventArgs)
		{
			NibriClient client = new NibriClient(this, eventArgs.ConnectingClient);

			client.Disconnected += (NibriClient disconnectedClient) => {
				NibriClients.Remove(disconnectedClient);
				Log.WriteLine("[NibriClient/#{1}] Client disconnected and removed from active clients list.");
			};
			NibriClients.Add(client);

			return Task.CompletedTask;
		}

		public override Task HandleClientDisconnected(object sender, ClientDisconnectedEventArgs eventArgs)
		{
			// We can't use this to remove the NibriClient from the list since we don't know which
			// NibriClient objects wrap which WebsocketClient instances
			return Task.CompletedTask;
		}

		public override async Task HandleHttpRequest(HttpRequest request, HttpResponse response)
		{
			if(request.Method != HttpMethod.GET)
			{
				response.ResponseCode = HttpResponseCode.MethodNotAllowed;
				response.ContentType = "text/plain";
				await response.SetBody("Error: That method isn't supported yet.");
				logRequest(request, response);
				return;
			}

			if(request.Url == "/Settings.json")
			{

				string settingsJson = JsonConvert.SerializeObject(clientSettings);
				response.ContentLength = settingsJson.Length;
				response.ContentType = "application/json";
				await response.SetBody(settingsJson);

				Log.WriteLine("[Http/ClientSettings] Sent settings to {0}", request.ClientAddress);
				return;
			}


			string expandedFilePath = getEmbeddedFileReference(request.Url);
			if(!embeddedFiles.Contains(expandedFilePath))
			{
				expandedFilePath += "index.html";
			}
			if(!embeddedFiles.Contains(expandedFilePath))
			{
				response.ResponseCode = HttpResponseCode.NotFound;
				response.ContentType = "text/plain";
				await response.SetBody($"Can't find expandedFilePath.");
				logRequest(request, response);
				return;
			}

			response.ContentType = LookupMimeType(expandedFilePath);

			string embeddedFile = EmbeddedFiles.ReadAllText(expandedFilePath);

			try
			{
				await response.SetBody(embeddedFile);
			}
			catch(Exception error)
			{
				Log.WriteLine($"[Nibriboard/EmbeddedFileHandler] Error: {error.Message} Details:");
				Log.WriteLine(error.ToString());
			}
			logRequest(request, response);
		}

		#region Interface Methods

		/// <summary>
		/// Sends a message to all the connected clients, except the one who's sending it.
		/// </summary>
		/// <param name="sendingClient">The client sending the message.</param>
		/// <param name="message">The message that is to bee sent.</param>
		public void Broadcast(NibriClient sendingClient, Message message)
		{
			foreach(NibriClient client in NibriClients)
			{
				// Don't send the message to the sender
				if(client == sendingClient)
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
			foreach(NibriClient client in NibriClients)
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
		/// Sends a message to everyone on a specified plane.
		/// </summary>
		/// <param name="plane">The plane to send the message to.</param>
		/// <param name="message">The message to send.</param>
		public void ReflectPlane(Plane plane, Message message)
		{
			foreach(NibriClient client in NibriClients)
			{
				if(client.CurrentPlane != plane)
					continue;
				client.Send(message);
			}
		}

		#endregion

		#region Utility Methods

		protected string getEmbeddedFileReference(string uri)
		{
			return filePrefix + "." + uri.TrimStart("/".ToCharArray()).Replace('/', '.');
		}

		private void logRequest(HttpRequest request, HttpResponse response)
		{
			Log.WriteLine(
				"[Http/FileHandler] {0} {1} {2} {3}",
				response.ResponseCode,
				response.ContentType,
				request.Method,
				request.Url
			);
		}

		#endregion
	}
}
