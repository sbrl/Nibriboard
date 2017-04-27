"use strict";

window.EventEmitter = require("event-emitter-es6");

class Interface extends EventEmitter
{
	constructor(inSidebar)
	{
		super();
		
		this.sidebar = inSidebar;
		
		this.currentToolElement = this.sidebar.querySelector(".tools .tool-selector[data-selected]");
		this.currentTool = "brush";
		this.setupToolSelectors();
	}
	
	setupToolSelectors()
	{
		var toolSelectors = this.sidebar.querySelectorAll(".tools .tool-selector");
		for(let i = 0; i < toolSelectors.length; i++)
		{
			toolSelectors[i].addEventListener("mouseup", this.handleSelectTool.bind(this));
			toolSelectors[i].addEventListener("touchend", this.handleSelectTool.bind(this));
		}
	}
	
	/**
	 * Handles tool changes requested by the user.
	 */
	handleSelectTool(event)
	{
		delete this.currentToolElement.dataset.selected;
		this.currentToolElement = event.target;
		this.currentToolElement.dataset.selected = "yes";
		this.currentTool = this.currentToolElement.dataset.toolName;
		console.info("Selected tool", this.currentTool);
		this.emit("toolchange", { newTool: this.currentTool });
	}
	
	/**
	 * Sets the displayed connection status.
	 * @param {bool} newStatus The new connection status to display. true = connected - false = disconnected.
	 */
	setConnectedStatus(newStatus)
	{
		if(newStatus)
			this.sidebar.querySelector(".connection-indicator").dataset.connected = "yes";
		else
			delete this.sidebar.querySelector(".connection-indicator").dataset.connected;
	}
	
	/**
	 * Fetches our colour from the ui.
	 */
	get OurColour()
	{
		return this.sidebar.querySelector(".name").style.borderTopColor;
	}
	/**
	 * Sets the colour displayed just above the name
	 * @param {HTMLColour} newColour The new colour to display jsut above the
	 *                               name in the sidebar.
	 */
	set OurColour(newColour)
	{
		this.sidebar.querySelector(".name").style.borderTopColor = newColour;
	}
	
	/**
	 * Updates the username displayed.
	 * @param  {string} newName The new name to display inthe sidebar.
	 */
	updateDisplayedName(newName)
	{
		this.sidebar.querySelector(".name").innerHTML = newName;
	}
	
	
}

export default Interface;
