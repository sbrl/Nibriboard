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
	public class CommandUsers : ICommandModule
	{
		private NibriboardServer server;

		public ModuleDescription Description { get; } = new ModuleDescription(
			"users",
			"{{subcommand}}",
			"Interact with user accounts"
		);

		public CommandUsers()
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
				await request.WriteLine("Nibriboard Server Command Console: users");
				await request.WriteLine("----------------------------------------");
				await request.WriteLine(Description.ToLongString());
				await request.WriteLine();
				await request.WriteLine("Subcommands:");
				await request.WriteLine("    list");
				await request.WriteLine("        Lists all users.");
				await request.WriteLine("    add {{{{username}}}} {{{{password}}}}");
				await request.WriteLine("        Adds a new user");
				await request.WriteLine("    checkpassword {{{{username}}}} {{{{password}}}}");
				await request.WriteLine("        Checks a user's password");
				await request.WriteLine("    setpassword {{{{username}}}} {{{{password}}}}");
				await request.WriteLine("        Resets a user's password");
				return;
			}

			await CommandParser.ExecuteSubcommand(this, request.Arguments[1], request);
		}

		public async Task List(CommandRequest request)
		{
			await request.WriteLine(
				string.Join("\n", server.AccountManager.Users.Select(
					(User user) => $"{user.CreationTime}\t{user.Username}\t{string.Join(", ", user.Roles.Select((RbacRole role) => role.Name))}"
				))
			);
		}

		public async Task Add(CommandRequest request)
		{
			string newUsername = (request.Arguments[2] ?? "").Trim();
			string password = (request.Arguments[3] ?? "").Trim();

			if (newUsername.Length == 0)
			{
				await request.WriteLine("Error: No username specified!");
				return;
			}
			if (password.Length == 0)
			{
				await request.WriteLine("Error: No password specified!");
				return;
			}

			server.AccountManager.AddUser(newUsername, password);
			await server.SaveUserData();
			await request.WriteLine($"Ok: Added user with name {newUsername} successfully.");
		}

		public async Task CheckPassword(CommandRequest request)
		{
			string checkUsername = (request.Arguments[2] ?? "").Trim();
			string checkPassword = (request.Arguments[3] ?? "").Trim();

			if (checkUsername.Length == 0)
			{
				await request.WriteLine("Error: No username specified!");
				return;
			}
			if (checkPassword.Length == 0)
			{
				await request.WriteLine("Error: No password specified!");
				return;
			}

			User checkUser = server.AccountManager.GetByName(checkUsername);
			if (checkUser == null)
			{
				await request.WriteLine($"Error: User '{checkUsername}' was not found.");
				return;
			}
			if (!checkUser.CheckPassword(checkPassword))
			{
				await request.WriteLine("Error: That password was incorrect.");
				return;
			}

			await request.WriteLine("Password check ok!");
		}

		public async Task SetPassword(CommandRequest request)
		{
			string setPasswordUsername = (request.Arguments[2] ?? "").Trim();
			string setPasswordPass = (request.Arguments[3] ?? "").Trim();

			if (setPasswordUsername.Length == 0)
			{
				await request.WriteLine("Error: No username specified.");
				return;
			}
			if (setPasswordPass.Length == 0)
			{
				await request.WriteLine("Error: No password specified.");
			}

			User setPasswordUser = server.AccountManager.GetByName(setPasswordUsername);
			if (setPasswordUser == null)
			{
				await request.WriteLine($"Error: User '{setPasswordUsername}' wasn't found.");
				return;
			}

			setPasswordUser.SetPassword(setPasswordPass);
			await server.SaveUserData();
			await request.WriteLine($"Ok: Updated password for {setPasswordUser.Username} successfully.");
		}
	}
}
