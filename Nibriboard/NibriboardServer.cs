using System;
using System.Reflection;
using IotWeb.Server;
using IotWeb.Common.Http;
using System.Net;
namespace Nibriboard
{
	public class NibriboardServer
	{
		private HttpServer httpServer;

		public readonly int Port = 31586;

		public NibriboardServer()
		{
		}

		public void Setup()
		{
			httpServer = new HttpServer(Port);
			httpServer.AddHttpRequestHandler(
				"/",
				new HttpResourceHandler(
					Assembly.GetExecutingAssembly(),
					"Nibriboard",
					"index.html"
				)
			);
			httpServer.AddWebSocketRequestHandler(
				"/RipplespaceConnection",

			);
			httpServer.Start();
			Log.WriteLine("[NibriboardServer] Started on port {0}", Port);
		}

		private void Test()
		{
			HttpListener.
		}
	}
}
