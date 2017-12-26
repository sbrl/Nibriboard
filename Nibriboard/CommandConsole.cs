using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Nibriboard.Client;
using Nibriboard.RippleSpace;
using Nibriboard.Utilities;

namespace Nibriboard
{
	public class CommandConsole
	{
		private NibriboardServer server;
		private TcpListener commandServer;

		private int commandPort;

		public CommandConsole(NibriboardServer inServer, int inCommandPort)
		{
			server = inServer;
			commandPort = inCommandPort;
		}

		public async Task Start()
		{
			commandServer = new TcpListener(IPAddress.IPv6Loopback, server.CommandPort);
			commandServer.Start();
			Log.WriteLine("[CommandConsole] Listening on {0}.", new IPEndPoint(IPAddress.IPv6Loopback, server.CommandPort));
			while(true)
			{
				TcpClient nextClient = await commandServer.AcceptTcpClientAsync();

				StreamReader source = new StreamReader(nextClient.GetStream());
				StreamWriter destination = new StreamWriter(nextClient.GetStream()) { AutoFlush = true };

				string rawCommand = await source.ReadLineAsync();
				string[] commandParts = rawCommand.Split(" \t".ToCharArray());
				Log.WriteLine("[CommandConsole] Client executing {0}", rawCommand);

				try
				{
					await executeCommand(destination, commandParts);
				}
				catch(Exception error)
				{
					try
					{
						await destination.WriteLineAsync(error.ToString());
					}
					catch { nextClient.Close(); } // Make absolutely sure that the command server won't die
				}
				nextClient.Close();
			}
		}

