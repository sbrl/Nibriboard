"use strict";

import Vector from './Utilities/Vector';

import Mouse from './Utilities/Mouse';

var cuid = require("cuid");
var simplify_line = require("visvalingam-simplifier").simplify_line;

class Pencil
{
	/**
	 * Creates a new Pencil class instance.
	 * @param	{RippleLink}	inRippleLink	The connection to the nibri server.
	 * @return	{Pencil}		A new Pencil class instance.
	 */
	constructor(inRippleLink, inBoardWindow, inCanvas)
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
		
		/** The id of the current line-in-progress.*/
		this.currentLineId = cuid();
		/** Holds the (unsimplified) line segments before the pencil is lifted. */
		this.currentLineSegments = [];
		/** Holds the (simplified) line segment befoer the pencil is lifted. */
		this.currentSimplifiedLineSegments = [];
		
		// The segments of the (unsimplified) line that haven't yet been sent
		// to the server.
		this.unsentSegments = [];
		
		// The time of the last push of the line to the server.
		this.lastServerPush = 0;
		
		this.canvas = inCanvas;
		
		// Event Listeners
		document.addEventListener("mousedown", this.handleMouseDown.bind(this));
		document.addEventListener("mousemove", this.handleMouseMove.bind(this));
		document.addEventListener("mouseup", this.handleMouseUp.bind(this));
		
		this.setupInterfaceBindings(this.boardWindow.interface);
		
		// Whether the pencil is on the board at the moment.
		this.pencilDown = false;
	}
	
	setupInterfaceBindings(inInterface)
	{
		// Snag the initial colour from the interface
		this.currentColour = inInterface.currentColour;
		// Listen for future colour updates
		inInterface.on("colourchange", (function(event) {
		    this.currentColour = event.newColour;
		}).bind(this))
		
		// Look up the initial line with in the interface
		this.currentLineWidth = inInterface.brushIndicator.width;
		// Listen for future updates fromt he interface
		inInterface.on("brushwidthchange", (function(event) {
		    this.currentLineWidth = event.newWidth;
		}).bind(this))
	}
	
	handleMouseDown(event) {
		if(event.target != this.canvas)
			return;
		
		this.pencilDown = true;
		
		console.log(`Starting line with id ${this.currentLineId}.`);
		
		this.rippleLink.send({
			Event: "LineStart",
			LineId: this.currentLineId,
			LineColour: this.currentColour,
			LineWidth: this.currentLineWidth
		});
	}
	
	handleMouseMove(event) {
		// Don't handle mouse movements on anything other than the canvas itself
		if(event.target != this.canvas)
			return;
		
		// Don't do anything at all if the brush tool isn't selected
		if(this.boardWindow.interface.currentTool !== "brush")
			return;
		
		// Don't draw anything if the left mouse button isn't down
		if(!this.mouse.leftDown || !this.pencilDown)
			return;
		// Oh and don't bother drawing anything if the control key is held down
		// either - that indicates that we're in panning mode
		// todo Create a tools systme where you can select a panning tool
		// too / instead...?
		if(this.boardWindow.keyboard.DownKeys.includes(17))
			return;
		
		// The server only supports ints atm, so we have to round here :-(
		var nextPoint = new Vector(
			Math.floor((event.clientX / this.boardWindow.viewport.zoomLevel) + this.boardWindow.viewport.x),
			Math.floor((event.clientY / this.boardWindow.viewport.zoomLevel) + this.boardWindow.viewport.y)
		);
		this.unsentSegments.push(nextPoint);
		this.currentLineSegments.push(nextPoint);
		
		this.recalculateSimplifiedLine();
		
		var timeSinceLastPush = new Date() - this.lastServerPush;
		if(timeSinceLastPush > this.pushDelay)
			this.sendUnsent();
	}
	
	handleMouseUp(event) {
		// Don't do anything at all if the brush tool isn't selected
		if(this.boardWindow.interface.currentTool !== "brush")
			return;
		// Ignore it if the ctrl key is held down - see above
		if(this.boardWindow.keyboard.DownKeys.includes(17))
			return;
		
		this.sendUnsent();
		// Tell the server that the line is complete
		this.rippleLink.send({
			Event: "LineComplete",
			LineId: this.currentLineId
		});
		
		this.pencilDown = false;
		
		// Reset the current line segments
		this.currentLineSegments = [];
		this.currentSimplifiedLineSegments = [];
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
	 * Recalculates the simplified line points array.
	 */
	recalculateSimplifiedLine()
	{
		this.currentSimplifiedLineSegments = simplify_line(this.currentLineSegments);
	}
	
	/**
	 * Renders the line that is currently being drawn to the screen.
	 * @param  {HTMLCanvasElement} canvas  The canvas to draw to.
	 * @param  {CanvasRenderingContext2D} context The rendering context to use to draw to the canvas.
	 */
	render(canvas, context) {
		if(this.currentSimplifiedLineSegments.length == 0)
			return;
		
		context.save();
		
		context.beginPath();
		context.moveTo(this.currentSimplifiedLineSegments[0].x, this.currentSimplifiedLineSegments[0].y);
		for(let i = 1; i < this.currentSimplifiedLineSegments.length; i++) {
			context.lineTo(this.currentSimplifiedLineSegments[i].x, this.currentSimplifiedLineSegments[i].y);
		}
		
		context.lineWidth = this.currentLineWidth;
		context.strokeStyle = this.currentColour;
		context.lineCap = "round";
		context.lineJoin = "round";
		context.stroke();
		
		context.restore();
	}
}

export default Pencil;
