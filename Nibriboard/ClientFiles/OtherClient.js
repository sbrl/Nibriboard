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

export default OtherClient;
