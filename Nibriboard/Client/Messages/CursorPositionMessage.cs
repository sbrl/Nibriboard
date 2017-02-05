using System;

using SBRL.Utilities;

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
		public Vector2 AbsCursorPosition;

		public CursorPositionMessage()
		{
		}
	}
}

