"use strict";

import Vector from './Utilities/Vector.js';

class CursorSyncer
{
	constructor(inBoardWindow, syncFrequency)
	{
		this.boardWindow = inBoardWindow
		// The ripple link we should send the cursor updates down
		this.rippleLink = this.boardWindow.rippleLink;
		// The target frequency in fps at we should send cursor updates.
		this.cursorUpdateFrequency = syncFrequency;
		
		// Register ourselves to start sending cursor updates once the ripple
		// link connects
		this.rippleLink.on("connect", this.setup.bind(this));
		
		/** The current cursor position in screen-space. @type {Vector} */
		this.cursorPosition = new Vector(0, 0);
		/** The current cursor position in plane-space. @type {Vector} */
		this.absCursorPosition = new Vector(0, 0);
	}
	
	setup()
	{
		// The last time we sent a cursor update to the server.
		this.lastCursorUpdate = 0;
		
		document.addEventListener("mousemove", (function(event) {
			this.cursorPosition.x = event.clientX;
			this.cursorPosition.y = event.clientY;
			
			this.absCursorPosition.x = this.boardWindow.viewport.x + this.cursorPosition.x;
			this.absCursorPosition.y = this.boardWindow.viewport.y + this.cursorPosition.y;
			
			setTimeout((function() {
			    // Throttle the cursor updates we send to the server - a high
			    // update frequency here will just consume bandwidth and is only
			    // noticable if you have a good connection
				if(+new Date() - this.lastCursorUpdate < (1 / this.cursorUpdateFrequency) * 1000)
					return false;
					
				// Update the server on the mouse's position
				this.sendCursorUpdate();
				
				this.lastCursorUpdate = +new Date();
			}).bind(this), 1 / this.cursorUpdateFrequency);
			
		}).bind(this));
		
		this.sendCursorUpdate();
	}
	
	sendCursorUpdate()
	{
		// Update the server on the mouse's position
		this.rippleLink.send({
			"Event": "CursorPosition",
			"AbsCursorPosition": {
				X: Math.floor(this.absCursorPosition.x),
				Y: Math.floor(this.absCursorPosition.y)
			}
		});
	}
}

export default CursorSyncer;
