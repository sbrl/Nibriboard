"use strict";

import Vector from './Utilities/Vector';

import Mouse from './Utilities/Mouse';

var cuid = require("cuid");

class Pencil
{
	/**
	 * Creates a new Pencil class instance.
	 * @param	{RippleLink}	inRippleLink	The connection to the nibri server.
	 * @return	{Pencil}		A new Pencil class instance.
	 */
	constructor(inRippleLink, inBoardWindow, canvas)
	{
		this.boardWindow = inBoardWindow;
		
		// The time, in milliseconds, between pushes of the line to the server.
		this.pushDelay = 200;
		
		// The current line width
		this.currentLineWidth = 3;
		// The current line colour
		this.currentColour = "black";
		
		/**
		 * The ripple link connection to the server.
		 * @type {RippleLink}
		 */
		this.rippleLink = inRippleLink;
		/**
		 * The mouse information.
		 * @type {Mouse}
		 */
		this.mouse = new Mouse();
		
		// The id of the current line-in-progress.
		this.currentLineId = cuid();
		// Holds the (unsimplified) line segments before the pencil is lifted.
		this.currentLineSegments = [];
		// The segments of the (unsimplified) line that haven't yet been sent
		// to the server.
		this.unsentSegments = [];
		
		// The time of the last push of the line to the server.
		this.lastServerPush = 0;
		
		canvas.addEventListener("mousemove", this.handleMouseMove.bind(this));
		canvas.addEventListener("mouseup", this.handleMouseUp.bind(this));
	}
	
	handleMouseMove(event) {
		// todo add zoom support here
		
		// Don't draw anything if the left mouse button isn't down
		if(!this.mouse.leftDown)
			return;
		
		var nextPoint = new Vector(
			event.clientX + this.boardWindow.viewport.x,
			event.clientY + this.boardWindow.viewport.y
		);
		this.unsentSegments.push(nextPoint);
		this.currentLineSegments.push(nextPoint);
		
		var timeSinceLastPush = new Date() - this.lastServerPush;
		if(timeSinceLastPush > this.pushDelay)
			this.sendUnsent();
	}
	
	handleMouseUp(event) {
		this.sendUnsent();
		// Tell the server that the line is complete
		this.rippleLink.send({
			Event: "LineComplete",
			LineId: this.currentLineId,
			LineWidth: this.currentLineWidth,
			LineColour: this.currentColour
		});
		// Reset the current line segments
		this.currentLineSegments = [];
		// Regenerate the line id
		this.currentLineId = cuid();
	}
	
	/**
	 * Send the unsent segments of the line to the server and reset the line
	 * unsent segments buffer.
	 */
	sendUnsent() {
		// Don't bother if there aren't any segments to push
		if(this.unsentSegments.length == 0)
			return;
		
		// It's time for another push of the line to the server
		this.rippleLink.send({
			Event: "LinePart",
			Points: this.unsentSegments,
			LineId: this.currentLineId
		});
		// Reset the unsent segments buffer
		this.unsentSegments = [];
		// Update the time we last pushed to the server
		this.lastServerPush = +new Date();
	}
	
	/**
	 * Renders the line that is currently being drawn to the screen.
	 * @param  {HTMLCanvasElement} canvas  The canvas to draw to.
	 * @param  {CanvasRenderingContext2D} context The rendering context to use to draw to the canvas.
	 */
	render(canvas, context) {
		if(this.currentLineSegments.length == 0)
			return;
		
		context.save();
		
		context.beginPath();
		context.lineTo(this.currentLineSegments[0].x, this.currentLineSegments[0].y);
		for(let point of this.currentLineSegments) {
			context.lineTo(point.x, point.y);
		}
		context.lineWidth = this.currentColour;
		context.strokeStyle = this.currentColour;
		
		context.restore();
	}
}

export default Pencil;
