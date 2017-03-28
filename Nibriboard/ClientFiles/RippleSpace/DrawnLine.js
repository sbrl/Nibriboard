"use strict";

import { ChunkReference } from '../References';

var cuid = require("cuid");
var Color = require("color");

/**
 * Represents a line drawn on the screen.
 */
class DrawnLine
{
	constructor(lineId = "")
	{
		/**
		 * The id of this line.
		 * @type {string}
		 */
		this.LineId = lineId.length == 0 ? lineId : cuid();
		/**
		 * The width of this line.
		 * @type {Number}
		 */
		this.LineWidth = 3;
		/**
		 * The colour of this line.
		 * @type {Color}
		 */
		this.Colour = new Color("#333333");
		/**
		 * A list of points in this line.
		 * @type {Vector[]}
		 */
		this.Points = [];
	}
	
	SplitOnChunks(chunkSize)
	{
		var results = [];
		
		var nextLine = new DrawnLine(),
			currentChunk = null;
		
		for(let point of this.points)
		{
			if(currentChunk != null &&
				!point.ContainingChunk(chunkSize).is(currentChunk))
			{
				// We're heading into a new chunk! Split the line up here.
				// TODO: Add connecting lines to each DrawnLine instance to prevent gaps
				results.push(nextLine);
				nextLine = new DrawnLine(this.LineId);
			}

			nextLine.Points.push(point);
		}

		return results;
	}
}
