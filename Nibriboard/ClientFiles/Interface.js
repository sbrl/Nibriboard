"use strict";

window.EventEmitter = require("event-emitter-es6");

class Interface extends EventEmitter
{
	constructor(inSidebar)
	{
		super();
		
		this.sidebar = inSidebar;
		
		this.brushIndicator = this.sidebar.querySelector(".brush-indicator");
		
		this.setupToolSelectors();
		this.setupColourSelectors();
		this.setupBrushWidthControls();
		
		this.updateBrushIndicator();
	}
	
	/**
	 * Sets up the event listeners on the tool selectors.
	 */
	setupToolSelectors()
	{
		this.currentToolElement = this.sidebar.querySelector(".tools .tool-selector[data-selected]");
		this.currentTool = "brush";
		
		var toolSelectors = this.sidebar.querySelectorAll(".tools .tool-selector");
		for(let i = 0; i < toolSelectors.length; i++)
		{
			toolSelectors[i].addEventListener("mouseup", this.handleSelectTool.bind(this));
			toolSelectors[i].addEventListener("touchend", this.handleSelectTool.bind(this));
		}
	}
	
	/**
	 * Sets up the event listeners on the colour selectors.
	 */
	setupColourSelectors()
	{
		this.currentColourElement = this.sidebar.querySelector(".palette .palette-colour[data-selected]");
		this.currentColour = this.currentColourElement.style.backgroundColor;
		
		var colours = this.sidebar.querySelectorAll(".palette .palette-colour");
		for (let i = 0; i < colours.length; i++) {
			colours[i].addEventListener("mouseup", this.handleSelectColour.bind(this));
			colours[i].addEventListener("touchend", this.handleSelectColour.bind(this));
		}
	}
	
	/**
	 * Sets up the brush width controls
	 */
	setupBrushWidthControls()
	{
		this.brushWidthElement = this.sidebar.querySelector(".brush-width-controls");
		this.currentBrushWidth = parseInt(this.brushWidthElement.value);
		
		this.brushWidthElement.addEventListener("input", this.handleBrushWidthChange.bind(this));
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
	 * Handles colour selection changes requested by the user.
	 */
	handleSelectColour(event)
	{
		delete this.currentColourElement.datasest.selected;
		this.currentColourElement = event.target;
		this.currentColourElement.dataset.selected = "yes";
		this.currentColour = this.currentColourElement.style.backgroundColor;
		
		this.updateBrushIndicator();
		
		console.info("Selected colour", this.currentColour);
		this.emit("colourchange", { newColour: this.currentColour });
	}
	
	/**
	 * Handles brush widdth changes requested by the user
	 */
	handleBrushWidthChange(event)
	{
		this.currentBrushWidth = parseInt(event.target.value);
		
		this.updateBrushIndicator();
		
		this.emit("brushwidthchange", { newWidth: this.currentLineWidth });
	}
	
	updateBrushIndicator()
	{
		// The brush indicator is zoom-agnostic (for the moment, at least)
		this.brushIndicator.style.width = `${this.currentBrushWidth}px`;
		this.brushIndicator.style.height = this.brushIndicator.style.width;
		
		this.brushIndicator.style.backgroundColor = this.currentColour;
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
