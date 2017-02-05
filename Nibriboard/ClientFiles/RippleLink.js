"use strict";

import WebSocketStates from './Utilities/WebsocketStates';

const EventEmitter = require("event-emitter-es6");

class RippleLink extends EventEmitter
{
	constructor(inSocketUrl, inBoardWindow)
	{
		super();
		
		this.socketUrl = inSocketUrl;
		this.boardWindow = inBoardWindow;
		this.settings = this.boardWindow.settings;
		
		// Create the websocket and commect to the server
		this.websocket = new WebSocket(this.socketUrl, [ this.settings.WebsocketProtocol ]);
		this.websocket.addEventListener("open", this.handleConnection.bind(this));
		this.websocket.addEventListener("message", this.handleMessage.bind(this));
		this.websocket.addEventListener("close", this.handleDisconnection.bind(this));
		
		// Close the socket correctly
		window.addEventListener("beforeunload", (function(event) {
			this.websocket.close();
		}).bind(this));
	}
	
	handleConnection(event) {
		console.info("[ripple link] Established connection successfully.");
		// Tell everyone about it
		this.emit("connect", event);
	}
	
	handleDisconnection(event) {
		console.error("[ripple link] Lost connection.");
		this.emit("disconnect", event);
	}
	
	handleMessage(event) {
		// Decode the message form the server
		var message = JSON.parse(event.data);
		console.debug(message);
		
		// Pass it on to the board manager by triggering the appropriate event
		this.emit(message.event, message);
	}
	
	/**
	 * Sends a message object to the server.
	 */
	send(message) {
		if(this.websocket.readyState !== WebSocketStates.ready)
		{
			console.error(`Attempt to send data on the RippleLine when it is not ready (state ${this.websocket.readyState})`);
			return false;
		}
		
		this.websocket.send(JSON.stringify(message));
		return true;
	}
}

export default RippleLink;
