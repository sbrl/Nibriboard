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
		
		this.maxFps = 60;
		this.renderTimeIndicator = document.createElement("span");
		this.renderTimeIndicator.innerHTML = "0ms";
		document.querySelector(".fps").appendChild(this.renderTimeIndicator);
		
		this.canvas = canvas;
		this.context = canvas.getContext("2d");
		FaviconNotification.init({
			color: '#ff6333'
		});
		FaviconNotification.add();
		
		get("/Settings.json").then(JSON.parse).then((function(settings) {
			console.info("[setup]", "Obtained settings from server:", settings);
			this.settings = settings;
			this.setup();
		}).bind(this), function(errorMessage) {
			console.error(`Error: Failed to fetch settings from server! Response: ${errorMessage}`);
		});
		
		this.trackWindowSize();
	}
	
	setup() {
		this.rippleLink = new RippleLink(this.settings.WebsocketUri, this);
	}
	
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
