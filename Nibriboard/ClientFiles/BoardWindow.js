"use strict";

// npm modules
window.EventEmitter = require("event-emitter-es6");
window.FaviconNotification = require("favicon-notification");
window.panzoom = require("pan-zoom");

// Our files
import RippleLink from './RippleLink';
import CursorSyncer from './CursorSyncer';
import ClientManager from './ClientManager';
import Pencil from './Pencil';
import { get, clamp } from './Utilities';
import Keyboard from './Utilities/Keyboard';
import Interface from './Interface';
import ChunkCache from './ChunkCache';

class BoardWindow extends EventEmitter
{
	constructor(canvas)
	{
		super(); // Run the parent constructor
		
		// The maximum target fps.
		this.maxFps = 60;
		// The target fps at which we should send cursor updates.
		this.cursorUpdateFrequency = 5;
		
		// The radius of other clients' cursors.
		this.otherCursorRadius = 10;
		// The width of the lines in other clients' cursors.
		this.otherCursorWidth = 2;
		
		this.currentPlaneName = "(You haven't landed yet!)";
		// Whether to display the chunk grid or not
		this.displayGrid = false;
		
		this.gridLineWidth = 2;
		this.gridLineColour = "rgba(22, 123, 228, 0.53)";
		this.primaryGridLineWidth = 4;
		this.primaryGridLineColour = "rgba(31, 223, 4, 0.68)";
		
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
		
		// The current state of the viewport.
		this.lastViewportUpdate = +new Date();
		this.viewport = {
			// The x coordinate of the viewport.
			x: 0,
			// The y coordinate of the viewport.
			y: 0,
			// The width of the viewport
			get width() {
				return canvas.width * (1/this.zoomLevel)
			},
			// The height of the viewport
			get height() {
				return canvas.height * (1/this.zoomLevel)
			},	
			// The zoom level of the viewport. 1 = normal.
			zoomLevel: 1,
			
			toString() {
				return `${+this.width.toFixed(2)}x${+this.height.toFixed(2)} @ (${+this.x.toFixed(2)}, ${+this.y.toFixed(2)}) @ ${+this.zoomLevel.toFixed(2)}x`
			}
		};
		
		// The current grid size
		this.gridSize = -1;
		
		// --~~~--
		
		// Setup the canvas
		this.canvas = canvas;
		this.context = canvas.getContext("2d");
		
		/**
		 * The current state of the keyboard.
		 * @type {Keyboard}
		 */
		this.keyboard = new Keyboard();
		// Toggle the grid display when the g key is released
		this.keyboard.on("keyup-g", (function(event) {
		    this.displayGrid = this.displayGrid ? false : true;
			console.info(`[BoardWindow/KeyboardHandler] Grid display set to ${this.displayGrid ? "on" : "off"}`);
		}).bind(this));
		this.keyboard.on("keyup-left", (function(event) {
		    this.interface.seekColour("backwards");
		}).bind(this));
		this.keyboard.on("keyup-right", (function(event) {
		    this.interface.seekColour("forwards");
		}).bind(this));
		this.keyboard.on("keyup-up", (function(event) {
		    this.interface.updateBrushWidth(this.interface.currentBrushWidth + 2, true);
		}).bind(this));
		this.keyboard.on("keyup-down", (function(event) {
		    this.interface.updateBrushWidth(this.interface.currentBrushWidth - 2, true);
		}).bind(this));
		this.keyboard.on("keyup-c", (function(event) {
		    this.chunkCache.showRenderedChunks = !this.chunkCache.showRenderedChunks;
		}).bind(this));
		
		// Grab a reference to the sidebar and wrap it in an Interface class instance
		this.interface = new Interface(
			this,
			document.getElementById("sidebar"),
			document.getElementById("debuginfo")
		);
		this.interface.on("toolchange", (function({oldTool, newTool}) {
		    this.canvas.classList.remove(oldTool);
			this.canvas.classList.add(newTool);
		}).bind(this));
		
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
		
		// Make the canvas track the window size
		this.trackWindowSize();
	}
	
