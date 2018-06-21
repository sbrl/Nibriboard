using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Nibriboard.RippleSpace;
using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole.Modules
{
	public class CommandPlane : ICommandModule
	{
		private NibriboardServer server;

		public ModuleDescription Description { get; } = new ModuleDescription(
			"plane",
			"{{subcommand}}",
			"Interact with planes"
		);

		public CommandPlane()
		{
		}

		public void Setup(NibriboardServer inServer)
		{
			server = inServer;
		}

		public async Task Handle(CommandRequest request)
		{
			if (request.Arguments.Length < 2)
			{
				await request.WriteLine("Nibriboard Server Command Console: plane");
				await request.WriteLine("----------------------------------------");
				await request.WriteLine(Description.ToLongString());
				await request.WriteLine();
				await request.WriteLine("Subcommands:");
				await request.WriteLine("    list");
				await request.WriteLine("        List all the currently loaded planes");
				await request.WriteLine("    create {{new-plane-name}} [{{chunkSize}}]");
				await request.WriteLine("        Create a new named plane, optionally with the specified chunk size");
				await request.WriteLine("    status {{plane-name}}");
				await request.WriteLine("        Show the statistics of the specified plane");
				await request.WriteLine("    grant {{role:Creator|Member}} {{plane-name}} {{username}}");
				await request.WriteLine();
				return;
			}

			await CommandParser.ExecuteSubcommand(this, request.Arguments[1], request);
		}

		public async Task List(CommandRequest request)
		{
			await request.WriteLine("Planes:");
			foreach (Plane plane in server.PlaneManager.Planes)
				await request.WriteLine($"    {plane.Name} @ {plane.ChunkSize} ({plane.LoadedChunks} / ~{plane.SoftLoadedChunkLimit} chunks loaded, {plane.UnloadableChunks} inactive, {plane.TotalSavedChunks} total at last save)");
			await request.WriteLine();
			await request.WriteLine($"Total {server.PlaneManager.Planes.Count}");
		}

		public async Task Create(CommandRequest request)
		{
			if (request.Arguments.Length < 3)
			{
				await request.WriteLine("Error: No name specified for the new plane!");
				return;
			}

			string newPlaneName = request.Arguments[2];
			int chunkSize = server.PlaneManager.DefaultChunkSize;
			if (request.Arguments.Length >= 4)
				chunkSize = int.Parse(request.Arguments[3]);

			// Create the plane and save it to disk
			Plane createdPlane = server.PlaneManager.CreatePlane(new PlaneInfo(
				newPlaneName,
				chunkSize
			));
			await createdPlane.Save(PlaneSavingMode.MetadataOnly);


			await request.WriteLine($"Created plane with name {newPlaneName} and chunk size {chunkSize}.");
		}

		public async Task Status(CommandRequest request)
		{
			if (request.Arguments.Length < 3)
			{
				await request.WriteLine("Error: No plane name specified!");
				return;
			}

			string targetPlaneName = request.Arguments[2];
			Plane targetPlane = server.PlaneManager.GetByName(targetPlaneName);
			if (targetPlane == null)
			{
				await request.WriteLine($"Error: A plane with the name {targetPlaneName} doesn't exist.");
				return;
			}

			await request.WriteLine($"Name: {targetPlane.Name}");
			await request.WriteLine($"Chunk size: {targetPlane.ChunkSize}");
			await request.WriteLine($"Loaded chunks: {targetPlane.LoadedChunks}");
			// BUG: This isn't technically correct, as we can have phantom chunks loaded in the primary chunk area that haven't been drawn to, and so aren't on disk.
			await request.WriteLine($"Unloaded chunks: {targetPlane.TotalSavedChunks - targetPlane.LoadedChunks}");
			await request.WriteLine($"Total saved chunks: {targetPlane.TotalSavedChunks}");
			await request.WriteLine($"Primary chunk area size: {targetPlane.PrimaryChunkAreaSize}");
			await request.WriteLine($"Min unloadeable chunks: {targetPlane.MinUnloadeableChunks}");
			await request.WriteLine($"Soft loaded chunk limit: {targetPlane.SoftLoadedChunkLimit}");
			await request.WriteLine($"Creators: {string.Join(", ", targetPlane.Creators)}");
			await request.WriteLine($"Members: {string.Join(", ", targetPlane.Members)}");
		}
	}
}
