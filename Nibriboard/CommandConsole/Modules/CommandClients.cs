using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Nibriboard.Client;
using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole.Modules
{
	public class CommandClients : ICommandModule
	{
		private NibriboardServer server;

		public ModuleDescription Description { get; } = new ModuleDescription(
			"clients",
			"[{{output_mode = text|csv}}]",
			"List the currently connected clients"
		);


		public CommandClients()
		{
		}

		public void Setup(NibriboardServer inServer)
		{
			server = inServer;
		}

		public async Task Handle(CommandRequest request)
		{
			OutputMode outputMode = CommandParser.ParseOutputMode(request.GetArg(1, "text"));

			if (outputMode == OutputMode.CSV)
				await request.WriteLine("Id,Name,Remote Endpoint,Current Plane,Viewport");

			foreach (NibriClient client in server.AppServer.NibriClients)
			{
				object[] lineParams = new object[] {
					client.Id,
					client.Name,
					client.RemoteEndpoint,
					client.CurrentPlane.Name,
					client.CurrentViewPort
				};
				string outputLine = string.Format("{0}: {1} from {1}, on {3} looking at {4}", lineParams);

				if (outputMode == OutputMode.CSV)
					outputLine = string.Join(",", lineParams);

				await request.WriteLine(outputLine);
			}
			await request.WriteLine();

			if(outputMode == OutputMode.Text)
				await request.WriteLine($"Total {server.AppServer.ClientCount} clients");

		}

	}
}
