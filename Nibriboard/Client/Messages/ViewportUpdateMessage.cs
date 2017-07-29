using System;
using SBRL.Utilities;

namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Represents an update from the client on the new size of their viewport.
	/// </summary>
	public class ViewportUpdateMessage : Message
	{
		/// <summary>
		/// The new dimensions of the client's viewport.
		/// </summary>
		public Rectangle NewViewport { get; set; }

		public ViewportUpdateMessage()
		{
			
		}
	}
}
