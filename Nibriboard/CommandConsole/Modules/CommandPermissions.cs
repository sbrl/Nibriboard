﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Nibriboard.RippleSpace;
using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole.Modules
{
	public class CommandPermissions : ICommandModule
	{
		private NibriboardServer server;

		public ModuleDescription Description { get; } = new ModuleDescription(
			"perms",
			"{{subcommand}}",
			"View and assign permissions"
		);

		public CommandPermissions()
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
				await request.WriteLine("Nibriboard Server Command Console: perms");
				await request.WriteLine("----------------------------------------");
				await request.WriteLine(Description.ToLongString());
				await request.WriteLine();
				await request.WriteLine("Subcommands:");
				await request.WriteLine("    grant {{{{perm:Creator|Member}}}} {{{{plane-name}}}} {{{{username}}}}");
				await request.WriteLine("        Grant permission on plane-name to username");
				await request.WriteLine("    revoke {{{{perm:Creator|Member}}}} {{{{plane-name}}}} {{{{username}}}}");
				await request.WriteLine("        Revoke username's role permission plane-name");
				await request.WriteLine("    get {{{{plane-name}}}} {{{{username}}}}");
				await request.WriteLine("        Get a username's permission on the specified plane. Note that a user's role needs to be consulted too to determine whether they can or cannot perform an action.");
				await request.WriteLine("    set {{{{perm:Creator|Member}}}} {{{{plane-name}}}} {{{{username}}}}");
				await request.WriteLine("        Set username's perm on plane-name to a specific value, removing all other permissions");
				return;
			}

			await CommandParser.ExecuteSubcommand(this, request.Arguments[1], request);
		}

		public async Task Grant(CommandRequest request)
		{
			if (request.Arguments.Length < 5) {
				await request.WriteLine("Error: No username specified!");
				return;
			}
			if (request.Arguments.Length < 4) {
				await request.WriteLine("Error: No plane name specified!");
				return;
			}
			if (request.Arguments.Length < 3) {
				await request.WriteLine("Error: No role specified!");
				return;
			}
			string grantRoleName = request.Arguments[2];
			string grantPlaneName = request.Arguments[3];
			string grantUsername = request.Arguments[4];

			if (grantRoleName.ToLower() != "creator" && grantRoleName.ToLower() != "member") {
				await request.WriteLine($"Error: Invalid role {grantRoleName}. Valid values: Creator, Member. Is not case-sensitive.");
				return;
			}
			Plane grantPlane = server.PlaneManager.GetByName(grantPlaneName);
			if (grantPlane == null)
			{
				await request.WriteLine($"Error: The plane with the name {grantPlaneName} could not be found.");
				return;
			}
			if (server.AccountManager.GetByName(grantUsername) == null)
			{
				await request.WriteLine($"Error: No user could be found with the name {grantUsername}.");
				return;
			}

			switch (grantRoleName.ToLower())
			{
				case "creator":
					if (grantPlane.Creators.Contains(grantUsername)) {
						await request.WriteLine($"Error: {grantUsername} is already a creator on {grantPlaneName}.");
						return;
					}
					grantPlane.Creators.Add(grantUsername);
					break;
				case "member":
					if (grantPlane.Members.Contains(grantUsername)) {
						await request.WriteLine($"Error: {grantUsername} is already a member on {grantPlaneName}.");
						return;
					}
					grantPlane.Members.Add(grantUsername);
					break;
			}

			await request.WriteLine($"Ok: {grantUsername} has been granted {grantRoleName} on {grantPlaneName} successfully.");
			await request.Write("Saving - ");

			DateTime grantTimeStart = DateTime.Now;
			await grantPlane.Save(PlaneSavingMode.MetadataOnly);

			await request.WriteLine($"done in {(DateTime.Now - grantTimeStart).Milliseconds}ms.");
			
		}

		public async Task Revoke(CommandRequest request)
		{
			if (request.Arguments.Length < 5) {
				await request.WriteLine("Error: No username specified!");
				return;
			}
			if (request.Arguments.Length < 4) {
				await request.WriteLine("Error: No plane name specified!");
				return;
			}
			if (request.Arguments.Length < 3) {
				await request.WriteLine("Error: No role specified!");
				return;
			}
			string roleName = request.Arguments[2];
			string planeName = request.Arguments[3];
			string username = request.Arguments[4];

			if (roleName.ToLower() != "creator" && roleName.ToLower() != "member") {
				await request.WriteLine($"Error: Invalid role {roleName}. Valid values: Creator, Member. Is not case-sensitive.");
				return;
			}
			Plane targetPlane = server.PlaneManager.GetByName(planeName);
			if (targetPlane == null) {
				await request.WriteLine($"Error: The plane with the name {planeName} could not be found.");
				return;
			}
			if (server.AccountManager.GetByName(username) == null) {
				await request.WriteLine($"Error: No user could be found with the name {username}.");
				return;
			}

			switch (roleName.ToLower())
			{
				case "creator":
					targetPlane.Creators.Remove(username);
					break;
				case "member":
					targetPlane.Members.Remove(username);
					break;
			}

			await request.WriteLine($"Ok: {username} has been revoked {roleName} on {planeName} successfully.");
			await request.Write("Saving - ");

			DateTime timeStart = DateTime.Now;
			await targetPlane.Save(PlaneSavingMode.MetadataOnly);

			await request.WriteLine($"done in {(DateTime.Now - timeStart).Milliseconds}ms.");

		}

		public async Task Get(CommandRequest request)
		{
			if (request.Arguments.Length < 4) {
				await request.WriteLine("Error: No username specified!");
				return;
			}
			if (request.Arguments.Length < 3) {
				await request.WriteLine("Error: No plane name specified!");
				return;
			}
			string planeName = request.Arguments[2];
			string username = request.Arguments[3];


			Plane targetPlane = server.PlaneManager.GetByName(planeName);
			if (targetPlane == null) {
				await request.WriteLine($"Error: The plane with the name {planeName} could not be found.");
				return;
			}
			if (server.AccountManager.GetByName(username) == null) {
				await request.WriteLine($"Error: No user could be found with the name {username}.");
				return;
			}

			string role = "None";
			if (targetPlane.Creators.Contains(username))
				role = "Creator";
			else if (targetPlane.Members.Contains(username))
				role = "Member";

			await request.WriteLine(role);

		}

		public async Task Set(CommandRequest request)
		{
			if (request.Arguments.Length < 5) {
				await request.WriteLine("Error: No username specified!");
				return;
			}
			if (request.Arguments.Length < 4) {
				await request.WriteLine("Error: No plane name specified!");
				return;
			}
			if (request.Arguments.Length < 3) {
				await request.WriteLine("Error: No role specified!");
				return;
			}
			string roleName = request.Arguments[2];
			string planeName = request.Arguments[3];
			string username = request.Arguments[4];

			if (roleName.ToLower() != "creator" && roleName.ToLower() != "member")
			{
				await request.WriteLine($"Error: Invalid role {roleName}. Valid values: Creator, Member. Is not case-sensitive.");
				return;
			}

			Plane targetPlane = server.PlaneManager.GetByName(planeName);
			if (targetPlane == null)
			{
				await request.WriteLine($"Error: The plane with the name {planeName} could not be found.");
				return;
			}
			if (server.AccountManager.GetByName(username) == null)
			{
				await request.WriteLine($"Error: No user could be found with the name {username}.");
				return;
			}

			targetPlane.Creators.Remove(username);
			targetPlane.Members.Remove(username);

			// The grant command handler should be able to handle it from here
			await Grant(request);
		}
	}
}
