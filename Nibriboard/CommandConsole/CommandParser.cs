using System;
namespace Nibriboard.CommandConsole
{
	public enum OutputMode
	{
		Text,
		CSV
	}

	public static class CommandParser
	{

		public static OutputMode ParseOutputMode(string outputModeText, OutputMode defaultMode = OutputMode.Text)
		{
			outputModeText = outputModeText.Trim();

			if (outputModeText == string.Empty)
				return defaultMode;
			
			return (OutputMode)Enum.Parse(typeof(OutputMode), outputModeText, true);
		}

	}
}
