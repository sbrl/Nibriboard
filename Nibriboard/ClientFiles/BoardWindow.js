"use strict";

// npm modules
window.EventEmitter = require("event-emitter-es6");
window.FaviconNotification = require("favicon-notification");

// Our files
import RippleLink from './RippleLink';
import { get } from './Utilities';

class BoardWindow extends EventEmitter
{
	constructor(canvas)
	{
		super(); // Run the parent constructor
		
		this.canvas = canvas;
		this.context = canvas.getContext("2d");
		FaviconNotification.init({
			color: '#ff6333'
		});
		FaviconNotification.add();
		
		get("/Settings.json").then(JSON.parse).then((function(settings) {
			this.settings = settings;
			this.setup();
		}).bind(this), function(errorMessage) {
			console.error(`Error: Failed to fetch settings from server! Response: ${errorMessage}`);
		});
		
		this.trackWindowSize();
	}
	
	nextFrame()
	{
		this.update();
		this.render(this.canvas, this.context);
		requestAnimationFrame(this.nextFrame.bind(this));
	}
	
	update()
	{
		
	}
	
	render(canvas, context)
	{
		context.clearRect(0, 0, canvas.width, canvas.height);
		
		context.fillStyle = "red";
		context.fillRect(10, 10, 100, 100);
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
}

export default BoardWindow;
