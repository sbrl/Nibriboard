"use strict";

class BrushIndicator
{
	constructor(canvas)
	{
		this.canvas = canvas;
		this.context = canvas.getContext("2d");
		
		this.width = 10;
		this.colour = "red";
	}
	
	render()
	{
		this.context.clearRect(
			0, 0,
			this.canvas.width, this.canvas.height
		);
		
		this.context.beginPath();
		this.context.arc(
			this.canvas.width / 2, this.canvas.height / 2,
			this.width / 2,
			0, Math.PI * 2,
			false
		);
		this.context.fillStyle = this.colour;
		this.context.fill();
	}
}

export default BrushIndicator;
