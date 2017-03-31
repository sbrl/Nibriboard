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
}
