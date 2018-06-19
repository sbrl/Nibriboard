using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Nibriboard.CommandConsole
{
	public class CommandRequest
	{
		private TcpClient client;
		private StreamWriter dest;

		public string[] Arguments { get; }

		public CommandRequest(TcpClient inClient, string[] inArguments)
		{
			client = inClient;
			Arguments = inArguments;

			dest = new StreamWriter(client.GetStream()) { AutoFlush = true };
		}

		public async Task WriteLine(string text = "", params object[] args)
		{
			await Write($"{text}\n", args);
		}

		public async Task Write(string text, params object[] args)
		{
			await dest.WriteAsync(string.Format(text, args));
		}
	}
}