	/**
	 * Setup ready for user input.
	 * This mainly consists of establishing the RippleLink connection to the server.
	 */
	setup() {
		this.rippleLink = new RippleLink(`ws${this.settings.SecureWebSocket ? "s" : ""}://${location.host}${this.settings.WebSocketPath}`, this);
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
				InitialAbsCursorPosition: this.cursorSyncer.cursorPosition
			});
		}).bind(this));
		this.rippleLink.on("disconnect", (function(event) {
		    this.interface.setConnectedStatus(false);
		}).bind(this))
		
		// Keep the server up to date on our viewport and cursor position
		this.cursorSyncer = new CursorSyncer(this.rippleLink, this.cursorUpdateFrequency)
		
		// RippleLink message bindings
		
		// Handle the HandshakeResponse when it comes in
		this.rippleLink.on("HandshakeResponse", this.handleHandshakeResponse.bind(this));
		// Handle the plane change confirmations
		this.rippleLink.on("PlaneChangeOk", this.handlePlaneChangeOk.bind(this));
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
			this.update(frameStart - this.lastFrameStart);
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
	update(dt)
	{
		if(typeof this.chunkCache != "undefined" && this.gridSize != -1)
			this.chunkCache.update(dt, this.viewport);
		
		this.interface.updateDebugInfo(dt);
	}
	
	/**
	 * Renders the next frame.
	 */
	render(canvas, context)
	{
		context.clearRect(0, 0, canvas.width, canvas.height);
		
		context.save();
		context.scale(this.viewport.zoomLevel, this.viewport.zoomLevel);
		
		// Draw the grid if it's enabled
		if(this.displayGrid)
			this.renderGrid(canvas, context);
		
		
		context.save();
		context.translate(
			-this.viewport.x,
			-this.viewport.y
		);
		
		// Only render the visible chunks if the chunk cache has been created
		// The chunk cache is only created once the ripple link connects successfully
		// to the nibriboard server.
		if(typeof this.chunkCache != "undefined" && this.gridSize != -1)
			this.chunkCache.renderVisible(this.viewport, canvas, context);
		
		if(typeof this.otherClients != "undefined")
			this.otherClients.render(canvas, context);
		
		// Render the currently active line
		if(typeof this.pencil !== "undefined")
			this.pencil.render(canvas, context);
		
		context.restore();
		
		context.restore();
	}
	
	renderGrid(canvas, context)
	{
		context.save();
		
		
		for(let ax = (this.viewport.x + (this.gridSize - (this.viewport.x % this.gridSize))) - this.gridSize; ax < (this.viewport.x + this.viewport.width); ax += this.gridSize)
		{
			context.beginPath();
			context.moveTo(ax - this.viewport.x, 0);
			context.lineTo(ax - this.viewport.x, this.viewport.height);
			
			if(Math.round(ax) == 0) {
				context.lineWidth = this.primaryGridLineWidth;
				context.strokeStyle = this.primaryGridLineColour;
			} else {
				context.lineWidth = this.gridLineWidth;
				context.strokeStyle = this.gridLineColour;
			}
			context.stroke();
		}
		for(let ay = (this.viewport.y + (this.gridSize - (this.viewport.y % this.gridSize))) - this.gridSize; ay < (this.viewport.y + this.viewport.height); ay += this.gridSize)
		{
			context.beginPath();
			context.moveTo(0, ay - this.viewport.y);
			context.lineTo(this.viewport.width, ay - this.viewport.y);
			
			if(Math.round(ay) == 0) {
				context.lineWidth = this.primaryGridLineWidth;
				context.strokeStyle = this.primaryGridLineColour;
			} else {
				context.lineWidth = this.gridLineWidth;
				context.strokeStyle = this.gridLineColour;
			}
			context.stroke();
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
		// Don't bother processing it if it's a mouse / touch interaction and
		// the control key isn't pressed
		if([ "mouse", "touch" ].includes(event.type) &&
			!this.keyboard.DownKeys.includes(17) &&
			this.interface.currentTool !== "pan")
			return;
		// Store the viewport information for later
		this.viewportState = event;
		
		this.viewport.x -= event.dx * 1/this.viewport.zoomLevel;
		this.viewport.y -= event.dy * 1/this.viewport.zoomLevel;
		this.viewport.zoomLevel += event.dz / 1000;
		this.viewport.zoomLevel = clamp(this.viewport.zoomLevel, 0.1, 10000);
		
		// Update the server on the new size of our viewport
		
		setTimeout((function() {
			if(+new Date() - this.lastViewportUpdate < (1 / this.cursorSyncer.cursorUpdateFrequency) * 1000)
				return false;
			
			this.rippleLink.send({
				Event: "ViewportUpdate",
				NewViewport: {
					X: parseInt(this.viewport.x),
					Y: parseInt(this.viewport.y),
					Width: parseInt(this.viewport.width),
					Height: parseInt(this.viewport.height)
				}
			});
			
			this.lastViewportUpdate = +new Date();
		}).bind(this));
	}
	
	/**
	 * Handles the server's response to our handshake request
	 * @param  {object} message The server's response to our handshake request.
	 */
	handleHandshakeResponse(message) {
		console.log("Received handshake response");
		
		// Store the information send by the server
		this.Id = message.Id;
		this.Colour = message.Colour;
		
		this.interface.OurColour = this.Colour;
		this.interface.setConnectedStatus(true);
		
		// The pencil that draws the lines
		this.pencil = new Pencil(this.rippleLink, this, this.canvas);
		// The cache for the chunks
		this.chunkCache = new ChunkCache(this);
		// Create a new data structure to store client information in
		this.otherClients = new ClientManager(this.rippleLink);
		
		// Land on a default plane
		// future ask the user which plane they want to join
		this.rippleLink.send({
			"Event": "PlaneChange",
			"NewPlaneName": "default-plane"
		});
	}
	
	/**
	 * Store the details about the new plane we've landed on that the server
	 * sends us.
	 * @param  {object} message The plane change confirmation message to handle.
	 */
	handlePlaneChangeOk(message) {
		this.currentPlaneName = message.NewPlaneName;
		this.gridSize = message.GridSize;
		console.info(`Plane changed to ${this.currentPlaneName} with a grid size of ${this.gridSize} successfully.`);
	}
}

export default BoardWindow;
