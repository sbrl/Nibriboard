"use strict";

import FpsIndicator from "fps-indicator";

import BoardWindow from './BoardWindow';

window.addEventListener("load", function (event) {
	let fpsIndicator = FpsIndicator({
		updatePeriod: 1000,
		maxFps: 60
	});
	fpsIndicator.element.style.color = "rgb(114, 194, 179)";
	
	let canvas = document.getElementById("canvas-main"),
		boardWindow = new BoardWindow(canvas);
	boardWindow.nextFrame();
	window.boardWindow = boardWindow;
});
