"use strict";

// npm modules
window.EventEmitter = require("event-emitter-es6");
window.FaviconNotification = require("favicon-notification");
window.panzoom = require("pan-zoom");

// Our files
import RippleLink from './RippleLink';
import ViewportSyncer from './ViewportSyncer';
import OtherClient from './OtherClient';
import { get } from './Utilities';

class BoardWindow extends EventEmitter
{
	constructor(canvas)
	{
		super(); // Run the parent constructor
		
		// The maximum target fps.
		this.maxFps = 60;
		// The target fps at which we should send sursor updates.
		this.cursorUpdateFrequency = 5;
		
		// The radius of other clients' cursors.
		this.otherCursorRadius = 10;
		// The width of the lines in other clients' cursors.
		this.otherCursorWidth = 2;
		
		// --~~~--
		
		// Setup the fps indicator in the top right corner
		this.renderTimeIndicator = document.createElement("span");
		this.renderTimeIndicator.innerHTML = "0ms";
		document.querySelector(".fps").appendChild(this.renderTimeIndicator);
		
		// --~~~--
		
		// Our unique id
		this.Id = -1;
		// Our colour
		this.Colour = "rgba(255, 255, 255, 0.3)";
		
		// --~~~--
		
		// Setup the canvas
		this.canvas = canvas;
		this.context = canvas.getContext("2d");
		
		// Grab a reference to the sidebar
		this.sidebar = document.getElementById("sidebar");
		
		// Create a map to store information about other clients in
		this.otherClients = new Map();
		
		// --~~~--
		
		// Setup the favicon thingy
		
		FaviconNotification.init({
			color: '#ff6333'
		});
		FaviconNotification.add();
		
		// Setup the input controls
		window.panzoom(canvas, this.handleCanvasMovement.bind(this));
		
		// --~~~--
		
		// Fetch the RippleLink connection information and other settings from
		// the server
		get("/Settings.json").then(JSON.parse).then((function(settings) {
			console.info("[setup]", "Obtained settings from server:", settings);
			this.settings = settings;
			this.setup();
		}).bind(this), function(errorMessage) {
			console.error(`Error: Failed to fetch settings from server! Response: ${errorMessage}`);
		});
		
		// --~~~--
		
		// Setup a bunch of event listeners
		// The ones that depend on the RippleLink will get setup later
		
		// Make the canvas track the window size
		this.trackWindowSize();
	}
	
	/**
	 * Setup ready for user input.
	 * This mainly consists of establishing the RippleLink connection to the server.
	 */
	setup() {
		this.rippleLink = new RippleLink(this.settings.WebsocketUri, this);
		this.rippleLink.on("connect", (function(event) {
			// Send the handshake request
			this.rippleLink.send({
				Event: "HandshakeRequest",
				InitialViewport: { // TODO: Add support for persisting this between sessions
					X: 0,
					Y: 0,
					Width: window.innerWidth,
					Height: window.innerHeight
				},
				InitialAbsCursorPosition: this.cursorPosition
			});
		}).bind(this));
		
		// Keep the server up to date on our viewport and cursor position
		this.viewportSyncer = new ViewportSyncer(this.rippleLink, this.cursorUpdateFrequency)
		
		// RippleLink message bindings
		
		// Handle the HandshakeResponse when it comes in
		this.rippleLink.on("HandshakeResponse", this.handleHandshakeResponse.bind(this));
		
		// Handle other clients' state updates
		this.rippleLink.on("ClientStates", this.handlePeerUpdates.bind(this));
	}
	
	/**
	 * Renders the next frame.
	 */
	nextFrame()
	{
		// The time at which the current frame started rendering.
		let frameStart = +new Date();
		
		if(frameStart - this.lastFrameStart >= (1 / this.maxFps) * 1000)
		{
			this.update();
			this.render(this.canvas, this.context);
		}
		
		// Update the time the last frame started rendering
		this.lastFrameStart = frameStart;
		// Update the time we took rendering the last frame
		this.lastFrameTime = +new Date() - frameStart;
		
		this.renderTimeIndicator.innerHTML = `${this.lastFrameTime}ms`;
		
		// Limit the maximum fps
		requestAnimationFrame(this.nextFrame.bind(this));
	}
	
	/**
	 * Updates everything ready for the next frame to be rendered.
	 */
	update()
	{
		
	}
	
	/**
	 * Renders the next frame.
	 */
	render(canvas, context)
	{
		context.clearRect(0, 0, canvas.width, canvas.height);
		
		this.renderOthers(canvas, context);
	}
	
	renderOthers(canvas, context)
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
	 * Updates the canvas size to match the current viewport size.
	 */
	matchWindowSize() {
		this.canvas.width = window.innerWidth;
		this.canvas.height = window.innerHeight;
		
		this.render(this.canvas, this.context);
	}
	
	/**
	 * Makes the canvas size track the window size.
	 */
	trackWindowSize() {
		this.matchWindowSize();
		window.addEventListener("resize", this.matchWindowSize.bind(this));
	}
	
	/**
	 * Handles events generated by pan-zoom, the package that handles the
	 * dragging and zooming of the whiteboard.
	 */
	handleCanvasMovement(event) {
		this.viewportState = event; // Store the viewport information for later
	}
	
	handleHandshakeResponse(message) {
		console.log("Received handshake response");
		
		// Store the information send by the server
		this.Id = message.Id;
		this.Colour = message.Colour;
		
		this.sidebar.style.borderTopColor = this.Colour;
	}
	
	/**
	 * Handles peer update messages recieved from the server via the RippleLink.
	 */
	handlePeerUpdates(message) {
		// Update our knowledge about other clients
		for (let otherClient of message.ClientStates) {
			// If this client is new, emit an event about it
			if(!this.otherClients.has(otherClient.Id)) {
				this.emit("OtherClientConnect", otherClient);
				
				// Convert the raw object into a class instance
				let otherClientObj = new OtherClient();
				otherClientObj.Id = otherClient.Id;
			}
			else { // If not, emit a normal update message about it
				this.emit("OtherClientUpdate", otherClient);
			} 
			
			// Get the OtherClient instance to pull in the rest of the data
			this.otherClients.get(otherClient.Id).update(otherClient);
		}
	}
}

export default BoardWindow;
