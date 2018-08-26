using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole.Modules
{
	public class CommandShutdown : ICommandModule
	{
		private NibriboardServer server;


		public ModuleDescription Description { get; } = new ModuleDescription(
			"shutdown",
			"Shutdown the server"
		);


		public CommandShutdown()
		{
		}

		public void Setup(NibriboardServer inServer)
		{
			server = inServer;
		}

		public async Task Handle(CommandRequest request)
		{
			// Save the ripplespace
			CommandSave saver = new CommandSave();
			saver.Setup(server);

			await saver.Handle(request);

			// Stop the listeners
			server.AppServer.Stop(request.GetArg(1, "This server is shutting down."));
			server.PlaneManager.StopMaintenanceMonkey();
			server.CommandServer.Stop();

			await request.WriteLine("Stopping Nibriboard Server.");
		}

	}
}
