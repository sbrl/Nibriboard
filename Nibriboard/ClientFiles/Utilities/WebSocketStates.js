"use strict";

/**
 * Constants for the different readyStates that a WebSocket can be in.
 * @type {Object}
 */
export const WebSocketStates = {
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

export const ReverseWebSocketStates = {
	// The WebSocket is in the process of connecting.
	0: "connecting",
	// The WebSocket is connected and ready to exchange data with the remote server.
	1: "ready",
	// The WebSocket is in the process of closing.
	2: "closing",
	// The WebSocket is closed.
	3: "closed"
}
