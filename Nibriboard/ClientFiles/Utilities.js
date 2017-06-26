"use strict";

function get(u){return new Promise(function(r,t,a){a=new XMLHttpRequest();a.onload=function(b,c){b=a.status;c=a.response;if(b>199&&b<300){r(c)}else{t(c)}};a.open("GET",u,true);a.send(null)})}

/**
 * Clamps x to be between min and max.
 * @param  {number} x   The number to clamp.
 * @param  {number} min The minimum allowed value.
 * @param  {number} max The maximum allowed value.
 * @return {number}     The clamped number.
 */
function clamp(x, min, max)
{
	if(x < min) return min;
	if(x > max) return max; 
	return x;
}

export {
	get,
	clamp
};
