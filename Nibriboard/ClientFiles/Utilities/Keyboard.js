"use strict";

window.EventEmitter = require("event-emitter-es6");
window.keycode = require("keycode");

/**
 * Makes handling keyboard input just that little bit easier.
 */
class Keyboard extends EventEmitter
{
	constructor()
	{
		super();
		
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
		if(!this.DownKeys.includes(event.keyCode))
			this.DownKeys.push(event.keyCode);
		
		console.log("DownKeys:", this.DownKeys);
		console.debug("[keyboard] Emitting key down event", `keydown-${keycode(event.keyCode)}`);
		this.emit(`keydown-${keycode(event.keyCode)}`, event);
	}
	
	/**
	 * Handles keyup events.
	 * @param  {KeyboardEvent} event The keyboard event to handle.
	 */
	handleKeyUp(event) {
		if(this.DownKeys.indexOf(event.keyCode) !== -1)
			this.DownKeys.splice(this.DownKeys.indexOf(event.keyCode), 1);
			
		console.log("DownKeys:", this.DownKeys);
		console.debug("[keyboard] Emitting key up event", `keyup-${keycode(event.keyCode)}`);
		this.emit(`keyup-${keycode(event.keyCode)}`, event);
	}
}

export default Keyboard;
