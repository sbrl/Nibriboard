using System;
using System.Reflection;
using System.Threading.Tasks;
using SBRL.Utilities;

namespace Nibriboard
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Log.WriteLine("[core] Starting Nibriboard.");

			Log.WriteLine("[core] Detected embedded files: ");
			EmbeddedFiles.WriteResourceList();

			NibriboardServer server = new NibriboardServer();
			Task.WaitAll(
				server.Start(),
				server.StartCommandListener()
			);
		}
	}
}
