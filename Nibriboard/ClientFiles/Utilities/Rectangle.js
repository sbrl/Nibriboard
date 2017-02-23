"use strict";

import Vector from './Vector';

/// <summary>
/// Represents a rectangle in 2D space.
/// </summary>
class Rectangle
{
	/**
	 * The top-left corner of the rectangle.
	 * @returns {Vector}
	 */
	get TopLeft() {
		return new Vector(this.x, this.y);
	}
	
	/**
	 * The top-right corner of the rectangle.
	 * @returns {Vector}
	 */
	get TopRight() {
		return new Vector(this.x + this.width, this.y);
	}
	
	/**
	 * The bottom-left corner of the rectangle.
	 * @returns {Vector}
	 */
	get BottomLeft() {
		return new Vector(this.x, this.y + this.height);
	}
	
	/**
	 * The bottom-right corner of the rectangle.
	 */
	get BottomRight() {
		return new Vector(this.x + this.width, this.y + this.height);
	}
	
	/**
	 * The Y coordinate of the top of the rectangle.
	 * @returns {Number}
	 */
	get Top() {
		return this.y;
	}
	
	/**
	 * The Y coordinate of the bottom of the rectangle.
	 * @returns {Number}
	 */
	get Bottom() {
		return this.y + this.height;
	}
	
	/**
	 * The X coordinate of the left side of the rectangle.
	 * @returns {Number}
	 */
	get Left() {
		return this.x;
	}
	
	/**
	 * The X coordinate of the right side of the rectangle.
	 * @returns {Number}
	 */
	get Right() {
		return this.x + this.width;
	}

	constructor(x, y, width, height)
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
	}
	
	/**
	 * Returns a copy of this rectangle that can be safely edited without affecting the original.
	 * @returns {Rectangle}
	 */
	clone()
	{
		return new Rectangle(this.x, this.y, this.width, this.height);
	}
}

/**
 * A rectangle with all it's values initalised to zero.
 * Don't forget to clone it before editing!
 * @type {Rectangle}
 */
Rectangle.Zero = new Rectangle(0, 0, 0, 0);

export default Rectangle;
