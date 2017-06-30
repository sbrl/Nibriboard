using System;
using System.Collections.Generic;

using SBRL.Utilities;

namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// The reflection of a <see cref="LinePartMessage" /> that's sent to everyone else on the same plane.
	/// Contains a list of point the client has added to the line they're drawing.
	/// </summary>
	public class LinePartReflectionMessage : Message
	{
		/// <summary>
		/// The id of the client drawing the line.
		/// </summary>
		public int OtherClientId;
		/// <summary>
		/// The id of the line to add the points to.
		/// </summary>
		public string LineId;
		/// <summary>
		/// The points to add to the line
		/// </summary>
		public List<Vector2> Points;

		public LinePartReflectionMessage()
		{
		}
	}
}
