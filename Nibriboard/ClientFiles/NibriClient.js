(function e(t,n,r){function s(o,u){if(!n[o]){if(!t[o]){var a=typeof require=="function"&&require;if(!u&&a)return a(o,!0);if(i)return i(o,!0);var f=new Error("Cannot find module '"+o+"'");throw f.code="MODULE_NOT_FOUND",f}var l=n[o]={exports:{}};t[o][0].call(l.exports,function(e){var n=t[o][1][e];return s(n?n:e)},l,l.exports,e,t,n,r)}return n[o].exports}var i=typeof require=="function"&&require;for(var o=0;o<r.length;o++)s(r[o]);return s})({1:[function(require,module,exports){
'use strict';

/**
 * Constants for the different readyStates that a WebSocket can be in.
 * @type {Object}
 */
const WebSocketStates = {
	/**
	 * Indicates that the WebSocket is connecting to the remote server.
	 * @type {Number}
	 */
	connecting: 0,
	/**
	 * Indicates that the WebSocket is connected to the remote server and ready to send / receive data.
	 * @type {Number}
	 */
	ready: 1,
	/**
	 * Indicates that the WebSocket is in the process of closing the connection to the remote server.
	 * @type {Number}
	 */
	closing: 2,
	/**
	 * Indicates that hte WebSocket is not connected to the remote server (either because the connection was closed, or dropped by the remote server).
	 * @type {Number}
	 */
	closed: 3
};

const EventEmitter$1 = require("event-emitter-es6");

class RippleLink extends EventEmitter$1
{
	constructor(inSocketUrl, inBoardWindow)
	{
		super();
		
		this.socketUrl = inSocketUrl;
		this.boardWindow = inBoardWindow;
		this.settings = this.boardWindow.settings;
		
		// Create the websocket and commect to the server
		this.websocket = new WebSocket(this.socketUrl, [ this.settings.WebsocketProtocol ]);
		this.websocket.addEventListener("open", this.handleConnection.bind(this));
		this.websocket.addEventListener("message", this.handleMessage.bind(this));
		this.websocket.addEventListener("close", this.handleDisconnection.bind(this));
		
		// Close the socket correctly
		window.addEventListener("beforeunload", (function(event) {
			this.websocket.close();
		}).bind(this));
	}
	
	handleConnection(event) {
		console.info("[ripple link] Established connection successfully.");
		// Tell everyone about it
		this.emit("connect", event);
	}
	
	handleDisconnection(event) {
		console.error("[ripple link] Lost connection.");
		this.emit("disconnect", event);
	}
	
	handleMessage(event) {
		// Decode the message form the server
		var message = JSON.parse(event.data);
		console.debug(message);
		
		// Pass it on to the board manager by triggering the appropriate event
		this.emit(message.event, message);
	}
	
	/**
	 * Sends a message object to the server.
	 */
	send(message) {
		if(this.websocket.readyState !== WebSocketStates.ready)
		{
			console.error(`Attempt to send data on the RippleLine when it is not ready (state ${this.websocket.readyState})`);
			return false;
		}
		
		this.websocket.send(JSON.stringify(message));
		return true;
	}
}

function get(u){return new Promise(function(r,t,a){a=new XMLHttpRequest();a.onload=function(b,c){b=a.status;c=a.response;if(b>199&&b<300){r(c);}else{t(c);}};a.open("GET",u,true);a.send(null);})}

// npm modules
window.EventEmitter = require("event-emitter-es6");
window.FaviconNotification = require("favicon-notification");
window.panzoom = require("pan-zoom");

// Our files
class BoardWindow extends EventEmitter
{
	constructor(canvas)
	{
		super(); // Run the parent constructor
		
		// The maximum target fps.
		this.maxFps = 60;
		// Setup the fps indicator in the corner
		this.renderTimeIndicator = document.createElement("span");
		this.renderTimeIndicator.innerHTML = "0ms";
		document.querySelector(".fps").appendChild(this.renderTimeIndicator);
		
		// Setup the canvas
		this.canvas = canvas;
		this.context = canvas.getContext("2d");
		
		// --~~~--
		
		// Setup the favicon thingy
		
		FaviconNotification.init({
			color: '#ff6333'
		});
		FaviconNotification.add();
		
		// Setup the input controls
		window.panzoom(canvas, this.handleCanvasMovement.bind(this));
		
		// Fetch the RippleLink connection information and other settings from
		// the server
		get("/Settings.json").then(JSON.parse).then((function(settings) {
			console.info("[setup]", "Obtained settings from server:", settings);
			this.settings = settings;
			this.setup();
		}).bind(this), function(errorMessage) {
			console.error(`Error: Failed to fetch settings from server! Response: ${errorMessage}`);
		});
		
		// Make the canvas track the window size
		this.trackWindowSize();
		// Track the mouse position
		this.trackMousePosition();
	}
	
	/**
	 * Setup ready for user input.
	 * This mainly consists of establishing the RippleLink connection to the server.
	 */
	setup() {
		this.rippleLink = new RippleLink(this.settings.WebsocketUri, this);
		this.rippleLink.on("connect", (function(event) {
			// Send the handshake request
			this.rippleLink.send({
				event: "handshakeRequest",
				InitialViewport: { // TODO: Add support for persisting this between sessions
					X: 0,
					Y: 0,
					Width: window.innerWidth,
					Height: window.innerHeight
				},
				InitialAbsCursorPosition: this.cursorPosition
			});
		}).bind(this));
		
		// RippleLink message bindings
		
	}
	
	/**
	 * Renders the next frame.
	 */
	nextFrame()
	{
		// The time at which the current frame started rendering.
		let frameStart = +new Date();
		
		if(frameStart - this.lastFrameStart >= (1 / this.maxFps) * 1000)
		{
			this.update();
			this.render(this.canvas, this.context);
		}
		
		// Update the time the last frame started rendering
		this.lastFrameStart = frameStart;
		// Update the time we took rendering the last frame
		this.lastFrameTime = +new Date() - frameStart;
		
		this.renderTimeIndicator.innerHTML = `${this.lastFrameTime}ms`;
		
		// Limit the maximum fps
		requestAnimationFrame(this.nextFrame.bind(this));
	}
	
	/**
	 * Updates everything ready for the next frame to be rendered.
	 */
	update()
	{
		
	}
	
	/**
	 * Renders the next frame.
	 */
	render(canvas, context)
	{
		context.clearRect(0, 0, canvas.width, canvas.height);
		
		context.fillStyle = "red";
		context.fillRect(10, 10, 100, 100);
	}
	
	/**
	 * Updates the canvas size to match the current viewport size.
	 */
	matchWindowSize() {
		this.canvas.width = window.innerWidth;
		this.canvas.height = window.innerHeight;
		
		this.render(this.canvas, this.context);
	}
	
	/**
	 * Makes the canvas size track the window size.
	 */
	trackWindowSize() {
		this.matchWindowSize();
		window.addEventListener("resize", this.matchWindowSize.bind(this));
	}
	
	trackMousePosition() {
		document.addEventListener("mousemove", (function(event) {
			this.cursorPosition = {
				X: event.clientX,
				Y: event.clientY
			};
		}).bind(this));
	}
	
	/**
	 * Handles events generated by pan-zoom, the package that handles the
	 * dragging and zooming of the whiteboard.
	 */
	handleCanvasMovement(event) {
		this.viewportState = event; // Store the viewport information for later
	}
}

window.FpsIndicator = require("fps-indicator");

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

},{"event-emitter-es6":3,"favicon-notification":4,"fps-indicator":5,"pan-zoom":10}],2:[function(require,module,exports){
module.exports = defaultProperty

function defaultProperty (get, set) {
  return {
    configurable: true,
    enumerable: true,
    get: get,
    set: set
  }
}

},{}],3:[function(require,module,exports){
'use strict';

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

var DEFAULT_VALUES = {
    emitDelay: 10,
    strictMode: false
};

/**
 * @typedef {object} EventEmitterListenerFunc
 * @property {boolean} once
 * @property {function} fn
 */

/**
 * @class EventEmitter
 *
 * @private
 * @property {Object.<string, EventEmitterListenerFunc[]>} _listeners
 * @property {string[]} events
 */

var EventEmitter = function () {

    /**
     * @constructor
     * @param {{}}      [opts]
     * @param {number}  [opts.emitDelay = 10] - Number in ms. Specifies whether emit will be sync or async. By default - 10ms. If 0 - fires sync
     * @param {boolean} [opts.strictMode = false] - is true, Emitter throws error on emit error with no listeners
     */

    function EventEmitter() {
        var opts = arguments.length <= 0 || arguments[0] === undefined ? DEFAULT_VALUES : arguments[0];

        _classCallCheck(this, EventEmitter);

        var emitDelay = void 0,
            strictMode = void 0;

        if (opts.hasOwnProperty('emitDelay')) {
            emitDelay = opts.emitDelay;
        } else {
            emitDelay = DEFAULT_VALUES.emitDelay;
        }
        this._emitDelay = emitDelay;

        if (opts.hasOwnProperty('strictMode')) {
            strictMode = opts.strictMode;
        } else {
            strictMode = DEFAULT_VALUES.strictMode;
        }
        this._strictMode = strictMode;

        this._listeners = {};
        this.events = [];
    }

    /**
     * @protected
     * @param {string} type
     * @param {function} listener
     * @param {boolean} [once = false]
     */


    _createClass(EventEmitter, [{
        key: '_addListenner',
        value: function _addListenner(type, listener, once) {
            if (typeof listener !== 'function') {
                throw TypeError('listener must be a function');
            }

            if (this.events.indexOf(type) === -1) {
                this._listeners[type] = [{
                    once: once,
                    fn: listener
                }];
                this.events.push(type);
            } else {
                this._listeners[type].push({
                    once: once,
                    fn: listener
                });
            }
        }

        /**
         * Subscribes on event type specified function
         * @param {string} type
         * @param {function} listener
         */

    }, {
        key: 'on',
        value: function on(type, listener) {
            this._addListenner(type, listener, false);
        }

        /**
         * Subscribes on event type specified function to fire only once
         * @param {string} type
         * @param {function} listener
         */

    }, {
        key: 'once',
        value: function once(type, listener) {
            this._addListenner(type, listener, true);
        }

        /**
         * Removes event with specified type. If specified listenerFunc - deletes only one listener of specified type
         * @param {string} eventType
         * @param {function} [listenerFunc]
         */

    }, {
        key: 'off',
        value: function off(eventType, listenerFunc) {
            var _this = this;

            var typeIndex = this.events.indexOf(eventType);
            var hasType = eventType && typeIndex !== -1;

            if (hasType) {
                if (!listenerFunc) {
                    delete this._listeners[eventType];
                    this.events.splice(typeIndex, 1);
                } else {
                    (function () {
                        var removedEvents = [];
                        var typeListeners = _this._listeners[eventType];

                        typeListeners.forEach(
                        /**
                         * @param {EventEmitterListenerFunc} fn
                         * @param {number} idx
                         */
                        function (fn, idx) {
                            if (fn.fn === listenerFunc) {
                                removedEvents.unshift(idx);
                            }
                        });

                        removedEvents.forEach(function (idx) {
                            typeListeners.splice(idx, 1);
                        });

                        if (!typeListeners.length) {
                            _this.events.splice(typeIndex, 1);
                            delete _this._listeners[eventType];
                        }
                    })();
                }
            }
        }

        /**
         * Applies arguments to specified event type
         * @param {string} eventType
         * @param {*[]} eventArguments
         * @protected
         */

    }, {
        key: '_applyEvents',
        value: function _applyEvents(eventType, eventArguments) {
            var typeListeners = this._listeners[eventType];

            if (!typeListeners || !typeListeners.length) {
                if (this._strictMode) {
                    throw 'No listeners specified for event: ' + eventType;
                } else {
                    return;
                }
            }

            var removableListeners = [];
            typeListeners.forEach(function (eeListener, idx) {
                eeListener.fn.apply(null, eventArguments);
                if (eeListener.once) {
                    removableListeners.unshift(idx);
                }
            });

            removableListeners.forEach(function (idx) {
                typeListeners.splice(idx, 1);
            });
        }

        /**
         * Emits event with specified type and params.
         * @param {string} type
         * @param eventArgs
         */

    }, {
        key: 'emit',
        value: function emit(type) {
            var _this2 = this;

            for (var _len = arguments.length, eventArgs = Array(_len > 1 ? _len - 1 : 0), _key = 1; _key < _len; _key++) {
                eventArgs[_key - 1] = arguments[_key];
            }

            if (this._emitDelay) {
                setTimeout(function () {
                    _this2._applyEvents.call(_this2, type, eventArgs);
                }, this._emitDelay);
            } else {
                this._applyEvents(type, eventArgs);
            }
        }

        /**
         * Emits event with specified type and params synchronously.
         * @param {string} type
         * @param eventArgs
         */

    }, {
        key: 'emitSync',
        value: function emitSync(type) {
            for (var _len2 = arguments.length, eventArgs = Array(_len2 > 1 ? _len2 - 1 : 0), _key2 = 1; _key2 < _len2; _key2++) {
                eventArgs[_key2 - 1] = arguments[_key2];
            }

            this._applyEvents(type, eventArgs);
        }

        /**
         * Destroys EventEmitter
         */

    }, {
        key: 'destroy',
        value: function destroy() {
            this._listeners = {};
            this.events = [];
        }
    }]);

    return EventEmitter;
}();

module.exports = EventEmitter;

},{}],4:[function(require,module,exports){
(function (root, factory) {
    if (typeof define === 'function' && define.amd) {
        // AMD. Register as an anonymous module.
        define([], factory);
    } else if (typeof exports === 'object') {
        // Node. Does not work with strict CommonJS, but
        // only CommonJS-like environments that support module.exports,
        // like Node.
        module.exports = factory();
    } else {
        // Browser globals (root is window)
        root.FaviconNotification = factory();
  }
}(this, function() {

    // Just return a value to define the module export.
    // This example returns an object, but the module
    // can return a function as the exported value.

    // Only run in browser
    if (typeof document === 'undefined') {
      console.log('This script only run in browsers.');
      return;
    }

    // Private properties
    var _options = {};

    var _defaults = {
      url: '/favicon.ico',
      color: '#eb361e',
      lineColor: '#ffffff'
    };

    var _generatedFavicon;
    var _iconElement;

    // Provate methods
    var _addFavicon = function(src) {
      var head = document.getElementsByTagName('head')[0];
      _iconElement = document.createElement('link');
      _iconElement.type = 'image/x-icon';
      _iconElement.rel = 'icon';
      _iconElement.href = src;

      // remove existing favicons
      var links = document.getElementsByTagName('link');
      for(var i=0, len=links.length; i < len; i++) {
    		var exists = (typeof(links[i]) !== 'undefined');
    		if (exists && (links[i].getAttribute('rel') || '').match(/\bicon\b/)) {
    			head.removeChild(links[i]);
    		}
    	}

      head.appendChild(_iconElement);
    };

    var _generateIcon = function(cb) {
      var img = document.createElement('img');
      img.src = _options.url;

      img.onload = function() {
        var lineWidth = 2;
        var canvas = document.createElement('canvas');
        canvas.width = img.width;
        canvas.height = img.height;

        var context = canvas.getContext('2d');
        context.clearRect(0, 0, img.width, img.height);
        context.drawImage(img, 0, 0);

        var centerX = img.width - (img.width / 4.5) - lineWidth;
        var centerY = img.height - (img.height / 4.5) - lineWidth;
        var radius = img.width / 4.5;

        context.fillStyle = _options.color;
        context.strokeStyle = _options.lineColor;
        context.lineWidth = lineWidth;

        context.beginPath();
        context.arc(centerX, centerY, radius, 0, Math.PI * 2, false);
        context.closePath();
        context.fill();
        context.stroke();

        cb(null, context.canvas.toDataURL());
      };
    };

    var _setOptions = function(options) {
      if (!options) {
        _options = _defaults;
        return;
      }

      _options = {};

      for(var key in _defaults){
           _options[key] = options.hasOwnProperty(key) ? options[key] : _defaults[key];
      }
    };

    var FaviconNotification = {
      init: function(options) {

        _setOptions(options);

        _generateIcon(function(err, url){
          _generatedFavicon = url;
        });

        _addFavicon(_options.url);

      },

      add: function() {
        if (!_generatedFavicon && !_iconElement) {
          _setOptions();
          _generateIcon(function(err, url) {
            _generatedFavicon = url;
            _addFavicon(url);
          });
        } else {
          _iconElement.href = _generatedFavicon;
        }

      },

      remove: function() {
        _iconElement.href = _options.url;
      }
    };


    return FaviconNotification;


}));

},{}],5:[function(require,module,exports){
/**
 * @module fps-indicator
 */

const raf = require('raf');
const now = require('right-now');

module.exports = fps;



function fps (opts) {
	if (!(this instanceof fps)) return new fps(opts);

	opts = opts || {};

	if (opts.container) {
		if (typeof opts.container === 'string') {
			this.container = document.querySelector(opts.container);
		}
		else {
			this.container = opts.container;
		}
	}
	else {
		this.container = document.body || document.documentElement;
	}

	//init fps
	this.element = document.createElement('div');
	this.element.classList.add('fps');
	this.element.innerHTML = `
		<div class="fps-bg"></div>
		<canvas class="fps-canvas"></canvas>
		<span class="fps-text">fps <span class="fps-value">60.0</span></span>
	`;
	this.container.appendChild(this.element);

	this.canvas = this.element.querySelector('.fps-canvas');
	this.textEl = this.element.querySelector('.fps-text');
	this.valueEl = this.element.querySelector('.fps-value');
	this.bgEl = this.element.querySelector('.fps-bg');

	this.element.style.cssText = `
		line-height: 1;
		position: absolute;
		z-index: 1;
		top: 0;
		right: 0;
	`;

	this.canvas.style.cssText = `
		position: relative;
		width: 2em;
		height: 1em;
		display: block;
		float: left;
		margin-right: .333em;
	`;

	this.bgEl.style.cssText = `
		position: absolute;
		height: 1em;
		width: 2em;
		background: currentcolor;
		opacity: .1;
	`;

	this.canvas.width = parseInt(getComputedStyle(this.canvas).width) || 1;
	this.canvas.height = parseInt(getComputedStyle(this.canvas).height) || 1;

	this.context = this.canvas.getContext('2d');

	let ctx = this.context;
	let w = this.canvas.width;
	let h = this.canvas.height;
	let count = 0;
	let lastTime = 0;
	let values = opts.values || Array(this.canvas.width);
	let updatePeriod = opts.updatePeriod || 1000;
	let maxFps = opts.maxFps || 100;

	//enable update routine
	let that = this;
	raf(function measure () {
		count++;
		let t = now();

		if (t - lastTime > updatePeriod) {
			let color = that.color;
			lastTime = t;
			values.push(count / (maxFps * updatePeriod * 0.001));
			values = values.slice(-w);
			count = 0;

			ctx.clearRect(0, 0, w, h);
			ctx.fillStyle = getComputedStyle(that.canvas).color;
			for (let i = w; i--;) {
				let value = values[i];
				if (value == null) break;
				ctx.fillRect(i, h - h * value, 1, h * value);
			}

			that.valueEl.innerHTML = (values[values.length - 1]*maxFps).toFixed(1);
		}

		raf(measure);
	});
}
},{"raf":13,"right-now":14}],6:[function(require,module,exports){
module.exports = distance

/**
 * Calculates the euclidian distance between two vec2's
 *
 * @param {vec2} a the first operand
 * @param {vec2} b the second operand
 * @returns {Number} distance between a and b
 */
function distance(a, b) {
    var x = b[0] - a[0],
        y = b[1] - a[1]
    return Math.sqrt(x*x + y*y)
}
},{}],7:[function(require,module,exports){
(function (global, factory) {
	if (typeof define === 'function' && define.amd) {
		define(['exports', 'module'], factory);
	} else if (typeof exports !== 'undefined' && typeof module !== 'undefined') {
		factory(exports, module);
	} else {
		var mod = {
			exports: {}
		};
		factory(mod.exports, mod);
		global.Impetus = mod.exports;
	}
})(this, function (exports, module) {
	'use strict';

	function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

	var stopThresholdDefault = 0.3;
	var bounceDeceleration = 0.04;
	var bounceAcceleration = 0.11;

	var Impetus = function Impetus(_ref) {
		var _ref$source = _ref.source;
		var sourceEl = _ref$source === undefined ? document : _ref$source;
		var updateCallback = _ref.update;
		var _ref$multiplier = _ref.multiplier;
		var multiplier = _ref$multiplier === undefined ? 1 : _ref$multiplier;
		var _ref$friction = _ref.friction;
		var friction = _ref$friction === undefined ? 0.92 : _ref$friction;
		var initialValues = _ref.initialValues;
		var boundX = _ref.boundX;
		var boundY = _ref.boundY;
		var _ref$bounce = _ref.bounce;
		var bounce = _ref$bounce === undefined ? true : _ref$bounce;

		_classCallCheck(this, Impetus);

		var boundXmin, boundXmax, boundYmin, boundYmax, pointerLastX, pointerLastY, pointerCurrentX, pointerCurrentY, pointerId, decVelX, decVelY;
		var targetX = 0;
		var targetY = 0;
		var stopThreshold = stopThresholdDefault * multiplier;
		var ticking = false;
		var pointerActive = false;
		var paused = false;
		var decelerating = false;
		var trackingPoints = [];

		/**
   * Initialize instance
   */
		(function init() {
			sourceEl = typeof sourceEl === 'string' ? document.querySelector(sourceEl) : sourceEl;
			if (!sourceEl) {
				throw new Error('IMPETUS: source not found.');
			}

			if (!updateCallback) {
				throw new Error('IMPETUS: update function not defined.');
			}

			if (initialValues) {
				if (initialValues[0]) {
					targetX = initialValues[0];
				}
				if (initialValues[1]) {
					targetY = initialValues[1];
				}
				callUpdateCallback();
			}

			// Initialize bound values
			if (boundX) {
				boundXmin = boundX[0];
				boundXmax = boundX[1];
			}
			if (boundY) {
				boundYmin = boundY[0];
				boundYmax = boundY[1];
			}

			sourceEl.addEventListener('touchstart', onDown);
			sourceEl.addEventListener('mousedown', onDown);
		})();

		/**
   * Disable movement processing
   * @public
   */
		this.pause = function () {
			pointerActive = false;
			paused = true;
		};

		/**
   * Enable movement processing
   * @public
   */
		this.resume = function () {
			paused = false;
		};

		/**
   * Update the current x and y values
   * @public
   * @param {Number} x
   * @param {Number} y
   */
		this.setValues = function (x, y) {
			if (typeof x === 'number') {
				targetX = x;
			}
			if (typeof y === 'number') {
				targetY = y;
			}
		};

		/**
   * Update the multiplier value
   * @public
   * @param {Number} val
   */
		this.setMultiplier = function (val) {
			multiplier = val;
			stopThreshold = stopThresholdDefault * multiplier;
		};

		/**
   * Executes the update function
   */
		function callUpdateCallback() {
			updateCallback.call(sourceEl, targetX, targetY);
		}

		/**
   * Creates a custom normalized event object from touch and mouse events
   * @param  {Event} ev
   * @returns {Object} with x, y, and id properties
   */
		function normalizeEvent(ev) {
			if (ev.type === 'touchmove' || ev.type === 'touchstart' || ev.type === 'touchend') {
				var touch = ev.targetTouches[0] || ev.changedTouches[0];
				return {
					x: touch.clientX,
					y: touch.clientY,
					id: touch.identifier
				};
			} else {
				// mouse events
				return {
					x: ev.clientX,
					y: ev.clientY,
					id: null
				};
			}
		}

		/**
   * Initializes movement tracking
   * @param  {Object} ev Normalized event
   */
		function onDown(ev) {
			var event = normalizeEvent(ev);
			if (!pointerActive && !paused) {
				pointerActive = true;
				decelerating = false;
				pointerId = event.id;

				pointerLastX = pointerCurrentX = event.x;
				pointerLastY = pointerCurrentY = event.y;
				trackingPoints = [];
				addTrackingPoint(pointerLastX, pointerLastY);

				document.addEventListener('touchmove', onMove);
				document.addEventListener('touchend', onUp);
				document.addEventListener('touchcancel', stopTracking);
				document.addEventListener('mousemove', onMove);
				document.addEventListener('mouseup', onUp);
			}
		}

		/**
   * Handles move events
   * @param  {Object} ev Normalized event
   */
		function onMove(ev) {
			ev.preventDefault();
			var event = normalizeEvent(ev);

			if (pointerActive && event.id === pointerId) {
				pointerCurrentX = event.x;
				pointerCurrentY = event.y;
				addTrackingPoint(pointerLastX, pointerLastY);
				requestTick();
			}
		}

		/**
   * Handles up/end events
   * @param {Object} ev Normalized event
   */
		function onUp(ev) {
			var event = normalizeEvent(ev);

			if (pointerActive && event.id === pointerId) {
				stopTracking();
			}
		}

		/**
   * Stops movement tracking, starts animation
   */
		function stopTracking() {
			pointerActive = false;
			addTrackingPoint(pointerLastX, pointerLastY);
			startDecelAnim();

			document.removeEventListener('touchmove', onMove);
			document.removeEventListener('touchend', onUp);
			document.removeEventListener('touchcancel', stopTracking);
			document.removeEventListener('mouseup', onUp);
			document.removeEventListener('mousemove', onMove);
		}

		/**
   * Records movement for the last 100ms
   * @param {number} x
   * @param {number} y [description]
   */
		function addTrackingPoint(x, y) {
			var time = Date.now();
			while (trackingPoints.length > 0) {
				if (time - trackingPoints[0].time <= 100) {
					break;
				}
				trackingPoints.shift();
			}

			trackingPoints.push({ x: x, y: y, time: time });
		}

		/**
   * Calculate new values, call update function
   */
		function updateAndRender() {
			var pointerChangeX = pointerCurrentX - pointerLastX;
			var pointerChangeY = pointerCurrentY - pointerLastY;

			targetX += pointerChangeX * multiplier;
			targetY += pointerChangeY * multiplier;

			if (bounce) {
				var diff = checkBounds();
				if (diff.x !== 0) {
					targetX -= pointerChangeX * dragOutOfBoundsMultiplier(diff.x) * multiplier;
				}
				if (diff.y !== 0) {
					targetY -= pointerChangeY * dragOutOfBoundsMultiplier(diff.y) * multiplier;
				}
			} else {
				checkBounds(true);
			}

			callUpdateCallback();

			pointerLastX = pointerCurrentX;
			pointerLastY = pointerCurrentY;
			ticking = false;
		}

		/**
   * Returns a value from around 0.5 to 1, based on distance
   * @param {Number} val
   */
		function dragOutOfBoundsMultiplier(val) {
			return 0.000005 * Math.pow(val, 2) + 0.0001 * val + 0.55;
		}

		/**
   * prevents animating faster than current framerate
   */
		function requestTick() {
			if (!ticking) {
				requestAnimFrame(updateAndRender);
			}
			ticking = true;
		}

		/**
   * Determine position relative to bounds
   * @param {Boolean} restrict Whether to restrict target to bounds
   */
		function checkBounds(restrict) {
			var xDiff = 0;
			var yDiff = 0;

			if (boundXmin !== undefined && targetX < boundXmin) {
				xDiff = boundXmin - targetX;
			} else if (boundXmax !== undefined && targetX > boundXmax) {
				xDiff = boundXmax - targetX;
			}

			if (boundYmin !== undefined && targetY < boundYmin) {
				yDiff = boundYmin - targetY;
			} else if (boundYmax !== undefined && targetY > boundYmax) {
				yDiff = boundYmax - targetY;
			}

			if (restrict) {
				if (xDiff !== 0) {
					targetX = xDiff > 0 ? boundXmin : boundXmax;
				}
				if (yDiff !== 0) {
					targetY = yDiff > 0 ? boundYmin : boundYmax;
				}
			}

			return {
				x: xDiff,
				y: yDiff,
				inBounds: xDiff === 0 && yDiff === 0
			};
		}

		/**
   * Initialize animation of values coming to a stop
   */
		function startDecelAnim() {
			var firstPoint = trackingPoints[0];
			var lastPoint = trackingPoints[trackingPoints.length - 1];

			var xOffset = lastPoint.x - firstPoint.x;
			var yOffset = lastPoint.y - firstPoint.y;
			var timeOffset = lastPoint.time - firstPoint.time;

			var D = timeOffset / 15 / multiplier;

			decVelX = xOffset / D || 0; // prevent NaN
			decVelY = yOffset / D || 0;

			var diff = checkBounds();

			if (Math.abs(decVelX) > 1 || Math.abs(decVelY) > 1 || !diff.inBounds) {
				decelerating = true;
				requestAnimFrame(stepDecelAnim);
			}
		}

		/**
   * Animates values slowing down
   */
		function stepDecelAnim() {
			if (!decelerating) {
				return;
			}

			decVelX *= friction;
			decVelY *= friction;

			targetX += decVelX;
			targetY += decVelY;

			var diff = checkBounds();

			if (Math.abs(decVelX) > stopThreshold || Math.abs(decVelY) > stopThreshold || !diff.inBounds) {

				if (bounce) {
					var reboundAdjust = 2.5;

					if (diff.x !== 0) {
						if (diff.x * decVelX <= 0) {
							decVelX += diff.x * bounceDeceleration;
						} else {
							var adjust = diff.x > 0 ? reboundAdjust : -reboundAdjust;
							decVelX = (diff.x + adjust) * bounceAcceleration;
						}
					}
					if (diff.y !== 0) {
						if (diff.y * decVelY <= 0) {
							decVelY += diff.y * bounceDeceleration;
						} else {
							var adjust = diff.y > 0 ? reboundAdjust : -reboundAdjust;
							decVelY = (diff.y + adjust) * bounceAcceleration;
						}
					}
				} else {
					if (diff.x !== 0) {
						if (diff.x > 0) {
							targetX = boundXmin;
						} else {
							targetX = boundXmax;
						}
						decVelX = 0;
					}
					if (diff.y !== 0) {
						if (diff.y > 0) {
							targetY = boundYmin;
						} else {
							targetY = boundYmax;
						}
						decVelY = 0;
					}
				}

				callUpdateCallback();

				requestAnimFrame(stepDecelAnim);
			} else {
				decelerating = false;
			}
		}
	}

	/**
  * @see http://www.paulirish.com/2011/requestanimationframe-for-smart-animating/
  */
	;

	module.exports = Impetus;
	var requestAnimFrame = (function () {
		return window.requestAnimationFrame || window.webkitRequestAnimationFrame || window.mozRequestAnimationFrame || function (callback) {
			window.setTimeout(callback, 1000 / 60);
		};
	})();
});

},{}],8:[function(require,module,exports){
var rootPosition = { left: 0, top: 0 }

module.exports = mouseEventOffset
function mouseEventOffset (ev, target, out) {
  target = target || ev.currentTarget || ev.srcElement
  if (!Array.isArray(out)) {
    out = [ 0, 0 ]
  }
  var cx = ev.clientX || 0
  var cy = ev.clientY || 0
  var rect = getBoundingClientOffset(target)
  out[0] = cx - rect.left
  out[1] = cy - rect.top
  return out
}

function getBoundingClientOffset (element) {
  if (element === window ||
      element === document ||
      element === document.body) {
    return rootPosition
  } else {
    return element.getBoundingClientRect()
  }
}

},{}],9:[function(require,module,exports){
'use strict'

var toPX = require('to-px')

module.exports = mouseWheelListen

function mouseWheelListen(element, callback, noScroll) {
  if(typeof element === 'function') {
    noScroll = !!callback
    callback = element
    element = window
  }
  var lineHeight = toPX('ex', element)
  var listener = function(ev) {
    if(noScroll) {
      ev.preventDefault()
    }
    var dx = ev.deltaX || 0
    var dy = ev.deltaY || 0
    var dz = ev.deltaZ || 0
    var mode = ev.deltaMode
    var scale = 1
    switch(mode) {
      case 1:
        scale = lineHeight
      break
      case 2:
        scale = window.innerHeight
      break
    }
    dx *= scale
    dy *= scale
    dz *= scale
    if(dx || dy || dz) {
      return callback(dx, dy, dz, ev)
    }
  }
  element.addEventListener('wheel', listener)
  return listener
}

},{"to-px":15}],10:[function(require,module,exports){
/**
 * @module  pan-zoom
 *
 * Events for pan and zoom
 */
'use strict';


const Impetus = require('impetus');
const wheel = require('mouse-wheel');
const touchPinch = require('touch-pinch');
const position = require('touch-position');


module.exports = panzoom;


function panzoom (target, cb) {
	if (!target || !(cb instanceof Function)) return false;


	//enable panning
	let pos = position({
		element: target
	});

	let impetus;

	let lastY = 0, lastX = 0;
	impetus = new Impetus({
		source: target,
		update: (x, y) => {
			let e = {
				type: 'mouse',
				dx: x-lastX, dy: y-lastY, dz: 0,
				x: pos[0], y: pos[1]
			};

			lastX = x;
			lastY = y;

			cb(e);
		},
		multiplier: 1,
		friction: .75
	});


	//enable zooming
	wheel(target, (dx, dy, dz, e) => {
		e.preventDefault();
		cb({
			type: 'mouse',
			dx: 0, dy: 0, dz: dy,
			x: pos[0], y: pos[1]
		});
	});

	//mobile pinch zoom
	let pinch = touchPinch(target);
	let mult = 2;
	let initialCoords;

	pinch.on('start', (curr) => {
		impetus && impetus.pause();

		let [f1, f2] = pinch.fingers;

		initialCoords = [f2.position[0]*.5 + f1.position[0]*.5, f2.position[1]*.5 + f1.position[1]*.5];
	});
	pinch.on('end', () => {
		initialCoords = null;

		impetus && impetus.resume();
	});
	pinch.on('change', (curr, prev) => {
		if (!pinch.pinching || !initialCoords) return;

		cb({
			type: 'touch',
			dx: 0, dy: 0, dz: -(curr - prev)*mult,
			x: initialCoords[0], y: initialCoords[1]
		});
	});
}

},{"impetus":7,"mouse-wheel":9,"touch-pinch":16,"touch-position":17}],11:[function(require,module,exports){
module.exports = function parseUnit(str, out) {
    if (!out)
        out = [ 0, '' ]

    str = String(str)
    var num = parseFloat(str, 10)
    out[0] = num
    out[1] = str.match(/[\d.\-\+]*\s*(.*)/)[1] || ''
    return out
}
},{}],12:[function(require,module,exports){
(function (process){
// Generated by CoffeeScript 1.7.1
(function() {
  var getNanoSeconds, hrtime, loadTime;

  if ((typeof performance !== "undefined" && performance !== null) && performance.now) {
    module.exports = function() {
      return performance.now();
    };
  } else if ((typeof process !== "undefined" && process !== null) && process.hrtime) {
    module.exports = function() {
      return (getNanoSeconds() - loadTime) / 1e6;
    };
    hrtime = process.hrtime;
    getNanoSeconds = function() {
      var hr;
      hr = hrtime();
      return hr[0] * 1e9 + hr[1];
    };
    loadTime = getNanoSeconds();
  } else if (Date.now) {
    module.exports = function() {
      return Date.now() - loadTime;
    };
    loadTime = Date.now();
  } else {
    module.exports = function() {
      return new Date().getTime() - loadTime;
    };
    loadTime = new Date().getTime();
  }

}).call(this);

}).call(this,require('_process'))
},{"_process":19}],13:[function(require,module,exports){
(function (global){
var now = require('performance-now')
  , root = typeof window === 'undefined' ? global : window
  , vendors = ['moz', 'webkit']
  , suffix = 'AnimationFrame'
  , raf = root['request' + suffix]
  , caf = root['cancel' + suffix] || root['cancelRequest' + suffix]

for(var i = 0; !raf && i < vendors.length; i++) {
  raf = root[vendors[i] + 'Request' + suffix]
  caf = root[vendors[i] + 'Cancel' + suffix]
      || root[vendors[i] + 'CancelRequest' + suffix]
}

// Some versions of FF have rAF but not cAF
if(!raf || !caf) {
  var last = 0
    , id = 0
    , queue = []
    , frameDuration = 1000 / 60

  raf = function(callback) {
    if(queue.length === 0) {
      var _now = now()
        , next = Math.max(0, frameDuration - (_now - last))
      last = next + _now
      setTimeout(function() {
        var cp = queue.slice(0)
        // Clear queue here to prevent
        // callbacks from appending listeners
        // to the current frame's queue
        queue.length = 0
        for(var i = 0; i < cp.length; i++) {
          if(!cp[i].cancelled) {
            try{
              cp[i].callback(last)
            } catch(e) {
              setTimeout(function() { throw e }, 0)
            }
          }
        }
      }, Math.round(next))
    }
    queue.push({
      handle: ++id,
      callback: callback,
      cancelled: false
    })
    return id
  }

  caf = function(handle) {
    for(var i = 0; i < queue.length; i++) {
      if(queue[i].handle === handle) {
        queue[i].cancelled = true
      }
    }
  }
}

module.exports = function(fn) {
  // Wrap in a new function to prevent
  // `cancel` potentially being assigned
  // to the native rAF function
  return raf.call(root, fn)
}
module.exports.cancel = function() {
  caf.apply(root, arguments)
}
module.exports.polyfill = function() {
  root.requestAnimationFrame = raf
  root.cancelAnimationFrame = caf
}

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})
},{"performance-now":12}],14:[function(require,module,exports){
(function (global){
module.exports =
  global.performance &&
  global.performance.now ? function now() {
    return performance.now()
  } : Date.now || function now() {
    return +new Date
  }

}).call(this,typeof global !== "undefined" ? global : typeof self !== "undefined" ? self : typeof window !== "undefined" ? window : {})
},{}],15:[function(require,module,exports){
'use strict'

var parseUnit = require('parse-unit')

module.exports = toPX

var PIXELS_PER_INCH = 96

function getPropertyInPX(element, prop) {
  var parts = parseUnit(getComputedStyle(element).getPropertyValue(prop))
  return parts[0] * toPX(parts[1], element)
}

//This brutal hack is needed
function getSizeBrutal(unit, element) {
  var testDIV = document.createElement('div')
  testDIV.style['font-size'] = '128' + unit
  element.appendChild(testDIV)
  var size = getPropertyInPX(testDIV, 'font-size') / 128
  element.removeChild(testDIV)
  return size
}

function toPX(str, element) {
  element = element || document.body
  str = (str || 'px').trim().toLowerCase()
  if(element === window || element === document) {
    element = document.body 
  }
  switch(str) {
    case '%':  //Ambiguous, not sure if we should use width or height
      return element.clientHeight / 100.0
    case 'ch':
    case 'ex':
      return getSizeBrutal(str, element)
    case 'em':
      return getPropertyInPX(element, 'font-size')
    case 'rem':
      return getPropertyInPX(document.body, 'font-size')
    case 'vw':
      return window.innerWidth/100
    case 'vh':
      return window.innerHeight/100
    case 'vmin':
      return Math.min(window.innerWidth, window.innerHeight) / 100
    case 'vmax':
      return Math.max(window.innerWidth, window.innerHeight) / 100
    case 'in':
      return PIXELS_PER_INCH
    case 'cm':
      return PIXELS_PER_INCH / 2.54
    case 'mm':
      return PIXELS_PER_INCH / 25.4
    case 'pt':
      return PIXELS_PER_INCH / 72
    case 'pc':
      return PIXELS_PER_INCH / 6
  }
  return 1
}
},{"parse-unit":11}],16:[function(require,module,exports){
var getDistance = require('gl-vec2/distance')
var EventEmitter = require('events').EventEmitter
var dprop = require('dprop')
var eventOffset = require('mouse-event-offset')

module.exports = touchPinch
function touchPinch (target) {
  target = target || window

  var emitter = new EventEmitter()
  var fingers = [ null, null ]
  var activeCount = 0

  var lastDistance = 0
  var ended = false
  var enabled = false

  // some read-only values
  Object.defineProperties(emitter, {
    pinching: dprop(function () {
      return activeCount === 2
    }),

    fingers: dprop(function () {
      return fingers
    })
  })

  enable()
  emitter.enable = enable
  emitter.disable = disable
  emitter.indexOfTouch = indexOfTouch
  return emitter

  function indexOfTouch (touch) {
    var id = touch.identifier
    for (var i = 0; i < fingers.length; i++) {
      if (fingers[i] &&
        fingers[i].touch &&
        fingers[i].touch.identifier === id) {
        return i
      }
    }
    return -1
  }

  function enable () {
    if (enabled) return
    enabled = true
    target.addEventListener('touchstart', onTouchStart, false)
    target.addEventListener('touchmove', onTouchMove, false)
    target.addEventListener('touchend', onTouchRemoved, false)
    target.addEventListener('touchcancel', onTouchRemoved, false)
  }

  function disable () {
    if (!enabled) return
    enabled = false
    target.removeEventListener('touchstart', onTouchStart, false)
    target.removeEventListener('touchmove', onTouchMove, false)
    target.removeEventListener('touchend', onTouchRemoved, false)
    target.removeEventListener('touchcancel', onTouchRemoved, false)
  }

  function onTouchStart (ev) {
    for (var i = 0; i < ev.changedTouches.length; i++) {
      var newTouch = ev.changedTouches[i]
      var id = newTouch.identifier
      var idx = indexOfTouch(id)

      if (idx === -1 && activeCount < 2) {
        var first = activeCount === 0

        // newest and previous finger (previous may be undefined)
        var newIndex = fingers[0] ? 1 : 0
        var oldIndex = fingers[0] ? 0 : 1
        var newFinger = new Finger()

        // add to stack
        fingers[newIndex] = newFinger
        activeCount++

        // update touch event & position
        newFinger.touch = newTouch
        eventOffset(newTouch, target, newFinger.position)

        var oldTouch = fingers[oldIndex] ? fingers[oldIndex].touch : undefined
        emitter.emit('place', newTouch, oldTouch)

        if (!first) {
          var initialDistance = computeDistance()
          ended = false
          emitter.emit('start', initialDistance)
          lastDistance = initialDistance
        }
      }
    }
  }

  function onTouchMove (ev) {
    var changed = false
    for (var i = 0; i < ev.changedTouches.length; i++) {
      var movedTouch = ev.changedTouches[i]
      var idx = indexOfTouch(movedTouch)
      if (idx !== -1) {
        changed = true
        fingers[idx].touch = movedTouch // avoid caching touches
        eventOffset(movedTouch, target, fingers[idx].position)
      }
    }

    if (activeCount === 2 && changed) {
      var currentDistance = computeDistance()
      emitter.emit('change', currentDistance, lastDistance)
      lastDistance = currentDistance
    }
  }

  function onTouchRemoved (ev) {
    for (var i = 0; i < ev.changedTouches.length; i++) {
      var removed = ev.changedTouches[i]
      var idx = indexOfTouch(removed)

      if (idx !== -1) {
        fingers[idx] = null
        activeCount--
        var otherIdx = idx === 0 ? 1 : 0
        var otherTouch = fingers[otherIdx] ? fingers[otherIdx].touch : undefined
        emitter.emit('lift', removed, otherTouch)
      }
    }

    if (!ended && activeCount !== 2) {
      ended = true
      emitter.emit('end')
    }
  }

  function computeDistance () {
    if (activeCount < 2) return 0
    return getDistance(fingers[0].position, fingers[1].position)
  }
}

function Finger () {
  this.position = [0, 0]
  this.touch = null
}

},{"dprop":2,"events":18,"gl-vec2/distance":6,"mouse-event-offset":8}],17:[function(require,module,exports){
var offset = require('mouse-event-offset');
var EventEmitter = require('events').EventEmitter;

function attach (opt) {
  opt = opt || {};
  var element = opt.element || window;

  var emitter = new EventEmitter();

  var position = opt.position || [0, 0];
  if (opt.touchstart !== false) {
    element.addEventListener('mousedown', update, false);
    element.addEventListener('touchstart', updateTouch, false);
  }

  element.addEventListener('mousemove', update, false);
  element.addEventListener('touchmove', updateTouch, false);

  emitter.position = position;
  emitter.dispose = dispose;
  return emitter;

  function updateTouch (ev) {
    var touch = ev.targetTouches[0];
    update(touch);
  }

  function update (ev) {
    offset(ev, element, position);
    emitter.emit('move', ev);
  }

  function dispose () {
    element.removeEventListener('mousemove', update, false);
    element.removeEventListener('mousedown', update, false);
    element.removeEventListener('touchmove', updateTouch, false);
    element.removeEventListener('touchstart', updateTouch, false);
  }
}

module.exports = function (opt) {
  return attach(opt).position;
};

module.exports.emitter = function (opt) {
  return attach(opt);
};

},{"events":18,"mouse-event-offset":8}],18:[function(require,module,exports){
// Copyright Joyent, Inc. and other Node contributors.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the
// following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
// NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
// USE OR OTHER DEALINGS IN THE SOFTWARE.

function EventEmitter() {
  this._events = this._events || {};
  this._maxListeners = this._maxListeners || undefined;
}
module.exports = EventEmitter;

// Backwards-compat with node 0.10.x
EventEmitter.EventEmitter = EventEmitter;

EventEmitter.prototype._events = undefined;
EventEmitter.prototype._maxListeners = undefined;

// By default EventEmitters will print a warning if more than 10 listeners are
// added to it. This is a useful default which helps finding memory leaks.
EventEmitter.defaultMaxListeners = 10;

// Obviously not all Emitters should be limited to 10. This function allows
// that to be increased. Set to zero for unlimited.
EventEmitter.prototype.setMaxListeners = function(n) {
  if (!isNumber(n) || n < 0 || isNaN(n))
    throw TypeError('n must be a positive number');
  this._maxListeners = n;
  return this;
};

EventEmitter.prototype.emit = function(type) {
  var er, handler, len, args, i, listeners;

  if (!this._events)
    this._events = {};

  // If there is no 'error' event listener then throw.
  if (type === 'error') {
    if (!this._events.error ||
        (isObject(this._events.error) && !this._events.error.length)) {
      er = arguments[1];
      if (er instanceof Error) {
        throw er; // Unhandled 'error' event
      } else {
        // At least give some kind of context to the user
        var err = new Error('Uncaught, unspecified "error" event. (' + er + ')');
        err.context = er;
        throw err;
      }
    }
  }

  handler = this._events[type];

  if (isUndefined(handler))
    return false;

  if (isFunction(handler)) {
    switch (arguments.length) {
      // fast cases
      case 1:
        handler.call(this);
        break;
      case 2:
        handler.call(this, arguments[1]);
        break;
      case 3:
        handler.call(this, arguments[1], arguments[2]);
        break;
      // slower
      default:
        args = Array.prototype.slice.call(arguments, 1);
        handler.apply(this, args);
    }
  } else if (isObject(handler)) {
    args = Array.prototype.slice.call(arguments, 1);
    listeners = handler.slice();
    len = listeners.length;
    for (i = 0; i < len; i++)
      listeners[i].apply(this, args);
  }

  return true;
};

EventEmitter.prototype.addListener = function(type, listener) {
  var m;

  if (!isFunction(listener))
    throw TypeError('listener must be a function');

  if (!this._events)
    this._events = {};

  // To avoid recursion in the case that type === "newListener"! Before
  // adding it to the listeners, first emit "newListener".
  if (this._events.newListener)
    this.emit('newListener', type,
              isFunction(listener.listener) ?
              listener.listener : listener);

  if (!this._events[type])
    // Optimize the case of one listener. Don't need the extra array object.
    this._events[type] = listener;
  else if (isObject(this._events[type]))
    // If we've already got an array, just append.
    this._events[type].push(listener);
  else
    // Adding the second element, need to change to array.
    this._events[type] = [this._events[type], listener];

  // Check for listener leak
  if (isObject(this._events[type]) && !this._events[type].warned) {
    if (!isUndefined(this._maxListeners)) {
      m = this._maxListeners;
    } else {
      m = EventEmitter.defaultMaxListeners;
    }

    if (m && m > 0 && this._events[type].length > m) {
      this._events[type].warned = true;
      console.error('(node) warning: possible EventEmitter memory ' +
                    'leak detected. %d listeners added. ' +
                    'Use emitter.setMaxListeners() to increase limit.',
                    this._events[type].length);
      if (typeof console.trace === 'function') {
        // not supported in IE 10
        console.trace();
      }
    }
  }

  return this;
};

EventEmitter.prototype.on = EventEmitter.prototype.addListener;

EventEmitter.prototype.once = function(type, listener) {
  if (!isFunction(listener))
    throw TypeError('listener must be a function');

  var fired = false;

  function g() {
    this.removeListener(type, g);

    if (!fired) {
      fired = true;
      listener.apply(this, arguments);
    }
  }

  g.listener = listener;
  this.on(type, g);

  return this;
};

// emits a 'removeListener' event iff the listener was removed
EventEmitter.prototype.removeListener = function(type, listener) {
  var list, position, length, i;

  if (!isFunction(listener))
    throw TypeError('listener must be a function');

  if (!this._events || !this._events[type])
    return this;

  list = this._events[type];
  length = list.length;
  position = -1;

  if (list === listener ||
      (isFunction(list.listener) && list.listener === listener)) {
    delete this._events[type];
    if (this._events.removeListener)
      this.emit('removeListener', type, listener);

  } else if (isObject(list)) {
    for (i = length; i-- > 0;) {
      if (list[i] === listener ||
          (list[i].listener && list[i].listener === listener)) {
        position = i;
        break;
      }
    }

    if (position < 0)
      return this;

    if (list.length === 1) {
      list.length = 0;
      delete this._events[type];
    } else {
      list.splice(position, 1);
    }

    if (this._events.removeListener)
      this.emit('removeListener', type, listener);
  }

  return this;
};

EventEmitter.prototype.removeAllListeners = function(type) {
  var key, listeners;

  if (!this._events)
    return this;

  // not listening for removeListener, no need to emit
  if (!this._events.removeListener) {
    if (arguments.length === 0)
      this._events = {};
    else if (this._events[type])
      delete this._events[type];
    return this;
  }

  // emit removeListener for all listeners on all events
  if (arguments.length === 0) {
    for (key in this._events) {
      if (key === 'removeListener') continue;
      this.removeAllListeners(key);
    }
    this.removeAllListeners('removeListener');
    this._events = {};
    return this;
  }

  listeners = this._events[type];

  if (isFunction(listeners)) {
    this.removeListener(type, listeners);
  } else if (listeners) {
    // LIFO order
    while (listeners.length)
      this.removeListener(type, listeners[listeners.length - 1]);
  }
  delete this._events[type];

  return this;
};

EventEmitter.prototype.listeners = function(type) {
  var ret;
  if (!this._events || !this._events[type])
    ret = [];
  else if (isFunction(this._events[type]))
    ret = [this._events[type]];
  else
    ret = this._events[type].slice();
  return ret;
};

EventEmitter.prototype.listenerCount = function(type) {
  if (this._events) {
    var evlistener = this._events[type];

    if (isFunction(evlistener))
      return 1;
    else if (evlistener)
      return evlistener.length;
  }
  return 0;
};

EventEmitter.listenerCount = function(emitter, type) {
  return emitter.listenerCount(type);
};

function isFunction(arg) {
  return typeof arg === 'function';
}

function isNumber(arg) {
  return typeof arg === 'number';
}

function isObject(arg) {
  return typeof arg === 'object' && arg !== null;
}

function isUndefined(arg) {
  return arg === void 0;
}

},{}],19:[function(require,module,exports){
// shim for using process in browser
var process = module.exports = {};

// cached from whatever global is present so that test runners that stub it
// don't break things.  But we need to wrap it in a try catch in case it is
// wrapped in strict mode code which doesn't define any globals.  It's inside a
// function because try/catches deoptimize in certain engines.

var cachedSetTimeout;
var cachedClearTimeout;

function defaultSetTimout() {
    throw new Error('setTimeout has not been defined');
}
function defaultClearTimeout () {
    throw new Error('clearTimeout has not been defined');
}
(function () {
    try {
        if (typeof setTimeout === 'function') {
            cachedSetTimeout = setTimeout;
        } else {
            cachedSetTimeout = defaultSetTimout;
        }
    } catch (e) {
        cachedSetTimeout = defaultSetTimout;
    }
    try {
        if (typeof clearTimeout === 'function') {
            cachedClearTimeout = clearTimeout;
        } else {
            cachedClearTimeout = defaultClearTimeout;
        }
    } catch (e) {
        cachedClearTimeout = defaultClearTimeout;
    }
} ())
function runTimeout(fun) {
    if (cachedSetTimeout === setTimeout) {
        //normal enviroments in sane situations
        return setTimeout(fun, 0);
    }
    // if setTimeout wasn't available but was latter defined
    if ((cachedSetTimeout === defaultSetTimout || !cachedSetTimeout) && setTimeout) {
        cachedSetTimeout = setTimeout;
        return setTimeout(fun, 0);
    }
    try {
        // when when somebody has screwed with setTimeout but no I.E. maddness
        return cachedSetTimeout(fun, 0);
    } catch(e){
        try {
            // When we are in I.E. but the script has been evaled so I.E. doesn't trust the global object when called normally
            return cachedSetTimeout.call(null, fun, 0);
        } catch(e){
            // same as above but when it's a version of I.E. that must have the global object for 'this', hopfully our context correct otherwise it will throw a global error
            return cachedSetTimeout.call(this, fun, 0);
        }
    }


}
function runClearTimeout(marker) {
    if (cachedClearTimeout === clearTimeout) {
        //normal enviroments in sane situations
        return clearTimeout(marker);
    }
    // if clearTimeout wasn't available but was latter defined
    if ((cachedClearTimeout === defaultClearTimeout || !cachedClearTimeout) && clearTimeout) {
        cachedClearTimeout = clearTimeout;
        return clearTimeout(marker);
    }
    try {
        // when when somebody has screwed with setTimeout but no I.E. maddness
        return cachedClearTimeout(marker);
    } catch (e){
        try {
            // When we are in I.E. but the script has been evaled so I.E. doesn't  trust the global object when called normally
            return cachedClearTimeout.call(null, marker);
        } catch (e){
            // same as above but when it's a version of I.E. that must have the global object for 'this', hopfully our context correct otherwise it will throw a global error.
            // Some versions of I.E. have different rules for clearTimeout vs setTimeout
            return cachedClearTimeout.call(this, marker);
        }
    }



}
var queue = [];
var draining = false;
var currentQueue;
var queueIndex = -1;

function cleanUpNextTick() {
    if (!draining || !currentQueue) {
        return;
    }
    draining = false;
    if (currentQueue.length) {
        queue = currentQueue.concat(queue);
    } else {
        queueIndex = -1;
    }
    if (queue.length) {
        drainQueue();
    }
}

function drainQueue() {
    if (draining) {
        return;
    }
    var timeout = runTimeout(cleanUpNextTick);
    draining = true;

    var len = queue.length;
    while(len) {
        currentQueue = queue;
        queue = [];
        while (++queueIndex < len) {
            if (currentQueue) {
                currentQueue[queueIndex].run();
            }
        }
        queueIndex = -1;
        len = queue.length;
    }
    currentQueue = null;
    draining = false;
    runClearTimeout(timeout);
}

process.nextTick = function (fun) {
    var args = new Array(arguments.length - 1);
    if (arguments.length > 1) {
        for (var i = 1; i < arguments.length; i++) {
            args[i - 1] = arguments[i];
        }
    }
    queue.push(new Item(fun, args));
    if (queue.length === 1 && !draining) {
        runTimeout(drainQueue);
    }
};

// v8 likes predictible objects
function Item(fun, array) {
    this.fun = fun;
    this.array = array;
}
Item.prototype.run = function () {
    this.fun.apply(null, this.array);
};
process.title = 'browser';
process.browser = true;
process.env = {};
process.argv = [];
process.version = ''; // empty string to avoid regexp issues
process.versions = {};

function noop() {}

process.on = noop;
process.addListener = noop;
process.once = noop;
process.off = noop;
process.removeListener = noop;
process.removeAllListeners = noop;
process.emit = noop;

process.binding = function (name) {
    throw new Error('process.binding is not supported');
};

process.cwd = function () { return '/' };
process.chdir = function (dir) {
    throw new Error('process.chdir is not supported');
};
process.umask = function() { return 0; };

},{}]},{},[1]);
