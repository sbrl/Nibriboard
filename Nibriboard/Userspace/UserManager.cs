using System;
using System.Collections.Generic;

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
				GetPermission("view-public-plane")
			}));
			Roles.Add(new RbacRole("Member", new List<RbacPermission>() {
				GetPermission("view-own-plane"),
				GetPermission("create-plane"),
				GetPermission("delete-own-plane"),
				GetPermission("manage-own-plane-members")
			}, new List<RbacRole>() {
				GetRole("Guest")
			}));
			Roles.Add(new RbacRole("Root", new List<RbacPermission>() {
				GetPermission("view-any-plane"),
				GetPermission("delete-any-plane"),
				GetPermission("manage-any-plane-members")
			}, new List<RbacRole>() {
				GetRole("Member")
			}));
		}

		public RbacPermission GetPermission(string permissionName)
		{
			return Permissions.Find((RbacPermission permission) => permission.Name == permissionName);
		}
		public RbacRole GetRole(string roleName)
		{
			return Roles.Find((RbacRole role) => role.Name == roleName);
		}
	}
}
