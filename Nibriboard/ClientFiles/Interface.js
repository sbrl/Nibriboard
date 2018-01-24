"use strict";

import EventEmitter from "event-emitter-es6";

import BrushIndicator from './BrushIndicator.js';

class Interface extends EventEmitter
{
	constructor(inBoardWindow, inSidebar, inDebugDisplay)
	{
		super();
		
		this.boardWindow = inBoardWindow;
		this.sidebar = inSidebar;
		this.debugDisplay = inDebugDisplay;
		
		this.brushIndicator = new BrushIndicator(
			this.sidebar.querySelector(".brush-indicator")
		);
		
		this.setupToolSelectors();
		this.setupColourSelectors();
		this.setupBrushWidthControls();
		
		this.brushIndicator.render();
	}
	
	/**
	 * Sets up the event listeners on the tool selectors.
	 */
	setupToolSelectors()
	{
		this.currentToolElement = this.sidebar.querySelector(".tools .tool-selector[data-selected]");
		this.lastTool = this.currentTool = "brush";
		
		var toolSelectors = this.sidebar.querySelectorAll(".tools .tool-selector");
		for(let i = 0; i < toolSelectors.length; i++)
		{
			toolSelectors[i].addEventListener("mouseup", this.handleSelectTool.bind(this));
			toolSelectors[i].addEventListener("touchend", this.handleSelectTool.bind(this));
			this.boardWindow.keyboard.on(`keyup-${i + 1}`, (function(toolId, event) {
			    this.handleSelectTool({
					currentTarget: this.sidebar.querySelector(`.tools .tool-selector:nth-child(${toolId})`)
				});
			}).bind(this, i + 1));
		}
		
		this.boardWindow.keyboard.on(`keydown-ctrl`, (function(event) {
		    this.setTool("pan");
		}).bind(this));
		this.boardWindow.keyboard.on(`keyup-ctrl`, (function(event) {
		    this.revertTool();
		}).bind(this));
		
		this.emit("toolchange", {
			oldTool: this.currentToolElement.dataset.toolName,
			newTool: this.currentToolElement.dataset.toolName
		});
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
		this.brushIndicator.width = parseInt(this.brushWidthElement.value);
		
		this.brushWidthElement.addEventListener("input", this.handleBrushWidthChange.bind(this));
	}
	
	/**
	 * Sets the currently active tool.
	 * @param {string} newToolName The name of the tool to set to be the currently active one.
	 */
	setTool(newToolName)
	{
		this.handleSelectTool({
			currentTarget: this.sidebar.querySelector(`.tools .tool-selector[data-tool-name=${newToolName}]`)
		});
	}
	
	/**
	 * Reverts the currently selected tool back to the last one selected.
	 */
	revertTool()
	{
		this.setTool(this.lastTool);
	}
	
	/**
	 * Handles tool changes requested by the user.
	 */
	handleSelectTool(event)
	{
		this.lastTool = this.currentTool;
		
		delete this.currentToolElement.dataset.selected;
		this.currentToolElement = event.currentTarget;
		this.currentToolElement.dataset.selected = "yes";
		this.currentTool = this.currentToolElement.dataset.toolName;
		console.info("Selected tool", this.currentTool);
		this.emit("toolchange", { oldTool: this.lastTool, newTool: this.currentTool });
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
		this.brushIndicator.colour = this.currentColourElement.style.backgroundColor;
		this.brushIndicator.render();
		
		console.info("Selected colour", this.brushIndicator.colour);
		this.emit("colourchange", {
			newColour: this.brushIndicator.colour
		});
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
		
		this.brushIndicator.width = newWidth; // Store the new value
		if(updateInterface)
			this.brushWidthElement.value = newWidth; // Update the interface
		
		this.brushIndicator.render();
		
		// Emit the brush width change event
		this.emit("brushwidthchange", { newWidth: this.brushIndicator.width });
	}
	
	/**
	 * Sets the name displayed in the sidebar.
	 * @param {string} newName The new name to display.
	 */
	setDisplayName(newName)
	{
		this.sidebar.querySelector(".name").innerText = newName;
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
	
	updateDebugInfo(dt)
	{
		this.debugDisplay.querySelector("#debug-viewport").value = this.boardWindow.viewport;
		if(typeof this.boardWindow.cursorSyncer != "undefined")
			this.debugDisplay.querySelector("#debug-cursor").value = `${this.boardWindow.cursorSyncer.cursorPosition} -> ${this.boardWindow.cursorSyncer.absCursorPosition}`;
		this.debugDisplay.querySelector("#debug-framespacing").value = `${dt}ms`;
	}
}

export default Interface;
