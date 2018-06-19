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
			
		}

		private async Task list(CommandRequest request)
		{
			await request.WriteLine("Planes:");
			foreach (Plane plane in server.PlaneManager.Planes)
				await request.WriteLine($"    {plane.Name} @ {plane.ChunkSize} ({plane.LoadedChunks} / ~{plane.SoftLoadedChunkLimit} chunks loaded, {plane.UnloadableChunks} inactive, {plane.TotalChunks} total at last save)");
			await request.WriteLine();
			await request.WriteLine($"Total {server.PlaneManager.Planes.Count}");
		}

		private async Task create(CommandRequest request)
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

		private async Task status(CommandRequest request)
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
			await request.WriteLine($"Unloaded chunks: {targetPlane.TotalChunks - targetPlane.LoadedChunks}");
			await request.WriteLine($"Total chunks: {targetPlane.TotalChunks}");
			await request.WriteLine($"Primary chunk area size: {targetPlane.PrimaryChunkAreaSize}");
			await request.WriteLine($"Min unloadeable chunks: {targetPlane.MinUnloadeableChunks}");
			await request.WriteLine($"Soft loaded chunk limit: {targetPlane.SoftLoadedChunkLimit}");
			await request.WriteLine($"Creators: {string.Join(", ", targetPlane.Creators)}");
			await request.WriteLine($"Members: {string.Join(", ", targetPlane.Members)}");
		}
	}
}
