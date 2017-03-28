"use strict";

import Mouse from './Mouse';

class Pencil
{
	/**
	 * Creates a new Pencil class instance.
	 * @param	{RippleLink}	inRippleLink	The connection to the nibri server.
	 * @return	{Pencil}		A new Pencil class instance.
	 */
	constructor(inRippleLink)
	{
		this.rippleLink = inRippleLink;
		/**
		 * The mouse information.
		 * @type {Mouse}
		 */
		this.mouse = new Mouse();
		
		document.addEventListener("mouseMove", this.handleMouseMove.bind(this));
	}
	
	handleMouseMove(event) {
		
	}
}
