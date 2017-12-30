using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nibriboard.Userspace
{
	public class UserManager
	{
		/// <summary>
		/// The list of users that this <see cref="UserManager" /> is managing.
		/// </summary>
		public List<User> Users { get; private set; } = new List<User>();
		/// <summary>
		/// A list of the permissions that this <see cref="UserManager" /> is aware of.
		/// </summary>
		public List<RbacPermission> Permissions { get; private set; } = new List<RbacPermission>();
		/// <summary>
		/// A list of the roles that this <see cref="UserManager" /> is aware of.
		/// </summary>
		public List<RbacRole> Roles { get; private set; } = new List<RbacRole>();

		public UserManager()
		{
			Permissions.AddRange(new RbacPermission[] {
				new RbacPermission("view-public-plane", "View public planes"),
				new RbacPermission("view-own-plane", "View your own planes."),
				new RbacPermission("view-any-plane", "View anyone's planes."),
				new RbacPermission("create-plane", "Create a new plane."),
				new RbacPermission("delete-own-plane", "Delete a plane."),
				new RbacPermission("delete-any-plane", "Delete a plane."),
				new RbacPermission("manage-own-plane-members", "Manage the users allowed to access one of your planes."),
				new RbacPermission("manage-any-plane-members", "Manage the users allowed to access one any plane.")
			});
			Roles.Add(new RbacRole("Guest", new List<RbacPermission>() {
				ResolvePermission("view-public-plane")
			}));
			Roles.Add(new RbacRole("Member", new List<RbacPermission>() {
				ResolvePermission("view-own-plane"),
				ResolvePermission("create-plane"),
				ResolvePermission("delete-own-plane"),
				ResolvePermission("manage-own-plane-members")
			}, new List<RbacRole>() {
				ResolveRole("Guest")
			}));
			Roles.Add(new RbacRole("Root", new List<RbacPermission>() {
				ResolvePermission("view-any-plane"),
				ResolvePermission("delete-any-plane"),
				ResolvePermission("manage-any-plane-members")
			}, new List<RbacRole>() {
				ResolveRole("Member")
			}));
		}

		/// <summary>
		/// Loads the user data stored in the specified file.
		/// </summary>
		/// <param name="filename">The filename to load the user data from.</param>
		public async Task LoadUserDataFile(string filename)
		{
			StreamReader sourceStream = new StreamReader(filename);
			await LoadUserData(sourceStream);
		}
		/// <summary>
		/// Loads the user data from the specified stream.
		/// </summary>
		/// <param name="userDataStream">The stream to load the user data from.</param>
		public async Task LoadUserData(StreamReader userDataStream)
		{
			LoadUserData(await userDataStream.ReadToEndAsync());
		}
		/// <summary>
		/// Loads the user data from the specified JSON string.
		/// </summary>
		/// <param name="userData">The JSON-serialised user data to load.</param>
		public void LoadUserData(string userData)
		{
			Users = JsonConvert.DeserializeObject<List<User>>(userData, new UserCreationConverter(this));
		}

		/// <summary>
		/// Saves the user data to the specified file.
		/// </summary>
		/// <param name="filename">The filename to save the user data to.</param>
		public async Task SaveUserDataFile(string filename)
		{
			StreamWriter destination = new StreamWriter(filename);
			await SaveUserData(destination);
			destination.Close();
		}
		/// <summary>
		/// Saves the user data to specified destination.
		/// </summary>
		/// <param name="destination">The destination to save to.</param>
		public async Task SaveUserData(StreamWriter destination)
		{
			string json = JsonConvert.SerializeObject(Users);
			await destination.WriteLineAsync(json);
		}

		/// <summary>
		/// Resolves a permission name to it's associated <see cref="RbacPermission" /> object.
		/// </summary>
		/// <param name="permissionName">The permission name to resolve.</param>
		/// <returns>The resolved permission object.</returns>
		public RbacPermission ResolvePermission(string permissionName)
		{
			return Permissions.Find((RbacPermission permission) => permission.Name == permissionName);
		}
		/// <summary>
		/// Resolves a role name to it's associated <see cref="RbacRole" /> object.
		/// </summary>
		/// <param name="roleName">The role name to resolve.</param>
		/// <returns>The resolved role object.</returns>
		public RbacRole ResolveRole(string roleName)
		{
			return Roles.Find((RbacRole role) => role.Name == roleName);
		}
		/// <summary>
		/// Resolves a list of role names to their associated <see cref="RbacRole" /> objects.
		/// </summary>
		/// <param name="roleNames">The role names to resolve.</param>
		/// <returns>The resolved role objects.</returns>
		public IEnumerable<RbacRole> ResolveRoles(IEnumerable<string> roleNames)
		{
			foreach (RbacRole role in Roles)
			{
				if(roleNames.Contains(role.Name))
					yield return role;
			}
		}

		/// <summary>
		/// Works out whether a user exists with the specified username.
		/// </summary>
		/// <param name="username">The target username to search for.</param>
		/// <returns>Whether a user exists with the specified username or not.</returns>
		public bool UserExists(string username)
		{
			return Users.Any((User user) => user.Username == username);
		}
		/// <summary>
		/// Finds the user with the specified name.
		/// </summary>
		/// <param name="username">The username to search for.</param>
		/// <returns>The user with the specified name.</returns>
		public User GetByName(string username)
		{
			return Users.Find((User user) => user.Username == username);
		}

		/// <summary>
		/// Creates a new user with the specified username and password, and adds them to the system.
		/// </summary>
		/// <param name="username">The username for the new user.</param>
		/// <param name="password">The new user's password.</param>
		public void AddUser(string username, string password)
		{
			if (UserExists(username))
				throw new Exception($"Error: A user with the name {username} already exists, so it can't be created.");
			
			User newUser = new User(this) {
				Username = username,
				CreationTime = DateTime.Now
			};
			newUser.SetPassword(password);

			Users.Add(newUser);
		}
	}
}
