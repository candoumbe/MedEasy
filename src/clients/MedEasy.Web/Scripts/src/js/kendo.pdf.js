/*
* Kendo UI v2015.1.408 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI commercial licenses may be obtained at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
* If you do not own a commercial license, this file shall be governed by the trial license terms.
*/
(function(f, define){
    define([ "./kendo.core", "./kendo.color", "./kendo.drawing" ], f);
})(function(){

(function(window, parseFloat, undefined){

    "use strict";

    // WARNING: removing the following jshint declaration and turning
    // == into === to make JSHint happy will break functionality.
    /* jshint eqnull:true */
    /* jshint -W069 */
    /* jshint loopfunc:true */
    /* jshint newcap:false */
    /* global VBArray */

    var kendo = window.kendo;

    // XXX: remove this junk (assume `true`) when we no longer have to support IE < 10
    var HAS_TYPED_ARRAYS = !!window.Uint8Array;

    var NL = "\n";

    var RESOURCE_COUNTER = 0;

    var BASE64 = (function(){
        var keyStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
        return {
            decode: function(str) {
                var input = str.replace(/[^A-Za-z0-9\+\/\=]/g, ""), i = 0, n = input.length, output = [];

                while (i < n) {
                    var enc1 = keyStr.indexOf(input.charAt(i++));
                    var enc2 = keyStr.indexOf(input.charAt(i++));
                    var enc3 = keyStr.indexOf(input.charAt(i++));
                    var enc4 = keyStr.indexOf(input.charAt(i++));

                    var chr1 = (enc1 << 2) | (enc2 >>> 4);
                    var chr2 = ((enc2 & 15) << 4) | (enc3 >>> 2);
                    var chr3 = ((enc3 & 3) << 6) | enc4;

                    output.push(chr1);
                    if (enc3 != 64) {
                        output.push(chr2);
                    }
                    if (enc4 != 64) {
                        output.push(chr3);
                    }
                }

                return output;
            },
            encode: function(bytes) {
                var i = 0, n = bytes.length;
                var output = "";

                while (i < n) {
                    var chr1 = bytes[i++];
                    var chr2 = bytes[i++];
                    var chr3 = bytes[i++];

                    var enc1 = chr1 >>> 2;
                    var enc2 = ((chr1 & 3) << 4) | (chr2 >>> 4);
                    var enc3 = ((chr2 & 15) << 2) | (chr3 >>> 6);
                    var enc4 = chr3 & 63;

                    if (i - n == 2) {
                        enc3 = enc4 = 64;
                    } else if (i - n == 1) {
                        enc4 = 64;
                    }

                    output += keyStr.charAt(enc1) + keyStr.charAt(enc2) + keyStr.charAt(enc3) + keyStr.charAt(enc4);
                }
                return output;
            }
        };
    }());

    var PAPER_SIZE = {
        a0        : [ 2383.94 , 3370.39 ],
        a1        : [ 1683.78 , 2383.94 ],
        a2        : [ 1190.55 , 1683.78 ],
        a3        : [ 841.89  , 1190.55 ],
        a4        : [ 595.28  , 841.89  ],
        a5        : [ 419.53  , 595.28  ],
        a6        : [ 297.64  , 419.53  ],
        a7        : [ 209.76  , 297.64  ],
        a8        : [ 147.40  , 209.76  ],
        a9        : [ 104.88  , 147.40  ],
        a10       : [ 73.70   , 104.88  ],
        b0        : [ 2834.65 , 4008.19 ],
        b1        : [ 2004.09 , 2834.65 ],
        b2        : [ 1417.32 , 2004.09 ],
        b3        : [ 1000.63 , 1417.32 ],
        b4        : [ 708.66  , 1000.63 ],
        b5        : [ 498.90  , 708.66  ],
        b6        : [ 354.33  , 498.90  ],
        b7        : [ 249.45  , 354.33  ],
        b8        : [ 175.75  , 249.45  ],
        b9        : [ 124.72  , 175.75  ],
        b10       : [ 87.87   , 124.72  ],
        c0        : [ 2599.37 , 3676.54 ],
        c1        : [ 1836.85 , 2599.37 ],
        c2        : [ 1298.27 , 1836.85 ],
        c3        : [ 918.43  , 1298.27 ],
        c4        : [ 649.13  , 918.43  ],
        c5        : [ 459.21  , 649.13  ],
        c6        : [ 323.15  , 459.21  ],
        c7        : [ 229.61  , 323.15  ],
        c8        : [ 161.57  , 229.61  ],
        c9        : [ 113.39  , 161.57  ],
        c10       : [ 79.37   , 113.39  ],
        executive : [ 521.86  , 756.00  ],
        folio     : [ 612.00  , 936.00  ],
        legal     : [ 612.00  , 1008.00 ],
        letter    : [ 612.00  , 792.00  ],
        tabloid   : [ 792.00  , 1224.00 ]
    };

    function makeOutput() {
        var indentLevel = 0, output = BinaryStream();
        function out() {
            for (var i = 0; i < arguments.length; ++i) {
                var x = arguments[i];
                if (x === undefined) {
                    throw new Error("Cannot output undefined to PDF");
                }
                else if (x instanceof PDFValue) {
                    x.beforeRender(out);
                    x.render(out);
                }
                else if (isArray(x)) {
                    renderArray(x, out);
                }
                else if (isDate(x)) {
                    renderDate(x, out);
                }
                else if (typeof x == "number") {
                    if (isNaN(x)) {
                        throw new Error("Cannot output NaN to PDF");
                    }
                    // make sure it doesn't end up in exponent notation
                    var num = x.toFixed(7);
                    if (num.indexOf(".") >= 0) {
                        num = num.replace(/\.?0+$/, "");
                    }
                    if (num == "-0") {
                        num = "0";
                    }
                    output.writeString(num);
                }
                else if (/string|boolean/.test(typeof x)) {
                    output.writeString(x+"");
                }
                else if (typeof x.get == "function") {
                    output.write(x.get());
                }
                else if (typeof x == "object") {
                    if (!x) {
                        output.writeString("null");
                    } else {
                        out(new PDFDictionary(x));
                    }
                }
            }
        }
        out.writeData = function(data) {
            output.write(data);
        };
        out.withIndent = function(f) {
            ++indentLevel;
            f(out);
            --indentLevel;
        };
        out.indent = function() {
            out(NL, pad("", indentLevel * 2, "  "));
            out.apply(null, arguments);
        };
        out.offset = function() {
            return output.offset();
        };
        out.toString = function() {
            throw new Error("FIX CALLER");
        };
        out.get = function() {
            return output.get();
        };
        out.stream = function() {
            return output;
        };
        return out;
    }

    function wrapObject(value, id) {
        var beforeRender = value.beforeRender;
        var renderValue = value.render;

        value.beforeRender = function(){};

        value.render = function(out) {
            out(id, " 0 R");
        };

        value.renderFull = function(out) {
            value._offset = out.offset();
            out(id, " 0 obj ");
            beforeRender.call(value, out);
            renderValue.call(value, out);
            out(" endobj");
        };
    }

    function getPaperOptions(getOption) {
        var paperSize = getOption("paperSize", PAPER_SIZE.a4);
        if (!paperSize) {
            return {};
        }
        if (typeof paperSize == "string") {
            paperSize = PAPER_SIZE[paperSize.toLowerCase()];
            if (paperSize == null) {
                throw new Error("Unknown paper size");
            }
        }

        paperSize[0] = unitsToPoints(paperSize[0]);
        paperSize[1] = unitsToPoints(paperSize[1]);

        if (getOption("landscape", false)) {
            paperSize = [
                Math.max(paperSize[0], paperSize[1]),
                Math.min(paperSize[0], paperSize[1])
            ];
        }

        var margin = getOption("margin");
        if (margin) {
            if (typeof margin == "string" || typeof margin == "number") {
                margin = unitsToPoints(margin, 0);
                margin = { left: margin, top: margin, right: margin, bottom: margin };
            } else {
                margin = {
                    left   : unitsToPoints(margin.left, 0),
                    top    : unitsToPoints(margin.top, 0),
                    right  : unitsToPoints(margin.right, 0),
                    bottom : unitsToPoints(margin.bottom, 0)
                };
            }
            if (getOption("addMargin")) {
                paperSize[0] += margin.left + margin.right;
                paperSize[1] += margin.top + margin.bottom;
            }
        }
        return { paperSize: paperSize, margin: margin };
    }

    function PDFDocument(options) {
        var self = this;
        var out = makeOutput();
        var objcount = 0;
        var objects = [];

        function getOption(name, defval) {
            return (options && options[name] != null) ? options[name] : defval;
        }

        self.getOption = getOption;

        self.attach = function(value) {
            if (objects.indexOf(value) < 0) {
                wrapObject(value, ++objcount);
                objects.push(value);
            }
            return value;
        };

        self.pages = [];

        self.FONTS = {};
        self.IMAGES = {};
        self.GRAD_COL_FUNCTIONS = {}; // cache for color gradient functions
        self.GRAD_OPC_FUNCTIONS = {}; // cache for opacity gradient functions
        self.GRAD_COL = {};     // cache for whole color gradient objects
        self.GRAD_OPC = {};     // cache for whole opacity gradient objects

        var catalog = self.attach(new PDFCatalog());
        var pageTree = self.attach(new PDFPageTree());
        catalog.setPages(pageTree);

        self.addPage = function(options) {
            var paperOptions  = getPaperOptions(function(name, defval){
                return (options && options[name] != null) ? options[name] : defval;
            });
            var paperSize     = paperOptions.paperSize;
            var margin        = paperOptions.margin;
            var contentWidth  = paperSize[0];
            var contentHeight = paperSize[1];
            if (margin) {
                contentWidth -= margin.left + margin.right;
                contentHeight -= margin.top + margin.bottom;
            }
            var content = new PDFStream(makeOutput(), null, true);
            var props = {
                Contents : self.attach(content),
                Parent   : pageTree,
                MediaBox : [ 0, 0, paperSize[0], paperSize[1] ]
            };
            var page = new PDFPage(self, props);
            page._content = content;
            pageTree.addPage(self.attach(page));

            // canvas-like coord. system.  (0,0) is upper-left.
            // text must be vertically mirorred before drawing.
            page.transform(1, 0, 0, -1, 0, paperSize[1]);

            if (margin) {
                page.translate(margin.left, margin.top);
                // XXX: clip to right/bottom margin.  Make this optional?
                page.rect(0, 0, contentWidth, contentHeight);
                page.clip();
            }

            self.pages.push(page);
            return page;
        };

        self.render = function() {
            var i;
            /// file header
            out("%PDF-1.4", NL, "%\xc2\xc1\xda\xcf\xce", NL, NL);

            /// file body
            for (i = 0; i < objects.length; ++i) {
                objects[i].renderFull(out);
                out(NL, NL);
            }

            /// cross-reference table
            var xrefOffset = out.offset();
            out("xref", NL, 0, " ", objects.length + 1, NL);
            out("0000000000 65535 f ", NL);
            for (i = 0; i < objects.length; ++i) {
                out(zeropad(objects[i]._offset, 10), " 00000 n ", NL);
            }
            out(NL);

            /// trailer
            out("trailer", NL);
            out(new PDFDictionary({
                Size: objects.length + 1,
                Root: catalog,
                Info: new PDFDictionary({
                    Producer     : new PDFString(getOption("producer", "Kendo UI PDF Generator")),
                    Title        : new PDFString(getOption("title", "")),
                    Author       : new PDFString(getOption("author", "")),
                    Subject      : new PDFString(getOption("subject", "")),
                    Keywords     : new PDFString(getOption("keywords", "")),
                    Creator      : new PDFString(getOption("creator", "Kendo UI PDF Generator")),
                    CreationDate : getOption("date", new Date())
                })
            }), NL, NL);

            /// end
            out("startxref", NL, xrefOffset, NL);
            out("%%EOF", NL);

            return out.stream().offset(0);
        };
    }

    var FONT_CACHE = {
        "Times-Roman"           : true,
        "Times-Bold"            : true,
        "Times-Italic"          : true,
        "Times-BoldItalic"      : true,
        "Helvetica"             : true,
        "Helvetica-Bold"        : true,
        "Helvetica-Oblique"     : true,
        "Helvetica-BoldOblique" : true,
        "Courier"               : true,
        "Courier-Bold"          : true,
        "Courier-Oblique"       : true,
        "Courier-BoldOblique"   : true,
        "Symbol"                : true,
        "ZapfDingbats"          : true
    };

    function loadBinary(url, cont) {
        function error() {
            if (window.console) {
                if (window.console.error) {
                    window.console.error("Cannot load URL: %s", url);
                } else {
                    window.console.log("Cannot load URL: %s", url);
                }
            }
            cont(null);
        }
        var req = new XMLHttpRequest();
        req.open('GET', url, true);
        if (HAS_TYPED_ARRAYS) {
            req.responseType = "arraybuffer";
        }
        req.onload = function() {
            if (req.status == 200 || req.status == 304) {
                if (HAS_TYPED_ARRAYS) {
                    cont(new Uint8Array(req.response));
                } else {
                    cont(new VBArray(req.responseBody).toArray()); // IE9 only
                }
            } else {
                error();
            }
        };
        req.onerror = error;
        req.send(null);
    }

    function loadFont(url, cont) {
        var font = FONT_CACHE[url];
        if (font) {
            cont(font);
        } else {
            loadBinary(url, function(data){
                if (data == null) {
                    throw new Error("Cannot load font from " + url);
                } else {
                    var font = new kendo.pdf.TTFFont(data);
                    FONT_CACHE[url] = font;
                    cont(font);
                }
            });
        }
    }

    var IMAGE_CACHE = {};

    function loadImage(url, cont) {
        var img = IMAGE_CACHE[url], bloburl, blob;
        if (img) {
            cont(img);
        } else {
            img = new Image();
            if (!(/^data:/i.test(url))) {
                img.crossOrigin = "Anonymous";
            }
            if (HAS_TYPED_ARRAYS) {
                // IE10 fails to load images from another domain even when the server sends the
                // proper CORS headers.  a XHR, however, will be able to load the data.
                // http://stackoverflow.com/a/19734516/154985
                //
                // On the other hand, it's worth doing it this way for all browsers which support
                // responseType = "blob" (HAS_TYPED_ARRAYS will be true), because we can inspect the
                // mime type and if it's a JPEG (very common case) we can save a lot of time in
                // _load below.
                var xhr = new XMLHttpRequest();
                xhr.onload = function() {
                    blob = xhr.response;
                    bloburl = URL.createObjectURL(blob);
                    _load(bloburl);
                };
                xhr.onerror = _onerror;
                xhr.open("GET", url, true);
                xhr.responseType = "blob";
                xhr.send();
            } else {
                _load(url);
            }
        }

        function _load(url) {
            img.src = url;
            if (img.complete) {
                _onload();
            } else {
                img.onload = _onload;
                img.onerror = _onerror;
            }
        }

        function _onerror() {
            cont(IMAGE_CACHE[url] = "TAINTED");
        }

        function _onload() {
            if (blob && /^image\/jpe?g$/i.test(blob.type)) {
                // If we know we got a JPEG, we can skip the process of rendering it to a
                // canvas, getting the pixel data, searching for transparency we know we won't
                // find, getting back a data URI and then decoding the BASE64 to finally get the
                // binary we already have.  Also, we avoid downgrading the image quality, with
                // the possible drawback of making a bigger PDF; still, seems legit.
                //
                // Besides saving a lot of work, this also reuses the buffer memory
                // (BinaryStream does not create a copy), potentially saving some GC cycles.
                var reader = new FileReader();
                reader.onload = function() {
                    img = new PDFJpegImage(img.width, img.height, BinaryStream(new Uint8Array(this.result)));
                    URL.revokeObjectURL(bloburl);
                    cont(IMAGE_CACHE[url] = img);
                };
                reader.readAsArrayBuffer(blob);
                return;
            }

            var canvas = document.createElement("canvas");
            canvas.width = img.width;
            canvas.height = img.height;
            var ctx = canvas.getContext("2d");

            ctx.drawImage(img, 0, 0);

            var imgdata;
            try {
                imgdata = ctx.getImageData(0, 0, img.width, img.height);
            } catch(ex) {
                // it tainted the canvas -- can't draw it.
                return _onerror();
            } finally {
                if (bloburl) {
                    URL.revokeObjectURL(bloburl);
                }
            }

            // in case it contains transparency, we must separate rgb data from the alpha
            // channel and create a PDFRawImage image with opacity.  otherwise we can use a
            // PDFJpegImage.
            //
            // to do this in one step, we create the rgb and alpha streams anyway, even if
            // we might end up not using them if hasAlpha remains false.

            var hasAlpha = false, rgb = BinaryStream(), alpha = BinaryStream();
            var rawbytes = imgdata.data;
            var i = 0;
            while (i < rawbytes.length) {
                rgb.writeByte(rawbytes[i++]);
                rgb.writeByte(rawbytes[i++]);
                rgb.writeByte(rawbytes[i++]);
                var a = rawbytes[i++];
                if (a < 255) {
                    hasAlpha = true;
                }
                alpha.writeByte(a);
            }

            if (hasAlpha) {
                img = new PDFRawImage(img.width, img.height, rgb, alpha);
            } else {
                // no transparency, encode as JPEG.
                var data = canvas.toDataURL("image/jpeg");
                data = data.substr(data.indexOf(";base64,") + 8);

                var stream = BinaryStream();
                stream.writeBase64(data);
                stream.offset(0);
                img = new PDFJpegImage(img.width, img.height, stream);
            }

            cont(IMAGE_CACHE[url] = img);
        }
    }

    function manyLoader(loadOne) {
        return function(urls, callback) {
            var n = urls.length, i = n;
            if (n === 0) {
                return callback();
            }
            while (i-- > 0) {
                loadOne(urls[i], function(){
                    if (--n === 0) {
                        callback();
                    }
                });
            }
        };
    }

    var loadFonts = manyLoader(loadFont);
    var loadImages = manyLoader(loadImage);

    PDFDocument.prototype = {
        loadFonts: loadFonts,
        loadImages: loadImages,

        getFont: function(url) {
            var font = this.FONTS[url];
            if (!font) {
                font = FONT_CACHE[url];
                if (!font) {
                    throw new Error("Font " + url + " has not been loaded");
                }
                if (font === true) {
                    font = this.attach(new PDFStandardFont(url));
                } else {
                    font = this.attach(new PDFFont(this, font));
                }
                this.FONTS[url] = font;
            }
            return font;
        },

        getImage: function(url) {
            var img = this.IMAGES[url];
            if (!img) {
                img = IMAGE_CACHE[url];
                if (!img) {
                    throw new Error("Image " + url + " has not been loaded");
                }
                if (img === "TAINTED") {
                    return null;
                }
                img = this.IMAGES[url] = this.attach(img.asStream(this));
            }
            return img;
        },

        getOpacityGS: function(opacity, forStroke) {
            var id = parseFloat(opacity).toFixed(3);
            opacity = parseFloat(id);
            id += forStroke ? "S" : "F";
            var cache = this._opacityGSCache || (this._opacityGSCache = {});
            var gs = cache[id];
            if (!gs) {
                var props = {
                    Type: _("ExtGState")
                };
                if (forStroke) {
                    props.CA = opacity;
                } else {
                    props.ca = opacity;
                }
                gs = this.attach(new PDFDictionary(props));
                gs._resourceName = _("GS" + (++RESOURCE_COUNTER));
                cache[id] = gs;
            }
            return gs;
        },

        dict: function(props) {
            return new PDFDictionary(props);
        },

        name: function(str) {
            return _(str);
        },

        stream: function(props, content) {
            return new PDFStream(content, props);
        }
    };

    /* -----[ utils ]----- */

    function pad(str, len, ch) {
        while (str.length < len) {
            str = ch + str;
        }
        return str;
    }

    function zeropad(n, len) {
        return pad(n+"", len, "0");
    }

    function hasOwnProperty(obj, key) {
        return Object.prototype.hasOwnProperty.call(obj, key);
    }

    var isArray = Array.isArray || function(obj) {
        return obj instanceof Array;
    };

    function isDate(obj) {
        return obj instanceof Date;
    }

    function renderArray(a, out) {
        out("[");
        if (a.length > 0) {
            out.withIndent(function(){
                for (var i = 0; i < a.length; ++i) {
                    if (i > 0 && i % 8 === 0) {
                        out.indent(a[i]);
                    } else {
                        out(" ", a[i]);
                    }
                }
            });
            //out.indent();
        }
        out(" ]");
    }

    function renderDate(date, out) {
        out("(D:",
            zeropad(date.getUTCFullYear(), 4),
            zeropad(date.getUTCMonth() + 1, 2),
            zeropad(date.getUTCDate(), 2),
            zeropad(date.getUTCHours(), 2),
            zeropad(date.getUTCMinutes(), 2),
            zeropad(date.getUTCSeconds(), 2),
            "Z)");
    }

    function mm2pt(mm) {
        return mm * (72/25.4);
    }

    function cm2pt(cm) {
        return mm2pt(cm * 10);
    }

    function in2pt(inch)  {
        return inch * 72;
    }

    function unitsToPoints(x, def) {
        if (typeof x == "number") {
            return x;
        }
        if (typeof x == "string") {
            var m;
            m = /^\s*([0-9.]+)\s*(mm|cm|in|pt)\s*$/.exec(x);
            if (m) {
                var num = parseFloat(m[1]);
                if (!isNaN(num)) {
                    if (m[2] == "pt") {
                        return num;
                    }
                    return {
                        "mm": mm2pt,
                        "cm": cm2pt,
                        "in": in2pt
                    }[m[2]](num);
                }
            }
        }
        if (def != null) {
            return def;
        }
        throw new Error("Can't parse unit: " + x);
    }

    /* -----[ PDF basic objects ]----- */

    function PDFValue(){}

    PDFValue.prototype.beforeRender = function(){};

    function defclass(Ctor, proto, Base) {
        if (!Base) {
            Base = PDFValue;
        }
        Ctor.prototype = new Base();
        for (var i in proto) {
            if (hasOwnProperty(proto, i)) {
                Ctor.prototype[i] = proto[i];
            }
        }
        return Ctor;
    }

    /// strings

    var PDFString = defclass(function PDFString(value){
        this.value = value;
    }, {
        render: function(out) {
            //out("(\xFE\xFF", utf16_be_encode(this.escape()), ")");
            var txt = "", esc = this.escape();
            for (var i = 0; i < esc.length; ++i) {
                txt += String.fromCharCode(esc.charCodeAt(i) & 0xFF);
            }
            out("(", txt, ")");
        },
        escape: function() {
            return this.value.replace(/([\(\)\\])/g, "\\$1");
        },
        toString: function() {
            return this.value;
        }
    });

    var PDFHexString = defclass(function PDFHexString(value){
        this.value = value;
    }, {
        render: function(out) {
            out("<");
            for (var i = 0; i < this.value.length; ++i) {
                out(zeropad(this.value.charCodeAt(i).toString(16), 4));
            }
            out(">");
        }
    }, PDFString);

    /// names

    var PDFName = defclass(function PDFName(name) {
        this.name = name;
    }, {
        render: function(out) {
            out("/" + this.escape());
        },
        escape: function() {
            return this.name.replace(/[^\x21-\x7E]/g, function(c){
                return "#" + zeropad(c.charCodeAt(0).toString(16), 2);
            });
        },
        toString: function() {
            return this.name;
        }
    });

    var PDFName_cache = {};
    PDFName.get = _;

    function _(name) {
        if (hasOwnProperty(PDFName_cache, name)) {
            return PDFName_cache[name];
        }
        return (PDFName_cache[name] = new PDFName(name));
    }

    /// dictionary

    var PDFDictionary = defclass(function PDFDictionary(props) {
        this.props = props;
    }, {
        render: function(out) {
            var props = this.props, empty = true;
            out("<<");
            out.withIndent(function(){
                for (var i in props) {
                    if (hasOwnProperty(props, i) && !/^_/.test(i)) {
                        empty = false;
                        out.indent(_(i), " ", props[i]);
                    }
                }
            });
            if (!empty) {
                out.indent();
            }
            out(">>");
        }
    });

    /// streams

    var PDFStream = defclass(function PDFStream(data, props, compress) {
        if (typeof data == "string") {
            var tmp = BinaryStream();
            tmp.write(data);
            data = tmp;
        }
        this.data = data;
        this.props = props || {};
        this.compress = compress;
    }, {
        render: function(out) {
            var data = this.data.get(), props = this.props;
            if (this.compress && window.pako && typeof window.pako.deflate == "function") {
                if (!props.Filter) {
                    props.Filter = [];
                } else if (!(props.Filter instanceof Array)) {
                    props.Filter = [ props.Filter ];
                }
                props.Filter.unshift(_("FlateDecode"));
                data = window.pako.deflate(data);
            }
            props.Length = data.length;
            out(new PDFDictionary(props), " stream", NL);
            out.writeData(data);
            out(NL, "endstream");
        }
    });

    /// catalog

    var PDFCatalog = defclass(function PDFCatalog(props){
        props = this.props = props || {};
        props.Type = _("Catalog");
    }, {
        setPages: function(pagesObj) {
            this.props.Pages = pagesObj;
        }
    }, PDFDictionary);

    /// page tree

    var PDFPageTree = defclass(function PDFPageTree(){
        this.props = {
            Type  : _("Pages"),
            Kids  : [],
            Count : 0
        };
    }, {
        addPage: function(pageObj) {
            this.props.Kids.push(pageObj);
            this.props.Count++;
        }
    }, PDFDictionary);

    /// images

    // JPEG

    function PDFJpegImage(width, height, data) {
        this.asStream = function() {
            var stream = new PDFStream(data, {
                Type             : _("XObject"),
                Subtype          : _("Image"),
                Width            : width,
                Height           : height,
                BitsPerComponent : 8,
                ColorSpace       : _("DeviceRGB"),
                Filter           : _("DCTDecode")
            });
            stream._resourceName = _("I" + (++RESOURCE_COUNTER));
            return stream;
        };
    }

    // PDFRawImage will be used for images with transparency (PNG)

    function PDFRawImage(width, height, rgb, alpha) {
        this.asStream = function(pdf) {
            var mask = new PDFStream(alpha, {
                Type             : _("XObject"),
                Subtype          : _("Image"),
                Width            : width,
                Height           : height,
                BitsPerComponent : 8,
                ColorSpace       : _("DeviceGray")
            }, true);
            var stream = new PDFStream(rgb, {
                Type             : _("XObject"),
                Subtype          : _("Image"),
                Width            : width,
                Height           : height,
                BitsPerComponent : 8,
                ColorSpace       : _("DeviceRGB"),
                SMask            : pdf.attach(mask)
            }, true);
            stream._resourceName = _("I" + (++RESOURCE_COUNTER));
            return stream;
        };
    }

    /// standard fonts

    var PDFStandardFont = defclass(function PDFStandardFont(name){
        this.props = {
            Type     : _("Font"),
            Subtype  : _("Type1"),
            BaseFont : _(name)
        };
        this._resourceName = _("F" + (++RESOURCE_COUNTER));
    }, {
        encodeText: function(str) {
            return new PDFString(str+"");
        }
    }, PDFDictionary);

    /// TTF fonts

    var PDFFont = defclass(function PDFFont(pdf, font, props){
        props = this.props = props || {};
        props.Type = _("Font");
        props.Subtype = _("Type0");
        props.Encoding = _("Identity-H");

        this._pdf = pdf;
        this._font = font;
        this._sub = font.makeSubset();
        this._resourceName = _("F" + (++RESOURCE_COUNTER));

        var head = font.head;

        this.name = font.psName;
        var scale = this.scale = font.scale;
        this.bbox = [
            head.xMin * scale,
            head.yMin * scale,
            head.xMax * scale,
            head.yMax * scale
        ];

        this.italicAngle = font.post.italicAngle;
        this.ascent = font.ascent * scale;
        this.descent = font.descent * scale;
        this.lineGap = font.lineGap * scale;
        this.capHeight = font.os2.capHeight || this.ascent;
        this.xHeight = font.os2.xHeight || 0;
        this.stemV = 0;

        this.familyClass = (font.os2.familyClass || 0) >> 8;
        this.isSerif = this.familyClass >= 1 && this.familyClass <= 7;
        this.isScript = this.familyClass == 10;

        this.flags = ((font.post.isFixedPitch ? 1 : 0) |
                      (this.isSerif ? 1 << 1 : 0) |
                      (this.isScript ? 1 << 3 : 0) |
                      (this.italicAngle !== 0 ? 1 << 6 : 0) |
                      (1 << 5));
    }, {
        encodeText: function(text) {
            return new PDFHexString(this._sub.encodeText(text+""));
        },
        getTextWidth: function(fontSize, text) {
            var width = 0, codeMap = this._font.cmap.getUnicodeEntry().codeMap;
            for (var i = 0; i < text.length; ++i) {
                var glyphId = codeMap[text.charCodeAt(i)];
                width += this._font.widthOfGlyph(glyphId || 0);
            }
            return width * fontSize / 1000;
        },
        beforeRender: function() {
            var self = this;
            var font = self._font;
            var sub = self._sub;

            // write the TTF data
            var data = sub.render();
            var fontStream = new PDFStream(BinaryStream(data), {
                Length1: data.length
            }, true);

            var descriptor = self._pdf.attach(new PDFDictionary({
                Type         : _("FontDescriptor"),
                FontName     : _(self._sub.psName),
                FontBBox     : self.bbox,
                Flags        : self.flags,
                StemV        : self.stemV,
                ItalicAngle  : self.italicAngle,
                Ascent       : self.ascent,
                Descent      : self.descent,
                CapHeight    : self.capHeight,
                XHeight      : self.xHeight,
                FontFile2    : self._pdf.attach(fontStream)
            }));

            var cmap = sub.ncid2ogid;
            var firstChar = sub.firstChar;
            var lastChar = sub.lastChar;
            var charWidths = [];
            (function loop(i, chunk){
                if (i <= lastChar) {
                    var gid = cmap[i];
                    if (gid == null) {
                        loop(i + 1);
                    } else {
                        if (!chunk) {
                            charWidths.push(i, chunk = []);
                        }
                        chunk.push(self._font.widthOfGlyph(gid));
                        loop(i + 1, chunk);
                    }
                }
            })(firstChar);

            // As if two dictionaries weren't enough, we need another
            // one, the "descendant font".  Only that one can be of
            // Subtype CIDFontType2.  PDF is the X11 of document
            // formats: portable but full of legacy that nobody cares
            // about anymore.

            var descendant = new PDFDictionary({
                Type: _("Font"),
                Subtype: _("CIDFontType2"),
                BaseFont: _(self._sub.psName),
                CIDSystemInfo: new PDFDictionary({
                    Registry   : new PDFString("Adobe"),
                    Ordering   : new PDFString("Identity"),
                    Supplement : 0
                }),
                FontDescriptor: descriptor,
                FirstChar: firstChar,
                LastChar: lastChar,
                DW: Math.round(self._font.widthOfGlyph(0)),
                W: charWidths,
                CIDToGIDMap: self._pdf.attach(self._makeCidToGidMap())
            });

            var dict = self.props;
            dict.BaseFont = _(self._sub.psName);
            dict.DescendantFonts = [ self._pdf.attach(descendant) ];

            // Compute the ToUnicode map so that apps can extract
            // meaningful text from the PDF.
            var unimap = new PDFToUnicodeCmap(firstChar, lastChar, sub.subset);
            var unimapStream = new PDFStream(makeOutput(), null, true);
            unimapStream.data(unimap);
            dict.ToUnicode = self._pdf.attach(unimapStream);
        },
        _makeCidToGidMap: function() {
            return new PDFStream(BinaryStream(this._sub.cidToGidMap()), null, true);
        }
    }, PDFDictionary);

    var PDFToUnicodeCmap = defclass(function PDFUnicodeCMap(firstChar, lastChar, map){
        this.firstChar = firstChar;
        this.lastChar = lastChar;
        this.map = map;
    }, {
        render: function(out) {
            out.indent("/CIDInit /ProcSet findresource begin");
            out.indent("12 dict begin");
            out.indent("begincmap");
            out.indent("/CIDSystemInfo <<");
            out.indent("  /Registry (Adobe)");
            out.indent("  /Ordering (UCS)");
            out.indent("  /Supplement 0");
            out.indent(">> def");
            out.indent("/CMapName /Adobe-Identity-UCS def");
            out.indent("/CMapType 2 def");
            out.indent("1 begincodespacerange");
            out.indent("  <0000><ffff>");
            out.indent("endcodespacerange");

            var self = this;
            out.indent(self.lastChar - self.firstChar + 1, " beginbfchar");
            out.withIndent(function(){
                for (var code = self.firstChar; code <= self.lastChar; ++code) {
                    var unicode = self.map[code];
                    out.indent("<", zeropad(code.toString(16), 4), ">",
                               "<", zeropad(unicode.toString(16), 4), ">");
                }
            });
            out.indent("endbfchar");

            out.indent("endcmap");
            out.indent("CMapName currentdict /CMap defineresource pop");
            out.indent("end");
            out.indent("end");
        }
    });

    /// gradients

    function makeHash(a) {
        return a.map(function(x){
            return isArray(x) ? makeHash(x)
                : typeof x == "number" ? (Math.round(x * 1000) / 1000).toFixed(3)
                : x;
        }).join(" ");
    }

    function cacheColorGradientFunction(pdf, r1, g1, b1, r2, g2, b2) {
        var hash = makeHash([ r1, g1, b1, r2, g2, b2 ]);
        var func = pdf.GRAD_COL_FUNCTIONS[hash];
        if (!func) {
            func = pdf.GRAD_COL_FUNCTIONS[hash] = pdf.attach(new PDFDictionary({
                FunctionType: 2,
                Domain: [ 0, 1 ],
                Range: [ 0, 1, 0, 1, 0, 1 ],
                N: 1,
                C0: [ r1 , g1 , b1 ],
                C1: [ r2 , g2 , b2 ]
            }));
        }
        return func;
    }

    function cacheOpacityGradientFunction(pdf, a1, a2) {
        var hash = makeHash([ a1, a2 ]);
        var func = pdf.GRAD_OPC_FUNCTIONS[hash];
        if (!func) {
            func = pdf.GRAD_OPC_FUNCTIONS[hash] = pdf.attach(new PDFDictionary({
                FunctionType: 2,
                Domain: [ 0, 1 ],
                Range: [ 0, 1 ],
                N: 1,
                C0: [ a1 ],
                C1: [ a2 ]
            }));
        }
        return func;
    }

    function makeGradientFunctions(pdf, stops) {
        var hasAlpha = false;
        var opacities = [];
        var colors = [];
        var offsets = [];
        var encode = [];
        var i, prev, cur, prevColor, curColor;
        for (i = 1; i < stops.length; ++i) {
            prev = stops[i - 1];
            cur = stops[i];
            prevColor = prev.color;
            curColor = cur.color;
            colors.push(cacheColorGradientFunction(
                pdf,
                prevColor.r, prevColor.g, prevColor.b,
                curColor.r,  curColor.g,  curColor.b
            ));
            if (prevColor.a < 1 || curColor.a < 1) {
                hasAlpha = true;
            }
            offsets.push(cur.offset);
            encode.push(0, 1);
        }
        if (hasAlpha) {
            for (i = 1; i < stops.length; ++i) {
                prev = stops[i - 1];
                cur = stops[i];
                prevColor = prev.color;
                curColor = cur.color;
                opacities.push(cacheOpacityGradientFunction(
                    pdf, prevColor.a, curColor.a
                ));
            }
        }
        offsets.pop();
        return {
            hasAlpha  : hasAlpha,
            colors    : assemble(colors),
            opacities : hasAlpha ? assemble(opacities) : null
        };
        function assemble(funcs) {
            if (funcs.length == 1) {
                return funcs[0];
            }
            return {
                FunctionType: 3,
                Functions: funcs,
                Domain: [ 0, 1 ],
                Bounds: offsets,
                Encode: encode
            };
        }
    }

    function cacheColorGradient(pdf, isRadial, stops, coords, funcs, box) {
        var shading, hash;
        // if box is given then we have user-space coordinates, which
        // means the gradient is designed for a certain position/size
        // on page.  caching won't do any good.
        if (!box) {
            var a = [ isRadial ].concat(coords);
            stops.forEach(function(x){
                a.push(x.offset, x.color.r, x.color.g, x.color.b);
            });
            hash = makeHash(a);
            shading = pdf.GRAD_COL[hash];
        }
        if (!shading) {
            shading = new PDFDictionary({
                Type: _("Shading"),
                ShadingType: isRadial ? 3 : 2,
                ColorSpace: _("DeviceRGB"),
                Coords: coords,
                Domain: [ 0, 1 ],
                Function: funcs,
                Extend: [ true, true ]
            });
            pdf.attach(shading);
            shading._resourceName = "S" + (++RESOURCE_COUNTER);
            if (hash) {
                pdf.GRAD_COL[hash] = shading;
            }
        }
        return shading;
    }

    function cacheOpacityGradient(pdf, isRadial, stops, coords, funcs, box) {
        var opacity, hash;
        // if box is given then we have user-space coordinates, which
        // means the gradient is designed for a certain position/size
        // on page.  caching won't do any good.
        if (!box) {
            var a = [ isRadial ].concat(coords);
            stops.forEach(function(x){
                a.push(x.offset, x.color.a);
            });
            hash = makeHash(a);
            opacity = pdf.GRAD_OPC[hash];
        }
        if (!opacity) {
            opacity = new PDFDictionary({
                Type: _("ExtGState"),
                AIS: false,
                CA: 1,
                ca: 1,
                SMask: {
                    Type: _("Mask"),
                    S: _("Luminosity"),
                    G: pdf.attach(new PDFStream("/a0 gs /s0 sh", {
                        Type: _("XObject"),
                        Subtype: _("Form"),
                        FormType: 1,
                        BBox: (box ? [
                            box.left, box.top + box.height, box.left + box.width, box.top
                        ] : [ 0, 1, 1, 0 ]),
                        Group: {
                            Type: _("Group"),
                            S: _("Transparency"),
                            CS: _("DeviceGray"),
                            I: true
                        },
                        Resources: {
                            ExtGState: {
                                a0: { CA: 1, ca: 1 }
                            },
                            Shading: {
                                s0: {
                                    ColorSpace: _("DeviceGray"),
                                    Coords: coords,
                                    Domain: [ 0, 1 ],
                                    ShadingType: isRadial ? 3 : 2,
                                    Function: funcs,
                                    Extend: [ true, true ]
                                }
                            }
                        }
                    }))
                }
            });
            pdf.attach(opacity);
            opacity._resourceName = "O" + (++RESOURCE_COUNTER);
            if (hash) {
                pdf.GRAD_OPC[hash] = opacity;
            }
        }
        return opacity;
    }

    function cacheGradient(pdf, gradient, box) {
        var isRadial = gradient.type == "radial";
        var funcs = makeGradientFunctions(pdf, gradient.stops);
        var coords = isRadial ? [
            gradient.start.x , gradient.start.y , gradient.start.r,
            gradient.end.x   , gradient.end.y   , gradient.end.r
        ] : [
            gradient.start.x , gradient.start.y,
            gradient.end.x   , gradient.end.y
        ];
        var shading = cacheColorGradient(
            pdf, isRadial, gradient.stops, coords, funcs.colors, gradient.userSpace && box
        );
        var opacity = funcs.hasAlpha ? cacheOpacityGradient(
            pdf, isRadial, gradient.stops, coords, funcs.opacities, gradient.userSpace && box
        ) : null;
        return {
            hasAlpha: funcs.hasAlpha,
            shading: shading,
            opacity: opacity
        };
    }

    /// page object

    var PDFPage = defclass(function PDFPage(pdf, props){
        this._pdf = pdf;
        this._rcount = 0;
        this._textMode = false;
        this._fontResources = {};
        this._gsResources = {};
        this._xResources = {};
        this._patResources = {};
        this._shResources = {};
        this._opacity = 1;
        this._matrix = [ 1, 0, 0, 1, 0, 0 ];
        this._annotations = [];

        this._font = null;
        this._fontSize = null;

        this._contextStack = [];

        props = this.props = props || {};
        props.Type = _("Page");
        props.ProcSet = [
            _("PDF"),
            _("Text"),
            _("ImageB"),
            _("ImageC"),
            _("ImageI")
        ];
        props.Resources = new PDFDictionary({
            Font      : new PDFDictionary(this._fontResources),
            ExtGState : new PDFDictionary(this._gsResources),
            XObject   : new PDFDictionary(this._xResources),
            Pattern   : new PDFDictionary(this._patResources),
            Shading   : new PDFDictionary(this._shResources)
        });
        props.Annots = this._annotations;
    }, {
        _out: function() {
            this._content.data.apply(null, arguments);
        },
        transform: function(a, b, c, d, e, f) {
            if (!isIdentityMatrix(arguments)) {
                this._matrix = mmul(arguments, this._matrix);
                this._out(a, " ", b, " ", c, " ", d, " ", e, " ", f, " cm");
                // XXX: debug
                // this._out(" % current matrix: ", this._matrix);
                this._out(NL);
            }
        },
        translate: function(dx, dy) {
            this.transform(1, 0, 0, 1, dx, dy);
        },
        scale: function(sx, sy) {
            this.transform(sx, 0, 0, sy, 0, 0);
        },
        rotate: function(angle) {
            var cos = Math.cos(angle), sin = Math.sin(angle);
            this.transform(cos, sin, -sin, cos, 0, 0);
        },
        beginText: function() {
            this._textMode = true;
            this._out("BT", NL);
        },
        endText: function() {
            this._textMode = false;
            this._out("ET", NL);
        },
        _requireTextMode: function() {
            if (!this._textMode) {
                throw new Error("Text mode required; call page.beginText() first");
            }
        },
        _requireFont: function() {
            if (!this._font) {
                throw new Error("No font selected; call page.setFont() first");
            }
        },
        setFont: function(font, size) {
            this._requireTextMode();
            if (font == null) {
                font = this._font;
            } else if (!(font instanceof PDFFont)) {
                font = this._pdf.getFont(font);
            }
            if (size == null) {
                size = this._fontSize;
            }
            this._fontResources[font._resourceName] = font;
            this._font = font;
            this._fontSize = size;
            this._out(font._resourceName, " ", size, " Tf", NL);
        },
        setTextLeading: function(size) {
            this._requireTextMode();
            this._out(size, " TL", NL);
        },
        setTextRenderingMode: function(mode) {
            this._requireTextMode();
            this._out(mode, " Tr", NL);
        },
        showText: function(text, requestedWidth) {
            this._requireFont();
            if (text.length > 1 && requestedWidth && this._font instanceof PDFFont) {
                var outputWidth = this._font.getTextWidth(this._fontSize, text);
                var scale = requestedWidth / outputWidth * 100;
                this._out(scale, " Tz ");
            }
            this._out(this._font.encodeText(text), " Tj", NL);
        },
        showTextNL: function(text) {
            this._requireFont();
            this._out(this._font.encodeText(text), " '", NL);
        },
        addLink: function(uri, box) {
            var ll = this._toPage({ x: box.left, y: box.bottom });
            var ur = this._toPage({ x: box.right, y: box.top });
            this._annotations.push(new PDFDictionary({
                Type    : _("Annot"),
                Subtype : _("Link"),
                Rect    : [ ll.x, ll.y, ur.x, ur.y ],
                Border  : [ 0, 0, 0 ],
                A       : new PDFDictionary({
                    Type : _("Action"),
                    S    : _("URI"),
                    URI  : new PDFString(uri)
                })
            }));
        },
        setStrokeColor: function(r, g, b) {
            this._out(r, " ", g, " ", b, " RG", NL);
        },
        setOpacity: function(opacity) {
            this.setFillOpacity(opacity);
            this.setStrokeOpacity(opacity);
            this._opacity *= opacity;
        },
        setStrokeOpacity: function(opacity) {
            if (opacity < 1) {
                var gs = this._pdf.getOpacityGS(this._opacity * opacity, true);
                this._gsResources[gs._resourceName] = gs;
                this._out(gs._resourceName, " gs", NL);
            }
        },
        setFillColor: function(r, g, b) {
            this._out(r, " ", g, " ", b, " rg", NL);
        },
        setFillOpacity: function(opacity) {
            if (opacity < 1) {
                var gs = this._pdf.getOpacityGS(this._opacity * opacity, false);
                this._gsResources[gs._resourceName] = gs;
                this._out(gs._resourceName, " gs", NL);
            }
        },
        gradient: function(gradient, box) {
            this.save();
            this.rect(box.left, box.top, box.width, box.height);
            this.clip();
            if (!gradient.userSpace) {
                this.transform(box.width, 0, 0, box.height, box.left, box.top);
            }
            var g = cacheGradient(this._pdf, gradient, box);
            var sname = g.shading._resourceName, oname;
            this._shResources[sname] = g.shading;
            if (g.hasAlpha) {
                oname = g.opacity._resourceName;
                this._gsResources[oname] = g.opacity;
                this._out("/" + oname + " gs ");
            }
            this._out("/" + sname + " sh", NL);
            this.restore();
        },
        setDashPattern: function(dashArray, dashPhase) {
            this._out(dashArray, " ", dashPhase, " d", NL);
        },
        setLineWidth: function(width) {
            this._out(width, " w", NL);
        },
        setLineCap: function(lineCap) {
            this._out(lineCap, " J", NL);
        },
        setLineJoin: function(lineJoin) {
            this._out(lineJoin, " j", NL);
        },
        setMitterLimit: function(mitterLimit) {
            this._out(mitterLimit, " M", NL);
        },
        save: function() {
            this._contextStack.push(this._context());
            this._out("q", NL);
        },
        restore: function() {
            this._out("Q", NL);
            this._context(this._contextStack.pop());
        },

        // paths
        moveTo: function(x, y) {
            this._out(x, " ", y, " m", NL);
        },
        lineTo: function(x, y) {
            this._out(x, " ", y, " l", NL);
        },
        bezier: function(x1, y1, x2, y2, x3, y3) {
            this._out(x1, " ", y1, " ", x2, " ", y2, " ", x3, " ", y3, " c", NL);
        },
        bezier1: function(x1, y1, x3, y3) {
            this._out(x1, " ", y1, " ", x3, " ", y3, " y", NL);
        },
        bezier2: function(x2, y2, x3, y3) {
            this._out(x2, " ", y2, " ", x3, " ", y3, " v", NL);
        },
        close: function() {
            this._out("h", NL);
        },
        rect: function(x, y, w, h) {
            this._out(x, " ", y, " ", w, " ", h, " re", NL);
        },
        ellipse: function(x, y, rx, ry) {
            function _X(v) { return x + v; }
            function _Y(v) { return y + v; }

            // how to get to the "magic number" is explained here:
            // http://www.whizkidtech.redprince.net/bezier/circle/kappa/
            var k = 0.5522847498307936;

            this.moveTo(_X(0), _Y(ry));
            this.bezier(
                _X(rx * k) , _Y(ry),
                _X(rx)     , _Y(ry * k),
                _X(rx)     , _Y(0)
            );
            this.bezier(
                _X(rx)     , _Y(-ry * k),
                _X(rx * k) , _Y(-ry),
                _X(0)      , _Y(-ry)
            );
            this.bezier(
                _X(-rx * k) , _Y(-ry),
                _X(-rx)     , _Y(-ry * k),
                _X(-rx)     , _Y(0)
            );
            this.bezier(
                _X(-rx)     , _Y(ry * k),
                _X(-rx * k) , _Y(ry),
                _X(0)       , _Y(ry)
            );
        },
        circle: function(x, y, r) {
            this.ellipse(x, y, r, r);
        },
        stroke: function() {
            this._out("S", NL);
        },
        nop: function() {
            this._out("n", NL);
        },
        clip: function() {
            this._out("W n", NL);
        },
        clipStroke: function() {
            this._out("W S", NL);
        },
        closeStroke: function() {
            this._out("s", NL);
        },
        fill: function() {
            this._out("f", NL);
        },
        fillStroke: function() {
            this._out("B", NL);
        },
        drawImage: function(url) {
            var img = this._pdf.getImage(url);
            if (img) { // the result can be null for a cross-domain image
                this._xResources[img._resourceName] = img;
                this._out(img._resourceName, " Do", NL);
            }
        },
        comment: function(txt) {
            var self = this;
            txt.split(/\r?\n/g).forEach(function(line){
                self._out("% ", line, NL);
            });
        },

        // internal
        _context: function(val) {
            if (val != null) {
                this._opacity = val.opacity;
                this._matrix = val.matrix;
            } else {
                return {
                    opacity: this._opacity,
                    matrix: this._matrix
                };
            }
        },

        _toPage: function(p) {
            var m = this._matrix;
            var a = m[0], b = m[1], c = m[2], d = m[3], e = m[4], f = m[5];
            return {
                x: a*p.x + c*p.y + e,
                y: b*p.x + d*p.y + f
            };
        }
    }, PDFDictionary);

    function BinaryStream(data) {
        var offset = 0, length = 0;
        if (data == null) {
            data = HAS_TYPED_ARRAYS ? new Uint8Array(256) : [];
        } else {
            length = data.length;
        }

        var ensure = HAS_TYPED_ARRAYS ? function(len) {
            if (len >= data.length) {
                var tmp = new Uint8Array(Math.max(len + 256, data.length * 2));
                tmp.set(data, 0);
                data = tmp;
            }
        } : function() {};

        var get = HAS_TYPED_ARRAYS ? function() {
            return new Uint8Array(data.buffer, 0, length);
        } : function() {
            return data;
        };

        var write = HAS_TYPED_ARRAYS ? function(bytes) {
            if (typeof bytes == "string") {
                return writeString(bytes);
            }
            var len = bytes.length;
            ensure(offset + len);
            data.set(bytes, offset);
            offset += len;
            if (offset > length) {
                length = offset;
            }
        } : function(bytes) {
            if (typeof bytes == "string") {
                return writeString(bytes);
            }
            for (var i = 0; i < bytes.length; ++i) {
                writeByte(bytes[i]);
            }
        };

        var slice = HAS_TYPED_ARRAYS ? function(start, length) {
            if (data.buffer.slice) {
                return new Uint8Array(data.buffer.slice(start, start + length));
            } else {
                // IE10
                var x = new Uint8Array(length);
                x.set(new Uint8Array(data.buffer, start, length));
                return x;
            }
        } : function(start, length) {
            return data.slice(start, start + length);
        };

        function eof() {
            return offset >= length;
        }
        function readByte() {
            return offset < length ? data[offset++] : 0;
        }
        function writeByte(b) {
            ensure(offset);
            data[offset++] = b & 0xFF;
            if (offset > length) {
                length = offset;
            }
        }
        function readShort() {
            return (readByte() << 8) | readByte();
        }
        function writeShort(w) {
            writeByte(w >> 8);
            writeByte(w);
        }
        function readShort_() {
            var w = readShort();
            return w >= 0x8000 ? w - 0x10000 : w;
        }
        function writeShort_(w) {
            writeShort(w < 0 ? w + 0x10000 : w);
        }
        function readLong() {
            return (readShort() * 0x10000) + readShort();
        }
        function writeLong(w) {
            writeShort((w >>> 16) & 0xFFFF);
            writeShort(w & 0xFFFF);
        }
        function readLong_() {
            var w = readLong();
            return w >= 0x80000000 ? w - 0x100000000 : w;
        }
        function writeLong_(w) {
            writeLong(w < 0 ? w + 0x100000000 : w);
        }
        function readFixed() {
            return readLong() / 0x10000;
        }
        function writeFixed(f) {
            writeLong(Math.round(f * 0x10000));
        }
        function readFixed_() {
            return readLong_() / 0x10000;
        }
        function writeFixed_(f) {
            writeLong_(Math.round(f * 0x10000));
        }
        function read(len) {
            return times(len, readByte);
        }
        function readString(len) {
            return String.fromCharCode.apply(String, read(len));
        }
        function writeString(str) {
            for (var i = 0; i < str.length; ++i) {
                writeByte(str.charCodeAt(i));
            }
        }
        function times(n, reader) {
            for (var ret = new Array(n), i = 0; i < n; ++i) {
                ret[i] = reader();
            }
            return ret;
        }

        var stream = {
            eof         : eof,
            readByte    : readByte,
            writeByte   : writeByte,
            readShort   : readShort,
            writeShort  : writeShort,
            readLong    : readLong,
            writeLong   : writeLong,
            readFixed   : readFixed,
            writeFixed  : writeFixed,

            // signed numbers.
            readShort_  : readShort_,
            writeShort_ : writeShort_,
            readLong_   : readLong_,
            writeLong_  : writeLong_,
            readFixed_  : readFixed_,
            writeFixed_ : writeFixed_,

            read        : read,
            write       : write,
            readString  : readString,
            writeString : writeString,

            times       : times,
            get         : get,
            slice       : slice,

            offset: function(pos) {
                if (pos != null) {
                    offset = pos;
                    return stream;
                }
                return offset;
            },

            skip: function(nbytes) {
                offset += nbytes;
            },

            toString: function() {
                throw new Error("FIX CALLER.  BinaryStream is no longer convertible to string!");
            },

            length: function() { return length; },

            saveExcursion: function(f) {
                var pos = offset;
                try {
                    return f();
                } finally {
                    offset = pos;
                }
            },

            writeBase64: function(base64) {
                if (window.atob) {
                    writeString(window.atob(base64));
                } else {
                    write(BASE64.decode(base64));
                }
            },
            base64: function() {
                return BASE64.encode(get());
            }
        };

        return stream;
    }

    function unquote(str) {
        return str.replace(/^\s*(['"])(.*)\1\s*$/, "$2");
    }

    function parseFontDef(fontdef) {
        // XXX: this is very crude for now and buggy.  Proper parsing is quite involved.
        var rx = /^\s*((normal|italic)\s+)?((normal|small-caps)\s+)?((normal|bold|\d+)\s+)?(([0-9.]+)(px|pt))(\/(([0-9.]+)(px|pt)|normal))?\s+(.*?)\s*$/i;
        var m = rx.exec(fontdef);
        if (!m) {
            return { fontSize: 12, fontFamily: "sans-serif" };
        }
        var fontSize = m[8] ? parseInt(m[8], 10) : 12;
        return {
            italic     : m[2] && m[2].toLowerCase() == "italic",
            variant    : m[4],
            bold       : m[6] && /bold|700/i.test(m[6]),
            fontSize   : fontSize,
            lineHeight : m[12] ? m[12] == "normal" ? fontSize : parseInt(m[12], 10) : null,
            fontFamily : m[14].split(/\s*,\s*/g).map(unquote)
        };
    }

    function getFontURL(style) {
        function mkFamily(name) {
            if (style.bold) {
                name += "|bold";
            }
            if (style.italic) {
                name += "|italic";
            }
            return name.toLowerCase();
        }
        var fontFamily = style.fontFamily;
        var name, url;
        if (fontFamily instanceof Array) {
            for (var i = 0; i < fontFamily.length; ++i) {
                name = mkFamily(fontFamily[i]);
                url = FONT_MAPPINGS[name];
                if (url) {
                    break;
                }
            }
        } else {
            url = FONT_MAPPINGS[fontFamily.toLowerCase()];
        }
        while (typeof url == "function") {
            url = url();
        }
        if (!url) {
            url = "Times-Roman";
        }
        return url;
    }

    var FONT_MAPPINGS = {
        "serif"                    : "Times-Roman",
        "serif|bold"               : "Times-Bold",
        "serif|italic"             : "Times-Italic",
        "serif|bold|italic"        : "Times-BoldItalic",
        "sans-serif"               : "Helvetica",
        "sans-serif|bold"          : "Helvetica-Bold",
        "sans-serif|italic"        : "Helvetica-Oblique",
        "sans-serif|bold|italic"   : "Helvetica-BoldOblique",
        "monospace"                : "Courier",
        "monospace|bold"           : "Courier-Bold",
        "monospace|italic"         : "Courier-Oblique",
        "monospace|bold|italic"    : "Courier-BoldOblique",
        "zapfdingbats"             : "ZapfDingbats",
        "zapfdingbats|bold"        : "ZapfDingbats",
        "zapfdingbats|italic"      : "ZapfDingbats",
        "zapfdingbats|bold|italic" : "ZapfDingbats"
    };

    function fontAlias(alias, name) {
        alias = alias.toLowerCase();
        FONT_MAPPINGS[alias] = function() {
            return FONT_MAPPINGS[name];
        };
        FONT_MAPPINGS[alias + "|bold"] = function() {
            return FONT_MAPPINGS[name + "|bold"];
        };
        FONT_MAPPINGS[alias + "|italic"] = function() {
            return FONT_MAPPINGS[name + "|italic"];
        };
        FONT_MAPPINGS[alias + "|bold|italic"] = function() {
            return FONT_MAPPINGS[name + "|bold|italic"];
        };
    }

    // Let's define some common names to an appropriate replacement.
    // These are overridable via kendo.pdf.defineFont, should the user
    // want to include the proper versions.

    fontAlias("Times New Roman" , "serif");
    fontAlias("Courier New"     , "monospace");
    fontAlias("Arial"           , "sans-serif");
    fontAlias("Helvetica"       , "sans-serif");
    fontAlias("Verdana"         , "sans-serif");
    fontAlias("Tahoma"          , "sans-serif");
    fontAlias("Georgia"         , "sans-serif");
    fontAlias("Monaco"          , "monospace");
    fontAlias("Andale Mono"     , "monospace");

    function defineFont(name, url) {
        if (arguments.length == 1) {
            for (var i in name) {
                if (hasOwnProperty(name, i)) {
                    defineFont(i, name[i]);
                }
            }
        } else {
            name = name.toLowerCase();
            FONT_MAPPINGS[name] = url;

            // special handling for DejaVu fonts: if they get defined,
            // let them also replace the default families, for good
            // Unicode support out of the box.
            switch (name) {
              case "dejavu sans"               : FONT_MAPPINGS["sans-serif"]              = url; break;
              case "dejavu sans|bold"          : FONT_MAPPINGS["sans-serif|bold"]         = url; break;
              case "dejavu sans|italic"        : FONT_MAPPINGS["sans-serif|italic"]       = url; break;
              case "dejavu sans|bold|italic"   : FONT_MAPPINGS["sans-serif|bold|italic"]  = url; break;
              case "dejavu serif"              : FONT_MAPPINGS["serif"]                   = url; break;
              case "dejavu serif|bold"         : FONT_MAPPINGS["serif|bold"]              = url; break;
              case "dejavu serif|italic"       : FONT_MAPPINGS["serif|italic"]            = url; break;
              case "dejavu serif|bold|italic"  : FONT_MAPPINGS["serif|bold|italic"]       = url; break;
              case "dejavu mono"               : FONT_MAPPINGS["monospace"]               = url; break;
              case "dejavu mono|bold"          : FONT_MAPPINGS["monospace|bold"]          = url; break;
              case "dejavu mono|italic"        : FONT_MAPPINGS["monospace|italic"]        = url; break;
              case "dejavu mono|bold|italic"   : FONT_MAPPINGS["monospace|bold|italic"]   = url; break;
            }
        }
    }

    /// exports.

    kendo.pdf = {
        Document        : PDFDocument,
        BinaryStream    : BinaryStream,
        defineFont      : defineFont,
        parseFontDef    : parseFontDef,
        getFontURL      : getFontURL,
        loadFonts       : loadFonts,
        loadImages      : loadImages,
        getPaperOptions : getPaperOptions,

        TEXT_RENDERING_MODE : {
            fill           : 0,
            stroke         : 1,
            fillAndStroke  : 2,
            invisible      : 3,
            fillAndClip    : 4,
            strokeAndClip  : 5,
            fillStrokeClip : 6,
            clip           : 7
        }
    };

    function mmul(a, b) {
        var a1 = a[0], b1 = a[1], c1 = a[2], d1 = a[3], e1 = a[4], f1 = a[5];
        var a2 = b[0], b2 = b[1], c2 = b[2], d2 = b[3], e2 = b[4], f2 = b[5];
        return [
            a1*a2 + b1*c2,          a1*b2 + b1*d2,
            c1*a2 + d1*c2,          c1*b2 + d1*d2,
            e1*a2 + f1*c2 + e2,     e1*b2 + f1*d2 + f2
        ];
    }

    function isIdentityMatrix(m) {
        return m[0] === 1 && m[1] === 0 && m[2] === 0 && m[3] === 1 && m[4] === 0 && m[5] === 0;
    }

})(window, parseFloat);

(function(window){

/*****************************************************************************\
 *
 * The code in this file, although written from scratch, is influenced by the
 * TrueType parser/encoder in PDFKit -- http://pdfkit.org/ (a CoffeeScript
 * library for producing PDF files).
 *
 * PDFKit is (c) Devon Govett 2014 and released under the MIT License.
 *
\*****************************************************************************/

"use strict";

// WARNING: removing the following jshint declaration and turning
// == into === to make JSHint happy will break functionality.
/* jshint eqnull:true */
/* jshint loopfunc:true */
/* jshint newcap:false */

function hasOwnProperty(obj, key) {
    return Object.prototype.hasOwnProperty.call(obj, key);
}

function sortedKeys(obj) {
    return Object.keys(obj).sort(function(a, b){ return a - b; }).map(parseFloat);
}

var PDF = window.kendo.pdf;
var BinaryStream = PDF.BinaryStream;

///

function Directory(data) {
    this.raw = data;
    this.scalerType = data.readLong();
    this.tableCount = data.readShort();
    this.searchRange = data.readShort();
    this.entrySelector = data.readShort();
    this.rangeShift = data.readShort();

    var tables = this.tables = {};
    for (var i = 0; i < this.tableCount; ++i) {
        var entry = {
            tag      : data.readString(4),
            checksum : data.readLong(),
            offset   : data.readLong(),
            length   : data.readLong()
        };
        tables[entry.tag] = entry;
    }
}

Directory.prototype = {

    readTable: function(name, Ctor) {
        var def = this.tables[name];
        if (!def) {
            throw new Error("Table " + name + " not found in directory");
        }
        return (this[name] = def.table = new Ctor(this, def));
    },

    render: function(tables) {
        var tableCount = Object.keys(tables).length;

        var maxpow2 = Math.pow(2, Math.floor(Math.log(tableCount) / Math.LN2));
        var searchRange = maxpow2 * 16;
        var entrySelector = Math.floor(Math.log(maxpow2) / Math.LN2);
        var rangeShift = tableCount * 16 - searchRange;

        var out = BinaryStream();
        out.writeLong(this.scalerType);
        out.writeShort(tableCount);
        out.writeShort(searchRange);
        out.writeShort(entrySelector);
        out.writeShort(rangeShift);

        var directoryLength = tableCount * 16;
        var offset = out.offset() + directoryLength;
        var headOffset = null;
        var tableData = BinaryStream();

        for (var tag in tables) {
            if (hasOwnProperty(tables, tag)) {
                var table = tables[tag];

                out.writeString(tag);
                out.writeLong(this.checksum(table));
                out.writeLong(offset);
                out.writeLong(table.length);

                tableData.write(table);
                if (tag == "head") {
                    headOffset = offset;
                }
                offset += table.length;

                while (offset % 4) {
                    tableData.writeByte(0);
                    offset++;
                }
            }
        }

        out.write(tableData.get());
        var sum = this.checksum(out.get());
        var adjustment = 0xB1B0AFBA - sum;

        out.offset(headOffset + 8);
        out.writeLong(adjustment);
        return out.get();
    },

    checksum: function(data) {
        data = BinaryStream(data);
        var sum = 0;
        while (!data.eof()) {
            sum += data.readLong();
        }
        return sum & 0xFFFFFFFF;
    }
};

function deftable(methods) {
    function Ctor(file, def) {
        this.definition = def;
        this.length = def.length;
        this.offset = def.offset;
        this.file = file;
        this.rawData = file.raw;
        this.parse(file.raw);
    }
    Ctor.prototype.raw = function() {
        return this.rawData.slice(this.offset, this.length);
    };
    for (var i in methods) {
        if (hasOwnProperty(methods, i)) {
            Ctor[i] = Ctor.prototype[i] = methods[i];
        }
    }
    return Ctor;
}

var HeadTable = deftable({
    parse: function(data) {
        data.offset(this.offset);
        this.version             = data.readLong();
        this.revision            = data.readLong();
        this.checkSumAdjustment  = data.readLong();
        this.magicNumber         = data.readLong();
        this.flags               = data.readShort();
        this.unitsPerEm          = data.readShort();
        this.created             = data.read(8);
        this.modified            = data.read(8);

        this.xMin = data.readShort_();
        this.yMin = data.readShort_();
        this.xMax = data.readShort_();
        this.yMax = data.readShort_();

        this.macStyle           = data.readShort();
        this.lowestRecPPEM      = data.readShort();
        this.fontDirectionHint  = data.readShort_();
        this.indexToLocFormat   = data.readShort_();
        this.glyphDataFormat    = data.readShort_();
    },
    render: function(indexToLocFormat) {
        var out = BinaryStream();
        out.writeLong(this.version);
        out.writeLong(this.revision);
        out.writeLong(0);       // checksum adjustment; shall be computed later
        out.writeLong(this.magicNumber);
        out.writeShort(this.flags);
        out.writeShort(this.unitsPerEm);
        out.write(this.created);
        out.write(this.modified);
        out.writeShort_(this.xMin);
        out.writeShort_(this.yMin);
        out.writeShort_(this.xMax);
        out.writeShort_(this.yMax);
        out.writeShort(this.macStyle);
        out.writeShort(this.lowestRecPPEM);
        out.writeShort_(this.fontDirectionHint);
        out.writeShort_(indexToLocFormat); // this will depend on the `loca` table
        out.writeShort_(this.glyphDataFormat);
        return out.get();
    }
});

var LocaTable = deftable({
    parse: function(data) {
        data.offset(this.offset);
        var format = this.file.head.indexToLocFormat;
        if (format === 0) {
            this.offsets = data.times(this.length / 2, function(){
                return 2 * data.readShort();
            });
        } else {
            this.offsets = data.times(this.length / 4, data.readLong);
        }
    },
    offsetOf: function(id) {
        return this.offsets[id];
    },
    lengthOf: function(id) {
        return this.offsets[id + 1] - this.offsets[id];
    },
    render: function(offsets) {
        var out = BinaryStream();
        var needsLongFormat = offsets[offsets.length - 1] > 0xFFFF;
        for (var i = 0; i < offsets.length; ++i) {
            if (needsLongFormat) {
                out.writeLong(offsets[i]);
            } else {
                out.writeShort(offsets[i] / 2);
            }
        }
        return {
            format: needsLongFormat ? 1 : 0,
            table: out.get()
        };
    }
});

var HheaTable = deftable({
    parse: function(data) {
        data.offset(this.offset);

        this.version              = data.readLong();
        this.ascent               = data.readShort_();
        this.descent              = data.readShort_();
        this.lineGap              = data.readShort_();
        this.advanceWidthMax      = data.readShort();
        this.minLeftSideBearing   = data.readShort_();
        this.minRightSideBearing  = data.readShort_();
        this.xMaxExtent           = data.readShort_();
        this.caretSlopeRise       = data.readShort_();
        this.caretSlopeRun        = data.readShort_();
        this.caretOffset          = data.readShort_();

        data.skip(4 * 2);       // reserved

        this.metricDataFormat     = data.readShort_();
        this.numOfLongHorMetrics  = data.readShort();
    },
    render: function(ids) {
        var out = BinaryStream();
        out.writeLong(this.version);
        out.writeShort_(this.ascent);
        out.writeShort_(this.descent);
        out.writeShort_(this.lineGap);
        out.writeShort(this.advanceWidthMax);
        out.writeShort_(this.minLeftSideBearing);
        out.writeShort_(this.minRightSideBearing);
        out.writeShort_(this.xMaxExtent);
        out.writeShort_(this.caretSlopeRise);
        out.writeShort_(this.caretSlopeRun);
        out.writeShort_(this.caretOffset);

        out.write([ 0, 0, 0, 0, 0, 0, 0, 0 ]); // reserved bytes

        out.writeShort_(this.metricDataFormat);
        out.writeShort(ids.length);
        return out.get();
    }
});

var MaxpTable = deftable({
    parse: function(data) {
        data.offset(this.offset);
        this.version = data.readLong();
        this.numGlyphs = data.readShort();
        this.maxPoints = data.readShort();
        this.maxContours = data.readShort();
        this.maxComponentPoints = data.readShort();
        this.maxComponentContours = data.readShort();
        this.maxZones = data.readShort();
        this.maxTwilightPoints = data.readShort();
        this.maxStorage = data.readShort();
        this.maxFunctionDefs = data.readShort();
        this.maxInstructionDefs = data.readShort();
        this.maxStackElements = data.readShort();
        this.maxSizeOfInstructions = data.readShort();
        this.maxComponentElements = data.readShort();
        this.maxComponentDepth = data.readShort();
    },
    render: function(glyphIds) {
        var out = BinaryStream();
        out.writeLong(this.version);
        out.writeShort(glyphIds.length);
        out.writeShort(this.maxPoints);
        out.writeShort(this.maxContours);
        out.writeShort(this.maxComponentPoints);
        out.writeShort(this.maxComponentContours);
        out.writeShort(this.maxZones);
        out.writeShort(this.maxTwilightPoints);
        out.writeShort(this.maxStorage);
        out.writeShort(this.maxFunctionDefs);
        out.writeShort(this.maxInstructionDefs);
        out.writeShort(this.maxStackElements);
        out.writeShort(this.maxSizeOfInstructions);
        out.writeShort(this.maxComponentElements);
        out.writeShort(this.maxComponentDepth);
        return out.get();
    }
});

var HmtxTable = deftable({
    parse: function(data) {
        data.offset(this.offset);
        var dir = this.file, hhea = dir.hhea;
        this.metrics = data.times(hhea.numOfLongHorMetrics, function(){
            return {
                advance: data.readShort(),
                lsb: data.readShort_()
            };
        });
        var lsbCount = dir.maxp.numGlyphs - dir.hhea.numOfLongHorMetrics;
        this.leftSideBearings = data.times(lsbCount, data.readShort_);
    },
    forGlyph: function(id) {
        var metrics = this.metrics;
        var n = metrics.length;
        if (id < n) {
            return metrics[id];
        }
        return {
            advance: metrics[n - 1].advance,
            lsb: this.leftSideBearings[id - n]
        };
    },
    render: function(glyphIds) {
        var out = BinaryStream();
        for (var i = 0; i < glyphIds.length; ++i) {
            var m = this.forGlyph(glyphIds[i]);
            out.writeShort(m.advance);
            out.writeShort_(m.lsb);
        }
        return out.get();
    }
});

var GlyfTable = (function(){

    function SimpleGlyph(raw) {
        this.raw = raw;
    }
    SimpleGlyph.prototype = {
        compound: false,
        render: function() {
            return this.raw.get();
        }
    };

    var ARG_1_AND_2_ARE_WORDS     = 0x0001;
    var WE_HAVE_A_SCALE           = 0x0008;
    var MORE_COMPONENTS           = 0x0020;
    var WE_HAVE_AN_X_AND_Y_SCALE  = 0x0040;
    var WE_HAVE_A_TWO_BY_TWO      = 0x0080;
    var WE_HAVE_INSTRUCTIONS      = 0x0100;

    function CompoundGlyph(data) {
        this.raw = data;
        var ids = this.glyphIds = [];
        var offsets = this.idOffsets = [];
        while (true) {
            var flags = data.readShort();
            offsets.push(data.offset());
            ids.push(data.readShort());

            if (!(flags & MORE_COMPONENTS)) {
                break;
            }

            data.skip(flags & ARG_1_AND_2_ARE_WORDS ? 4 : 2);

            if (flags & WE_HAVE_A_TWO_BY_TWO) {
                data.skip(8);
            } else if (flags & WE_HAVE_AN_X_AND_Y_SCALE) {
                data.skip(4);
            } else if (flags & WE_HAVE_A_SCALE) {
                data.skip(2);
            }
        }
    }

    CompoundGlyph.prototype = {
        compound: true,
        render: function(old2new) {
            var out = BinaryStream(this.raw.get());
            for (var i = 0; i < this.glyphIds.length; ++i) {
                var id = this.glyphIds[i];
                out.offset(this.idOffsets[i]);
                out.writeShort(old2new[id]);
            }
            return out.get();
        }
    };

    return deftable({
        parse: function(data) {
            this.cache = {};
        },
        glyphFor: function(id) {
            var cache = this.cache;
            if (hasOwnProperty(cache, id)) {
                return cache[id];
            }

            var loca = this.file.loca;
            var length = loca.lengthOf(id);

            if (length === 0) {
                return (cache[id] = null);
            }

            var data = this.rawData;
            var offset = this.offset + loca.offsetOf(id);
            var raw = BinaryStream(data.slice(offset, length));

            var numberOfContours = raw.readShort_();
            var xMin = raw.readShort_();
            var yMin = raw.readShort_();
            var xMax = raw.readShort_();
            var yMax = raw.readShort_();

            var glyph = cache[id] = numberOfContours == -1 ? new CompoundGlyph(raw) : new SimpleGlyph(raw);

            glyph.numberOfContours = numberOfContours;
            glyph.xMin = xMin;
            glyph.yMin = yMin;
            glyph.xMax = xMax;
            glyph.yMax = yMax;

            return glyph;
        },
        render: function(glyphs, oldIds, old2new) {
            var out = BinaryStream(), offsets = [];
            for (var i = 0; i < oldIds.length; ++i) {
                var id = oldIds[i];
                var glyph = glyphs[id];
                offsets.push(out.offset());
                if (glyph) {
                    out.write(glyph.render(old2new));
                }
            }
            offsets.push(out.offset());
            return {
                table: out.get(),
                offsets: offsets
            };
        }
    });

}());

var NameTable = (function(){

    function NameEntry(text, entry) {
        this.text = text;
        this.length = text.length;
        this.platformID = entry.platformID;
        this.platformSpecificID = entry.platformSpecificID;
        this.languageID = entry.languageID;
        this.nameID = entry.nameID;
    }

    return deftable({
        parse: function(data) {
            data.offset(this.offset);
            var format = data.readShort();
            var count = data.readShort();
            var stringOffset = this.offset + data.readShort();
            var nameRecords = data.times(count, function(){
                return {
                    platformID         : data.readShort(),
                    platformSpecificID : data.readShort(),
                    languageID         : data.readShort(),
                    nameID             : data.readShort(),
                    length             : data.readShort(),
                    offset             : data.readShort() + stringOffset
                };
            });
            var strings = this.strings = {};
            for (var i = 0; i < nameRecords.length; ++i) {
                var rec = nameRecords[i];
                data.offset(rec.offset);
                var text = data.readString(rec.length);
                if (!strings[rec.nameID]) {
                    strings[rec.nameID] = [];
                }
                strings[rec.nameID].push(new NameEntry(text, rec));
            }
            this.postscriptEntry = strings[6][0];
            this.postscriptName = this.postscriptEntry.text.replace(/[^\x20-\x7F]/g, "");
        },

        render: function(psName) {
            var strings = this.strings;
            var strCount = 0;
            for (var i in strings) {
                if (hasOwnProperty(strings, i)) {
                    strCount += strings[i].length;
                }
            }
            var out = BinaryStream();
            var strTable = BinaryStream();

            out.writeShort(0);  // format
            out.writeShort(strCount);
            out.writeShort(6 + 12 * strCount); // stringOffset

            for (i in strings) {
                if (hasOwnProperty(strings, i)) {
                    var list = i == 6 ? [
                        new NameEntry(psName, this.postscriptEntry)
                    ] : strings[i];
                    for (var j = 0; j < list.length; ++j) {
                        var str = list[j];
                        out.writeShort(str.platformID);
                        out.writeShort(str.platformSpecificID);
                        out.writeShort(str.languageID);
                        out.writeShort(str.nameID);
                        out.writeShort(str.length);
                        out.writeShort(strTable.offset());

                        strTable.writeString(str.text);
                    }
                }
            }

            out.write(strTable.get());

            return out.get();
        }
    });

})();

var PostTable = (function(){

    var POSTSCRIPT_GLYPHS = ".notdef .null nonmarkingreturn space exclam quotedbl numbersign dollar percent ampersand quotesingle parenleft parenright asterisk plus comma hyphen period slash zero one two three four five six seven eight nine colon semicolon less equal greater question at A B C D E F G H I J K L M N O P Q R S T U V W X Y Z bracketleft backslash bracketright asciicircum underscore grave a b c d e f g h i j k l m n o p q r s t u v w x y z braceleft bar braceright asciitilde Adieresis Aring Ccedilla Eacute Ntilde Odieresis Udieresis aacute agrave acircumflex adieresis atilde aring ccedilla eacute egrave ecircumflex edieresis iacute igrave icircumflex idieresis ntilde oacute ograve ocircumflex odieresis otilde uacute ugrave ucircumflex udieresis dagger degree cent sterling section bullet paragraph germandbls registered copyright trademark acute dieresis notequal AE Oslash infinity plusminus lessequal greaterequal yen mu partialdiff summation product pi integral ordfeminine ordmasculine Omega ae oslash questiondown exclamdown logicalnot radical florin approxequal Delta guillemotleft guillemotright ellipsis nonbreakingspace Agrave Atilde Otilde OE oe endash emdash quotedblleft quotedblright quoteleft quoteright divide lozenge ydieresis Ydieresis fraction currency guilsinglleft guilsinglright fi fl daggerdbl periodcentered quotesinglbase quotedblbase perthousand Acircumflex Ecircumflex Aacute Edieresis Egrave Iacute Icircumflex Idieresis Igrave Oacute Ocircumflex apple Ograve Uacute Ucircumflex Ugrave dotlessi circumflex tilde macron breve dotaccent ring cedilla hungarumlaut ogonek caron Lslash lslash Scaron scaron Zcaron zcaron brokenbar Eth eth Yacute yacute Thorn thorn minus multiply onesuperior twosuperior threesuperior onehalf onequarter threequarters franc Gbreve gbreve Idotaccent Scedilla scedilla Cacute cacute Ccaron ccaron dcroat".split(/\s+/g);

    return deftable({
        parse: function(data) {
            data.offset(this.offset);

            this.format = data.readLong();
            this.italicAngle = data.readFixed_();
            this.underlinePosition = data.readShort_();
            this.underlineThickness = data.readShort_();
            this.isFixedPitch = data.readLong();
            this.minMemType42 = data.readLong();
            this.maxMemType42 = data.readLong();
            this.minMemType1 = data.readLong();
            this.maxMemType1 = data.readLong();

            var numberOfGlyphs;

            switch (this.format) {
              case 0x00010000:
              case 0x00030000:
                break;

              case 0x00020000:
                numberOfGlyphs = data.readShort();
                this.glyphNameIndex = data.times(numberOfGlyphs, data.readShort);
                this.names = [];
                var limit = this.offset + this.length;
                while (data.offset() < limit) {
                    this.names.push(data.readString(data.readByte()));
                }
                break;

              case 0x00025000:
                numberOfGlyphs = data.readShort();
                this.offsets = data.read(numberOfGlyphs);
                break;

              case 0x00040000:
                this.map = data.times(this.file.maxp.numGlyphs, data.readShort);
                break;
            }
        },
        glyphFor: function(code) {
            switch (this.format) {
              case 0x00010000:
                return POSTSCRIPT_GLYPHS[code] || ".notdef";

              case 0x00020000:
                var index = this.glyphNameIndex[code];
                if (index < POSTSCRIPT_GLYPHS.length) {
                    return POSTSCRIPT_GLYPHS[index];
                }
                return this.names[index - POSTSCRIPT_GLYPHS.length] || ".notdef";

              case 0x00025000:

              case 0x00030000:
                return ".notdef";

              case 0x00040000:
                return this.map[code] || 0xFFFF;
            }
        },
        render: function(mapping) {
            if (this.format == 0x00030000) {
                return this.raw();
            }

            // keep original header, but set format to 2.0
            var out = BinaryStream(this.rawData.slice(this.offset, 32));
            out.writeLong(0x00020000);
            out.offset(32);

            var indexes = [];
            var strings = [];

            for (var i = 0; i < mapping.length; ++i) {
                var id = mapping[i];
                var post = this.glyphFor(id);
                var index = POSTSCRIPT_GLYPHS.indexOf(post);
                if (index >= 0) {
                    indexes.push(index);
                } else {
                    indexes.push(POSTSCRIPT_GLYPHS.length + strings.length);
                    strings.push(post);
                }
            }

            out.writeShort(mapping.length);

            for (i = 0; i < indexes.length; ++i) {
                out.writeShort(indexes[i]);
            }

            for (i = 0; i < strings.length; ++i) {
                out.writeByte(strings[i].length);
                out.writeString(strings[i]);
            }

            return out.get();
        }
    });
})();

var CmapTable = (function(){

    function CmapEntry(data, offset) {
        var self = this;
        self.platformID = data.readShort();
        self.platformSpecificID = data.readShort();
        self.offset = offset + data.readLong();

        data.saveExcursion(function(){
            data.offset(self.offset);
            self.format = data.readShort();
            self.length = data.readShort();
            self.language = data.readShort();

            self.isUnicode = (
                self.platformID == 3 && self.platformSpecificID == 1 && self.format == 4
            ) || (
                self.platformID === 0 && self.format == 4
            );

            self.codeMap = {};
            switch (self.format) {
              case 0:
                for (var i = 0; i < 256; ++i) {
                    self.codeMap[i] = data.readByte();
                }
                break;

              case 4:
                var segCount = data.readShort() / 2;

                data.skip(6);       // searchRange, entrySelector, rangeShift
                var endCode = data.times(segCount, data.readShort);
                data.skip(2);       // reserved pad
                var startCode = data.times(segCount, data.readShort);
                var idDelta = data.times(segCount, data.readShort_);
                var idRangeOffset = data.times(segCount, data.readShort);

                var count = (self.length + self.offset - data.offset()) / 2;
                var glyphIds = data.times(count, data.readShort);

                for (i = 0; i < segCount; ++i) {
                    var start = startCode[i], end = endCode[i];
                    for (var code = start; code <= end; ++code) {
                        var glyphId;
                        if (idRangeOffset[i] === 0) {
                            glyphId = code + idDelta[i];
                        } else {
                            ///
                            // When non-zero, idRangeOffset contains for each segment the byte offset of the Glyph ID
                            // into the glyphIds table, from the *current* `i` cell of idRangeOffset.  In other words,
                            // this offset spans from the first into the second array.  This works, because the arrays
                            // are consecutive in the TTF file:
                            //
                            //     [ ...idRangeOffset... ][ ...glyphIds... ]
                            //       ...... 48 ......       .... ID ....
                            //              ^----- 48 bytes -----^
                            //
                            // (but I can't stop wondering why is it not just a plain index, possibly incremented by 1
                            // so that we can have that special `zero` value.)
                            //
                            // The elements of idRangeOffset are even numbers, because both arrays contain 16-bit words,
                            // yet the offset is in bytes.  That is why we divide it by 2.  Then we subtract the
                            // remaining segments (segCount-i), and add the code-start offset, to which we need to add
                            // the corresponding delta to get the actual glyph ID.
                            ///
                            var index = idRangeOffset[i] / 2 - (segCount - i) + (code - start);
                            glyphId = glyphIds[index] || 0;
                            if (glyphId !== 0) {
                                glyphId += idDelta[i];
                            }
                        }
                        self.codeMap[code] = glyphId & 0xFFFF;
                    }
                }
            }
        });
    }

    function renderCharmap(ncid2ogid, ogid2ngid) {
        var codes = sortedKeys(ncid2ogid);
        var startCodes = [];
        var endCodes = [];
        var last = null;
        var diff = null;

        function new_gid(charcode) {
            return ogid2ngid[ncid2ogid[charcode]];
        }

        for (var i = 0; i < codes.length; ++i) {
            var code = codes[i];
            var gid = new_gid(code);
            var delta = gid - code;
            if (last == null || delta !== diff) {
                if (last) {
                    endCodes.push(last);
                }
                startCodes.push(code);
                diff = delta;
            }
            last = code;
        }

        if (last) {
            endCodes.push(last);
        }
        endCodes.push(0xFFFF);
        startCodes.push(0xFFFF);

        var segCount = startCodes.length;
        var segCountX2 = segCount * 2;
        var searchRange = 2 * Math.pow(2, Math.floor(Math.log(segCount) / Math.LN2));
        var entrySelector = Math.log(searchRange / 2) / Math.LN2;
        var rangeShift = segCountX2 - searchRange;

        var deltas = [];
        var rangeOffsets = [];
        var glyphIds = [];

        for (i = 0; i < segCount; ++i) {
            var startCode = startCodes[i];
            var endCode = endCodes[i];
            if (startCode == 0xFFFF) {
                deltas.push(0);
                rangeOffsets.push(0);
                break;
            }
            var startGlyph = new_gid(startCode);
            if (startCode - startGlyph >= 0x8000) {
                deltas.push(0);
                rangeOffsets.push(2 * (glyphIds.length + segCount - i));
                for (var j = startCode; j <= endCode; ++j) {
                    glyphIds.push(new_gid(j));
                }
            } else {
                deltas.push(startGlyph - startCode);
                rangeOffsets.push(0);
            }
        }

        var out = BinaryStream();

        out.writeShort(3);      // platformID
        out.writeShort(1);      // platformSpecificID
        out.writeLong(12);      // offset
        out.writeShort(4);      // format
        out.writeShort(16 + segCount * 8 + glyphIds.length * 2); // length
        out.writeShort(0);      // language
        out.writeShort(segCountX2);
        out.writeShort(searchRange);
        out.writeShort(entrySelector);
        out.writeShort(rangeShift);

        endCodes.forEach(out.writeShort);
        out.writeShort(0);      // reserved pad
        startCodes.forEach(out.writeShort);
        deltas.forEach(out.writeShort_);
        rangeOffsets.forEach(out.writeShort);
        glyphIds.forEach(out.writeShort);

        return out.get();
    }

    return deftable({
        parse: function(data) {
            var self = this;
            var offset = self.offset;
            data.offset(offset);

            self.version = data.readShort();
            var tableCount = data.readShort();
            self.unicodeEntry = null;
            self.tables = data.times(tableCount, function(){
                var entry = new CmapEntry(data, offset);
                if (entry.isUnicode) {
                    self.unicodeEntry = entry;
                }
                return entry;
            });
        },
        render: function(ncid2ogid, ogid2ngid) {
            var out = BinaryStream();
            out.writeShort(0);  // version
            out.writeShort(1);  // tableCount
            out.write(renderCharmap(ncid2ogid, ogid2ngid));
            return out.get();
        },
        getUnicodeEntry: function() {
            if (!this.unicodeEntry) {
                throw new Error("Font doesn't have an Unicode encoding");
            }
            return this.unicodeEntry;
        }
    });

})();

var OS2Table = deftable({
    parse: function(data) {
        data.offset(this.offset);
        this.version = data.readShort();
        this.averageCharWidth = data.readShort_();
        this.weightClass = data.readShort();
        this.widthClass = data.readShort();
        this.type = data.readShort();
        this.ySubscriptXSize = data.readShort_();
        this.ySubscriptYSize = data.readShort_();
        this.ySubscriptXOffset = data.readShort_();
        this.ySubscriptYOffset = data.readShort_();
        this.ySuperscriptXSize = data.readShort_();
        this.ySuperscriptYSize = data.readShort_();
        this.ySuperscriptXOffset = data.readShort_();
        this.ySuperscriptYOffset = data.readShort_();
        this.yStrikeoutSize = data.readShort_();
        this.yStrikeoutPosition = data.readShort_();
        this.familyClass = data.readShort_();

        this.panose = data.times(10, data.readByte);
        this.charRange = data.times(4, data.readLong);

        this.vendorID = data.readString(4);
        this.selection = data.readShort();
        this.firstCharIndex = data.readShort();
        this.lastCharIndex = data.readShort();

        if (this.version > 0) {
            this.ascent = data.readShort_();
            this.descent = data.readShort_();
            this.lineGap = data.readShort_();
            this.winAscent = data.readShort();
            this.winDescent = data.readShort();
            this.codePageRange = data.times(2, data.readLong);

            if (this.version > 1) {
                this.xHeight = data.readShort();
                this.capHeight = data.readShort();
                this.defaultChar = data.readShort();
                this.breakChar = data.readShort();
                this.maxContext = data.readShort();
            }
        }
    },
    render: function() {
        return this.raw();
    }
});

var subsetTag = 100000;

function nextSubsetTag() {
    var ret = "", n = subsetTag+"";
    for (var i = 0; i < n.length; ++i) {
        ret += String.fromCharCode(n.charCodeAt(i) - 48 + 65);
    }
    ++subsetTag;
    return ret;
}

function Subfont(font) {
    this.font = font;
    this.subset = {};
    this.unicodes = {};
    this.ogid2ngid = { 0: 0 };
    this.ngid2ogid = { 0: 0 };
    this.ncid2ogid = {};
    this.next = this.firstChar = 1;
    this.nextGid = 1;
    this.psName = nextSubsetTag() + "+" + this.font.psName;
}

Subfont.prototype = {
    use: function(ch) {
        var code;
        if (typeof ch == "string") {
            var ret = "";
            for (var i = 0; i < ch.length; ++i) {
                code = this.use(ch.charCodeAt(i));
                ret += String.fromCharCode(code);
            }
            return ret;
        }
        code = this.unicodes[ch];
        if (!code) {
            code = this.next++;
            this.subset[code] = ch;
            this.unicodes[ch] = code;

            // generate new GID (glyph ID) and maintain newGID ->
            // oldGID and back mappings
            var old_gid = this.font.cmap.getUnicodeEntry().codeMap[ch];
            if (old_gid) {
                this.ncid2ogid[code] = old_gid;
                if (this.ogid2ngid[old_gid] == null) {
                    var new_gid = this.nextGid++;
                    this.ogid2ngid[old_gid] = new_gid;
                    this.ngid2ogid[new_gid] = old_gid;
                }
            }
        }
        return code;
    },
    encodeText: function(text) {
        return this.use(text);
    },
    glyphIds: function() {
        return sortedKeys(this.ogid2ngid);
    },
    glyphsFor: function(glyphIds, result) {
        if (!result) {
            result = {};
        }
        for (var i = 0; i < glyphIds.length; ++i) {
            var id = glyphIds[i];
            if (!result[id]) {
                var glyph = result[id] = this.font.glyf.glyphFor(id);
                if (glyph && glyph.compound) {
                    this.glyphsFor(glyph.glyphIds, result);
                }
            }
        }
        return result;
    },
    render: function() {
        var glyphs = this.glyphsFor(this.glyphIds());

        // add missing sub-glyphs
        for (var old_gid in glyphs) {
            if (hasOwnProperty(glyphs, old_gid)) {
                old_gid = parseInt(old_gid, 10);
                if (this.ogid2ngid[old_gid] == null) {
                    var new_gid = this.nextGid++;
                    this.ogid2ngid[old_gid] = new_gid;
                    this.ngid2ogid[new_gid] = old_gid;
                }
            }
        }

        // must obtain old_gid_ids in an order matching sorted
        // new_gid_ids
        var new_gid_ids = sortedKeys(this.ngid2ogid);
        var old_gid_ids = new_gid_ids.map(function(id){
            return this.ngid2ogid[id];
        }, this);

        var font = this.font;
        var glyf = font.glyf.render(glyphs, old_gid_ids, this.ogid2ngid);
        var loca = font.loca.render(glyf.offsets);

        this.lastChar = this.next - 1;

        var tables = {
            "cmap" : CmapTable.render(this.ncid2ogid, this.ogid2ngid),
            "glyf" : glyf.table,
            "loca" : loca.table,
            "hmtx" : font.hmtx.render(old_gid_ids),
            "hhea" : font.hhea.render(old_gid_ids),
            "maxp" : font.maxp.render(old_gid_ids),
            "post" : font.post.render(old_gid_ids),
            "name" : font.name.render(this.psName),
            "head" : font.head.render(loca.format),
            "OS/2" : font.os2.render()
        };

        return this.font.directory.render(tables);
    },
    cidToGidMap: function() {
        var out = BinaryStream(), len = 0;
        for (var cid = this.firstChar; cid < this.next; ++cid) {
            while (len < cid) {
                out.writeShort(0);
                len++;
            }
            var old_gid = this.ncid2ogid[cid];
            if (old_gid) {
                var new_gid = this.ogid2ngid[old_gid];
                out.writeShort(new_gid);
            } else {
                out.writeShort(0);
            }
            len++;
        }
        return out.get();
    }
};

function TTFFont(rawData, name) {
    var self = this;
    var data = self.contents = BinaryStream(rawData);
    if (data.readString(4) == "ttcf") {
        if (!name) {
            throw new Error("Must specify a name for TTC files");
        }
        var version = data.readLong();
        var numFonts = data.readLong();
        for (var i = 0; i < numFonts; ++i) {
            var offset = data.readLong();
            data.saveExcursion(function(){
                data.offset(offset);
                self.parse();
            });
            if (self.psName == name) {
                return;
            }
        }
        throw new Error("Font " + name + " not found in collection");
    } else {
        data.offset(0);
        self.parse();
    }
}

TTFFont.prototype = {
    parse: function() {
        var dir = this.directory = new Directory(this.contents);

        this.head = dir.readTable("head", HeadTable);
        this.loca = dir.readTable("loca", LocaTable);
        this.hhea = dir.readTable("hhea", HheaTable);
        this.maxp = dir.readTable("maxp", MaxpTable);
        this.hmtx = dir.readTable("hmtx", HmtxTable);
        this.glyf = dir.readTable("glyf", GlyfTable);
        this.name = dir.readTable("name", NameTable);
        this.post = dir.readTable("post", PostTable);
        this.cmap = dir.readTable("cmap", CmapTable);
        this.os2  = dir.readTable("OS/2", OS2Table);

        this.psName = this.name.postscriptName;
        this.ascent = this.os2.ascent || this.hhea.ascent;
        this.descent = this.os2.descent || this.hhea.descent;
        this.lineGap = this.os2.lineGap || this.hhea.lineGap;
        this.scale = 1000 / this.head.unitsPerEm;
    },
    widthOfGlyph: function(glyph) {
        return this.hmtx.forGlyph(glyph).advance * this.scale;
    },
    makeSubset: function() {
        return new Subfont(this);
    }
};

PDF.TTFFont = TTFFont;

})(window);

(function(kendo, $){

    "use strict";

    // WARNING: removing the following jshint declaration and turning
    // == into === to make JSHint happy will break functionality.
    /*jshint eqnull:true  */

    var drawing     = kendo.drawing;
    var geo         = kendo.geometry;
    var Color       = drawing.Color;

    var TEXT_RENDERING_MODE = kendo.pdf.TEXT_RENDERING_MODE;

    var DASH_PATTERNS = {
        dash           : [ 4 ],
        dashDot        : [ 4, 2, 1, 2 ],
        dot            : [ 1, 2 ],
        longDash       : [ 8, 2 ],
        longDashDot    : [ 8, 2, 1, 2 ],
        longDashDotDot : [ 8, 2, 1, 2, 1, 2 ],
        solid          : []
    };

    var LINE_CAP = {
        butt   : 0,
        round  : 1,
        square : 2
    };

    var LINE_JOIN = {
        miter : 0,
        round : 1,
        bevel : 2
    };

    function render(group, callback) {
        var fonts = [], images = [], options = group.options;

        function getOption(name, defval, hash) {
            if (!hash) {
                hash = options;
            }
            if (hash.pdf && hash.pdf[name] != null) {
                return hash.pdf[name];
            }
            return defval;
        }

        var multiPage = getOption("multiPage");

        group.traverse(function(element){
            dispatch({
                Image: function(element) {
                    if (images.indexOf(element.src()) < 0) {
                        images.push(element.src());
                    }
                },
                Text: function(element) {
                    var style = kendo.pdf.parseFontDef(element.options.font);
                    var url = kendo.pdf.getFontURL(style);
                    if (fonts.indexOf(url) < 0) {
                        fonts.push(url);
                    }
                }
            }, element);
        });

        function doIt() {
            if (--count > 0) {
                return;
            }

            var pdf = new (kendo.pdf.Document)({
                producer  : getOption("producer"),
                title     : getOption("title"),
                author    : getOption("author"),
                subject   : getOption("subject"),
                keywords  : getOption("keywords"),
                creator   : getOption("creator"),
                date      : getOption("date")
            });

            function drawPage(group) {
                var options = group.options;

                var tmp = optimize(group);
                var bbox = tmp.bbox;
                group = tmp.root;
                // var tmp, bbox;
                
                var paperSize = getOption("paperSize", getOption("paperSize", "auto"), options), addMargin = false;
                if (paperSize == "auto") {
                    if (bbox) {
                        var size = bbox.getSize();
                        paperSize = [ size.width, size.height ];
                        addMargin = true;
                        var origin = bbox.getOrigin();
                        tmp = new drawing.Group();
                        tmp.transform(new geo.Matrix(1, 0, 0, 1, -origin.x, -origin.y));
                        tmp.append(group);
                        group = tmp;
                    }
                    else {
                        paperSize = "A4";
                    }
                }

                var page;
                page = pdf.addPage({
                    paperSize : paperSize,
                    margin    : getOption("margin", getOption("margin"), options),
                    addMargin : addMargin,
                    landscape : getOption("landscape", getOption("landscape", false), options)
                });
                drawElement(group, page, pdf);
            }

            if (multiPage) {
                group.children.forEach(drawPage);
            } else {
                drawPage(group);
            }

            callback(pdf.render(), pdf);
        }

        var count = 2;
        kendo.pdf.loadFonts(fonts, doIt);
        kendo.pdf.loadImages(images, doIt);
    }

    function toDataURL(group, callback) {
        render(group, function(data){
            callback("data:application/pdf;base64," + data.base64());
        });
    }

    function toBlob(group, callback) {
        render(group, function(data){
            callback(new Blob([ data.get() ], { type: "application/pdf" }));
        });
    }

    function saveAs(group, filename, proxy, callback) {
        // XXX: Safari has Blob, but does not support the download attribute
        //      so we'd end up converting to dataURL and using the proxy anyway.
        if (window.Blob && !kendo.support.browser.safari) {
            toBlob(group, function(blob){
                kendo.saveAs({ dataURI: blob, fileName: filename });
                if (callback) {
                    callback(blob);
                }
            });
        } else {
            toDataURL(group, function(dataURL){
                kendo.saveAs({ dataURI: dataURL, fileName: filename, proxyURL: proxy });
                if (callback) {
                    callback(dataURL);
                }
            });
        }
    }

    function dispatch(handlers, element) {
        var handler = handlers[element.nodeType];
        if (handler) {
            return handler.call.apply(handler, arguments);
        }
        return element;
    }

    function drawElement(element, page, pdf) {
        if (element.options._pdfDebug) {
            page.comment("BEGIN: " + element.options._pdfDebug);
        }

        var transform = element.transform();
        var opacity = element.opacity();

        page.save();

        if (opacity != null && opacity < 1) {
            page.setOpacity(opacity);
        }

        setStrokeOptions(element, page, pdf);
        setFillOptions(element, page, pdf);
        setClipping(element, page, pdf);

        if (transform) {
            var m = transform.matrix();
            page.transform(m.a, m.b, m.c, m.d, m.e, m.f);
        }

        dispatch({
            Path      : drawPath,
            MultiPath : drawMultiPath,
            Circle    : drawCircle,
            Arc       : drawArc,
            Text      : drawText,
            Image     : drawImage,
            Group     : drawGroup
        }, element, page, pdf);

        page.restore();

        if (element.options._pdfDebug) {
            page.comment("END: " + element.options._pdfDebug);
        }
    }

    function setStrokeOptions(element, page, pdf) {
        var stroke = element.stroke && element.stroke();
        if (!stroke) {
            return;
        }

        var color = stroke.color;
        if (color) {
            color = parseColor(color);
            if (color == null) {
                return; // no stroke
            }
            page.setStrokeColor(color.r, color.g, color.b);
            if (color.a != 1) {
                page.setStrokeOpacity(color.a);
            }
        }

        var width = stroke.width;
        if (width != null) {
            if (width === 0) {
                return; // no stroke
            }
            page.setLineWidth(width);
        }

        var dashType = stroke.dashType;
        if (dashType) {
            page.setDashPattern(DASH_PATTERNS[dashType], 0);
        }

        var lineCap = stroke.lineCap;
        if (lineCap) {
            page.setLineCap(LINE_CAP[lineCap]);
        }

        var lineJoin = stroke.lineJoin;
        if (lineJoin) {
            page.setLineJoin(LINE_JOIN[lineJoin]);
        }

        var opacity = stroke.opacity;
        if (opacity != null) {
            page.setStrokeOpacity(opacity);
        }
    }

    function setFillOptions(element, page, pdf) {
        var fill = element.fill && element.fill();
        if (!fill) {
            return;
        }

        if (fill instanceof drawing.Gradient) {
            return;
        }

        var color = fill.color;
        if (color) {
            color = parseColor(color);
            if (color == null) {
                return; // no fill
            }
            page.setFillColor(color.r, color.g, color.b);
            if (color.a != 1) {
                page.setFillOpacity(color.a);
            }
        }

        var opacity = fill.opacity;
        if (opacity != null) {
            page.setFillOpacity(opacity);
        }
    }

    function setClipping(element, page, pdf) {
        // XXX: only Path supported at the moment.
        var clip = element.clip();
        if (clip) {
            _drawPath(clip, page, pdf);
            page.clip();
            // page.setStrokeColor(Math.random(), Math.random(), Math.random());
            // page.setLineWidth(1);
            // page.stroke();
        }
    }

    function shouldDraw(thing) {
        return (thing &&
                (thing instanceof drawing.Gradient ||
                 (thing.color && !/^(none|transparent)$/i.test(thing.color) &&
                  (thing.width == null || thing.width > 0) &&
                  (thing.opacity == null || thing.opacity > 0))));
    }

    function maybeGradient(element, page, pdf, stroke) {
        var fill = element.fill();
        if (fill instanceof drawing.Gradient) {
            if (stroke) {
                page.clipStroke();
            } else {
                page.clip();
            }
            var isRadial = fill instanceof drawing.RadialGradient;
            var start, end;
            if (isRadial) {
                start = { x: fill.center().x , y: fill.center().y , r: 0 };
                end   = { x: fill.center().x , y: fill.center().y , r: fill.radius() };
            } else {
                start = { x: fill.start().x , y: fill.start().y };
                end   = { x: fill.end().x   , y: fill.end().y   };
            }
            var gradient = {
                type: isRadial ? "radial" : "linear",
                start: start,
                end: end,
                userSpace: fill.userSpace(),
                stops: fill.stops.elements().map(function(stop){
                    var offset = stop.offset();
                    if (/%$/.test(offset)) {
                        offset = parseFloat(offset) / 100;
                    } else {
                        offset = parseFloat(offset);
                    }
                    var color = parseColor(stop.color());
                    color.a *= stop.opacity();
                    return {
                        offset: offset,
                        color: color
                    };
                })
            };
            var box = element.rawBBox();
            var tl = box.topLeft(), size = box.getSize();
            box = {
                left   : tl.x,
                top    : tl.y,
                width  : size.width,
                height : size.height
            };
            page.gradient(gradient, box);
            return true;
        }
    }

    function maybeFillStroke(element, page, pdf) {
        if (shouldDraw(element.fill()) && shouldDraw(element.stroke())) {
            if (!maybeGradient(element, page, pdf, true)) {
                page.fillStroke();
            }
        } else if (shouldDraw(element.fill())) {
            if (!maybeGradient(element, page, pdf, false)) {
                page.fill();
            }
        } else if (shouldDraw(element.stroke())) {
            page.stroke();
        } else {
            // we should not get here; the path should have been
            // optimized away.  but let's be prepared.
            page.nop();
        }
    }

    function maybeDrawRect(path, page, pdf) {
        var segments = path.segments;
        if (segments.length == 4 && path.options.closed) {
            // detect if this path looks like a rectangle parallel to the axis
            var a = [];
            for (var i = 0; i < segments.length; ++i) {
                if (segments[i].controlIn()) { // has curve?
                    return false;
                }
                a[i] = segments[i].anchor();
            }
            // it's a rectangle if the y/x/y/x or x/y/x/y coords of
            // consecutive points are the same.
            var isRect = (
                a[0].y == a[1].y && a[1].x == a[2].x && a[2].y == a[3].y && a[3].x == a[0].x
            ) || (
                a[0].x == a[1].x && a[1].y == a[2].y && a[2].x == a[3].x && a[3].y == a[0].y
            );
            if (isRect) {
                // this saves a bunch of instructions in PDF:
                // moveTo, lineTo, lineTo, lineTo, close -> rect.
                page.rect(a[0].x, a[0].y,
                          a[2].x - a[0].x /*width*/,
                          a[2].y - a[0].y /*height*/);
                return true;
            }
        }
    }

    function _drawPath(element, page, pdf) {
        var segments = element.segments;
        if (segments.length === 0) {
            return;
        }
        if (!maybeDrawRect(element, page, pdf)) {
            for (var prev, i = 0; i < segments.length; ++i) {
                var seg = segments[i];
                var anchor = seg.anchor();
                if (!prev) {
                    page.moveTo(anchor.x, anchor.y);
                } else {
                    var prevOut = prev.controlOut();
                    var controlIn = seg.controlIn();
                    if (prevOut && controlIn) {
                        page.bezier(
                            prevOut.x   , prevOut.y,
                            controlIn.x , controlIn.y,
                            anchor.x    , anchor.y
                        );
                    } else {
                        page.lineTo(anchor.x, anchor.y);
                    }
                }
                prev = seg;
            }
            if (element.options.closed) {
                page.close();
            }
        }
    }

    function drawPath(element, page, pdf) {
        _drawPath(element, page, pdf);
        maybeFillStroke(element, page, pdf);
    }

    function drawMultiPath(element, page, pdf) {
        var paths = element.paths;
        for (var i = 0; i < paths.length; ++i) {
            _drawPath(paths[i], page, pdf);
        }
        maybeFillStroke(element, page, pdf);
    }

    function drawCircle(element, page, pdf) {
        var g = element.geometry();
        page.circle(g.center.x, g.center.y, g.radius);
        maybeFillStroke(element, page, pdf);
    }

    function drawArc(element, page, pdf) {
        var points = element.geometry().curvePoints();
        page.moveTo(points[0].x, points[0].y);
        for (var i = 1; i < points.length;) {
            page.bezier(
                points[i].x, points[i++].y,
                points[i].x, points[i++].y,
                points[i].x, points[i++].y
            );
        }
        maybeFillStroke(element, page, pdf);
    }

    function drawText(element, page, pdf) {
        var style = kendo.pdf.parseFontDef(element.options.font);
        var pos = element._position;
        var mode;
        if (element.fill() && element.stroke()) {
            mode = TEXT_RENDERING_MODE.fillAndStroke;
        } else if (element.fill()) {
            mode = TEXT_RENDERING_MODE.fill;
        } else if (element.stroke()) {
            mode = TEXT_RENDERING_MODE.stroke;
        }

        page.transform(1, 0, 0, -1, pos.x, pos.y + style.fontSize);
        page.beginText();
        page.setFont(kendo.pdf.getFontURL(style), style.fontSize);
        page.setTextRenderingMode(mode);
        page.showText(element.content(), element._pdfRect ? element._pdfRect.width() : null);
        page.endText();
    }

    function drawGroup(element, page, pdf) {
        if (element._pdfLink) {
            page.addLink(element._pdfLink.url, element._pdfLink);
        }
        var children = element.children;
        for (var i = 0; i < children.length; ++i) {
            drawElement(children[i], page, pdf);
        }
    }

    function drawImage(element, page, pdf) {
        var url = element.src();
        if (!url) {
            return;
        }

        var rect = element.rect();
        var tl = rect.getOrigin();
        var sz = rect.getSize();
        page.transform(sz.width, 0, 0, -sz.height, tl.x, tl.y + sz.height);
        page.drawImage(url);
    }

    function exportPDF(group, options) {
        var defer = $.Deferred();

        for (var i in options) {
            group.options.set("pdf." + i, options[i]);
        }

        drawing.pdf.toDataURL(group, defer.resolve);

        return defer.promise();
    }

    function parseColor(x) {
        var color = kendo.parseColor(x, true);
        return color ? color.toRGB() : null;
    }

    function optimize(root) {
        var clipbox = false;
        var matrix = geo.Matrix.unit();
        var currentBox = null;
        var changed;
        do {
            changed = false;
            root = opt(root);
        } while (root && changed);
        return { root: root, bbox: currentBox };

        function change(newShape) {
            changed = true;
            return newShape;
        }

        function visible(shape) {
            return (shape.visible() && shape.opacity() > 0 &&
                    ( shouldDraw(shape.fill()) ||
                      shouldDraw(shape.stroke()) ));
        }

        function optArray(a) {
            var b = [];
            for (var i = 0; i < a.length; ++i) {
                var el = opt(a[i]);
                if (el != null) {
                    b.push(el);
                }
            }
            return b;
        }

        function withClipping(shape, f) {
            var saveclipbox = clipbox;
            var savematrix = matrix;

            if (shape.transform()) {
                matrix = matrix.multiplyCopy(shape.transform().matrix());
            }

            var clip = shape.clip();
            if (clip) {
                clip = clip.bbox();
                if (clip) {
                    clip = clip.bbox(matrix);
                    clipbox = clipbox ? geo.Rect.intersect(clipbox, clip) : clip;
                }
            }

            try {
                return f();
            }
            finally {
                clipbox = saveclipbox;
                matrix = savematrix;
            }
        }

        function inClipbox(shape) {
            if (clipbox == null) {
                return false;
            }
            var box = shape.rawBBox().bbox(matrix);
            if (clipbox && box) {
                box = geo.Rect.intersect(box, clipbox);
            }
            return box;
        }

        function opt(shape) {
            return withClipping(shape, function(){
                if (!(shape instanceof drawing.Group || shape instanceof drawing.MultiPath)) {
                    var box = inClipbox(shape);
                    if (!box) {
                        return change(null);
                    }
                    currentBox = currentBox ? geo.Rect.union(currentBox, box) : box;
                }
                return dispatch({
                    Path: function(shape) {
                        if (shape.segments.length === 0 || !visible(shape)) {
                            return change(null);
                        }
                        return shape;
                    },
                    MultiPath: function(shape) {
                        if (!visible(shape)) {
                            return change(null);
                        }
                        var el = new drawing.MultiPath(shape.options);
                        el.paths = optArray(shape.paths);
                        if (el.paths.length === 0) {
                            return change(null);
                        }
                        return el;
                    },
                    Circle: function(shape) {
                        if (!visible(shape)) {
                            return change(null);
                        }
                        return shape;
                    },
                    Arc: function(shape) {
                        if (!visible(shape)) {
                            return change(null);
                        }
                        return shape;
                    },
                    Text: function(shape) {
                        if (!/\S/.test(shape.content()) || !visible(shape)) {
                            return change(null);
                        }
                        return shape;
                    },
                    Image: function(shape) {
                        if (!(shape.visible() && shape.opacity() > 0)) {
                            return change(null);
                        }
                        return shape;
                    },
                    Group: function(shape) {
                        var el = new drawing.Group(shape.options);
                        el.children = optArray(shape.children);
                        el._pdfLink = shape._pdfLink;
                        if (shape !== root && el.children.length === 0 && !shape._pdfLink) {
                            return change(null);
                        }
                        return el;
                    }
                }, shape);
            });
        }
    }

    kendo.deepExtend(drawing, {
        exportPDF: exportPDF,

        pdf: {
            toDataURL  : toDataURL,
            toBlob     : toBlob,
            saveAs     : saveAs,
            toStream   : render
        }
    });

})(window.kendo, window.kendo.jQuery);



(function(kendo){

kendo.PDFMixin = {
    extend: function(proto) {
        proto.events.push("pdfExport");
        proto.options.pdf = this.options;
        proto.saveAsPDF = this.saveAsPDF;
        proto._drawPDF = this._drawPDF;
        proto._drawPDFShadow = this._drawPDFShadow;
    },
    options: {
        fileName  : "Export.pdf",
        proxyURL  : "",

        // paperSize can be an usual name, i.e. "A4", or an array of two Number-s specifying the
        // width/height in points (1pt = 1/72in), or strings including unit, i.e. "10mm".  Supported
        // units are "mm", "cm", "in" and "pt".  The default "auto" means paper size is determined
        // by content.
        paperSize : "auto",

        // Export all pages, if applicable
        allPages: false,

        // True to reverse the paper dimensions if needed such that width is the larger edge.
        landscape : false,

        // An object containing { left, top, bottom, right } margins with units.
        margin    : null,

        // Optional information for the PDF Info dictionary; all strings except for the date.
        title     : null,
        author    : null,
        subject   : null,
        keywords  : null,
        creator   : "Kendo UI PDF Generator",

        // Creation Date; defaults to new Date()
        date      : null
    },

    saveAsPDF: function() {
        var progress = new $.Deferred();
        var promise = progress.promise();
        var args = { promise: promise };

        if (this.trigger("pdfExport", args)) {
            return;
        }

        var options = this.options.pdf;
        options.multiPage = options.allPages;

        this._drawPDF(progress)
        .then(function(root) {
            return kendo.drawing.exportPDF(root, options);
        })
        .done(function(dataURI) {
            kendo.saveAs({
                dataURI: dataURI,
                fileName: options.fileName,
                proxyURL: options.proxyURL,
                forceProxy: options.forceProxy
            });

            progress.resolve();
        })
        .fail(function(err) {
            progress.reject(err);
        });

        return promise;
    },

    _drawPDF: function() {
        return kendo.drawing.drawDOM(this.wrapper);
    },

    _drawPDFShadow: function(content) {
        var wrapper = this.wrapper;
        var shadow = $("<div class='k-pdf-export-shadow'>")
                     .css("width", wrapper.width());

        wrapper.before(shadow);
        shadow.append(content || wrapper.clone(true, true));

        var promise = kendo.drawing.drawDOM(shadow);
        promise.always(function() {
            shadow.remove();
        });

        return promise;
    }
};

})(kendo);

return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });