using System;
using System.Drawing;

namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Represents an update by the client of their cursor position.
	/// </summary>
	public class CursorPositionMessage
	{
		/// <summary>
		/// The absolute cursor position.
		/// </summary>
		public Point AbsCursorPosition;

		public CursorPositionMessage()
		{
		}
	}
}

