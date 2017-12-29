using System;
using System.Collections.Generic;
using System.Linq;

using SimpleHashing.Net;

namespace Nibriboard.Userspace
{
	public class User
	{
		private static ISimpleHash passwordHasher = new SimpleHash();

		public DateTime CreationTime { get; set; }
		public string Username { get; set; }
		public string HashedPassword { get; set; }

		public List<RbacRole> Roles { get; set; }

		public User()
		{
		}

		/// <summary>
		/// Updates this user's password.
		/// </summary>
		/// <param name="newPassword">The new (unhashed) password.</param>
		public void SetPassword(string newPassword)
		{
			HashedPassword = passwordHasher.Compute(newPassword);
		}
		/// <summary>
		/// Checks whether a specified (unhashed) password matches 
		/// </summary>
		/// <param name="providedPassword">The password to check.</param>
		/// <returns>Whether the specified password matches the stored password or not.</returns>
		public bool CheckPassword(string providedPassword)
		{
			return passwordHasher.Verify(providedPassword, HashedPassword);
		}

		/// <summary>
		/// Recursively works out whether this user has the specified permission.
		/// </summary>
		/// <param name="permission">The permission to search for.</param>
		/// <returns>Whether this user has the specified permission through one of their roles or not.</returns>
		public bool HasPermission(RbacPermission permission)
		{
			return Roles.Any((RbacRole role) => role.HasPermission(permission));
		}
	}
}
