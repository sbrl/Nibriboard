using System;
using System.Collections.Generic;
using Nibriboard.RippleSpace;
using SBRL.Utilities;

namespace Nibriboard.Client
{
	/// <summary>
	/// Manages the construction of lines that the clients are drawing bit by bit.
	/// </summary>
	public class LineIncubator
	{
		/// <summary>
		/// The current lines that haven't been completed yet.
		/// </summary>
		private Dictionary<Guid, DrawnLine> currentLines = new Dictionary<Guid, DrawnLine>();

		/// <summary>
		/// The number of lines that this line incubator has completed.
		/// </summary>
		public int LinesCompleted { get; private set; } = 0;

		/// <summary>
		/// The number of lines that are still incubating and haven't been completed yet.
		/// </summary>
		public int IncompletedLines {
			get {
				return currentLines.Count;
			}
		}

		public LineIncubator()
		{
		}

		public void AddBit(Guid lineId, List<LocationReference> points)
		{
			// Create a new line if one doesn't exist already
			if(!currentLines.ContainsKey(lineId))
				currentLines.Add(lineId, new DrawnLine());
			
			// Add these points to the line
			currentLines[lineId].Points.AddRange(points);
		}

		/// <summary>
		/// Mark a line as complete and remove it from the incubator.
		/// </summary>
		/// <param name="lineId">The id of the line to complete.</param>
		/// <returns>The completed line.</returns>
		public DrawnLine CompleteLine(Guid lineId)
		{
			if(!currentLines.ContainsKey(lineId))
				throw new KeyNotFoundException("Error: A line with that id wasn't found in this LineIncubator.");

			DrawnLine completedLine = currentLines[lineId];
			currentLines.Remove(lineId);

			return completedLine;
		}
	}
}
