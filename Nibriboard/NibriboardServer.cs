using System;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Nibriboard.RippleSpace;
using Nibriboard.Client;

namespace Nibriboard
{
	/// <summary>
	/// The main Nibriboard server.
	/// This class manages not only the connected clients, but also holds the master reference to the <see cref="Nibriboard.RippleSpace.RippleSpaceManager"/>.
	/// </summary>
	public class NibriboardServer
	{
		private ClientSettings clientSettings;

		private CommandConsole commandServer;

		public readonly int CommandPort = 31587;
		public readonly int Port = 31586;

		public RippleSpaceManager PlaneManager;
		public NibriboardApp AppServer;

		public NibriboardServer(string pathToRippleSpace, int inPort = 31586)
		{
			Port = inPort;

			// Load the specified packed ripple space file if it exists - otherwise save it to disk
			if(File.Exists(pathToRippleSpace))
			{
				PlaneManager = RippleSpaceManager.FromFile(pathToRippleSpace).Result;
			}
			else
			{
				Log.WriteLine("[NibriboardServer] Couldn't find packed ripple space at {0} - creating new ripple space instead.", pathToRippleSpace);
				PlaneManager = new RippleSpaceManager() { SourceFilename = pathToRippleSpace };
			}

			clientSettings = new ClientSettings() {
				SecureWebSocket = false,
				WebSocketHost = "192.168.0.56",
				WebSocketPort = Port,
				WebSocketPath = "/RipplespaceLink"
			};

			// HTTP / Websockets Server setup
			SBRL.GlidingSquirrel.Log.LoggingLevel = SBRL.GlidingSquirrel.LogLevel.Debug;
			AppServer = new NibriboardApp(new NibriboardAppStartInfo() {
				FilePrefix = "Nibriboard.obj.client_dist",
				ClientSettings = clientSettings,
				SpaceManager = PlaneManager
			}, IPAddress.Any, Port);

			// Command Console Server setup
			commandServer = new CommandConsole(this, CommandPort);
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
	}
}
