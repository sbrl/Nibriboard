using System;
namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Represents a tantrum thrown by either the server or the client.
	/// Both act like 2 year olds, apparently :P
	/// </summary>
	public class ExceptionMessage : Message
	{
		/// <summary>
		/// The number code associated with this exception.
		/// </summary>
		public uint Code = 0;
		/// <summary>
		/// The error message.
		/// </summary>
		public string Message = string.Empty;

		/// <summary>
		/// Creates a new ExceptionMessage.
		/// </summary>
		/// <param name="inCode">The exception code.</param>
		/// <param name="inMessage">The exception message.</param>
		public ExceptionMessage(uint inCode, string inMessage)
		{
			Code = inCode;
			Message = inMessage;
		}
		/// <summary>
		/// Creates a new ExceptionMessage.
		/// </summary>
		/// <param name="inMessage">The exception message.</param>
		public ExceptionMessage(string inMessage) : this(0, inMessage)
		{
		}
	}
}
