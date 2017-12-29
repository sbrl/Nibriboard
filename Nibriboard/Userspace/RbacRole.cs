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
	}
}
