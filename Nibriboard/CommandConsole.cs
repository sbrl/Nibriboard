using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Nibriboard.Client;
using Nibriboard.RippleSpace;
using Nibriboard.Userspace;
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
				case "version":
					await dest.WriteLineAsync($"Nibriboard Server {NibriboardServer.Version}, built on {NibriboardServer.BuildDate.ToString("R")}");
					await dest.WriteLineAsync("By Starbeamrainbowlabs, licensed under MPL-2.0");
					break;
				case "save":
					await dest.WriteAsync("Saving ripple space - ");
					Stopwatch timer = Stopwatch.StartNew();
					long bytesWritten = await server.PlaneManager.Save();
					long msTaken = timer.ElapsedMilliseconds;
					await dest.WriteLineAsync("done.");
					await dest.WriteLineAsync($"{Formatters.HumanSize(bytesWritten)} written in {msTaken}ms.");
					await dest.WriteLineAsync($"Save is now {Formatters.HumanSize(server.PlaneManager.LastSaveSize)} in size.");
					break;
				case "plane":
					await handlePlaneCommand(commandParts, dest);
					break;

				case "clients":

					foreach(NibriClient client in server.AppServer.NibriClients) {
						await dest.WriteLineAsync($"{client.Id}: {client.Name} from {client.RemoteEndpoint}, on {client.CurrentPlane.Name} looking at {client.CurrentViewPort}");
					}
					await dest.WriteLineAsync();
					await dest.WriteLineAsync($"Total {server.AppServer.ClientCount} clients");
					break;

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

		private async Task handlePlaneCommand(string[] commandParts, StreamWriter dest)
		{
			if (commandParts.Length < 2)
			{
				await dest.WriteLineAsync("Nibriboard Server Command Console: plane");
				await dest.WriteLineAsync("----------------------------------------");
				await dest.WriteLineAsync("Interact with planes.");
				await dest.WriteLineAsync("Usage:");
				await dest.WriteLineAsync("    plane {subcommand}");
				await dest.WriteLineAsync();
				await dest.WriteLineAsync("Subcommands:");
				await dest.WriteLineAsync("    list");
				await dest.WriteLineAsync("        List all the currently loaded planes");
				await dest.WriteLineAsync("    create {new-plane-name} [{chunkSize}]");
				await dest.WriteLineAsync("        Create a new named plane, optionally with the specified chunk size");
				await dest.WriteLineAsync("    status {plane-name}");
				await dest.WriteLineAsync("        Show the statistics of the specified plane");
				return;
			}
			string subAction = commandParts[1].Trim();
			switch (subAction)
			{
				case "list":
					await dest.WriteLineAsync("Planes:");
					foreach (Plane plane in server.PlaneManager.Planes)
						await dest.WriteLineAsync($"    {plane.Name} @ {plane.ChunkSize} ({plane.LoadedChunks} / ~{plane.SoftLoadedChunkLimit} chunks loaded, {plane.UnloadableChunks} inactive, {plane.TotalChunks} total at last save)");
					await dest.WriteLineAsync();
					await dest.WriteLineAsync($"Total {server.PlaneManager.Planes.Count}");
					break;
				case "create":
					if (commandParts.Length < 3)
					{
						await dest.WriteLineAsync("Error: No name specified for the new plane!");
						return;
					}
					string newPlaneName = commandParts[2].Trim();
					int chunkSize = server.PlaneManager.DefaultChunkSize;
					if (commandParts.Length >= 4)
						chunkSize = int.Parse(commandParts[3]);

					server.PlaneManager.CreatePlane(new PlaneInfo(
						newPlaneName,
						chunkSize
					));

					await dest.WriteLineAsync($"Created plane with name {newPlaneName} and chunk size {chunkSize}.");

					break;
				case "status":
					if (commandParts.Length < 3)
					{
						await dest.WriteLineAsync("Error: No plane name specified!");
						return;
					}

					string targetPlaneName = commandParts[2].Trim();
					Plane targetPlane = server.PlaneManager.GetByName(targetPlaneName);
					if (targetPlane == null)
					{
						await dest.WriteLineAsync($"Error: A plane with the name {targetPlaneName} doesn't exist.");
						return;
					}

					await dest.WriteLineAsync($"Name: {targetPlane.Name}");
					await dest.WriteLineAsync($"Chunk size: {targetPlane.ChunkSize}");
					await dest.WriteLineAsync($"Loaded chunks: {targetPlane.LoadedChunks}");
					await dest.WriteLineAsync($"Unloaded chunks: {targetPlane.TotalChunks - targetPlane.LoadedChunks}");
					await dest.WriteLineAsync($"Total chunks: {targetPlane.TotalChunks}");
					await dest.WriteLineAsync($"Primary chunk area size: {targetPlane.PrimaryChunkAreaSize}");
					await dest.WriteLineAsync($"Min unloadeable chunks: {targetPlane.MinUnloadeableChunks}");
					await dest.WriteLineAsync($"Soft loaded chunk limit: {targetPlane.SoftLoadedChunkLimit}");

					break;
				default:
					await dest.WriteLineAsync($"Error: Unknown sub-action {subAction}.");
					break;
			}
		}

		private async Task handleUsersCommand(string[] commandParts, StreamWriter dest)
		{
			if (commandParts.Length < 2)
			{
				await dest.WriteLineAsync("Nibriboard Server Command Console: users");
				await dest.WriteLineAsync("----------------------------------------");
				await dest.WriteLineAsync("Interact with user accounts.");
				await dest.WriteLineAsync("Usage:");
				await dest.WriteLineAsync("    users {subcommand}");
				await dest.WriteLineAsync();
				await dest.WriteLineAsync("Subcommands:");
				await dest.WriteLineAsync("    list");
				await dest.WriteLineAsync("        Lists all users.");
				await dest.WriteLineAsync("    add {username} {password}");
				await dest.WriteLineAsync("        Adds a new user");
				await dest.WriteLineAsync("    roles list");
				await dest.WriteLineAsync("        Lists all roles");
				await dest.WriteLineAsync("    roles grant {role-name} {username}");
				await dest.WriteLineAsync("        Adds a role to a user");
				await dest.WriteLineAsync("    roles revoke {role-name} {username}");
				await dest.WriteLineAsync("        Removes a role from a user");
				return;
			}

			string subAction = commandParts[1].Trim();
			switch (subAction)
			{
				case "list":
					await dest.WriteLineAsync(
						string.Join("\n", server.AccountManager.Users.Select(
							(User user) => $"{user.CreationTime}\t{user.Username}\t{string.Join(", ", user.Roles.Select((RbacRole role) => role.Name))}"
						))
					);
					break;
				case "add":
					string newUsername = (commandParts[2] ?? "").Trim();
					string password = (commandParts[3] ?? "").Trim();

					if (newUsername.Length == 0) {
						await dest.WriteLineAsync("Error: No username specified!");
						break;
					}
					if (password.Length == 0) {
						await dest.WriteLineAsync("Error: No password specified!");
						break;
					}

					server.AccountManager.AddUser(newUsername, password);
					await server.SaveUserData();
					await dest.WriteLineAsync($"Added user with name {newUsername} successfully.");
					break;
				case "roles":
					await handleRoleCommand(commandParts, dest);
					break;
				default:
					await dest.WriteLineAsync($"Unrecognised sub-command {subAction}.");
					break;
			}
		}

		private async Task handleRoleCommand(string[] commandParts, StreamWriter dest)
		{
			string subAction = (commandParts[2] ?? "").Trim();
			if (subAction.Length == 0)
			{
				await dest.WriteLineAsync($"Error: No sub-action specified.");
				return;
			}

			switch (subAction)
			{
				case "list":
					await dest.WriteLineAsync(string.Join("\n", server.AccountManager.Roles.Select(
						(RbacRole role) => role.ToString()
					)));
					break;
				case "grant":
					await handleRoleGrantCommand(commandParts, dest);
					break;

				case "revoke":
					await handleRoleRevokeCommand(commandParts, dest);
					break;
			}
		}

		private async Task handleRoleGrantCommand(string[] commandParts, StreamWriter dest)
		{

			string roleName = (commandParts[3] ?? "").Trim();
			string recievingUsername = (commandParts[4] ?? "").Trim();
			if (roleName.Length == 0) {
				await dest.WriteLineAsync("Error: No role name specified!");
				return;
			}
			if (recievingUsername.Length == 0) {
				await dest.WriteLineAsync("Error: No username specified!");
				return;
			}

			User user = server.AccountManager.GetByName(recievingUsername);
			RbacRole roleToGrant = server.AccountManager.ResolveRole(roleName);
			if (user == null) {
				await dest.WriteLineAsync($"Error: No user with the the name {recievingUsername} could be found.");
				return;
			}
			if (roleToGrant == null) {
				await dest.WriteLineAsync($"Error: No role with the the name {roleName} could be found.");
				return;
			}
			if (user.HasRole(roleToGrant)) {
				await dest.WriteLineAsync($"Error: {recievingUsername} already has the role {roleToGrant.Name}.");
				return;
			}

			user.Roles.Add(roleToGrant);
			await server.SaveUserData();

			await dest.WriteLineAsync($"Role {roleToGrant.Name} added to {user.Username} successfully.");
		}
		private async Task handleRoleRevokeCommand(string[] commandParts, StreamWriter dest)
		{

			string roleName = (commandParts[3] ?? "").Trim();
			string recievingUsername = (commandParts[4] ?? "").Trim();
			if (roleName.Length == 0) {
				await dest.WriteLineAsync("Error: No role name specified!");
				return;
			}
			if (recievingUsername.Length == 0) {
				await dest.WriteLineAsync("Error: No username specified!");
				return;
			}

			User user = server.AccountManager.GetByName(recievingUsername);
			RbacRole roleToGrant = server.AccountManager.ResolveRole(roleName);
			if (user == null) {
				await dest.WriteLineAsync($"Error: No user with the the name {recievingUsername} could be found.");
				return;
			}
			if (roleToGrant == null) {
				await dest.WriteLineAsync($"Error: No role with the the name {roleName} could be found.");
				return;
			}
			if (!user.HasRole(roleToGrant)) {
				await dest.WriteLineAsync($"Error: {recievingUsername} doesn't have the role {roleToGrant.Name}.");
				return;
			}

			user.Roles.Remove(roleToGrant);
			await server.SaveUserData();

			await dest.WriteLineAsync($"Role {roleToGrant.Name} removed from {user.Username} successfully.");
		}
	}
}
