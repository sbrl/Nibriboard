using System;
using System.Linq;
using System.Collections.Generic;

namespace Nibriboard.Userspace
{
	public class RbacRole
	{
		public readonly string Name;

		public readonly List<RbacRole> SubRoles = new List<RbacRole>();
		public readonly List<RbacPermission> Permissions = new List<RbacPermission>();

		public RbacRole()
		{
		}
		public RbacRole(string inRoleName, List<RbacPermission> inPermissions) : this(inRoleName, inPermissions, new List<RbacRole>())
		{
		}
		public RbacRole(string inRoleName, List<RbacPermission> inPermissions, List<RbacRole> inSubRoles)
		{
			Name = inRoleName;
			Permissions = inPermissions;
			SubRoles = inSubRoles;
		}

		public bool HasPermission(RbacPermission permission)
		{
			return Permissions.Contains(permission) || SubRoles.Any((RbacRole obj) => obj.HasPermission(permission));
		}

		public bool HasRole(RbacRole targetRole)
		{
			if (Name == targetRole.Name)
				return true;
			return SubRoles.Any((RbacRole subRole) => subRole.HasRole(targetRole));
		}

		public override string ToString()
		{
			List<string> subItems = new List<string>();
			subItems.AddRange(SubRoles.Select((RbacRole subRole) => $"[r] {subRole.Name}"));
			subItems.AddRange(Permissions.Select((RbacPermission subPermission) => $"[p] {subPermission.Name}"));

			return string.Format(
				"{0}: {1}",
				Name,
				string.Join(", ", subItems)
			);
		}
	}
}
