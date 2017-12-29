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
		public List<User> Users { get; private set; } = new List<User>();
		public List<RbacPermission> Permissions { get; private set; } = new List<RbacPermission>();
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

		public async Task LoadUserData(StreamReader userDataStream)
		{
			LoadUserData(await userDataStream.ReadToEndAsync());
		}
		public void LoadUserData(string userData)
		{
			Users = JsonConvert.DeserializeObject<List<User>>(userData, new UserCreationConverter(this));
		}

		public RbacPermission ResolvePermission(string permissionName)
		{
			return Permissions.Find((RbacPermission permission) => permission.Name == permissionName);
		}
		public RbacRole ResolveRole(string roleName)
		{
			return Roles.Find((RbacRole role) => role.Name == roleName);
		}
		public IEnumerable<RbacRole> ResolveRoles(IEnumerable<string> roleNames)
		{
			foreach (RbacRole role in Roles)
			{
				if(roleNames.Contains(role.Name))
					yield return role;
			}
		}
	}
}
