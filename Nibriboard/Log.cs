using System;
namespace Nibriboard
{
	public static class Log
	{

		public static int WriteLine(string text, params object[] args)
		{
			string outputText = $"[{Env.SecondsSinceStart}] " + string.Format(text, args);
			Console.WriteLine(outputText);
			return outputText.Length;
		}
	}
}
