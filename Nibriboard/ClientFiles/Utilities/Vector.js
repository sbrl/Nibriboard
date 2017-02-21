"use strict";

/******************************************************
 ************** Simple ES6 Vector Class  **************
 ******************************************************
 * Author: Starbeamrainbowlabs
 * Twitter: @SBRLabs
 * Email: feedback at starbeamrainbowlabs dot com
 * 
 * From https://gist.github.com/sbrl/69a8fa588865cacef9c0
 ******************************************************
 * Originally written for my 2D Graphics ACW at Hull
 * University.
 ******************************************************
 * Changelog
 ******************************************************
 * 19th December 2015:
	* Added this changelog.
 * 28th December 2015:
	* Rewrite tests with klud.js + Node.js
 * 30th January 2016:
	* Tweak angleFrom function to make it work properly.
 * 31st January 2016:
	* Add the moveTowards function.
	* Add the minComponent getter.
	* Add the maxComponent getter.
	* Add the equalTo function.
	* Tests still need to be written for all of the above.
 * 19th September 2016:
	* Added Vector support to the multiply method.
 */

class Vector {
	// Constructor
	constructor(inX, inY) {
		if(typeof inX != "number")
			throw new Error("Invalid x value.");
		if(typeof inY != "number")
			throw new Error("Invalid y value.");
		
		// Store the (x, y) coordinates
		this.x = inX;
		this.y = inY;
	}

	/**
	 * Add another vector to this vector.
	 * @param {Vector} v The vector to add.
	 * @return {Vector}   The current vector. useful for daisy-chaining calls.
	 */
	add(v) {
		this.x += v.x;
		this.y += v.y;

		return this;
	}

	/**
	 * Take another vector from this vector.
	 * @param  {Vector} v The vector to subtrace from this one.
	 * @return {Vector}   The current vector. useful for daisy-chaining calls.
	 */
	subtract(v) {
		this.x -= v.x;
		this.y -= v.y;

		return this;
	}

	/**
	 * Divide the current vector by a given value.
	 * @param  {number} value The value to divide by.
	 * @return {Vector}	   The current vector. Useful for daisy-chaining calls.
	 */
	divide(value) {
		if(typeof value != "number")
			throw new Error("Can't divide by non-number value.");
		
		this.x /= value;
		this.y /= value;

		return this;
	}

	/**
	 * Multiply the current vector by a given value.
	 * @param  {(number|Vector)} value The number (or Vector) to multiply the current vector by.
	 * @return {Vector}	   The current vector. useful for daisy-chaining calls.
	 */
	multiply(value) {
		if(value instanceof Vector)
		{
			this.x *= value.x;
			this.y *= value.y;
		}
		else if(typeof value == "number")
		{
			this.x *= value;
			this.y *= value;
		}
		else
			throw new Error("Can't multiply by non-number value.");
		
		return this;
	}
	
	/**
	 * Move the vector towards the given vector by the given amount.
	 * @param  {Vector} v      The vector to move towards.
	 * @param  {number} amount The distance to move towards the given vector.
	 */
	moveTowards(v, amount)
	{
		// From http://stackoverflow.com/a/2625107/1460422
		var dir = new Vector(
			v.x - this.x,
			v.y - this.y
		).limitTo(amount);
		this.x += dir.x;
		this.y += dir.y;
		
		return this;
	}

	/**
	 * Limit the length of the current vector to value without changing the
	 * direction in which the vector is pointing.
	 * @param  {number} value The number to limit the current vector's length to.
	 * @return {Vector}	   The current vector. useful for daisy-chaining calls.
	 */
	limitTo(value) {
		if(typeof value != "number")
			throw new Error("Can't limit to non-number value.");
		
		this.divide(this.length);
		this.multiply(value);

		return this;
	}

	/**
	 * Return the dot product of the current vector and another vector.
	 * @param  {Vector} v   The other vector we should calculate the dot product with.
	 * @return {Vector}	 The current vector. useful for daisy-chaining calls.
	 */
	dotProduct(v) {
		return (this.x * v.x) + (this.y * v.y);
	}

	/**
	 * Calculate the angle, in radians, from north to another vector.
	 * @param  {Vector} v The other vector to which to calculate the angle.
	 * @return {Vector}	 The current vector. useful for daisy-chaining calls.
	 */
	angleFrom(v) {
		// From http://stackoverflow.com/a/16340752/1460422
		var angle = Math.atan2(v.y - this.y, v.x - this.x) - (Math.PI / 2);
		angle += Math.PI/2;
		if(angle < 0) angle += Math.PI * 2;
		return angle;
	}

	/**
	 * Clones the current vector.
	 * @return {Vector} A clone of the current vector. Very useful for passing around copies of a vector if you don't want the original to be altered.
	 */
	clone() {
		return new Vector(this.x, this.y);
	}

	/*
	 * Returns a representation of the current vector as a string.
	 * @returns {string} A representation of the current vector as a string.
	 */
	toString() {
		return `(${this.x}, ${this.y})`;
	}
	
	/**
	 * Whether the vector is equal to another vector.
	 * @param  {Vector} v The vector to compare to.
	 * @return {boolean}  Whether the current vector is equal to the given vector.
	 */
	equalTo(v)
	{
		if(this.x == v.x && this.y == v.y)
			return true;
		else
			return false;
	}

	/**
	 * Get the unit vector of the current vector - that is a vector poiting in the same direction with a length of 1. Note that this does *not* alter the original vector.
	 * @return {Vector} The current vector's unit form.
	 */
	get unitVector() {
		var length = this.length;
		return new Vector(
		this.x / length,
		this.y / length);
	}

	/**
	 * Get the length of the current vector.
	 * @return {number} The length of the current vector.
	 */
	get length() {
		return Math.sqrt((this.x * this.x) + (this.y * this.y));
	}
	
	/**
	 * Get the value of the minimum component of the vector.
	 * @return {number} The minimum component of the vector.
	 */
	get minComponent() {
		return Math.min(this.x, this.y);
	}
	
	/**
	 * Get the value of the maximum component of the vector.
	 * @return {number} The maximum component of the vector.
	 */
	get maxComponent() {
		return Math.min(this.x, this.y);
	}
}

export default Vector;
