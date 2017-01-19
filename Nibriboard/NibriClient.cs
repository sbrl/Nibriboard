using System;
using IotWeb.Common.Http;
using System.Threading.Tasks;
namespace Nibriboard
{
	public class NibriClient
	{
		private readonly NibriClientManager manager;
		private readonly WebSocket client;

		public NibriClient(NibriClientManager inManager, WebSocket inClient)
		{
			manager = inManager;
			client = inClient;

			client.DataReceived += async (WebSocket clientSocket, string frame) => {
				try
				{
					await onMessage(frame);
				}
				catch (Exception error)
				{
					await Console.Error.WriteLineAsync(error.ToString());
					throw;
				}

				//Task.Run(async () => await onMessage(frame)).Wait();
			};

			
		}

		private async Task onMessage(string frame)
		{

		}
	}
}
