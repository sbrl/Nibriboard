using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Nibriboard.RippleSpace;
using Nibriboard.Client;
using System.Reflection;
using SBRL.Utilities;
using Nibriboard.Userspace;
using Nibriboard.Utilities;
using Nibriboard.CommandConsole;

namespace Nibriboard
{
	/// <summary>
	/// The main Nibriboard server.
	/// This class manages not only the connected clients, but also holds the master reference to the <see cref="Nibriboard.RippleSpace.NibriboardServer"/>.
	/// </summary>
	public class NibriboardServer
	{
		/// <summary>
		/// Gets the version of this Nibriboard Server instance.
		/// </summary>
		/// <value>The version.</value>
		public static string Version {
			get {
				Version asmVersion = Assembly.GetExecutingAssembly().GetName().Version;
				string commit = EmbeddedFiles.ReadAllText("Nibriboard.commit-hash.txt");
				return $"v{asmVersion.Major}.{asmVersion.Minor}.{asmVersion.Build}-{commit.Substring(0, 7)}";
			}
		}
		/// <summary>
		/// Gets the date on which this Nibriboard server instance was built.
		/// </summary>
		/// <value>The build date.</value>
		public static DateTime BuildDate {
			get {
				DateTime result = DateTime.MinValue;
				DateTime.TryParse(EmbeddedFiles.ReadAllText("Nibriboard.build-date.txt"), out result);
				return result;
			}
		}

		private ClientSettings clientSettings;

		private CommandConsoleServer commandServer;

		public readonly int CommandPort = 31587;
		public readonly int Port = 31586;

		public readonly UserManager AccountManager;
		public readonly RippleSpaceManager PlaneManager;
		public readonly NibriboardApp AppServer;

		public NibriboardServer(string pathToRippleSpace, int inPort = 31586)
		{
			Port = inPort;

			// Load the specified ripple space if it exists - otherwise save it to disk
			if(Directory.Exists(pathToRippleSpace)) {
				PlaneManager = RippleSpaceManager.FromDirectory(pathToRippleSpace).Result;
			}
			else {
				Log.WriteLine("[NibriboardServer] Couldn't find packed ripple space at {0} - creating new ripple space instead.", pathToRippleSpace);
				PlaneManager = new RippleSpace.RippleSpaceManager(pathToRippleSpace);
			}

			// Next, load the user account data
			string accountDataPath = CalcPaths.RippleSpaceAccountData(pathToRippleSpace);
			AccountManager = new UserManager();
			if(File.Exists(accountDataPath))
				AccountManager.LoadUserDataFile(CalcPaths.RippleSpaceAccountData(pathToRippleSpace)).Wait();
			

			clientSettings = new ClientSettings() {
				SecureWebSocket = false,
				WebSocketHost = "192.168.0.56",
				WebSocketPort = Port,
				WebSocketPath = "/RipplespaceLink"
			};

			// HTTP / Websockets Server setup
            SBRL.GlidingSquirrel.Log.LoggingLevel = SBRL.GlidingSquirrel.LogLevel.Info;
			AppServer = new NibriboardApp(new NibriboardAppStartInfo() {
				FilePrefix = "Nibriboard.obj.client_dist",
				ClientSettings = clientSettings,
				NibriServer = this
			}, IPAddress.Any, Port);

			// Command Console Server setup
			commandServer = new CommandConsoleServer(this, CommandPort);
		}

		public async Task Start()
		{
			await AppServer.Start();
			Log.WriteLine("[NibriboardServer] Started on port {0}", Port);

			await PlaneManager.StartMaintenanceMonkey();
		}

		/// <summary>
		/// Starts the command listener.
		/// The command listener is a light tcp-based command console that allows control
		/// of the Nibriboard server, since C# doesn't currently have support for signal handling.
		/// It listeners on [::1] _only_, to avoid security issues.
		/// In the future, a simple secret might be required to use it to aid security further.
		/// </summary>
		public async Task StartCommandListener()
		{
			await commandServer.Start();
		}

		public async Task SaveUserData()
		{
			await AccountManager.SaveUserDataFile(
				CalcPaths.RippleSpaceAccountData(PlaneManager.SourceDirectory)
			);
		}
	}
}
