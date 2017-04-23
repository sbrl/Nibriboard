using System;
namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Represents a complaint (usually by the server) about something that the other party
	/// has done that probably shouldn't have.
	/// </summary>
	public class ErrorMessage
	{
		/// <summary>
		/// The message describing the error that has occurred.
		/// </summary>
		string Message;

		public ErrorMessage()
		{
		}
	}
}
