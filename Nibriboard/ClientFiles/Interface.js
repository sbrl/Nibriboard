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
		this.switchColourTo(this.currentColourElement);
		
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
		this.switchColourTo(event.target);
	}
	
	/**
	 * Switches the current colour out to be the colour held by the specified
	 * colour palette element.
	 * @param  {HTMLElement} paletteElement The colour palette element to switch to.
	 */
	switchColourTo(paletteElement)
	{
		delete this.currentColourElement.dataset.selected;
		this.currentColourElement = paletteElement;
		this.currentColourElement.dataset.selected = "yes";
		this.currentColour = this.currentColourElement.style.backgroundColor;
		
		this.updateBrushIndicator();
		
		console.info("Selected colour", this.currentColour);
		this.emit("colourchange", { newColour: this.currentColour });
	}
	
	seekColour(direction = "forwards")
	{
		var newPaletteElement = null;
		if(direction == "forwards")
			newPaletteElement = this.currentColourElement.nextElementSibling || this.currentColourElement.parentElement.firstElementChild;
		else if(direction == "backwards")
			newPaletteElement = this.currentColourElement.previousElementSibling || this.currentColourElement.parentElement.lastElementChild;
		else
			throw new Error(`Unknown direction ${direction} when switching colour!`);
		
		this.switchColourTo(newPaletteElement);
	}
	
	/**
	 * Handles brush widdth changes requested by the user
	 */
	handleBrushWidthChange(event)
	{
		// Update the brush width, but don't update the interface, since that's where we got the new value from :P
		this.updateBrushWidth(parseInt(event.target.value, false));
	}
	
	/**
	 * Sets the brush width to the specified value, updating everyone else
	 * on the change.
	 * @param  {number} newWidth The new width of the brush.
	 */
	updateBrushWidth(newWidth, updateInterface = true)
	{
		// Clamp the new width to make sure it's in-bounds
		if(newWidth < 1) newWidth = 1;
		if(newWidth > parseInt(this.brushWidthElement.max)) newWidth = parseInt(this.brushWidthElement.max);
		
		this.currentBrushWidth = newWidth; // Store the new value
		if(updateInterface)
			this.brushWidthElement.value = newWidth; // Update the interface
		
		// Update the brush indicator
		this.updateBrushIndicator();
		// Emit the brush width change event
		this.emit("brushwidthchange", { newWidth: this.currentBrushWidth });
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
