using System;
using System.Collections.Generic;

using SBRL.Utilities;

namespace Nibriboard.Client.Messages
{
	/// <summary>
	/// Sent by a client who's in the middle of drawing a line.
	/// Contains a collection of points that should be added to the line they're drawing.
	/// </summary>
	public class LinePartMessage : Message
	{
		/// <summary>
		/// The id of the line to add the points to.
		/// </summary>
		public string LineId;
		/// <summary>
		/// The points to add to the line
		/// </summary>
		public List<Vector2> Points;

		public LinePartMessage()
		{
		}
	}
}
