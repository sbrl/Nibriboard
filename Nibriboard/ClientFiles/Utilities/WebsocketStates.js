"use strict";

/**
 * Constants for the different readyStates that a WebSocket can be in.
 * @type {Object}
 */
const WebSocketStates = {
	/**
	 * Indicates that the WebSocket is connecting to the remote server.
	 * @type {Number}
	 */
	connecting: 0,
	/**
	 * Indicates that the WebSocket is connected to the remote server and ready to send / receive data.
	 * @type {Number}
	 */
	ready: 1,
	/**
	 * Indicates that the WebSocket is in the process of closing the connection to the remote server.
	 * @type {Number}
	 */
	closing: 2,
	/**
	 * Indicates that hte WebSocket is not connected to the remote server (either because the connection was closed, or dropped by the remote server).
	 * @type {Number}
	 */
	closed: 3
};

export default WebSocketStates;
