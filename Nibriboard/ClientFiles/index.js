"use strict";

import BoardWindow from './BoardWindow';

window.addEventListener("load", function (event) {
	var canvas = document.getElementById("canvas-main"),
		boardWindow = new BoardWindow(canvas);
	boardWindow.nextFrame();
	window.boardWindow = boardWindow;
});
