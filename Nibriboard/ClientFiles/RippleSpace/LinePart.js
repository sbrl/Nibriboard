"use strict";

const cuid = require("cuid");

class LinePart
{
	constructor()
	{
		this.LineId = cuid();
		this.Points = [];
	}
	
	
}

export default LinePart;
