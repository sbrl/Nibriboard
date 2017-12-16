using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Nibriboard.Client;
using Nibriboard.RippleSpace;

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
					await destination.WriteLineAsync("    help                        Show this message");
					await destination.WriteLineAsync("    save                        Save the ripplespace to disk");
					await destination.WriteLineAsync("    plane list                  List all the currently loaded planes");
					await destination.WriteLineAsync("    plane create {new-plane-name} [{chunkSize}]   Create a new named plane, optionally with the specified chunk size");
					await destination.WriteLineAsync("    plane status {plane-name}   Show the statistics of the specified plane");
					await destination.WriteLineAsync("    clients                     List the currently connected clients");
					break;
				case "save":
					await destination.WriteAsync("Saving ripple space - ");
					await server.PlaneManager.Save();
					await destination.WriteLineAsync("done.");
					await destination.WriteLineAsync($"Save is now {BytesToString(server.PlaneManager.LastSaveFileSize)} in size.");
					break;
				case "plane":
					if(commandParts.Length < 2) {
						await destination.WriteLineAsync("Error: No sub-action specified.");
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

		public static string BytesToString(long byteCount)
		{
			string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; // Longs run out around EB
			if(byteCount == 0)
				return "0" + suf[0];
			long bytes = Math.Abs(byteCount);
			int place = (int)Math.Floor(Math.Log(bytes, 1024));
			double num = Math.Round(bytes / Math.Pow(1024, place), 1);
			return (Math.Sign(byteCount) * num).ToString() + suf[place];
		}
	}
}
