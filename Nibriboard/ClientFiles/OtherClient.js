"use strict";

import Vector from './Utilities/Vector';
import Rectangle from './Utilities/Rectangle';

class OtherClient
{
	constructor()
	{
		// The unique id of this client.
		this.Id = -1;
		// The name of this client.
		this.Name = "";
		// The colour the server assigned to this client.
		this.Colour = "green";
		// The position of this clients' cursor.
		this.CursorPosition = new Vector(0, 0);
		// The position and dimensions of this client's viewport.
		this.Viewport = Rectangle.Zero.clone();
		// The time this other client's information was last updated.
		this.LastUpdated = new Date();
	}
	
	update(data) {
		// Update our local copy of this clients' name and colour (just in case)
		this.Name = data.Name;
		this.Colour = data.Colour;
		// Update the cursor position
		this.CursorPosition.x = data.CursorPosition.X;
		this.CursorPosition.y = data.CursorPosition.Y;
		// Update the viewport
		this.Viewport.x = data.Viewport.X;
		this.Viewport.y = data.Viewport.X;
		this.Viewport.width = data.Viewport.Width;
		this.Viewport.height = data.Viewport.Height;
	}
}

/**
 * Converts raw NibriClient data sent from the server into an instance of OtherClient.
 * @param	{object}		raw		The raw data to convert. 
 * @return	{OtherClient}			The converted data.
 */
OtherClient.FromRaw = function(raw) {
	let newOtherClient = new OtherClient();
	newOtherClient.Id = raw.Id;
	newOtherClient.CursorPosition = new Vector(
		raw.CursorPosition.X,
		raw.CursorPosition.Y
	);
	newOtherClient.Viewport = new Rectangle(
		raw.Viewport.X,
		raw.Viewport.Y,
		raw.Viewport.Width,
		raw.Viewport.Height
	);
	newOtherClient.Colour = raw.Colour;
	newOtherClient.LastUpdated = new Date();
	
	return newOtherClient;
}

export default OtherClient;
