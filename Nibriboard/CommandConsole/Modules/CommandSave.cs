using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole.Modules
{
	public class CommandSave : ICommandModule
	{
		private NibriboardServer server;


		public ModuleDescription Description { get; } = new ModuleDescription(
			"save",
			"Save the ripplespace to disk"
		);


		public CommandSave()
		{
		}

		public void Setup(NibriboardServer inServer)
		{
			server = inServer;
		}

		public async Task Handle(CommandRequest request)
		{
			await request.Write("Saving ripple space - ");

			Stopwatch timer = Stopwatch.StartNew();
			long bytesWritten = await server.PlaneManager.Save();
			long msTaken = timer.ElapsedMilliseconds;

			await request.WriteLine("done.");

			await request.WriteLine($"{Formatters.HumanSize(bytesWritten)} written in {msTaken}ms.");
			await request.WriteLine($"Save is now {Formatters.HumanSize(server.PlaneManager.LastSaveSize)} in size.");

		}

	}
}
