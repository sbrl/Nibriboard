using System;
using System.Threading.Tasks;

using IotWeb.Server;
using IotWeb.Common.Http;

using Nibriboard.RippleSpace;
using Nibriboard.Client;

namespace Nibriboard
{
	public class NibriboardServer
	{
		private HttpServer httpServer;

		private ClientSettings clientSettings;
		private RippleSpaceManager planeManager = new RippleSpaceManager();

		public readonly int Port = 31586;

		public NibriboardServer(int inPort = 31586)
		{
			Port = inPort;

			clientSettings = new ClientSettings() {
				WebsocketHost = "localhost",
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
			httpServer.AddWebSocketRequestHandler(
				clientSettings.WebsocketPath,
				new NibriClientManager()
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
