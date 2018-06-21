using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nibriboard.Client;
using Nibriboard.CommandConsole.Modules;
using Nibriboard.RippleSpace;
using Nibriboard.Userspace;
using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole
{
	public class CommandConsole
	{
		private NibriboardServer server;
		private TcpListener commandServer;

		private List<ICommandModule> commandModules = new List<ICommandModule>();

		private int commandPort;

		public CommandConsole(NibriboardServer inServer, int inCommandPort)
		{
			server = inServer;
			commandPort = inCommandPort;

			registerModule(new CommandVersion());
			registerModule(new CommandClients());
			registerModule(new CommandSave());
			registerModule(new CommandPlane());
			registerModule(new CommandUsers());
			registerModule(new CommandRoles());
			registerModule(new CommandPermissions());
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
				string[] commandParts = rawCommand.Split(" \t".ToCharArray()).Select((string arg) => arg.Trim()).ToArray();
				string displayCommand = rawCommand;
				if (Regex.Match(displayCommand.ToLower(), @"^users (add|setpassword|checkpassword)") != null)
					displayCommand = Regex.Replace(displayCommand, "(add|checkpassword|setpassword) ([^ ]+) .*$", "$1 $2 *******", RegexOptions.IgnoreCase);
				Log.WriteLine($"[CommandConsole] Client executing {displayCommand}");

				CommandRequest request = new CommandRequest(nextClient, commandParts);

				try
				{
					await executeCommand(request);
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

		private async Task executeCommand(CommandRequest request)
		{
			string commandName = request.Arguments[0].ToLower();

			switch(commandName)
			{
				case "help":
					await request.WriteLine("Nibriboard Server Command Console");
					await request.WriteLine("=================================");
					await request.WriteLine("Available commands:");
					foreach (ICommandModule nextModule in commandModules)
						await request.WriteLine(nextModule.ToString());
					break;

				default:
					foreach (ICommandModule nextModule in commandModules) {
						if (nextModule.Description.Name.ToLower() == commandName) {
							await nextModule.Handle(request);
							return;
						}
					}

					await request.WriteLine($"Error: Unrecognised command {commandName}");
					break;
			}
		}

		private void registerModule(ICommandModule newCommandModule)
		{
			commandModules.Add(newCommandModule);
		}
		private void setupModules()
		{
			foreach (ICommandModule nextModule in commandModules)
				nextModule.Setup(server);
		}
	}
}
