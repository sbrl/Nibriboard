using System;
using System.Threading.Tasks;

using IotWeb.Server;
using IotWeb.Common.Http;

using Nibriboard.RippleSpace;
using Nibriboard.Client;
using System.Threading;

namespace Nibriboard
{
	/// <summary>
	/// The main Nibriboard server.
	/// This class manages not only the connected clients, but also holds the master reference to the <see cref="Nibriboard.RippleSpace.RippleSpaceManager"/>.
	/// </summary>
	public class NibriboardServer
	{
		private HttpServer httpServer;

		private ClientSettings clientSettings;
		private RippleSpaceManager planeManager = new RippleSpaceManager();

		private readonly CancellationTokenSource clientManagerCanceller = new CancellationTokenSource();
		private NibriClientManager clientManager;

		public readonly int Port = 31586;

		public NibriboardServer(int inPort = 31586)
		{
			Port = inPort;

			clientSettings = new ClientSettings() {
				WebsocketHost = "192.168.0.56",
				WebsocketPort = Port,
				WebsocketPath = "/RipplespaceLink"
			};

			// HTTP Server setup
			httpServer = new HttpServer(Port);
			httpServer.AddHttpRequestHandler(
				"/",
				new HttpEmbeddedFileHandler("Nibriboard.ClientFiles")
				/*new HttpResourceHandler(
					Assembly.GetExecutingAssembly(),
					"ClientFiles",
					"index.html"
				)*/
			);
			httpServer.AddHttpRequestHandler(
				"/Settings.json",
				new HttpClientSettingsHandler(clientSettings)
			);

			// Websocket setup
			clientManager = new NibriClientManager(
				clientSettings,
				
				clientManagerCanceller.Token
			);
			httpServer.AddWebSocketRequestHandler(
				clientSettings.WebsocketPath,

				clientManager
			);
		}

		public async Task Start()
		{
			httpServer.Start();
			Log.WriteLine("[NibriboardServer] Started on port {0}", Port);

			await planeManager.StartMaintenanceMonkey();
		}
	}
}
