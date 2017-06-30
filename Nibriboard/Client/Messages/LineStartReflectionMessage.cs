using System;
namespace Nibriboard.Client.Messages
{
	public class LineStartReflectionMessage : Message
	{
		/// <summary>
		/// The id of the client that has completed drawing a line.
		/// </summary>
		public int OtherClientId;
		/// <summary>
		/// The id of the line to complete
		/// </summary>
		public string LineId;
		/// <summary>
		/// The colour of the line. May be any valid colour accepted by the HTML5 Canvas API.
		/// </summary>
		public string LineColour;
		/// <summary>
		/// The width of the line, in pixels.
		/// </summary>
		public int LineWidth;

		public LineStartReflectionMessage()
		{
		}
	}
}
