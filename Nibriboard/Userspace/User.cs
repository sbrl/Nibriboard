using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SimpleHashing.Net;

namespace Nibriboard.Userspace
{
	/// <summary>
	/// Creates new <see cref="User" /> class instances for Newtonsoft.json.
	/// </summary>
	public class UserCreationConverter : CustomCreationConverter<User>
	{
		private UserManager userManager;
		public UserCreationConverter(UserManager inUserManager)
		{
			userManager = inUserManager;
		}

		public override User Create(Type objectType)
		{
			return new User(userManager);
		}
	}

	/// <summary>
	/// Represents a single Nibriboard user.
	/// </summary>
	[JsonObject(MemberSerialization.OptOut)]
	public class User
	{
		private static ISimpleHash passwordHasher = new SimpleHash();

		private UserManager userManager;

		public DateTime CreationTime { get; set; }
		public string Username { get; set; }
		public string HashedPassword { get; set; }

		[JsonIgnore]
		public List<RbacRole> Roles { get; set; } = new List<RbacRole>();

		private List<string> rolesText = null;
		public List<string> RolesText {
			get {
				return new List<string>(Roles.Select((RbacRole role) => role.Name));
			}
			set {
				rolesText = value;
			}
		}

		public User(UserManager inUserManager)
		{
			userManager = inUserManager;
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

		public bool HasRole(RbacRole targetRole)
		{
			return Roles.Any((RbacRole role) => role.HasRole(targetRole));
		}

		[OnDeserialized]
		internal void OnDeserialized(StreamingContext context)
		{
			Roles = new List<RbacRole>(userManager.ResolveRoles(rolesText));
		}
	}
}
