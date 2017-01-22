﻿using System;
using System.Threading.Tasks;
using System.Text;

using IotWeb.Common.Http;

namespace Nibriboard.Client
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

		public void Send(string message)
		{
			client.Send(Encoding.UTF8.GetBytes(message));
		}

		private async Task onMessage(string frame)
		{

		}
	}
}
