using System;
using System.Reflection;
using System.Threading.Tasks;
using Nibriboard.RippleSpace;
using SBRL.Utilities;

namespace Nibriboard
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			string packedRippleSpaceFile = "./default.ripplespace.zip";

			for(int i = 0; i < args.Length; i++)
			{
				switch(args[i])
				{
					case "-h":
					case "--help":
						Console.WriteLine("Nibriboard Server");
						Console.WriteLine("By Starbeamrainbowlabs");
						Console.WriteLine();
						Console.WriteLine("Usage:");
						Console.WriteLine("    ./Nibriboard.exe [options]");
						Console.WriteLine();
						Console.WriteLine("Options:");
						Console.WriteLine("    -h  --help             Shows this message");
						Console.WriteLine("    -f  --file [filepath]  Specify the path to the packed ripplespace file to load. Defaults to '{0}'.", packedRippleSpaceFile);
						Console.WriteLine();
						return;

					case "-f":
					case "--file":
						packedRippleSpaceFile = args[++i];
						break;
				}
			}

			Log.WriteLine($"[core] Nibriboard Server {NibriboardServer.Version}, built on {NibriboardServer.BuildDate.ToString("R")}");
			Log.WriteLine("[core] An infinite whiteboard for those big ideas.");
			Log.WriteLine("[core] By Starbeamrainbowlabs");
			Log.WriteLine("[core] Starting");

			Log.WriteLine("[core] Detected embedded files: ");
			EmbeddedFiles.WriteResourceList();

			Log.WriteLine("[core] Loading ripple space from \"{0}\".", packedRippleSpaceFile);

			NibriboardServer server = new NibriboardServer(packedRippleSpaceFile);
			Task.WaitAll(
				server.Start(),
				server.StartCommandListener()
			);
		}
	}
}
