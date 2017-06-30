using System;
namespace Nibriboard.Client.Messages
{
	public class LineCompleteReflectionMessage : Message
	{
		/// <summary>
		/// The id of the client that has completed drawing a line.
		/// </summary>
		public int OtherClientId;
		/// <summary>
		/// The id of the line to complete
		/// </summary>
		public string LineId;

		public LineCompleteReflectionMessage()
		{
		}
	}
}
