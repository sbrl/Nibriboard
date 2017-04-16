"use strict";

/*****************************************************************************
 ******************************** Mouse class ********************************
 ********************************************************************* v0.1 **
 * A simple mouse class, records and stores mouse information later use
 * outside an event listener. Depends on my earlier Vector class.
 * 
 * Author: Starbeamrainbowlabs
 * Twitter: @SBRLabs
 * Email: feedback at starbeamrainbowlabs dot com
 *
 * From https://gist.github.com/sbrl/06db619f2a1f75305fbd54f66402459b
 *****************************************************************************
 * Originally written for the Nibriboard client.
 *****************************************************************************
 * Changelog
 *****************************************************************************
 * v0.1 - 28th March 2017
 	 * Initial release.
 */

import Vector from './Vector.js';

/**
 * A reference for the different mouse button types.
 * @type {Object}
 */
var MouseButtons = {
	/**
	 * The left mouse button.
	 * @type {Number}
	 */
	Left: 0,
	/**
	 * The right mouse button.
	 * @type {Number}
	 */
	Right: 2,
	/**
	 * The middle mouse button.
	 * @type {Number}
	 */
	Middle: 1
};

class Mouse
{
	constructor()
	{
		/**
		 * Whether the left mouse button is down or not.
		 * @type {boolean}
		 */
		this.leftDown = false;
		/**
		 * Whether the right mouse button is down or not.
		 * @type {boolean}
		 */
		this.rightDown = false;
		/**
		 * Whether the middle mouse button is down or not.
		 * @type {boolean}
		 */
		this.middleDown = false;
		/**
		 * The last known position of the mouse in document-space.
		 * @type {Vector}
		 */
		this.position = new Vector(0, 0);
		
		// Register the required event listeners
		document.addEventListener("mouseup", this.handleMouseUp.bind(this));
		document.addEventListener("mousedown", this.handleMouseDown.bind(this));
		document.addEventListener("mousemove", this.handleMouseMove.bind(this));
	}
	
	handleMouseUp(event)
	{
		switch(event.button)
		{
			case MouseButtons.Left:
				this.leftDown = false;
				break;
			case MouseButtons.Right:
				this.rightDown = false;
				break;
			case MouseButtons.Middle:
				this.middleDown = false;
				break;
			default:
				console.warn(`Unknown mouse up button type ${even.button}`);
				break;
		}
	}
	
	handleMouseDown(event)
	{
		switch(event.button)
		{
			case MouseButtons.Left:
			this.leftDown = true;
			break;
			case MouseButtons.Right:
			this.rightDown = true;
			break;
			case MouseButtons.Middle:
			this.middleDown = true;
			break;
			default:
			console.warn(`Unknown mouse down button type ${even.button}`);
			break;
		}
	}
	
	handleMouseMove(event)
	{
		this.position.x = event.clientX;
		this.position.y = event.clientY;
		this.pressur = typeof event.pressure === "number" ? event.pressure : event.mozPressure;
	}
}


export { Mouse, MouseButtons };
export default Mouse;
