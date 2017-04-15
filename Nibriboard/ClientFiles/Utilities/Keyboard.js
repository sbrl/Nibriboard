"use strict";

/**
 * Makes handling keyboard input just that little bit easier.
 */
class Keyboard
{
	constructor()
	{
		/**
		 * The keyCodes of the keyboard keys that are currently pressed down.
		 * @type {[number]}
		 */
		this.DownKeys = [];
		
		document.addEventListener("keydown", this.handleKeyDown.bind(this));
		document.addEventListener("keyup", this.handleKeyUp.bind(this));
	}
	
	/**
	 * Handles keydown events.
	 * @param  {KeyboardEvent} event The keyboard event to handle.
	 */
	handleKeyDown(event) {
		if(!this.DownKeys.contains(event.keyCode))
			this.DownKeys.push(event.keyCode);
	}
	
	/**
	 * Handles keyup events.
	 * @param  {KeyboardEvent} event The keyboard event to handle.
	 */
	handleKeyUp(event) {
		if(this.DownKeys.indexOf(event.keyCode) !== -1)
			this.DownKeys.splice(this.DownKeys.indexOf(event.keyCode), 1);
	}
}

export default Keyboard;
