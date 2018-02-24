using System;
namespace Nibriboard
{
	public static class Log
	{
		// These don't work as-is, so we'll probably have to hack it and write them out byte-for-byte to avoid the Console class' weirdness if we want colour
		public static readonly string Reset = "\033[0m";
		public static readonly string Hicolor = "\033[1m";
		public static readonly string Underline = "\033[4m";
		public static readonly string Inverse = "\033[7m";
		public static readonly string LocolorDim = "\033[2m";
		public static readonly string ForegroundBlack = "\033[30m";
		public static readonly string ForegroundRed = "\033[31m";
		public static readonly string ForegroundGreen = "\033[32m";
		public static readonly string ForegroundYellow = "\033[33m";
		public static readonly string ForegroundBlue = "\033[34m";
		public static readonly string ForegroundMagenta = "\033[35m";
		public static readonly string ForegroundCyan = "\033[36m";
		public static readonly string ForegroundWhite = "\033[37m";
		public static readonly string BackgroundBlack = "\033[40m";
		public static readonly string BackgroundRed = "\033[41m";
		public static readonly string BackgroundGreen = "\033[42m";
		public static readonly string BackgroundYellow = "\033[43m";
		public static readonly string BackgroundBlue = "\033[44m";
		public static readonly string BackgroundMagenta = "\033[45m";
		public static readonly string BackgroundCyan = "\033[46m";
		public static readonly string BackgroundWhite = "\033[47m";


		public static int WriteLine(string text, params object[] args)
		{
			string outputText = $"[{Env.SecondsSinceStart.ToString("N3")}] " + string.Format(text, args);
			Console.WriteLine(outputText);
			return outputText.Length;
		}
	}
}
