"use strict";


class RippleLink
{
	constructor(inSocketUrl, inBoardWindow)
	{
		this.socketUrl = inSocketUrl;
		this.boardWindow = inBoardWindow;
		this.settings = this.boardWindow.settings;
		
		this.websocket = new WebSocket( this.socketUrl, [ this.settings.WebsocketProtocol ] );
	}
}

export default RippleLink;
