using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Nibriboard.RippleSpace;
using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole.Modules
{
	public class CommandStatus : ICommandModule
	{
		private NibriboardServer server;

		public ModuleDescription Description { get; } = new ModuleDescription(
			"status",
			"Show status information about this instance of nibriboard"
		);
		
		public CommandStatus()
		{
		}

		public void Setup(NibriboardServer inServer) {
			server = inServer;
		}

		public async Task Handle(CommandRequest request)
		{
			await request.WriteLine($"Version: {NibriboardServer.Version}");
			await request.WriteLine($"Build date: {NibriboardServer.BuildDate.ToString("R")}");
			using (Process process = Process.GetCurrentProcess()) {
				await request.WriteLine($"PID: {process.Id}");
				await request.WriteLine($"Private memory usage: {Formatters.HumanSize(process.PrivateMemorySize64)}");
			}
			await request.WriteLine($"Connected clients: {server.AppServer.NibriClients.Count}");
			await request.WriteLine($"Planes: {server.PlaneManager.Planes.Count}");
			await request.WriteLine($"Total chunks: {server.PlaneManager.Planes.Sum((Plane nextPlane) => nextPlane.TotalSavedChunks)}");
			await request.WriteLine($"...of which loaded: {server.PlaneManager.Planes.Sum((Plane nextPlane) => nextPlane.LoadedChunks)}");
		}

	}
}
