using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Nibriboard.CommandConsole.Modules;

namespace Nibriboard.CommandConsole
{
	public class CommandConsoleServer
	{
		private NibriboardServer server;
		private TcpListener commandServer;

		private CancellationTokenSource canceller = new CancellationTokenSource();
		private CancellationToken cancellationToken {
			get {
				return canceller.Token;
			}
		}

		private List<ICommandModule> commandModules = new List<ICommandModule>();

		private int commandPort;

		public CommandConsoleServer(NibriboardServer inServer, int inCommandPort)
		{
			server = inServer;
			commandPort = inCommandPort;

			registerModule(new CommandVersion());
			registerModule(new CommandStatus());
			registerModule(new CommandClients());
			registerModule(new CommandSave());
			registerModule(new CommandPlanes());
			registerModule(new CommandUsers());
			registerModule(new CommandRoles());
			registerModule(new CommandPermissions());
			registerModule(new CommandShutdown());
		}

		public async Task Start()
		{
			setupModules();

			
			commandServer = new TcpListener(IPAddress.IPv6Loopback, server.CommandPort);
			cancellationToken.Register(commandServer.Stop);
			commandServer.Start();
			Log.WriteLine("[CommandConsole] Listening on {0}.", new IPEndPoint(IPAddress.IPv6Loopback, server.CommandPort));
			while(!cancellationToken.IsCancellationRequested)
			{
				TcpClient nextClient;
				try {
					nextClient = await commandServer.AcceptTcpClientAsync();
				}
				catch (ObjectDisposedException) {
					break;
				}
				ThreadPool.QueueUserWorkItem(handleCommand, nextClient);
			}
			Log.WriteLine("[CommandConsole] Stopped listening on {0}", new IPEndPoint(IPAddress.IPv6Loopback, server.CommandPort));
		}

		public void Stop()
		{
			canceller.Cancel();
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
					await request.WriteLine();
					foreach (ICommandModule nextModule in commandModules)
						await request.WriteLine(nextModule.Description.ToString() + "\n");
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
			Log.WriteLine($"[CommandConsole] Registered module {newCommandModule.Description.Name}");
		}
		private void setupModules()
		{
			foreach (ICommandModule nextModule in commandModules)
				nextModule.Setup(server);
		}
	}
}
