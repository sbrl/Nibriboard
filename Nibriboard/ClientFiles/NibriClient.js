(function e(t,n,r){function s(o,u){if(!n[o]){if(!t[o]){var a=typeof require=="function"&&require;if(!u&&a)return a(o,!0);if(i)return i(o,!0);var f=new Error("Cannot find module '"+o+"'");throw f.code="MODULE_NOT_FOUND",f}var l=n[o]={exports:{}};t[o][0].call(l.exports,function(e){var n=t[o][1][e];return s(n?n:e)},l,l.exports,e,t,n,r)}return n[o].exports}var i=typeof require=="function"&&require;for(var o=0;o<r.length;o++)s(r[o]);return s})({1:[function(require,module,exports){
'use strict';

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

function get(u){return new Promise(function(r,t,a){a=new XMLHttpRequest();a.onload=function(b,c){b=a.status;c=a.response;if(b>199&&b<300){r(c);}else{t(c);}};a.open("GET",u,true);a.send(null);})}

// npm modules
window.EventEmitter = require("event-emitter-es6");
window.FaviconNotification = require("favicon-notification");

// Our files
class BoardWindow extends EventEmitter
{
	constructor(canvas)
	{
		super(); // Run the parent constructor
		
		this.canvas = canvas;
		this.context = canvas.getContext("2d");
		FaviconNotification.init({
			color: '#ff6333'
		});
		FaviconNotification.add();
		
		get("/Settings.json").then(JSON.parse).then((function(settings) {
			console.info("[setup]", "Obtained settings from server:", settings);
			this.settings = settings;
			this.setup();
		}).bind(this), function(errorMessage) {
			console.error(`Error: Failed to fetch settings from server! Response: ${errorMessage}`);
		});
		
		this.trackWindowSize();
	}
	
	setup() {
		this.rippleLink = new RippleLink(this.settings.WebsocketUri, this);
	}
	
	nextFrame()
	{
		this.update();
		this.render(this.canvas, this.context);
		requestAnimationFrame(this.nextFrame.bind(this));
	}
	
	update()
	{
		
	}
	
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
}

window.addEventListener("load", function (event) {
	var canvas = document.getElementById("canvas-main"),
		boardWindow = new BoardWindow(canvas);
	boardWindow.nextFrame();
	window.boardWindow = boardWindow;
});

},{"event-emitter-es6":2,"favicon-notification":3}],2:[function(require,module,exports){
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

},{}],3:[function(require,module,exports){
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

},{}]},{},[1]);
