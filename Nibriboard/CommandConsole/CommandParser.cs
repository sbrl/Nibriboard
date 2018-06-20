using System;
using System.Threading.Tasks;

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
			await (Task)parentCommandModule.GetType()
			                               .GetMethod(subcommandName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.IgnoreCase)
				.Invoke(parentCommandModule, new object[] { request });
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
