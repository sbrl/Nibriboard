"use strict";


export default class WhiteboardSwitcher
{
	/**
	 * Creates a new whiteboard switcher instance.
	 * @param	{HTMLDialogElement}	inDialogElement The dialog to utilise to when asking the user which whiteboard they wish to switch to.
	 * @param	{RippleLink}		inRippleLink The ripple link to use when switching planes.
	 */
	constructor(inDialogElement, inRippleLink)
	{
		this.dialog_element = inDialogElement;
		this.ripple_link = inRippleLink;
		
		this.dialog_element.querySelector(".switch-whiteboard-list")
			.addEventListener("click", (function(event) {
				if(!event.target.classList.contains("plane-list-item"))
					return;
				
				this.switch_to(event.target.dataset.plane_name);
			}).bind(this));
		
		this.register_plane_list_receiver();
	}
	
	/**
	 * Switches to the specified plane.
	 * @param	{string}	new_plane_name	The name of the new plane to switch to.
	 * @return	{Promise}				A promise that's resolves once the switchover is complete.
	 */
	switch_to(new_plane_name)
	{
		return new Promise((function(resolve, reject) {
			this.ripple_link.once("PlaneChangeOk", resolve);
			this.ripple_link.send({
				"Event": "PlaneChange",
				"NewPlaneName": new_plane_name
			});
		}).bind(this));
	}
	
	/**
	 * Gets a list of planes from the server and displays the plane change 
	 * dialog.
	 * @param	{bool}	update_plane_list	Whether to request a fresh list of planes from the server.
	 */
	display(update_plane_list = true)
	{
		if(update_plane_list)
			this.ripple_link.send({ "Event": "PlaneListRequest" });
		this.dialog_element.showModal();
	}
	
	/**
	 * Attaches the plane list response event listener to the ripple link
	 * to recieve the list of planes just once.
	 */
	register_plane_list_receiver() {
		this.ripple_link.on("PlaneListResponse", (function(message) {
			let planeListDisplay = this.dialog_element.querySelector(".switch-whiteboard-list");
			// Clear the old list out
			while (planeListDisplay.firstChild)
    			planeListDisplay.removeChild(planeListDisplay.firstChild);
			
			// Display the new list
			planeListDisplay.appendChild(
				this.build_plane_list_html(message.Planes)
			);
		}).bind(this));
	}
	
	/**
	 * Converts the given plane list to a document fragment of html that can be 
	 * displayed to the user.
	 * @param	{object[]}			plane_list	A list of plane information objects.
	 * @return	{DocumentFragment}	A document fragment containing the rendered HTML.
	 */
	build_plane_list_html(plane_list) {
		let result = document.createDocumentFragment();
		for(let plane_info of plane_list) {
			let nextItem = document.createElement("li");
			
			nextItem.classList.add("plane-list-item");
			nextItem.classList.add(`role-${plane_info.Role.toLowerCase()}`);
			nextItem.dataset.plane_name = plane_info.Name;
			
			nextItem.appendChild(document.createTextNode(plane_info.Name));
			
			result.appendChild(nextItem);
		}
		return result;
	}
}
