using System;
using System.Reflection;

using IotWeb.Server;
using IotWeb.Common.Http;

using Nibriboard.RippleSpace;
using System.Threading.Tasks;

namespace Nibriboard
{
	public class NibriboardServer
	{
		private HttpServer httpServer;

		private RippleSpaceManager planeManager = new RippleSpaceManager();

		public readonly int Port = 31586;

		public NibriboardServer(int inPort = 31586)
		{
			Port = inPort;

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
			httpServer.AddWebSocketRequestHandler(
				"/RipplespaceConnection",
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
