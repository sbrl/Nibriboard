"use strict";

/**
 * Makes handling keyboard input just that little bit easier.
 */
class Keyboard
{
	constructor()
	{
		this.DownKeys = [];
		
		document.addEventListener("keydown", this.handleKeyDown.bind(this));
		document.addEventListener("keyup", this.handleKeyUp.bind(this));
	}
	
	handleKeyDown(event) {
		if(!this.DownKeys.contains(event.keyCode))
			this.DownKeys.push(event.keyCode);
	}
	
	handleKeyUp(event) {
		if(this.DownKeys.indexOf(event.keyCode) !== -1)
			this.DownKeys.splice(this.DownKeys.indexOf(event.keyCode), 1);
	}
}
