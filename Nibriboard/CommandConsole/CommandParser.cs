using System;
using System.Reflection;
using System.Threading.Tasks;

using Nibriboard.Utilities;

namespace Nibriboard.CommandConsole
{
	public enum OutputMode
	{
		Text,
		CSV
	}

	public static class CommandParser
	{
		public static async Task ExecuteSubcommand(ICommandModule parentCommandModule, string subcommandName, CommandRequest request)
		{
			MethodInfo method = parentCommandModule.GetType()
				.GetMethod(
				    subcommandName,
				    // We want public methods that aren't static - and we don't know what casing the method has
				    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase
				);

			if (method == null) {
				await request.WriteLine($"Error: No subcommand with the name '{subcommandName}' exists (type 'help' instead for a list).");
				return;
			}

			await (Task)method.Invoke(parentCommandModule, new object[] { request });
		}

		public static OutputMode ParseOutputMode(string outputModeText, OutputMode defaultMode = OutputMode.Text)
		{
			outputModeText = outputModeText.Trim();

			if (outputModeText == string.Empty)
				return defaultMode;
			
			return (OutputMode)Enum.Parse(typeof(OutputMode), outputModeText, true);
		}

	}
}
