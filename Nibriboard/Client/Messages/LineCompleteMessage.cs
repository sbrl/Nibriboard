using System;
namespace Nibriboard.Client.Messages
{
	public class LineCompleteMessage : Message
	{
		/// <summary>
		/// The id of the line to complete
		/// </summary>
		public string LineId;

		public LineCompleteMessage()
		{
		}
	}
}