		private async Task executeCommand(StreamWriter destination, string[] commandParts)
		{
			string commandName = commandParts[0].Trim();
			switch(commandName)
			{
				case "help":
					await destination.WriteLineAsync("Nibriboard Server Command Console");
					await destination.WriteLineAsync("=================================");
					await destination.WriteLineAsync("Available commands:");
					await destination.WriteLineAsync("    help                 Show this message");
					await destination.WriteLineAsync("    version              Show the version of nibriboard that is currently running");
					await destination.WriteLineAsync("    save                 Save the ripplespace to disk");
					await destination.WriteLineAsync("    plane {subcommand}   Interact with planes");
					await destination.WriteLineAsync("    clients              List the currently connected clients");
					break;
				case "version":
					await destination.WriteLineAsync($"Nibriboard Server {NibriboardServer.Version}, built on {NibriboardServer.BuildDate.ToString("R")}");
					await destination.WriteLineAsync("By Starbeamrainbowlabs, licensed under MPL-2.0");
					break;
				case "save":
					await destination.WriteAsync("Saving ripple space - ");
					Stopwatch timer = Stopwatch.StartNew();
					long bytesWritten = await server.PlaneManager.Save();
					long msTaken = timer.ElapsedMilliseconds;
					await destination.WriteLineAsync("done.");
					await destination.WriteLineAsync($"{Formatters.HumanSize(bytesWritten)} written in {msTaken}ms.");
					await destination.WriteLineAsync($"Save is now {Formatters.HumanSize(server.PlaneManager.LastSaveSize)} in size.");
					break;
				case "plane":
					if(commandParts.Length < 2) {
						await destination.WriteLineAsync("Nibriboard Server Command Console: plane");
						await destination.WriteLineAsync("----------------------------------------");
						await destination.WriteLineAsync("Interact with planes.");
						await destination.WriteLineAsync("Usage:");
						await destination.WriteLineAsync("    plane {subcommand}");
						await destination.WriteLineAsync();
						await destination.WriteLineAsync("Subcommands:");
						await destination.WriteLineAsync("    list");
						await destination.WriteLineAsync("        List all the currently loaded planes");
						await destination.WriteLineAsync("    create {new-plane-name} [{chunkSize}]");
						await destination.WriteLineAsync("        Create a new named plane, optionally with the specified chunk size");
						await destination.WriteLineAsync("    status {plane-name}");
						await destination.WriteLineAsync("        Show the statistics of the specified plane");
						break;
					}
					string subAction = commandParts[1].Trim();
					switch(subAction)
					{
						case "list":
							await destination.WriteLineAsync("Planes:");
							foreach(Plane plane in server.PlaneManager.Planes)
								await destination.WriteLineAsync($"    {plane.Name} @ {plane.ChunkSize} ({plane.LoadedChunks} / ~{plane.SoftLoadedChunkLimit} chunks loaded, {plane.UnloadableChunks} inactive, {plane.TotalChunks} total at last save)");
							await destination.WriteLineAsync();
							await destination.WriteLineAsync($"Total {server.PlaneManager.Planes.Count}");
							break;
						case "create":
							if(commandParts.Length < 3) {
								await destination.WriteLineAsync("Error: No name specified for the new plane!");
								return;
							}
							string newPlaneName = commandParts[2].Trim();
							int chunkSize = server.PlaneManager.DefaultChunkSize;
							if(commandParts.Length >= 4)
								chunkSize = int.Parse(commandParts[3]);
							
							server.PlaneManager.CreatePlane(new PlaneInfo(
								newPlaneName,
								chunkSize
							));

							await destination.WriteLineAsync($"Created plane with name {newPlaneName} and chunk size {chunkSize}.");

							break;
						case "status":
							if(commandParts.Length < 3) {
								await destination.WriteLineAsync("Error: No plane name specified!");
								return;
							}

							string targetPlaneName = commandParts[2].Trim();
							Plane targetPlane = server.PlaneManager.GetByName(targetPlaneName);
							if(targetPlane == null) {
								await destination.WriteLineAsync($"Error: A plane with the name {targetPlaneName} doesn't exist.");
								return;
							}

							await destination.WriteLineAsync($"Name: {targetPlane.Name}");
							await destination.WriteLineAsync($"Chunk size: {targetPlane.ChunkSize}");
							await destination.WriteLineAsync($"Loaded chunks: {targetPlane.LoadedChunks}");
							await destination.WriteLineAsync($"Unloaded chunks: {targetPlane.TotalChunks - targetPlane.LoadedChunks}");
							await destination.WriteLineAsync($"Total chunks: {targetPlane.TotalChunks}");
							await destination.WriteLineAsync($"Primary chunk area size: {targetPlane.PrimaryChunkAreaSize}");
							await destination.WriteLineAsync($"Min unloadeable chunks: {targetPlane.MinUnloadeableChunks}");
							await destination.WriteLineAsync($"Soft loaded chunk limit: {targetPlane.SoftLoadedChunkLimit}");

							break;
						default:
							await destination.WriteLineAsync($"Error: Unknown sub-action {subAction}.");
							break;
					}
					break;

				case "clients":

					foreach(NibriClient client in server.AppServer.NibriClients) {
						await destination.WriteLineAsync($"{client.Id}: {client.Name} from {client.RemoteEndpoint}, on {client.CurrentPlane.Name} looking at {client.CurrentViewPort}");
					}
					await destination.WriteLineAsync();
					await destination.WriteLineAsync($"Total {server.AppServer.ClientCount} clients");

					break;

				/*case "chunk":
					if(commandParts.Length < 2) {
						await destination.WriteLineAsync("Error: No sub-action specified.");
						break;
					}

					string chunkSubAction = commandParts[1].Trim();
					switch(chunkSubAction)
					{
						case "list":
							if(commandParts.Length < 3) {
								await destination.WriteLineAsync("Error: No plane specified to list the chunks of!");
								return;
							}

							Plane plane = server.PlaneManager.GetByName(commandParts[2].Trim());

							foreach(Chunk chunk in plane.
							break;

						default:
							await destination.WriteLineAsync($"Error: Unknown sub-action {chunkSubAction}.");
							break;
					}

					break;*/

				default:
					await destination.WriteLineAsync($"Error: Unrecognised command {commandName}");
					break;
			}
		}
	}
}
