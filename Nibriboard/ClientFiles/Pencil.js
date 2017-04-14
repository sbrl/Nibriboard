"use strict";

import Mouse from './Utilities/Mouse';
import Vector from './Utilities/Vector';

var cuid = require("cuid");

class Pencil
{
	/**
	 * Creates a new Pencil class instance.
	 * @param	{RippleLink}	inRippleLink	The connection to the nibri server.
	 * @return	{Pencil}		A new Pencil class instance.
	 */
	constructor(inRippleLink, inBoardWindow)
	{
		// The time, in milliseconds, between pushes of the line to the server.
		this.pushDelay = 200;
		
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
		
		document.addEventListener("mouseDown", this.handleMouseDown.bind(this));
		document.addEventListener("mouseMove", this.handleMouseMove.bind(this));
		document.addEventListener("mouseUp", this.handleMouseUp.bind(this));
	}
	
	handleMouseDown(event) {
		
	}
	
	handleMouseMove(event) {
		var nextPoint = new Vector(event.clientX, event.clientY);
		this.unsentSegments.push(nextPoint);
		this.currentLineSegments.push(nextPoint);
		
		if(new Date() - this.lastServerPush > this.pushDelay)
			sendUnsent();
	}
	
	handleMouseUp(event) {
		sendUnsent();
		// Reset the currently line segments
		this.currentLineSegments = [];
		// Regenerate the line id
		this.currentLineId = cuid();
	}
	
	/**
	 * Send the unsent segments of the line to the server and reset the line
	 * unsent segments buffer.
	 */
	sendUnsent() {
		// It's time for another push of the line to the server
		this.rippleLink.send({
			Event: "LinePart",
			Points: this.unsentSegments,
			LineId: this.currentLineId
		});
		// Reset the unsent segments buffer
		this.unsentSegments = [];
	}
	
	render(canvas, context) {
		
	}
}
