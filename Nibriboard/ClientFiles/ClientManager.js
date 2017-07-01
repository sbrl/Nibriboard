"use strict";

window.EventEmitter = require("event-emitter-es6");

import OtherClient from './OtherClient';

class ClientManager extends EventEmitter
{
	constructor(inRippleLink)
	{
		super();
		
		this.otherClients = new Map();
		
		// Handle other clients' state updates
		inRippleLink.on("ClientStates", this.handlePeerUpdates.bind(this));
		// Handle line start events from other clients
		inRippleLink.on("LineStartReflection", this.handleOtherLineStart.bind(this));
		// Handle line part events from other clients
		inRippleLink.on("LinePartReflection", this.handleOtherLinePart.bind(this));
		// Handle line complete events from other clients
		inRippleLink.on("LineCompleteReflection", this.handleOtherLineComplete.bind(this));
	}
	
	render(canvas, context)
	{
		context.save();
		
		for (let [otherClientId, otherClient] of this.otherClients)
		{
			// TODO: Filter rendering by working out if this client is actually inside our viewport or not here
			context.save();
			context.translate(otherClient.CursorPosition.x, otherClient.CursorPosition.y);
			
			context.beginPath();
			// Horizontal line
			context.moveTo(-this.otherCursorRadius, 0);
			context.lineTo(this.otherCursorRadius, 0);
			// Vertical line
			context.moveTo(0, -this.otherCursorRadius);
			context.lineTo(0, this.otherCursorRadius);
			
			context.strokeStyle = otherClient.Colour;
			context.lineWidth = this.otherCursorWidth
			context.stroke();
			
			context.restore();
		}
		
		context.restore();
	}
	
	/**
	 * Handles peer update messages recieved from the server via the RippleLink.
	 */
	handlePeerUpdates(message) {
		// Update our knowledge about other clients
		for (let otherClient of message.ClientStates) {
			// If this client is new, emit an event about it
			if(!this.otherClients.has(otherClient.Id)) {
				// Convert the raw object into a class instance
				let otherClientObj = OtherClient.FromRaw(otherClient);
				this.otherClients.set(otherClientObj.Id, otherClientObj);
				
				this.emit("OtherClientConnect", otherClient);
			}
			else { // If not, emit a normal update message about it
				this.emit("OtherClientUpdate", otherClient);
			} 
			
			// Get the OtherClient instance to pull in the rest of the data
			this.otherClients.get(otherClient.Id).update(otherClient);
		}
	}
	
	/**
	 * Fetches an OtherClient instance based on its id.
	 * @param  {number}			targetId	The id of the OtherClient instance to fetch.
	 * @return {OtherClient}				The OtherClient instance with the
	 *                              		specified id.
	 */
	get(targetId) {
		if(!this.otherClients.has(targetId))
			throw new Exception(`Error: The other client with the id ${targetId} couldn't be found.`);
		
		return this.otherClients.get(targetId);
	}
	
	/**
	 * Handles a line start event from another client.
	 * @param  {LineStartReflectionMessage} message The line start message event to process.
	 */
	handleOtherLineStart(message) {
		let fromClient = this.get(message.OtherClientId);
		
		fromClient.addLine({
			LineId: message.LineId,
			Colour: message.LineColour,
			Width: message.LineWidth
		});
	}
	
	/**
	 * Handles a line part event from another client.
	 * @param  {LinePartReflectionMessage} message The line part reflection message to process.
	 */
	handleOtherLinePart(message) {
		let fromClient = this.get(message.OtherClientId);
		fromClient.appendLine(message.LineId, message.Points);
	}
	
	/**
	 * Handles a line complete event from another client.
	 * @param  {LineCompleteReflectionMessage} message The line complete reflection message to process.
	 */
	handleOtherLineComplete(message) {
		let fromClient = this.get(message.OtherClientId);
		fromClient.finishLine(message.LineId);
	}
	
}

export default ClientManager;
