using System;
using System.Reflection;
using SBRLUtilities;

namespace Nibriboard
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
			EmbeddedFiles.WriteResourceList();
		}
	}
}
