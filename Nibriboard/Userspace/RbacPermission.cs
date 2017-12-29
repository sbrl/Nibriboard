using System;

namespace Nibriboard.Userspace
{
	public class RbacPermission
	{
		public readonly string Name;
		public readonly string Description;

		public RbacPermission(string inName, string inDescription)
		{
			Name = inName;
			Description = inDescription;
		}

		public override bool Equals(object obj)
		{
			RbacPermission otherPermission = obj as RbacPermission;
			if (obj == null)
				return false;
			return Name == otherPermission.Name;
		}
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}
		public override string ToString()
		{
			return $"[RbacPermission -> {Name}: {Description}]";
		}
	}

}
