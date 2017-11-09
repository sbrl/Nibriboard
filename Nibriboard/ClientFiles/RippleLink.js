"use strict";

import { WebSocketStates, ReverseWebSocketStates } from './Utilities/WebSocketStates';

import EventEmitter from "event-emitter-es6";

class RippleLink extends EventEmitter
{
	constructor(inSocketUrl, inBoardWindow)
	{
		super();
		
		this.socketUrl = inSocketUrl;
		this.boardWindow = inBoardWindow;
		this.settings = this.boardWindow.settings;
		
		// Create the websocket and commect to the server
		
		// Whether the link ot the nibri server is open or not
		this.linkOpen = false;
		// The underlying websocket
		this.websocket = new WebSocket(this.socketUrl, [ this.settings.WebsocketProtocol ]);
		// Attach some event listeners
		this.websocket.addEventListener("open", this.handleConnection.bind(this));
		this.websocket.addEventListener("message", this.handleMessage.bind(this));
		let closeHandler = this.handleDisconnection.bind(this);
		this.websocket.addEventListener("close", closeHandler);
		
		// Respond to heartbeats from the server
		this.on("Heartbeat", this.handleHeartbeat.bind(this));
		this.on("Error", this.handleErrorMessage.bind(this));
		
		// Close the socket correctly
		window.addEventListener("beforeunload", (function(event) {
			// Remove the event listener to avoid issues on refresh
			this.websocket.removeEventListener("close", closeHandler);
			this.websocket.close();
		}).bind(this));
	}
	
	handleConnection(event) {
		console.info("[ripple link] Established connection successfully.");
		this.linkOpen = true;
		// Tell everyone about it
		this.emit("connect", event);
	}
	
	handleDisconnection(event) {
		// If the link was already down, then ignore this phantom disconnection
		if(!this.linkOpen)
			return;
		console.error("[ripple link] Lost connection.");
		this.boardWindow.interface.setConnectedStatus(false);
		
		this.emit("disconnect", event);
	}
	
	handleMessage(event) {
		// Decode the message form the server
		var message = JSON.parse(event.data);
		console.debug(message.Event, message);
		
		// Pass it on to the board manager by triggering the appropriate event
		this.emit(message.Event, message);
	}
	
	handleErrorMessage(message) {
		console.error(message.Message);
	}
	
	/**
	 * Replies to heartbeat messages from the server.
	 */
	handleHeartbeat(message) {
		// Reply with a heartbeat
		this.send({
			"Event": "Heartbeat"
		});
	}
	
	/**
	 * Sends a message object to the server.
	 */
	send(message) {
		if(this.websocket.readyState !== WebSocketStates.ready)
		{
			console.error(`Attempt to send data on the RippleLink when it is not ready (state ${ReverseWebSocketStates[this.websocket.readyState]})`);
			return false;
		}
		
		this.websocket.send(JSON.stringify(message));
		return true;
	}
}

export default RippleLink;
