using System;

namespace Nibriboard.Client.Messages
{
	public class Message
	{
		public readonly string Event = string.Empty;
		public readonly DateTime TimeSent = DateTime.Now;

		public Message()
		{
			Event = this.GetType().Name.Replace("Message", "");
		}
	}
}
