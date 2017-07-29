using System;
namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Represents a complaint (usually by the server) about something that the other party
	/// has done that they probably shouldn't have.
	/// </summary>
	public class ErrorMessage : Message
	{
		/// <summary>
		/// The message describing the error that has occurred.
		/// </summary>
		public string Message;

		public ErrorMessage()
		{
		}
	}
}
