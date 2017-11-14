using System;
using System.Collections.Generic;
using Nibriboard.RippleSpace;
using Nibriboard.Utilities;
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
		private Dictionary<string, DrawnLine> currentLines = new Dictionary<string, DrawnLine>();

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

		/// <summary>
		/// Figures out whether an incomplete line with the given id exists or not.
		/// </summary>
		/// <param name="lineId">The line id to check for.</param>
		public bool LineExists(string lineId)
		{
			if(currentLines.ContainsKey(lineId))
				return true;
			return false;
		}

		/// <summary>
		/// Creates a new line in the incubator.
		/// </summary>
		/// <param name="lineId">The line id to attach to the new line in the incubator.</param>
		public void CreateLine(string lineId, string newColour, int newWidth)
		{
			if(currentLines.ContainsKey(lineId))
				throw new InvalidOperationException($"Error: A line with the id {lineId} already exists, so you can't recreate it.");
			
			currentLines.Add(lineId, new DrawnLine(lineId) {
				Colour = newColour,
				Width = newWidth
			});
		}

		/// <summary>
		/// Adds a series of points to the incomplete line with the specified id.
		/// </summary>
		/// <param name="lineId">The line id to add the points to.</param>
		/// <param name="points">The points to add to the lines.</param>
		public void AddBit(string lineId, List<LocationReference> points)
		{
			// Create a new line if one doesn't exist already
			if(!currentLines.ContainsKey(lineId))
				throw new InvalidOperationException($"Error: A line with the id {lineId} doesn't exist, so you can't add to it.");
			
			// Add these points to the line
			currentLines[lineId].Points.AddRange(points);
		}

		/// <summary>
		/// Mark a line as complete and remove it from the incubator.
		/// </summary>
		/// <param name="lineId">The id of the line to complete.</param>
		/// <returns>The completed line.</returns>
		public DrawnLine CompleteLine(string lineId)
		{
			if(!currentLines.ContainsKey(lineId))
				throw new KeyNotFoundException("Error: A line with that id wasn't found in this LineIncubator.");

            DrawnLine completedLine = currentLines[lineId];
            currentLines.Remove(lineId);

            int originalPointCount = completedLine.Points.Count;
            completedLine.Points = LineSimplifier.SimplifyLine(completedLine.Points, 6);

            Log.WriteLine(
                "[LineIncubator] [LineComplete] #{0}: {1} -> {2} points ({3:0.00}% reduction)",
                lineId,
                originalPointCount,
                completedLine.Points.Count,
                ((float)completedLine.Points.Count / (float)originalPointCount) * 100f
            );

			return completedLine;
		}
	}
}
