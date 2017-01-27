using System;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

using IotWeb.Common.Http;
using SBRL.Utilities;

namespace Nibriboard.Client
{
	public class NibriClient
	{
		private readonly NibriClientManager manager;
		private readonly WebSocket client;

		private Dictionary<string, Type> messageEventTypes = new Dictionary<string, Type>()
		{

		};

		public NibriClient(NibriClientManager inManager, WebSocket inClient)
		{
			manager = inManager;
			client = inClient;

			client.DataReceived += async (WebSocket clientSocket, string frame) => {
				try
				{
					await handleMessage(frame);
				}
				catch (Exception error)
				{
					await Console.Error.WriteLineAsync(error.ToString());
					throw;
				}

				//Task.Run(async () => await onMessage(frame)).Wait();
			};

			
		}

		public void Send(string message)
		{
			client.Send(Encoding.UTF8.GetBytes(message));
		}

		private async Task handleMessage(string frame)
		{
			string eventName = JsonUtilities.DeserializeProperty(frame, "event");


		}
	}
}
