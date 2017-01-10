using System;
using System.Reflection;
using IotWeb.Server;
using IotWeb.Common.Http;
namespace Nibriboard
{
	public class Nibriboard
	{
		private HttpServer httpServer;

		public readonly int Port = 31586;

		public Nibriboard()
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
					"index.hmtl"
				)
			);
			httpServer.Start();
		}
	}
}
