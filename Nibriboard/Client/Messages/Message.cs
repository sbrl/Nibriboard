using System;

namespace Nibriboard.Client.Messages
{
	public class Message
	{
		/// <summary>
		/// The name of the event that this message is about.
		/// </summary>
		public readonly string Event = string.Empty;
		/// <summary>
		/// The date and time that this message was sent.
		/// </summary>
		public readonly DateTime TimeSent = DateTime.Now;

		public Message()
		{
			Event = this.GetType().Name.Replace("Message", "");
		}
	}
}
