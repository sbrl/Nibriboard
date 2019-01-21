using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nibriboard.RippleSpace;
using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole.Modules
{
	public class CommandPlanes : ICommandModule
	{
		private NibriboardServer server;

		public ModuleDescription Description { get; } = new ModuleDescription(
			"planes",
			"{{subcommand}}",
			"Interact with planes"
		);

		public CommandPlanes()
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
				await request.WriteLine("Nibriboard Server Command Console: planes");
				await request.WriteLine("-----------------------------------------");
				await request.WriteLine(Description.ToLongString());
				await request.WriteLine();
				await request.WriteLine("Subcommands:");
				await request.WriteLine("    list {{{{output_mode:text|csv}}");
				await request.WriteLine("        List all the currently loaded planes");
				await request.WriteLine("    listuser {{{{username}}}} {{{{output_mode:text|csv}}");
				await request.WriteLine("        List all the planes the specified username has access to");
				await request.WriteLine("    create {{new-plane-name}} [{{chunkSize}}]");
				await request.WriteLine("        Create a new named plane, optionally with the specified chunk size");
				await request.WriteLine("    status {{plane-name}}");
				await request.WriteLine("        Show the statistics of the specified plane");
				await request.WriteLine();
				return;
			}

			await CommandParser.ExecuteSubcommand(this, request.Arguments[1], request);
		}

		public async Task ListUser(CommandRequest request)
		{
			string username = request.GetArg(2, "");
			OutputMode outputMode = CommandParser.ParseOutputMode(request.GetArg(3, "text"));

			if (username.Length == 0) {
				await request.WriteLine("Error: no username specified.");
				return;
			}

			// FUTURE: We need to be able to distinguish between viewing & editing here, amongst other places
			bool canViewAny = server.AccountManager.GetByName(username).HasPermission(server.AccountManager.ResolvePermission("view-any-plane"));
			IEnumerable<Plane> planes = server.PlaneManager.Planes.Where((Plane nextPlane) => canViewAny || nextPlane.HasMember(username));

			if (outputMode == OutputMode.CSV)
				await request.WriteLine("Plane Name,Role");
			foreach (Plane nextPlane in planes)
			{
				object[] formatArgs = new object[] {
					nextPlane.Name,
					(nextPlane.HasCreator(username) || canViewAny ? "Creator" : "Member")
				};

				switch (outputMode) {
					case OutputMode.Text:
						await request.WriteLine("{0} as {1}", formatArgs);
						break;
					case OutputMode.CSV:
						await request.WriteLine(string.Join(",", formatArgs.Select((object arg) => arg.ToString())));
						break;
				}
			}

			//await request.Write(generatePlaneList(server.PlaneManager.Planes, outputMode));
		}

		public async Task List(CommandRequest request)
		{
			OutputMode outputMode = CommandParser.ParseOutputMode(request.GetArg(2, "text"));

			await request.Write(generatePlaneList(server.PlaneManager.Planes, outputMode));
		}

		private string generatePlaneList(IEnumerable<Plane> planes, OutputMode outputMode)
		{
			StringBuilder result = new StringBuilder();

			if (outputMode == OutputMode.CSV)
				result.AppendLine("Name,Chunk Size,Loaded Chunk Count,Soft Loaded Chunk Limit,Unloadable Chunks,Total Saved Chunks");
			else
				result.AppendLine("Planes:\n");
			
			foreach (Plane plane in planes)
			{
				object[] formatArgs = new object[] {
					plane.Name,
					plane.ChunkSize,
					plane.LoadedChunks,
					plane.SoftLoadedChunkLimit,
					plane.UnloadableChunks,
					plane.TotalSavedChunks
				};

				switch (outputMode) {
					case OutputMode.Text:
						result.AppendLine(string.Format("    {0} @ {1} ({2} / ~{3} chunks loaded, {4} inactive, {5} total at last save)", formatArgs));
						break;
					case OutputMode.CSV:
						result.AppendLine(string.Join(",", formatArgs.Select((object arg) => arg.ToString())));
						break;
				}
			}

			if (outputMode == OutputMode.Text)
				result.AppendLine($"Total {server.PlaneManager.Planes.Count}\n");

			return result.ToString();
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


			await request.WriteLine($"Ok: Created plane with name {newPlaneName} and chunk size {chunkSize}.");
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
