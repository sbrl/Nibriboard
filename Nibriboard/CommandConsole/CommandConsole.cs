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
				await dest.WriteLineAsync("    checkpassword {username} {password}");
				await dest.WriteLineAsync("        Checks a user's password");
				await dest.WriteLineAsync("    setpassword {username} {password}");
				await dest.WriteLineAsync("        Resets a user's password");
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
				case "checkpassword":
					string checkUsername = (commandParts[2] ?? "").Trim();
					string checkPassword = (commandParts[3] ?? "").Trim();

					if (checkUsername.Length == 0) {
						await dest.WriteLineAsync("Error: No username specified!");
						break;
					}
					if (checkPassword.Length == 0) {
						await dest.WriteLineAsync("Error: No password specified!");
						break;
					}

					User checkUser = server.AccountManager.GetByName(checkUsername);
					if (checkUser == null) {
						await dest.WriteLineAsync($"Error: User '{checkUsername}' was not found.");
						break;
					}
					if (!checkUser.CheckPassword(checkPassword)) {
						await dest.WriteLineAsync("Error: That password was incorrect.");
						break;
					}

					await dest.WriteLineAsync("Password check ok!");
					break;
				case "setpassword":
					string setPasswordUsername = (commandParts[2] ?? "").Trim();
					string setPasswordPass = (commandParts[3] ?? "").Trim();

					if(setPasswordUsername.Length == 0) {
						await dest.WriteLineAsync("Error: No username specified.");
						break;
					}
					if(setPasswordPass.Length == 0) {
						await dest.WriteLineAsync("Error: No password specified.");
					}

					User setPasswordUser = server.AccountManager.GetByName(setPasswordUsername);
					if(setPasswordUser == null) {
						await dest.WriteLineAsync($"Error: User '{setPasswordUsername}' wasn't found.");
						break;
					}

					setPasswordUser.SetPassword(setPasswordPass);
					await server.SaveUserData();
					await dest.WriteLineAsync($"Updated password for {setPasswordUser.Username} successfully.");
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
