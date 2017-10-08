/*
* Kendo UI v2015.1.408 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI commercial licenses may be obtained at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
* If you do not own a commercial license, this file shall be governed by the trial license terms.
*/
(function(f, define){
    define([ "./kendo.core" ], f);
})(function(){

(function ($, undefined) {
    var proxy = $.proxy;
    var NS = ".kendoResponsivePanel";
    var OPEN = "open";
    var CLOSE = "close";
    var ACTIVATE_EVENTS = "click" + NS +" touchstart" + NS;
    var Widget = kendo.ui.Widget;
    var ResponsivePanel = Widget.extend({
        init: function(element, options) {
            Widget.fn.init.call(this, element, options);

            this._toggleHandler = proxy(this._toggleButtonClick, this);
            this._closeHandler = proxy(this._close, this);

            $(document.documentElement).on(ACTIVATE_EVENTS, this.options.toggleButton, this._toggleHandler);

            this._registerBreakpoint();

            this.element
                .addClass("k-rpanel k-rpanel-" + this.options.orientation);

            this._resizeHandler = proxy(this.resize, this);
            $(window).on("resize" + NS, this._resizeHandler);
        },
        _mediaQuery:
            "@media (max-width: #= breakpoint-1 #px) {" +
                ".k-rpanel-animate.k-rpanel-left," +
                ".k-rpanel-animate.k-rpanel-right {" +
                    "-webkit-transition: -webkit-transform .2s ease-out;" +
                    "-ms-transition: -ms-transform .2s ease-out;" +
                    "transition: transform .2s ease-out;" +
                "} " +
                ".k-rpanel-animate.k-rpanel-top {" +
                    "-webkit-transition: max-height .2s linear;" +
                    "-ms-transition: max-height .2s linear;" +
                    "transition: max-height .2s linear;" +
                "}" +
            "} " +
            "@media (min-width: #= breakpoint #px) {" +
                "#= toggleButton # { display: none; } " +
                ".k-rpanel-left { float: left; } " +
                ".k-rpanel-right { float: right; } " +
                ".k-rpanel-left, .k-rpanel-right {" +
                    "position: relative;" +
                    "-webkit-transform: translateX(0) translateZ(0);" +
                    "-ms-transform: translateX(0) translateZ(0);" +
                    "transform: translateX(0) translateZ(0);" +
                "} " +
                ".k-rpanel-top { max-height: none; }" +
            "}",
        _registerBreakpoint: function() {
            var options = this.options;

            this._registerStyle(kendo.template(this._mediaQuery)({
                breakpoint: options.breakpoint,
                toggleButton: options.toggleButton
            }));
        },
        _registerStyle: function(cssText) {
            var head = $("head,body")[0];
            var style = document.createElement('style');

            if (style.styleSheet){
                style.styleSheet.cssText = cssText;
            } else {
                style.appendChild(document.createTextNode(cssText));
            }

            head.appendChild(style);
        },
        options: {
            name: "ResponsivePanel",
            orientation: "left",
            toggleButton: ".k-rpanel-toggle",
            breakpoint: 640,
            autoClose: true
        },
        events: [
            OPEN,
            CLOSE
        ],
        _resize: function() {
            this.element.removeClass("k-rpanel-animate");
        },
        _toggleButtonClick: function(e) {
            e.preventDefault();

            if (this.element.hasClass("k-rpanel-expanded")) {
                this.close();
            } else {
                this.open();
            }
        },
        open: function() {
            if (!this.trigger(OPEN)) {
                this.element.addClass("k-rpanel-animate k-rpanel-expanded");

                if (this.options.autoClose) {
                    $(document.documentElement).on(ACTIVATE_EVENTS, this._closeHandler);
                }
            }
        },
        close: function() {
            if (!this.trigger(CLOSE)) {
                this.element.addClass("k-rpanel-animate").removeClass("k-rpanel-expanded");

                $(document.documentElement).off(ACTIVATE_EVENTS, this._closeHandler);
            }
        },
        _close: function(e) {
            var prevented = e.isDefaultPrevented();
            var container = $(e.target).closest(this.options.toggleButton + ",.k-rpanel");

            if (!container.length && !prevented) {
                this.close();
            }
        },
        destroy: function() {
            Widget.fn.destroy.call(this);

            $(window).off("resize" + NS, this._resizeHandler);
            $(document.documentElement).off(ACTIVATE_EVENTS, this._closeHandler);
        }
    });

    kendo.ui.plugin(ResponsivePanel);
})(window.kendo.jQuery);

return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });