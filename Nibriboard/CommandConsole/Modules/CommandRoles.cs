using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nibriboard.RippleSpace;
using Nibriboard.Userspace;
using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole.Modules
{
	public class CommandRoles : ICommandModule
	{
		private NibriboardServer server;

		public ModuleDescription Description { get; } = new ModuleDescription(
			"roles",
			"{{subcommand}}",
			"Manage roles"
		);

		public CommandRoles()
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
				await request.WriteLine("Nibriboard Server Command Console: roles");
				await request.WriteLine("----------------------------------------");
				await request.WriteLine(Description.ToLongString());
				await request.WriteLine();
				await request.WriteLine("Subcommands:");
				await request.WriteLine("    list");
				await request.WriteLine("        Lists all roles");
				await request.WriteLine("    grant {{{{role-name}}}} {{{{username}}}}");
				await request.WriteLine("        Adds a role to a user");
				await request.WriteLine("    revoke {{{{role-name}}}} {{{{username}}}}");
				await request.WriteLine("        Removes a role from a user");
				return;
			}

			await CommandParser.ExecuteSubcommand(this, request.Arguments[1], request);
		}

		public async Task List(CommandRequest request)
		{
			await request.WriteLine(string.Join("\n", server.AccountManager.Roles.Select(
				(RbacRole role) => role.ToString()
			)));
		}

		public async Task Grant(CommandRequest request)
		{
			string roleName = (request.Arguments[2] ?? "").Trim();
			string targetUsername = (request.Arguments[3] ?? "").Trim();
			if (roleName.Length == 0) {
				await request.WriteLine("Error: No role name specified!");
				return;
			}
			if (targetUsername.Length == 0) {
				await request.WriteLine("Error: No username specified!");
				return;
			}

			User user = server.AccountManager.GetByName(targetUsername);
			RbacRole roleToGrant = server.AccountManager.ResolveRole(roleName);
			if (user == null) {
				await request.WriteLine($"Error: No user with the the name {targetUsername} could be found.");
				return;
			}
			if (roleToGrant == null) {
				await request.WriteLine($"Error: No role with the the name {roleName} could be found.");
				return;
			}
			if (user.HasRole(roleToGrant)) {
				await request.WriteLine($"Error: {targetUsername} already has the role {roleToGrant.Name}.");
				return;
			}

			user.Roles.Add(roleToGrant);
			await server.SaveUserData();

			await request.WriteLine($"Role {roleToGrant.Name} added to {user.Username} successfully.");
		}

		public async Task Revoke(CommandRequest request)
		{
			string roleName = (request.Arguments[2] ?? "").Trim();
			string recievingUsername = (request.Arguments[3] ?? "").Trim();
			if (roleName.Length == 0) {
				await request.WriteLine("Error: No role name specified!");
				return;
			}
			if (recievingUsername.Length == 0) {
				await request.WriteLine("Error: No username specified!");
				return;
			}

			User user = server.AccountManager.GetByName(recievingUsername);
			RbacRole roleToGrant = server.AccountManager.ResolveRole(roleName);
			if (user == null) {
				await request.WriteLine($"Error: No user with the the name {recievingUsername} could be found.");
				return;
			}
			if (roleToGrant == null) {
				await request.WriteLine($"Error: No role with the the name {roleName} could be found.");
				return;
			}
			if (!user.HasRole(roleToGrant)) {
				await request.WriteLine($"Error: {recievingUsername} doesn't have the role {roleToGrant.Name}.");
				return;
			}

			user.Roles.Remove(roleToGrant);
			await server.SaveUserData();

			await request.WriteLine($"Role {roleToGrant.Name} removed from {user.Username} successfully.");
		}
	}
}
