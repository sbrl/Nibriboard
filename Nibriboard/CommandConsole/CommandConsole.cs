using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nibriboard.Client;
using Nibriboard.RippleSpace;
using Nibriboard.Userspace;
using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole
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
				ThreadPool.QueueUserWorkItem(handleCommand, nextClient);
			}
		}

		private async void handleCommand(object nextClientObj)
		{
			TcpClient nextClient = nextClientObj as TcpClient;
			if (nextClient == null) {
				Log.WriteLine("[CommandConsole/HandleCommand] Unable to cast state object to TcpClient");
				return;
			}

			try
			{
				StreamReader source = new StreamReader(nextClient.GetStream());
				StreamWriter destination = new StreamWriter(nextClient.GetStream()) { AutoFlush = true };

				string rawCommand = await source.ReadLineAsync();
				string[] commandParts = rawCommand.Split(" \t".ToCharArray());
				string displayCommand = rawCommand;
				if (Regex.Match(displayCommand.ToLower(), @"^users (add|setpassword|checkpassword)") != null)
					displayCommand = Regex.Replace(displayCommand, "(add|checkpassword|setpassword) ([^ ]+) .*$", "$1 $2 *******", RegexOptions.IgnoreCase);
				Log.WriteLine($"[CommandConsole] Client executing {displayCommand}");

				try
				{
					await executeCommand(destination, commandParts);
				}
				catch (Exception error)
				{
					try
					{
						await destination.WriteLineAsync(error.ToString());
					}
					catch { nextClient.Close(); } // Make absolutely sure that the command server won't die
				}
				nextClient.Close();
			}
			catch (Exception error)
			{
				Log.WriteLine("[CommandConsole] Uncaught Error: {0}", error.ToString());
			}
		}

		private async Task executeCommand(StreamWriter dest, string[] commandParts)
		{
			string commandName = commandParts[0].Trim();
			switch(commandName)
			{
				case "help":
					await dest.WriteLineAsync("Nibriboard Server Command Console");
					await dest.WriteLineAsync("=================================");
					await dest.WriteLineAsync("Available commands:");
					await dest.WriteLineAsync("    help                 Show this message");
					await dest.WriteLineAsync("    version              Show the version of nibriboard that is currently running");
					await dest.WriteLineAsync("    save                 Save the ripplespace to disk");
					await dest.WriteLineAsync("    plane {subcommand}   Interact with planes");
					await dest.WriteLineAsync("    users                Interact with user accounts");
					await dest.WriteLineAsync("    clients              List the currently connected clients");
					break;
				case "plane":
					await handlePlaneCommand(commandParts, dest);

				case "users":
					await handleUsersCommand(commandParts, dest);
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
					await dest.WriteLineAsync($"Error: Unrecognised command {commandName}");
					break;
			}
		}
	}
}
