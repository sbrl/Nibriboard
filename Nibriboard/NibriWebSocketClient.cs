using System;
using IotWeb.Common.Http;
using System.Threading.Tasks;
namespace Nibriboard
{
	public class NibriWebSocketClient : IWebSocketRequestHandler
	{
		WebSocket client;

		public NibriWebSocketClient()
		{
		}

		public void Connected(WebSocket socket)
		{
			client = socket;
			client.DataReceived += async (WebSocket clientSocket, string frame) => {
				try {
					await onMessage(frame);
				}
				catch(Exception error) {
					await Console.Error.WriteLineAsync(error);
					throw;
				}

				//Task.Run(async () => await onMessage(frame)).Wait();
			};
		}

		/// <summary>
		/// Whether we will accept the WebSocket connection or not.
		/// </summary>
		/// <param name="uri">The uri the user connected to.</param>
		/// <param name="protocol">The protocol the user is connecting with.</param>
		/// <returns>Whether we want to accept the WebSocket connection attempt or not.</returns>
		public bool WillAcceptRequest(string uri, string protocol)
		{
			Log.WriteLine("[Nibriboard/Websocket] Accepting {0} via {1}.", uri, protocol);
			return true;
		}

		private async Task onMessage(string frame)
		{

		}
	}
}
