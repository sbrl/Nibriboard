using System;
namespace Nibriboard.CommandConsole
{
	public class ModuleDescription
	{
		public string Name { get; }
		public string ArgumentsSpec { get; }
		public string Description { get; }

		public ModuleDescription(string inName, string inArgumentsSpec, string inDescription)
		{
			Name = inName;
			ArgumentsSpec = inArgumentsSpec;
			Description = inDescription;
		}
		public ModuleDescription(string inName, string inDescription) : this(inName, "", inDescription) {  }


		public override string ToString()
		{
			return $"{Name} {ArgumentsSpec}\n" +
				$"\t{Description}";
		}
		public string ToLongString()
		{
			return $"{Description}\nUsage:\n{Name} {ArgumentsSpec}\n";
		}
	}
}
