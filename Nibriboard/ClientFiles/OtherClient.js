"use strict";

import Vector from './Utilities/Vector';
import Rectangle from './Utilities/Rectangle';

class OtherClient
{
	constructor()
	{
		// The unique id of this client.
		this.Id = -1;
		// The name of this client.
		this.Name = "";
		// The colour the server assigned to this client.
		this.Colour = "green";
		// The position of this clients' cursor.
		this.CursorPosition = new Vector(0, 0);
		// The position and dimensions of this client's viewport.
		this.Viewport = Rectangle.Zero.clone();
		// The time this other client's information was last updated.
		this.LastUpdated = new Date();
		
		// The lines that this client is currently drawing.
		this.currentLines = {};
	}
	
	update(data) {
		// Update our local copy of this clients' name and colour (just in case)
		this.Name = data.Name;
		this.Colour = data.Colour;
		// Update the cursor position
		this.CursorPosition.x = data.CursorPosition.X;
		this.CursorPosition.y = data.CursorPosition.Y;
		// Update the viewport
		this.Viewport.x = data.Viewport.X;
		this.Viewport.y = data.Viewport.X;
		this.Viewport.width = data.Viewport.Width;
		this.Viewport.height = data.Viewport.Height;
	}
	
	/**
	 * Renders this client and all it's lines in progress to the screen.
	 * @param  {HTMLCanvasElement} canvas  The canvas to render to.
	 * @param  {CanvasRenderingContext2D} context The context to use to draw to the canvas.
	 * @param  {ClientManager} manager The manager to draw rendering settings from.
	 */
	render(canvas, context, manager)
	{
		// Render ourselves
		context.save();
		
		context.translate(this.CursorPosition.x, this.CursorPosition.y);
		context.beginPath();
		context.moveTo(-manager.otherClientCursorSize, 0);
		context.lineTo(manager.otherClientCursorSize, 0);
		context.moveTo(0, -manager.otherClientCursorSize);
		context.lineTo(0, manager.otherClientCursorSize);
		
		context.strokeStyle = this.Colour;
		context.lineWidth = manager.otherClientCursorWidth;
		context.stroke();
		
		context.restore();
		
		// Render the lines we're currently drawing
		for (let lineId in this.currentLines)
		{
			let lineData = this.currentLines[lineId];
			let linePoints = lineData.Points;
			
			// Don't draw empty lines
			if(linePoints.length == 0)
				continue;
			
			context.save();
			
			context.beginPath();
			context.moveTo(linePoints[0].x, linePoints[0].y);
			for(let point of linePoints)
				context.lineTo(point.x, point.y);
			
			context.lineCap = "round";
			context.strokeStyle = lineData.Colour;
			context.lineWidth = lineData.Width;
			context.stroke();
			
			context.restore();
		}
	}
	
	/**
	 * Fetches a line that is currently being drawn by this client with the specified id
	 * @param  {[type]} lineId [description]
	 * @return {[type]}        [description]
	 */
	fetchLine(lineId)
	{
		if(!this.currentLines.hasOwnProperty(lineId))
			throw new Exception(`Error: A client with the id ${lineId} does not appear to be attached to the OtherClient with the id ${this.Id}`);
		
		return this.currentLines[lineId];
	}
	
	addLine(lineData) {
		if(typeof this.currentLines[lineData.LineId] != "undefined")
			throw new Error(`Error: The line with the id ${lineData.LineId} already exists.`);
		
		lineData.Points = [];
		this.currentLines[lineData.LineId] = lineData;
	}
	
	/**
	 * Adds a list of vectors to the line with the specified id.
	 * @param  {string} lineId The id of the line in progress to add to.
	 * @param  {Vector} points The list of points to add.
	 */
	appendLine(lineId, points)
	{
		if(typeof this.currentLines[lineId] == "undefined")
			throw new Error(`The line with the id ${lineId} does not exist, so it can't be appended to.`);
		
		this.currentLines[lineId].Points.push(...points);
	}
	
	/**
	 * Finishes the line in progress with the specified id.
	 * @param  {string} lineId The line id to finish.
	 */
	finishLine(lineId) {
		if(typeof this.currentLines[lineId] == "undefined")
			throw new Error(`The line with the id ${lineId} does not exist, so it can't be finalised.`);
		
		delete this.currentLines[lineId];
	}
}

/**
 * Converts raw NibriClient data sent from the server into an instance of OtherClient.
 * @param	{object}		raw		The raw data to convert. 
 * @return	{OtherClient}			The converted data.
 */
OtherClient.FromRaw = function(raw) {
	let newOtherClient = new OtherClient();
	newOtherClient.Id = raw.Id;
	newOtherClient.CursorPosition = new Vector(
		raw.CursorPosition.X,
		raw.CursorPosition.Y
	);
	newOtherClient.Viewport = new Rectangle(
		raw.Viewport.X,
		raw.Viewport.Y,
		raw.Viewport.Width,
		raw.Viewport.Height
	);
	newOtherClient.Colour = raw.Colour;
	newOtherClient.LastUpdated = new Date();
	
	return newOtherClient;
}

export default OtherClient;
