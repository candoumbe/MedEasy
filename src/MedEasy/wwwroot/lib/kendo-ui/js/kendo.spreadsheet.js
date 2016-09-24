/*
* Kendo UI v2015.3.1214 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI commercial licenses may be obtained at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
* If you do not own a commercial license, this file shall be governed by the trial license terms.
*/
(function(f, define){
    define([ "./kendo.core", "./kendo.binder", "./kendo.window", "./kendo.list", "./kendo.tabstrip", "./kendo.color", "./kendo.sortable", "./kendo.menu", "./kendo.data", "./kendo.dom", "./kendo.toolbar", "./kendo.colorpicker", "./kendo.combobox", "./kendo.dropdownlist", "./kendo.popup", "./kendo.treeview" ], f);
})(function(){

(function(){

(function(kendo) {
    var UndoRedoStack = kendo.Observable.extend({
        init: function(options) {
            kendo.Observable.fn.init.call(this, options);
            this.clear();
        },
        events: [ "undo", "redo" ],
        push: function (command) {
            this.stack = this.stack.slice(0, this.currentCommandIndex + 1);
            this.currentCommandIndex = this.stack.push(command) - 1;
        },
        undo: function () {
            if (this.canUndo()) {
                var command = this.stack[this.currentCommandIndex--];
                command.undo();
                this.trigger("undo", { command: command });
            }
        },
        redo: function () {
            if (this.canRedo()) {
                var command = this.stack[++this.currentCommandIndex];
                command.redo();
                this.trigger("redo", { command: command });
            }
        },
        clear: function() {
            this.stack = [];
            this.currentCommandIndex = -1;
        },
        canUndo: function () {
            return this.currentCommandIndex >= 0;
        },
        canRedo: function () {
            return this.currentCommandIndex != this.stack.length - 1;
        }
    });

    kendo.deepExtend(kendo, {
        util: {
            UndoRedoStack: UndoRedoStack
        }
    });
})(kendo);
})();

(function() {

(function () {
    // Imports ================================================================
    var math = Math,
        kendo = window.kendo,
        deepExtend = kendo.deepExtend;

    // Constants
    var DEG_TO_RAD = math.PI / 180,
        MAX_NUM = Number.MAX_VALUE,
        MIN_NUM = -Number.MAX_VALUE,
        UNDEFINED = "undefined";

    // Generic utility functions ==============================================
    function defined(value) {
        return typeof value !== UNDEFINED;
    }

    function round(value, precision) {
        var power = pow(precision);
        return math.round(value * power) / power;
    }

    // Extracted from round to get on the V8 "fast path"
    function pow(p) {
        if (p) {
            return math.pow(10, p);
        } else {
            return 1;
        }
    }

    function limitValue(value, min, max) {
        return math.max(math.min(value, max), min);
    }

    function rad(degrees) {
        return degrees * DEG_TO_RAD;
    }

    function deg(radians) {
        return radians / DEG_TO_RAD;
    }

    function isNumber(val) {
        return typeof val === "number" && !isNaN(val);
    }

    function valueOrDefault(value, defaultValue) {
        return defined(value) ? value : defaultValue;
    }

    function sqr(value) {
        return value * value;
    }

    function objectKey(object) {
        var parts = [];
        for (var key in object) {
            parts.push(key + object[key]);
        }

        return parts.sort().join("");
    }

    // Computes FNV-1 hash
    // See http://en.wikipedia.org/wiki/Fowler%E2%80%93Noll%E2%80%93Vo_hash_function
    function hashKey(str) {
        // 32-bit FNV-1 offset basis
        // See http://isthe.com/chongo/tech/comp/fnv/#FNV-param
        var hash = 0x811C9DC5;

        for (var i = 0; i < str.length; ++i)
        {
            hash += (hash << 1) + (hash << 4) + (hash << 7) + (hash << 8) + (hash << 24);
            hash ^= str.charCodeAt(i);
        }

        return hash >>> 0;
    }

    function hashObject(object) {
        return hashKey(objectKey(object));
    }

    var now = Date.now;
    if (!now) {
        now = function() {
            return new Date().getTime();
        };
    }

    // Array helpers ==========================================================
    function arrayLimits(arr) {
        var length = arr.length,
            i,
            min = MAX_NUM,
            max = MIN_NUM;

        for (i = 0; i < length; i ++) {
            max = math.max(max, arr[i]);
            min = math.min(min, arr[i]);
        }

        return {
            min: min,
            max: max
        };
    }

    function arrayMin(arr) {
        return arrayLimits(arr).min;
    }

    function arrayMax(arr) {
        return arrayLimits(arr).max;
    }

    function sparseArrayMin(arr) {
        return sparseArrayLimits(arr).min;
    }

    function sparseArrayMax(arr) {
        return sparseArrayLimits(arr).max;
    }

    function sparseArrayLimits(arr) {
        var min = MAX_NUM,
            max = MIN_NUM;

        for (var i = 0, length = arr.length; i < length; i++) {
            var n = arr[i];
            if (n !== null && isFinite(n)) {
                min = math.min(min, n);
                max = math.max(max, n);
            }
        }

        return {
            min: min === MAX_NUM ? undefined : min,
            max: max === MIN_NUM ? undefined : max
        };
    }

    function last(array) {
        if (array) {
            return array[array.length - 1];
        }
    }

    function append(first, second) {
        first.push.apply(first, second);
        return first;
    }

    // Template helpers =======================================================
    function renderTemplate(text) {
        return kendo.template(text, { useWithBlock: false, paramName: "d" });
    }

    function renderAttr(name, value) {
        return (defined(value) && value !== null) ? " " + name + "='" + value + "' " : "";
    }

    function renderAllAttr(attrs) {
        var output = "";
        for (var i = 0; i < attrs.length; i++) {
            output += renderAttr(attrs[i][0], attrs[i][1]);
        }

        return output;
    }

    function renderStyle(attrs) {
        var output = "";
        for (var i = 0; i < attrs.length; i++) {
            var value = attrs[i][1];
            if (defined(value)) {
                output += attrs[i][0] + ":" + value + ";";
            }
        }

        if (output !== "") {
            return output;
        }
    }

    function renderSize(size) {
        if (typeof size !== "string") {
            size += "px";
        }

        return size;
    }

    function renderPos(pos) {
        var result = [];

        if (pos) {
            var parts = kendo.toHyphens(pos).split("-");

            for (var i = 0; i < parts.length; i++) {
                result.push("k-pos-" + parts[i]);
            }
        }

        return result.join(" ");
    }

    function isTransparent(color) {
        return color === "" || color === null || color === "none" || color === "transparent" || !defined(color);
    }

    function arabicToRoman(n) {
        var literals = {
            1    : "i",       10   : "x",       100  : "c",
            2    : "ii",      20   : "xx",      200  : "cc",
            3    : "iii",     30   : "xxx",     300  : "ccc",
            4    : "iv",      40   : "xl",      400  : "cd",
            5    : "v",       50   : "l",       500  : "d",
            6    : "vi",      60   : "lx",      600  : "dc",
            7    : "vii",     70   : "lxx",     700  : "dcc",
            8    : "viii",    80   : "lxxx",    800  : "dccc",
            9    : "ix",      90   : "xc",      900  : "cm",
            1000 : "m"
        };
        var values = [ 1000,
                       900 , 800, 700, 600, 500, 400, 300, 200, 100,
                       90  , 80 , 70 , 60 , 50 , 40 , 30 , 20 , 10 ,
                       9   , 8  , 7  , 6  , 5  , 4  , 3  , 2  , 1 ];
        var roman = "";
        while (n > 0) {
            if (n < values[0]) {
                values.shift();
            } else {
                roman += literals[values[0]];
                n -= values[0];
            }
        }
        return roman;
    }

    function romanToArabic(r) {
        r = r.toLowerCase();
        var digits = {
            i: 1,
            v: 5,
            x: 10,
            l: 50,
            c: 100,
            d: 500,
            m: 1000
        };
        var value = 0, prev = 0;
        for (var i = 0; i < r.length; ++i) {
            var v = digits[r.charAt(i)];
            if (!v) {
                return null;
            }
            value += v;
            if (v > prev) {
                value -= 2 * prev;
            }
            prev = v;
        }
        return value;
    }

    function memoize(f) {
        var cache = Object.create(null);
        return function() {
            var id = "";
            for (var i = arguments.length; --i >= 0;) {
                id += ":" + arguments[i];
            }
            if (id in cache) {
                return cache[id];
            }
            return f.apply(this, arguments);
        };
    }

    // Exports ================================================================
    deepExtend(kendo, {
        util: {
            MAX_NUM: MAX_NUM,
            MIN_NUM: MIN_NUM,

            append: append,
            arrayLimits: arrayLimits,
            arrayMin: arrayMin,
            arrayMax: arrayMax,
            defined: defined,
            deg: deg,
            hashKey: hashKey,
            hashObject: hashObject,
            isNumber: isNumber,
            isTransparent: isTransparent,
            last: last,
            limitValue: limitValue,
            now: now,
            objectKey: objectKey,
            round: round,
            rad: rad,
            renderAttr: renderAttr,
            renderAllAttr: renderAllAttr,
            renderPos: renderPos,
            renderSize: renderSize,
            renderStyle: renderStyle,
            renderTemplate: renderTemplate,
            sparseArrayLimits: sparseArrayLimits,
            sparseArrayMin: sparseArrayMin,
            sparseArrayMax: sparseArrayMax,
            sqr: sqr,
            valueOrDefault: valueOrDefault,
            romanToArabic: romanToArabic,
            arabicToRoman: arabicToRoman,
            memoize: memoize
        }
    });

    kendo.drawing.util = kendo.util;
    kendo.dataviz.util = kendo.util;

})();



})();

(function(){

(function ($) {

    // Imports =================================================================
    var doc = document,

        kendo = window.kendo,
        Class = kendo.Class,

        util = kendo.util,
        defined = util.defined;

    // Text metrics calculations ===============================================
    var LRUCache = Class.extend({
        init: function(size) {
            this._size = size;
            this._length = 0;
            this._map = {};
        },

        put: function(key, value) {
            var lru = this,
                map = lru._map,
                entry = { key: key, value: value };

            map[key] = entry;

            if (!lru._head) {
                lru._head = lru._tail = entry;
            } else {
                lru._tail.newer = entry;
                entry.older = lru._tail;
                lru._tail = entry;
            }

            if (lru._length >= lru._size) {
                map[lru._head.key] = null;
                lru._head = lru._head.newer;
                lru._head.older = null;
            } else {
                lru._length++;
            }
        },

        get: function(key) {
            var lru = this,
                entry = lru._map[key];

            if (entry) {
                if (entry === lru._head && entry !== lru._tail) {
                    lru._head = entry.newer;
                    lru._head.older = null;
                }

                if (entry !== lru._tail) {
                    if (entry.older) {
                        entry.older.newer = entry.newer;
                        entry.newer.older = entry.older;
                    }

                    entry.older = lru._tail;
                    entry.newer = null;

                    lru._tail.newer = entry;
                    lru._tail = entry;
                }

                return entry.value;
            }
        }
    });

    var defaultMeasureBox = $("<div style='position: absolute !important; top: -4000px !important; width: auto !important; height: auto !important;" +
                      "padding: 0 !important; margin: 0 !important; border: 0 !important;" +
                      "line-height: normal !important; visibility: hidden !important; white-space: nowrap!important;' />")[0];

    var TextMetrics = Class.extend({
        init: function(options) {
            this._cache = new LRUCache(1000);
            this._initOptions(options);
        },

        options: {
            baselineMarkerSize: 1
        },

        measure: function(text, style, box) {
            var styleKey = util.objectKey(style),
                cacheKey = util.hashKey(text + styleKey),
                cachedResult = this._cache.get(cacheKey);

            if (cachedResult) {
                return cachedResult;
            }

            var size = { width: 0, height: 0, baseline: 0 };

            var measureBox = box ? box : defaultMeasureBox;
            var baselineMarker = this._baselineMarker().cloneNode(false);

            for (var key in style) {
                var value = style[key];
                if (defined(value)) {
                    measureBox.style[key] = value;
                }
            }

            $(measureBox).text(text);
            measureBox.appendChild(baselineMarker);
            doc.body.appendChild(measureBox);

            if ((text + "").length) {
                size.width = measureBox.offsetWidth - this.options.baselineMarkerSize;
                size.height = measureBox.offsetHeight;
                size.baseline = baselineMarker.offsetTop + this.options.baselineMarkerSize;
            }

            if (size.width > 0 && size.height > 0) {
                this._cache.put(cacheKey, size);
            }

            measureBox.parentNode.removeChild(measureBox);

            return size;
        },

        _baselineMarker: function() {
            return $("<div class='k-baseline-marker' " +
              "style='display: inline-block; vertical-align: baseline;" +
              "width: " + this.options.baselineMarkerSize + "px; height: " + this.options.baselineMarkerSize + "px;" +
              "overflow: hidden;' />")[0];
        }

    });

    TextMetrics.current = new TextMetrics();

    function measureText(text, style, measureBox) {
        return TextMetrics.current.measure(text, style, measureBox);
    }

    // Exports ================================================================
    kendo.util.TextMetrics = TextMetrics;
    kendo.util.LRUCache = LRUCache;
    kendo.util.measureText = measureText;

})(window.kendo.jQuery);

})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;

    var Command = kendo.spreadsheet.Command = kendo.Class.extend({
        init: function(options) {
            this.options = options;
            this._workbook = options.workbook;
            this._property = options && options.property;
            this._state = {};
        },
        range: function(range) {
            if (range !== undefined) {
                this._range = range;
            }

            return this._range;
        },
        redo: function() {
            this.exec();
        },
        undo: function() {
            this.range().setState(this._state);
        },
        getState: function() {
            this._state = this.range().getState(this._property);
        },
        _forEachCell: function(callback) {
            var range = this.range();
            var ref = range._ref;

            ref.forEach(function(ref) {
                range.sheet().forEach(ref.toRangeRef(), callback.bind(this));
            }.bind(this));
        }
    });

    var PropertyChangeCommand = kendo.spreadsheet.PropertyChangeCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this._value = options.value;
        },
        exec: function() {
            var range = this.range();
            this.getState();
            range[this._property](this._value);
        }
    });

    kendo.spreadsheet.ClearContentCommand = Command.extend({
        exec: function() {
            this.getState();
            this.range().clearContent();
        }
    });

    kendo.spreadsheet.EditCommand = PropertyChangeCommand.extend({
        init: function(options) {
            options.property = "input";
            PropertyChangeCommand.fn.init.call(this, options);
        },
        rejectState: function(validationState) {
            this.undo();

            return {
                title: validationState.title,
                body: validationState.message,
                reason: "error"
            };
        },
        exec: function() {
            var range = this.range();
            var value = this._value;
            this.getState();
            try {
                range.input(value);

                var validationState = range._getValidationState();
                if (validationState) {
                    return this.rejectState(validationState);
                }
            } catch(ex1) {
                if (ex1 instanceof kendo.spreadsheet.calc.ParseError) {
                    // it's a formula. maybe a closing paren fixes it?
                    try {
                        range.input(value + ")");

                        var validationState = range._getValidationState();
                        if (validationState) {
                            return this.rejectState(validationState);
                        }
                    } catch(ex2) {
                        if (ex2 instanceof kendo.spreadsheet.calc.ParseError) {
                            range.input("'" + value);

                            return {
                                title : "Error in formula",
                                body  : ex1+"",
                                reason: "error"
                            };
                        }
                    }
                } else {
                    throw ex1;
                }
            }
        }
    });

    kendo.spreadsheet.TextWrapCommand = PropertyChangeCommand.extend({
        init: function(options) {
            options.property = "wrap";
            PropertyChangeCommand.fn.init.call(this, options);

            this._value = options.value;
        },
        getState: function() {
            var rowHeight = {};
            this.range().forEachRow(function(range) {
                var index = range.topLeft().row;

                rowHeight[index] = range.sheet().rowHeight(index);
            });

            this._state = this.range().getState(this._property);
            this._rowHeight = rowHeight;
        },
        undo: function() {
            var sheet = this.range().sheet();
            var rowHeight = this._rowHeight;

            this.range().setState(this._state);

            for (var row in rowHeight) {
                sheet.rowHeight(row, rowHeight[row]);
            }
        }
    });

    kendo.spreadsheet.AdjustDecimalsCommand = Command.extend({
        init: function(options) {
            this._decimals = options.decimals;
            options.property = "format";
            Command.fn.init.call(this, options);
        },
        exec: function() {
            var sheet = this.range().sheet();
            var decimals = this._decimals;
            var formatting = kendo.spreadsheet.formatting;

            this.getState();

            sheet.batch(function() {
                this.range().forEachCell(function(row, col, cell) {
                    var format = cell.format;

                    if (format || decimals > 0) {
                        format = formatting.adjustDecimals(format || "#", decimals);
                        sheet.range(row, col).format(format);
                    }
                });
            }.bind(this));
        }
    });

    kendo.spreadsheet.BorderChangeCommand = Command.extend({
        init: function(options) {
            options.property = "border";
            Command.fn.init.call(this, options);
            this._type = options.border;
            this._style = options.style;
        },
        exec: function() {
            this.getState();
            this[this._type](this._style);
        },
        noBorders: function() {
            var range = this.range();
            range.sheet().batch(function() {
                range.borderLeft(null).borderTop(null).borderRight(null).borderBottom(null);
            }.bind(this), {});
        },
        allBorders: function(style) {
            var range = this.range();
            range.sheet().batch(function() {
                range.borderLeft(style).borderTop(style).borderRight(style).borderBottom(style);
            }.bind(this), {});
        },
        leftBorder: function(style) {
            this.range().leftColumn().borderLeft(style);
        },
        rightBorder: function(style) {
            this.range().rightColumn().borderRight(style);
        },
        topBorder: function(style) {
            this.range().topRow().borderTop(style);
        },
        bottomBorder: function(style) {
            this.range().bottomRow().borderBottom(style);
        },
        outsideBorders: function(style) {
            var range = this.range();
            range.sheet().batch(function() {
                range.leftColumn().borderLeft(style);
                range.topRow().borderTop(style);
                range.rightColumn().borderRight(style);
                range.bottomRow().borderBottom(style);
            }.bind(this), {});
        },
        insideBorders: function(style) {
            this.range().sheet().batch(function() {
                this.allBorders(style);
                this.outsideBorders(null);
            }.bind(this), {});
        },
        insideHorizontalBorders: function(style) {
            var range = this.range();

            range.sheet().batch(function() {
                range.borderBottom(style);
                range.bottomRow().borderBottom(null);
            }.bind(this), {});
        },
        insideVerticalBorders: function(style) {
            var range = this.range();

            range.sheet().batch(function() {
                range.borderRight(style);
                range.rightColumn().borderRight(null);
            }.bind(this), {});
        }
    });

    kendo.spreadsheet.MergeCellCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this._type = options.value;
        },
        exec: function() {
            this.getState();
            this[this._type]();
        },
        activate: function(ref) {
            this.range().sheet().activeCell(ref);
        },
        getState: function() {
            this._state = this.range().getState();
        },
        undo: function() {
            if (this._type !== "unmerge") {
                this.range().unmerge();
                this.activate(this.range().topLeft());
            }
            this.range().setState(this._state);
        },
        cells: function() {
            var range = this.range();
            var ref = range._ref;

            range.merge();
            this.activate(ref);
        },
        horizontally: function() {
            var ref = this.range().topRow()._ref;

            this.range().forEachRow(function(range) {
                range.merge();
            });

            this.activate(ref);
        },
        vertically: function() {
            var ref = this.range().leftColumn()._ref;

            this.range().forEachColumn(function(range) {
                range.merge();
            });

            this.activate(ref);
        },
        unmerge: function() {
            var range = this.range();
            var ref = range._ref.topLeft;

            range.unmerge();
            this.activate(ref);
        }
    });

    kendo.spreadsheet.FreezePanesCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this._type = options.value;
        },
        exec: function() {
            this.getState();
            this._topLeft = this.range().topLeft();
            this[this._type]();
        },
        getState: function() {
            this._state = this.range().sheet().getState();
        },
        undo: function() {
            this.range().sheet().setState(this._state);
        },
        panes: function() {
            var topLeft = this._topLeft;
            var sheet = this.range().sheet();

            sheet.frozenColumns(topLeft.col).frozenRows(topLeft.row);
        },
        rows: function() {
            var topLeft = this._topLeft;
            var sheet = this.range().sheet();

            sheet.frozenRows(topLeft.row);
        },
        columns: function() {
            var topLeft = this._topLeft;
            var sheet = this.range().sheet();

            sheet.frozenColumns(topLeft.col);
        },
        unfreeze: function() {
            var sheet = this.range().sheet();
            sheet.frozenRows(0).frozenColumns(0);
        }
    });

    kendo.spreadsheet.PasteCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this._clipboard = this._workbook.clipboard();
        },
        getState: function() {
            this._range = this._workbook.activeSheet().range(this._clipboard.pasteRef());
            this._state = this._range.getState();
        },
        exec: function() {
            var status = this._clipboard.canPaste();
            this._clipboard.menuInvoked = true;
            if(!status.canPaste) {
                if(status.menuInvoked) {
                    return { reason: "useKeyboard" };
                }
                if(status.pasteOnMerged) {
                    return { reason: "modifyMerged" };
                }
                return;
            }
            this.getState();
            this._clipboard.paste();
            this._workbook.execute({
                command: "AdjustRowHeightCommand",
                options: {
                    range: this._workbook.activeSheet().range(this._clipboard.pasteRef())
                }
            });
        }
    });

    kendo.spreadsheet.AdjustRowHeightCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            var sheet = this._workbook.activeSheet();
            this._workingRange = options.range || sheet.range(options.rowIndex);
        },
        exec: function() {
            var sheet = this._workbook.activeSheet();
            var range = this._workingRange;
            var state = range.getState();
            var mergedCells = [];
            for(var i=0; i < state.mergedCells.length; i++) {
                mergedCells.push(sheet.range(state.mergedCells[i]));
            }
            range.forEachRow(function(row) {
                var maxHeight = row.sheet().rowHeight(row.topLeft().row);
                row.forEachCell(function(rowIndex, colIndex, cell) {
                    var cellRange = sheet.range(rowIndex, colIndex);
                    var totalWidth = 0;
                    for(var i = 0; i < mergedCells.length; i++) {
                        if(cellRange._ref.intersects(mergedCells[i]._ref)) {
                            totalWidth += cell.width;
                            break;
                        }
                    }
                    var width = Math.max(sheet.columnWidth(colIndex), totalWidth);
                    maxHeight = Math.max(maxHeight, kendo.spreadsheet.util.getTextHeight(cell.value, width, cell.fontSize, cell.wrap));
                });
                sheet.rowHeight(row.topLeft().row, Math.max(sheet.rowHeight(row.topLeft().row), maxHeight));
            });
        }
    });

    kendo.spreadsheet.ToolbarPasteCommand = Command.extend({
        exec: function() {
            if(kendo.support.clipboard.paste) {
                this._workbook._view.clipboard.focus().select();
                //reason : focusclipbord
                document.execCommand('paste');
            } else {
                return { reason: "useKeyboard" };
            }
        }
    });

    kendo.spreadsheet.CopyCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this._clipboard = options.workbook.clipboard();
        },
        undo: $.noop,
        exec: function() {
            var status = this._clipboard.canCopy();
            this._clipboard.menuInvoked = true;
            if(!status.canCopy) {
                if(status.menuInvoked) {
                    return { reason: "useKeyboard" };
                } else if(status.multiSelection) {
                    return { reason: "unsupportedSelection" };
                }
                return;
            }
            this._clipboard.copy();
        }
    });

    kendo.spreadsheet.ToolbarCopyCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this._clipboard = options.workbook.clipboard();
        },
        undo: $.noop,
        exec: function() {
            if(kendo.support.clipboard.copy) {
                var clipboard = this._workbook._view.clipboard;
                var textarea = document.createElement('textarea');
                $(textarea).addClass("k-spreadsheet-clipboard").val(clipboard.html()).appendTo(document.body).focus().select();
                document.execCommand('copy');
                clipboard.trigger("copy");
                $(textarea).remove();
            } else {
                return { reason: "useKeyboard" };
            }
        }
    });

    kendo.spreadsheet.CutCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this._clipboard = options.workbook.clipboard();
        },
        exec: function() {
            if(this._clipboard.canCopy()) {
                this.getState();
                this._clipboard.cut();
            }
        }
    });

    kendo.spreadsheet.AutoFillCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
        },
        origin: function(origin) {
            this._origin = origin;
        },
        exec: function() {
            this.getState();
            this._range.fillFrom(this._origin);
        }
    });

    kendo.spreadsheet.ToolbarCutCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this._clipboard = options.workbook.clipboard();
        },
        exec: function() {
            if(kendo.support.clipboard.copy) {
                var clipboard = this._workbook._view.clipboard;
                var textarea = document.createElement('textarea');
                $(textarea).val(clipboard.html()).appendTo(document.body).focus().select();
                document.execCommand('copy');
                clipboard.trigger("cut");
                $(textarea).remove();
            } else {
                return { reason: "useKeyboard" };
            }
        }
    });

    kendo.spreadsheet.FilterCommand = Command.extend({
        undo: function() {
            this.range().filter(this._state);
        },
        exec: function() {
            var range = this.range();

            this._state = range.hasFilter();

            if (range.hasFilter()) {
                range.filter(false);
            } else if (!range.intersectingMerged().length) {
                range.filter(true);
            } else {
               return { reason: "filterRangeContainingMerges" };
            }
        }
    });

    kendo.spreadsheet.SortCommand = Command.extend({
        undo: function() {
            var sheet = this.range().sheet();
            sheet.setState(this._state);
        },
        exec: function() {
            var range = this.range();
            var sheet = range.sheet();
            var activeCell = sheet.activeCell();
            var col = activeCell.topLeft.col;
            var ascending = this.options.asc;

            this._state = sheet.getState();

            if (this.options.sheet) {
                this.expandRange().sort({ column: col, ascending: ascending });
            } else {
                range.sort({ column: this.options.column || 0, ascending: ascending });
            }
        },
        expandRange: function() {
            var sheet = this.range().sheet();
            return new kendo.spreadsheet.Range(sheet._sheetRef, sheet);
        }
    });

    var ApplyFilterCommand = kendo.spreadsheet.ApplyFilterCommand = Command.extend({
        column: function() {
            return this.options.column || 0;
        },
        undo: function() {
            var sheet = this.range().sheet();

            sheet.clearFilter(this.column());

            if (this._state.length) {
                this.range().filter(this._state);
            }
        },
        getState: function() {
            var sheet = this.range().sheet();
            var currentFilter = sheet.filter();

            if (currentFilter) {
                this._state = currentFilter.columns.filter(function(c) {
                    return c.index == this.column();
                }.bind(this));
            }
        },
        exec: function() {
            var range = this.range();
            var column = this.column();
            var filter;

            if (this.options.valueFilter) {
                filter = new kendo.spreadsheet.ValueFilter(this.options.valueFilter);
            } else if (this.options.customFilter) {
                filter = new kendo.spreadsheet.CustomFilter(this.options.customFilter);
            }

            this.getState();

            range.clearFilter(column);

            range.filter({
                column: column,
                filter: filter
            });
        }
    });

    kendo.spreadsheet.ClearFilterCommand = ApplyFilterCommand.extend({
        exec: function() {
            var range = this.range();
            var column = this.column();

            this.getState();
            range.clearFilter(column);
        }
    });

    kendo.spreadsheet.HideLineCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this.axis = options.axis;
        },

        undo: function() {
            var sheet = this.range().sheet();
            sheet.setAxisState(this._state);
        },

        exec: function() {
            var sheet = this.range().sheet();
            this._state = sheet.getAxisState();

            if (this.axis == "row") {
                sheet.axisManager().hideSelectedRows();
            } else {
                sheet.axisManager().hideSelectedColumns();
            }
        }
    });

    kendo.spreadsheet.UnHideLineCommand = kendo.spreadsheet.HideLineCommand.extend({
        exec: function() {
            var sheet = this.range().sheet();
            this._state = sheet.getAxisState();

            if (this.axis == "row") {
                sheet.axisManager().unhideSelectedRows();
            } else {
                sheet.axisManager().unhideSelectedColumns();
            }
        }
    });

    var DeleteCommand = kendo.spreadsheet.DeleteCommand = Command.extend({
        undo: function() {
            var sheet = this.range().sheet();
            sheet.setState(this._state);
        }
    });

    kendo.spreadsheet.DeleteRowCommand = DeleteCommand.extend({
        exec: function() {
            var sheet = this.range().sheet();
            this._state = sheet.getState();
            sheet.axisManager().deleteSelectedRows();
        }
    });

    kendo.spreadsheet.DeleteColumnCommand = DeleteCommand.extend({
        exec: function() {
            var sheet = this.range().sheet();
            this._state = sheet.getState();
            sheet.axisManager().deleteSelectedColumns();
        }
    });

    var AddCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this._value = options.value;
        },
        undo: function() {
            var sheet = this.range().sheet();
            sheet.setState(this._state);
        }
    });

    kendo.spreadsheet.AddColumnCommand = AddCommand.extend({
        exec: function() {
            var sheet = this.range().sheet();
            this._state = sheet.getState();

            if (this._value === "left") {
                sheet.axisManager().addColumnLeft();
            } else {
                sheet.axisManager().addColumnRight();
            }
        }
    });

    kendo.spreadsheet.AddRowCommand = AddCommand.extend({
        exec: function() {
            var sheet = this.range().sheet();

            if (!sheet.axisManager().canAddRow()) {
                return { reason: "error", type: "shiftingNonblankCells" };
            }

            this._state = sheet.getState();

            if (this._value === "above") {
                sheet.axisManager().addRowAbove();
            } else {
                sheet.axisManager().addRowBelow();
            }
        }
    });

    kendo.spreadsheet.EditValidationCommand = Command.extend({
        init: function(options) {
            Command.fn.init.call(this, options);
            this._value = options.value;
        },
        exec: function() {
            this.range().validation(this._value);
        }
    });

    kendo.spreadsheet.SaveAsCommand = Command.extend({
        exec: function() {
            this.options.workbook.saveAsExcel({
                fileName: this.options.fileName
            });
        }
    });

})(kendo);

})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;

    var classNames = {
        wrapper: "k-spreadsheet-formula-bar"
    };

    var FormulaBar = kendo.ui.Widget.extend({
        init: function(element, options) {
            kendo.ui.Widget.call(this, element, options);

            element = this.element.addClass(FormulaBar.classNames.wrapper);

            this.formulaInput = new kendo.spreadsheet.FormulaInput($("<div/>").appendTo(element));
        },

        destroy: function() {
            if (this.formulaInput) {
                this.formulaInput.destroy();
            }
            this.formulaInput = null;
        }
    });

    kendo.spreadsheet.FormulaBar = FormulaBar;
    $.extend(true, FormulaBar, { classNames: classNames });
})(window.kendo);
})();

(function(){

(function(kendo, window) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    /* jshint eqnull:true */
    /* jshint latedef: nofunc */

    var $ = kendo.jQuery;
    var Widget = kendo.ui.Widget;
    var ns = ".kendoFormulaInput";
    var keys = kendo.keys;
    var classNames = {
        wrapper: "k-spreadsheet-formula-input",
        listWrapper: "k-spreadsheet-formula-list"
    };
    var styles = [
        "font-family",
        "font-size",
        "font-stretch",
        "font-style",
        "font-weight",
        "letter-spacing",
        "text-transform",
        "line-height"
    ];

    //move to core
    var KEY_NAMES = {
        27: 'esc',
        37: 'left',
        39: 'right',
        35: 'end',
        36: 'home',
        32: 'spacebar'
    };

    var PRIVATE_FORMULA_CHECK = /(^_|[^a-z0-9]$)/i;

    var FormulaInput = Widget.extend({
        init: function(element, options) {
            Widget.call(this, element, options);

            element = this.element;

            element.addClass(FormulaInput.classNames.wrapper)
                .attr("contenteditable", true)
                .attr("spellcheck", false);

            if (this.options.autoScale) {
                element.on("input", this.scale.bind(this));
            }

            this._highlightedRefs = [];
            this._staticTokens = [];

            this._formulaSource();

            this._formulaList();

            this._popup();

            this._tooltip();

            element
                .on("keydown", this._keydown.bind(this))
                .on("keyup", this._keyup.bind(this))
                .on("blur", this._blur.bind(this))
                .on("input click", this._input.bind(this))
                .on("focus", this._focus.bind(this));
        },

        options: {
            name: "FormulaInput",
            autoScale: false,
            filterOperator: "startswith",
            scalePadding: 30,
            minLength: 1
        },

        events: [
            "keyup"
        ],

        getPos: function() {
            var div = this.element[0];
            var sel = window.getSelection();
            var a = lookup(sel.focusNode, sel.focusOffset);
            var b = lookup(sel.anchorNode, sel.anchorOffset);
            if (a != null && b != null) {
                if (a > b) {
                    var tmp = a;
                    a = b;
                    b = tmp;
                }
                return { begin: a, end: b, collapsed: a == b };
            }
            function lookup(lookupNode, pos) {
                try {
                    (function loop(node){
                        if (node === lookupNode) {
                            throw pos;
                        } else if (node.nodeType == 1 /* Element */) {
                            for (var i = node.firstChild; i; i = i.nextSibling) {
                                loop(i);
                            }
                        } else if (node.nodeType == 3 /* Text */) {
                            pos += node.nodeValue.length;
                        }
                    })(div);
                } catch(index) {
                    return index;
                }
            }
        },

        setPos: function(begin, end) {
            var eiv = this.element[0];
            begin = lookup(eiv, begin);
            if (end != null) {
                end = lookup(eiv, end);
            } else {
                end = begin;
            }
            if (begin && end) {
                var r = document.createRange();
                r.setStart(begin.node, begin.pos);
                r.setEnd(end.node, end.pos);
                var sel = window.getSelection();
                sel.removeAllRanges();
                sel.addRange(r);
            }
            function lookup(node, pos) {
                try {
                    (function loop(node){
                        if (node.nodeType == 3 /* Text */) {
                            var len = node.nodeValue.length;
                            if (len >= pos) {
                                throw node;
                            }
                            pos -= len;
                        } else if (node.nodeType == 1 /* Element */) {
                            for (var i = node.firstChild; i; i = i.nextSibling) {
                                loop(i);
                            }
                        }
                    })(node);
                } catch(el) {
                    return { node: el, pos: pos };
                }
            }
        },

        end: function() {
            this.setPos(this.length());
        },

        home: function() {
            this.setPos(0);
        },

        length: function() {
            return this.value().length;
        },

        _formulaSource: function() {
            var result = [];
            var value;

            for (var key in kendo.spreadsheet.calc.runtime.FUNCS) {
                if (!PRIVATE_FORMULA_CHECK.test(key)) {
                    value = key.toUpperCase();
                    result.push({ value: value, text: value });
                }
            }

            this.formulaSource = new kendo.data.DataSource({ data: result });
        },

        _formulaList: function() {
            this.list = new kendo.ui.StaticList($('<ul />')
                .addClass(FormulaInput.classNames.listWrapper)
                .insertAfter(this.element), {
                    autoBind: false,
                    selectable: true,
                    change: this._formulaListChange.bind(this),
                    dataSource: this.formulaSource,
                    dataValueField: "value",
                    template: "#:data.value#"
                });

            this.list.element.on("mousedown", function(e) {
                e.preventDefault();
            });
        },

        _formulaListChange: function() {
            var tokenCtx = this._tokenContext();

            if (!tokenCtx || this._mute) {
                return;
            }

            var activeToken = tokenCtx.token;
            var completion = this.list.value()[0];
            var ctx = {
                replace: true,
                token: activeToken,
                end: activeToken.end
            };

            if (!tokenCtx.nextToken || tokenCtx.nextToken.value != "(") {
                completion += "(";
            }

            this._replaceAt(ctx, completion);
            this.popup.close();
        },

        _popup: function() {
            this.popup = new kendo.ui.Popup(this.list.element, {
                anchor: this.element
            });
        },

        _blur: function() {
            this.popup.close();
            clearTimeout(this._focusId);
        },

        _isFormula: function() {
            return /^=/.test(this.value());
        },

        _keydown: function(e) {
            var key = e.keyCode;

            if (KEY_NAMES[key]) {
                this.popup.close();
                this._navigated = true;
            } else  if (this._move(key)) {
                this._navigated = true;
                e.preventDefault();
            }

            this._keyDownTimeout = setTimeout(this._syntaxHighlight.bind(this));
        },

        _keyup: function() {
            var popup = this.popup;
            var value;

            if (this._isFormula() && !this._navigated) {
                value = ((this._tokenContext() || {}).token || {}).value;

                this.filter(value);

                if (!value || !this.formulaSource.view().length) {
                    popup.close();
                } else {
                    popup[popup.visible() ? "position" : "open"]();
                    this.list.focusFirst();
                }
            }

            this._navigated = false;
            this._syntaxHighlight();

            this.trigger("keyup");
        },

        _input: function() {
            this._syntaxHighlight();
        },

        _focus: function() {
            this._focusTimeout = setTimeout(this._syntaxHighlight.bind(this));
        },

        _move: function(key) {
            var list = this.list;
            var pressed = false;

            if (key === keys.DOWN) {
                list.focusNext();
                if (!list.focus()) {
                    list.focusFirst();
                }
                pressed = true;
            } else if (key === keys.UP) {
                list.focusPrev();
                if (!list.focus()) {
                    list.focusLast();
                }
                pressed = true;
            } else if (key === keys.ENTER) {
                list.select(list.focus());
                this.popup.close();
                pressed = true;
            } else if (key === keys.TAB) {
                list.select(list.focus());
                this.popup.close();
                pressed = true;
            } else if (key === keys.PAGEUP) {
                list.focusFirst();
                pressed = true;
            } else if (key === keys.PAGEDOWN) {
                list.focusLast();
                pressed = true;
            }

            return pressed;
        },

        _tokenContext: function() {
            var point = this.getPos();
            var value = this.value();

            if (!value || !point || !point.collapsed) {
                return null;
            }

            var tokens = kendo.spreadsheet.calc.tokenize(value);
            var tok;

            for (var i = 0; i < tokens.length; ++i) {
                tok = tokens[i];
                if (touches(tok, point) && /^(?:str|sym|func)$/.test(tok.type)) {
                    return { token: tok, nextToken: tokens[i + 1] };
                }
            }

            return null;
        },

        _sync: function() {
            if (this._editorToSync && this.isActive()) {
                this._editorToSync.value(this.value());
            }
        },

        _textContainer: function() {
            var computedStyles = kendo.getComputedStyles(this.element[0], styles);

            computedStyles.position = "absolute";
            computedStyles.visibility = "hidden";
            computedStyles.top = -3333;
            computedStyles.left = -3333;

            this._span = $("<span/>").css(computedStyles).insertAfter(this.element);
        },

        _tooltip: function() {
            this._cellTooltip = $('<div class="k-widget k-tooltip" style="position:absolute; display:none">A1</div>').insertAfter(this.element);
        },

        tooltip: function(value) {
            this._cellTooltip.text(value);
        },

        toggleTooltip: function(show) {
            this._cellTooltip.toggle(show);
        },

        isActive: function() {
            return this.element[0] === kendo._activeElement();
        },

        filter: function(value) {
            if (!value || value.length < this.options.minLength) {
                return;
            }

            this._mute = true;
            this.list.select(-1);
            this._mute = false;

            this.formulaSource.filter({
                field: this.list.options.dataValueField,
                operator: this.options.filterOperator,
                value: value
            });
        },

        hide: function() {
            this.element.hide();
            this._cellTooltip.hide();
        },

        show: function() {
            this.element.show();
        },

        position: function(rectangle) {
            if (!rectangle) {
                return;
            }

            this.element
                .show()
                .css({
                    "top": rectangle.top + "px",
                    "left": rectangle.left + "px"
                });

            this._cellTooltip.css({
                "top": (rectangle.top - this._cellTooltip.height() - 10) + "px",
                "left": rectangle.left
            });
        },

        resize: function(rectangle) {
            if (!rectangle) {
                return;
            }

            this.element.css({
                width: rectangle.width + 1,
                height: rectangle.height + 1
            });
        },

        canInsertRef: function(isKeyboardAction) {
            var result = this._canInsertRef(isKeyboardAction);
            var token = result && result.token;
            var idx;

            if (token) {
                for (idx = 0; idx < this._staticTokens.length; idx++) {
                    if (isEqualToken(token, this._staticTokens[idx])) {
                        return null;
                    }
                }
            }

            return result;
        },

        _canInsertRef: function(isKeyboardAction) {
            if (this.popup.visible()) {
                return null;
            }
            var strictMode = isKeyboardAction;
            var point = this.getPos();
            var tokens, tok;

            if (point && this._isFormula()) {
                if (point.begin === 0) {
                    return null;
                }

                tokens = kendo.spreadsheet.calc.tokenize(this.value());

                for (var i = 0; i < tokens.length; ++i) {
                    tok = tokens[i];
                    if (touches(tok, point)) {
                        return canReplace(tok);
                    }
                    if (afterPoint(tok)) {
                        return canInsertBetween(tokens[i-1], tok);
                    }
                }
                return canInsertBetween(tok, null);
            }

            return null;

            function afterPoint(tok) {
                return tok.begin > point.begin;
            }
            function canReplace(tok) {
                if (tok) {
                    if (/^(?:num|str|bool|sym|ref)$/.test(tok.type)) {
                        return { replace: true, token: tok, end: tok.end };
                    }
                    if (/^(?:op|punc|startexp)$/.test(tok.type)) {
                        if (tok.end == point.end) {
                            return canInsertBetween(tok, tokens[i+1]);
                        }
                        return canInsertBetween(tokens[i-1], tok);
                    }
                }
            }
            function canInsertBetween(left, right) {
                if (left == null) {
                    return null;
                }
                if (right == null) {
                    if (/^(?:op|startexp)$/.test(left.type) || isOpenParen(left.value)) {
                        return { token: left, end: point.end };
                    }
                    return null;
                }
                if (strictMode) {
                    if (left.type == "op" && /^(?:punc|op)$/.test(right.type)) {
                        return { token: left, end: point.end };
                    }
                } else {
                    if (left.type == "startexp") {
                        return { token: left, end: point.end };
                    }
                    if (/^(?:ref|op|punc)$/.test(left.type)) { //this checks for op and punc
                        return { token: left, end: point.end };
                    }
                    if (/^(?:punc|op)$/.test(left.type)) { //this checks for op and punc
                        return (/^[,;({]$/.test(left.value) ?
                                { token: left, end: point.end } : null);
                    }
                }
                return false;
            }
        },

        refAtPoint: function(ref) {
            var x = this._canInsertRef();
            if (x) {
                this._replaceAt(x, ref.simplify().toString());
            }
        },

        _replaceAt: function(ctx, newValue) {
            var value = this.value();
            var tok = ctx.token;
            var rest = value.substr(ctx.end);
            value = value.substr(0, ctx.replace ? tok.begin : ctx.end) + newValue;
            var point = value.length;
            value += rest;
            this._value(value);
            this.setPos(point);
            this.scale();

            this._syntaxHighlight();
            this._sync();
        },

        syncWith: function(formulaInput) {
            var eventName = "input" + ns;

            this._editorToSync = formulaInput;
            this.element.off(eventName).on(eventName, this._sync.bind(this));
        },

        scale: function() {
            var element = this.element;
            var width;

            if (!this._span) {
                this._textContainer();
            }

            this._span.html(element.html());

            width = this._span.width() + this.options.scalePadding;

            if (width > element.width()) {
                element.width(width);
            }
        },

        _value: function(value) {
            this.element.text(value);
        },

        value: function(value) {
            if (value === undefined) {
                return this.element.text();
            }

            this._value(value);
            this._syntaxHighlight();
        },

        highlightedRefs: function() {
            return this._highlightedRefs.slice();
        },

        _syntaxHighlight: function() {
            var pos = this.getPos();
            var value = this.value();
            var refClasses = kendo.spreadsheet.Pane.classNames.series;
            var highlightedRefs = [];
            var refIndex = 0;
            var parens = [];
            var tokens = [];
            var activeToken;

            if (pos && !pos.collapsed) {
                // Backward selection (hold shift, move right to left)
                // will not work properly if we continuously re-set
                // the HTML.  If the selection is on, presumably the
                // text has already been highlighted, so stop here.
                return;
            }

            if (!(/^=/.test(value))) {
                // if an user deleted the initial =, we should discard
                // any highlighting.  we still need to restore caret
                // position thereafter.
                this.element.text(value);

                // also make sure the completion popup goes away
                this.popup.close();
            } else {
                tokens = kendo.spreadsheet.calc.tokenize(value);
                tokens.forEach(function(tok){
                    tok.active = false;
                    tok.cls = [ "k-syntax-" + tok.type ];

                    if (tok.type == "ref") {
                        tok.colorClass = refClasses[(refIndex++) % refClasses.length];
                        tok.cls.push(tok.colorClass);
                        highlightedRefs.push(tok);
                    }
                    if (pos && tok.type == "punc") {
                        if (isOpenParen(tok.value)) {
                            parens.unshift(tok);
                        } else if (isCloseParen(tok.value)) {
                            var open = parens.shift();
                            if (open) {
                                if (isMatchingParen(tok.value, open.value)) {
                                    if (touches(tok, pos) || touches(open, pos)) {
                                        tok.cls.push("k-syntax-paren-match");
                                        open.cls.push("k-syntax-paren-match");
                                    }
                                } else {
                                    tok.cls.push("k-syntax-error");
                                    open.cls.push("k-syntax-error");
                                }
                            } else {
                                tok.cls.push("k-syntax-error");
                            }
                        }
                    }
                    if (pos && touches(tok, pos)) {
                        tok.cls.push("k-syntax-at-point");
                        tok.active = true;
                        activeToken = tok;
                    }
                    if (tok.type == "func" && !knownFunction(tok.value) && (!pos || !touches(tok, pos))) {
                        tok.cls.push("k-syntax-error");
                    }
                });
                tokens.reverse().forEach(function(tok){
                    var begin = tok.begin, end = tok.end;
                    var text = kendo.htmlEncode(value.substring(begin, end));
                    value = value.substr(0, begin) +
                        "<span class='" + tok.cls.join(" ") + "'>" + text + "</span>" +
                        value.substr(end);
                });
                this.element.html(value);
            }
            if (pos) {
                this.setPos(pos.begin, pos.end);
            }

            if (activeToken && /^(?:startexp|op|punc)$/.test(activeToken.type)) {
                this._setStaticTokens(tokens);
            }

            this._highlightedRefs = highlightedRefs;
        },

        _setStaticTokens: function(tokens) {
            var idx, tok;

            this._staticTokens = [];

            for (idx = 0; idx < tokens.length; idx++) {
                tok = tokens[idx];
                if (/^(?:num|str|bool|sym|ref)$/.test(tok.type)) {
                    this._staticTokens.push(tok);
                }
            }
        },

        destroy: function() {
            this._editorToSync = null;

            this.element.off(ns);

            clearTimeout(this._focusTimeout);
            clearTimeout(this._keyDownTimeout);

            this._cellTooltip = null;
            this._span = null;

            this.popup.destroy();
            this.popup = null;

            Widget.fn.destroy.call(this);
        }
    });

    function isOpenParen(ch) {
        return ch == "(" || ch == "[" || ch == "{";
    }

    function isCloseParen(ch) {
        return ch == ")" || ch == "]" || ch == "}";
    }

    function isMatchingParen(close, open) {
        return open == "(" ? close == ")"
            :  open == "[" ? close == "]"
            :  open == "{" ? close == "}"
            :  false;
    }

    function touches(pos, target) {
        return pos.begin <= target.begin && pos.end >= target.end;
    }

    function knownFunction(name) {
        return kendo.spreadsheet.calc.runtime.FUNCS[name.toLowerCase()];
    }

    function isEqualToken(tok1, tok2) {
        if (!tok1 || !tok2) {
            return false;
        }

        if (tok1.type == "ref" && tok2.type == "ref") {
            return tok1.ref.eq(tok2.ref);
        } else {
            return tok1.value === tok2.value;
        }
    }

    kendo.spreadsheet.FormulaInput = FormulaInput;
    $.extend(true, FormulaInput, { classNames: classNames });
})(kendo, window);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;

    var KEY_NAMES = {
        8: 'backspace',
        9: 'tab',
        13: 'enter',
        27: 'esc',
        37: 'left',
        38: 'up',
        39: 'right',
        40: 'down',
        35: 'end',
        36: 'home',
        32: 'spacebar',
        33: 'pageup',
        34: 'pagedown',
        46: 'delete',
        113: ':edit'
    };

    var Mac = navigator.platform.toUpperCase().indexOf('MAC') >= 0;

    var isAlphaNum = function(keyCode) {
        if ((keyCode > 47 && keyCode < 58)   || // number keys
            (keyCode > 64 && keyCode < 91)   || // letter keys
            (keyCode > 95 && keyCode < 112)  || // numpad keys
            (keyCode > 185 && keyCode < 193) || // ;=,-./` (in order)
            (keyCode > 218 && keyCode < 223)) {   // [\]' (in order)
            return true;
        }

        return false;
    };

    var keyName = function(keyCode) {
        var name = KEY_NAMES[keyCode];

        if (!name && isAlphaNum(keyCode)) {
            name = ":alphanum";
        }

        return name;
    };

    var EventListener = kendo.Class.extend({
        init: function(target, observer, handlers) {
            this._handlers = {};
            this.target = target;
            this._observer = observer || window;

            this.keyDownProxy = this.keyDown.bind(this);
            this.mouseProxy = this.mouse.bind(this);
            this.threshold = 5;
            this._pressLocation = null;

            target.on("keydown", this.keyDownProxy);
            target.on("contextmenu mousedown cut copy paste scroll wheel click dblclick focus", this.mouseProxy);

            $(document.documentElement).on("mousemove mouseup", this.mouseProxy);

            if (handlers) {
                for (var key in handlers) {
                    this.on(key, handlers[key]);
                }
            }
        },

        keyDown: function(e) {
            this.handleEvent(e, keyName(e.keyCode));
        },

        mouse: function(e) {

            var rightClick;

            if (e.which) {
                rightClick = (e.which == 3);
            } else if (e.button) {
                rightClick = (e.button == 2);
            }

            var type = e.type;

            if (type === "mousedown") {
                if (rightClick) {
                   type = "rightmousedown";
                } else {
                    this._pressLocation = { x: e.pageX, y: e.pageY };
                }
            }

            if (type === "mouseup") {
                if (!rightClick) {
                    this._pressLocation = null;
                }
            }

            if (type === "mousemove" && this._pressLocation) {
                var dx = this._pressLocation.x - e.pageX;
                var dy = this._pressLocation.y - e.pageY;
                var distance = Math.sqrt(dx*dx + dy*dy);

                if (distance > this.threshold) {
                    type = "mousedrag";
                }
            }

            this.handleEvent(e, type);
        },

        handleEvent: function(e, name) {
            var eventKey = "";

            e.mod = Mac ? e.metaKey : e.ctrlKey;

            if (e.shiftKey) {
               eventKey += "shift+";
            }

            if (e.ctrlKey) {
               eventKey += "ctrl+";
            }

            eventKey += name;

            var catchAllHandler = this._handlers['*+' + name];

            if (catchAllHandler) {
                catchAllHandler.call(this._observer, e, eventKey);
            }

            var handler = this._handlers[eventKey];

            if (handler) {
                handler.call(this._observer, e, eventKey);
            }
        },

        on: function(event, callback) {
            var handlers = this._handlers;

            if (typeof callback === "string") {
                callback = this._observer[callback];
            }

            if (typeof event === "string") {
                event = event.split(",");
            }

            event.forEach(function(e) {
                handlers[e] = callback;
            });
        },

        destroy: function() {
            this.target.off("keydown", this.keyDownProxy);
            this.target.off("keydown", this.mouseProxy);
            $(document.documentElement).off("mousemove mouseup", this.mouseProxy);
        }
    });

    kendo.spreadsheet.EventListener = EventListener;
})(window.kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var RangeTreeNode = kendo.Class.extend({
        init: function Node(level, value, left, right) {
            this.level = level;
            this.value = value;
            this.left = left;
            this.right = right;
        }
    });

    var NilNode = new (function NIL() { // jshint ignore:line
        this.left = this;
        this.right = this;
        this.level = 0;
    })();

    function passThrough(value) {
        return value;
    }

    function skew(node) {
        if (node.left.level === node.level) {
            var temp = node;
            node = node.left;
            temp.left = node.right;
            node.right = temp;
        }

        return node;
    }

    function split(node) {
        if (node.right.right.level === node.level) {
            var temp = node;
            node = node.right;
            temp.right = node.left;
            node.left = temp;
            node.level += 1;
        }

        return node;
    }

    function insert(node, value) {
        if (node === NilNode) {
            return new RangeTreeNode(1, value, NilNode, NilNode);
        } else if (node.value.start - value.start > 0) {
            node.left = insert(node.left, value);
        } else {
            node.right = insert(node.right, value);
        }

        return split(skew(node));
    }

    function remove(node, value) {
        if (node === NilNode) {
            return node;
        }

        var diff = node.value.start - value.start;
        if (diff === 0) {
            if (node.left !== NilNode && node.right !== NilNode) {
                var heir = node.left;

                while (heir.right !== NilNode) {
                    heir = heir.right;
                }

                node.value = heir.value;
                node.left = remove(node.left, node.value);
            } else if (node.left === NilNode) {
                node = node.right;
            } else {
                node = node.left;
            }
        } else if (diff > 0) {
            node.left = remove(node.left, value);
        } else {
            node.right = remove(node.right, value);
        }

        if (node.left.level < (node.level - 1) || node.right.level < (node.level - 1)) {
            node.level -= 1;
            if (node.right.level > node.level) {
                node.right.level = node.level;
            }

            node = skew(node);
            node.right = skew(node.right);
            node.right.right = skew(node.right.right);
            node = split(node);
            node.right = split(node.right);
        }

        return node;
    }

    var Range = kendo.Class.extend({
        init: function Value(start, end, value) {
            this.start = start;
            this.end = end;
            this.value = value;
        },

        intersects: function(range) {
            return range.start <= this.end && range.end >= this.start;
        }
    });

    var RangeTree = kendo.Class.extend({
        init: function() {
            this.root = NilNode;
        },

        insert: function(value) {
            this.root = insert(this.root, value);
        },

        remove: function(value) {
            this.root = remove(this.root, value);
        },

        findrange: function(value) {
            var node = this.root;

            while (node != NilNode) {
                if (value < node.value.start) {
                    node = node.left;
                } else if (value > node.value.end) {
                    node = node.right;
                } else {
                    return node.value;
                }
            }

            return null;
        },

        values: function() {
            var result = [];
            values(this.root, result);
            return result;
        },

        intersecting: function(start, end) {
            var ranges = [];
            intersecting(this.root, new Range(start, end), ranges);
            return ranges;
        },

        map: function(callback) {
            var tree = new RangeTree();
            map(tree, this.root, callback);
            return tree;
        },

        clone: function() {
            return this.map(passThrough);
        },

        first: function() {
            var first = this.root;
            while(first.left != NilNode) {
                first = first.left;
            }

            return first;
        },

        last: function() {
            var last = this.root;
            while(last.right != NilNode) {
                last = last.right;
            }

            return last;
        }
    });

    function values(node, result) {
        if (node === NilNode) {
            return;
        }

        values(node.left, result);
        result.push(node.value);
        values(node.right, result);
    }

    function intersecting(node, range, ranges) {
        if (node === NilNode) {
            return;
        }

        var value = node.value;

        if (range.start < value.start) {
            intersecting(node.left, range, ranges);
        }

        if (value.intersects(range)) {
            ranges.push(value);
        }

        if (range.end > value.end) {
            intersecting(node.right, range, ranges);
        }
    }

    function map(tree, root, callback) {
        if (root === NilNode) {
            return;
        }

        map(tree, root.left, callback);
        tree.insert(callback(root.value));
        map(tree, root.right, callback);
    }

    var RangeList = kendo.Class.extend({
        init: function(start, end, value) {
            if (end === undefined) {
                this.tree = start;
            } else {
                this.tree = new RangeTree();
                this.tree.insert(new Range(start, end, value));
            }
        },

        values: function() {
            return this.tree.values();
        },

        map: function(callback) {
            return new RangeList(this.tree.map(callback));
        },

        intersecting: function(start, end) {
            return this.tree.intersecting(start, end);
        },

        first: function() {
            return this.tree.first().value;
        },

        last: function() {
            return this.tree.last().value;
        },

        insert: function(start, end, value) {
            return this.tree.insert(new Range(start, end, value));
        },

        value: function(start, end, value) {
            if (value === undefined) {
                if (end === undefined) {
                    end = start;
                }
                return this.intersecting(start, end)[0].value;
            }

            var ranges = this.tree.intersecting(start - 1, end + 1);

            if (ranges.length) {
                var firstRange = ranges[0], lastRange = ranges[ranges.length - 1];

                if (firstRange.end < start) {
                    if (firstRange.value === value) {
                        start = firstRange.start;
                    } else {
                        ranges.shift();
                    }
                }

                if (lastRange.start > end) {
                    if (lastRange.value === value) {
                        end = lastRange.end;
                    } else {
                        ranges.pop();
                    }
                }

                for (var i = 0, length = ranges.length; i < length; i++) {
                    var range = ranges[i];
                    var rangeValue = range.value;
                    var rangeStart = range.start;
                    var rangeEnd = range.end;

                    this.tree.remove(range);

                    if (rangeStart < start) {
                        if (rangeValue !== value) {
                            this.insert(rangeStart, start - 1, rangeValue);
                        } else {
                            start = rangeStart;
                        }
                    }

                    if (rangeEnd > end) {
                        if (rangeValue !== value) {
                            this.insert(end + 1, rangeEnd, rangeValue);
                        } else {
                            end = rangeEnd;
                        }
                    }
                }
            }

            this.insert(start, end, value);
        },

        expandedValues: function(start, end) {
            var ranges = this.intersecting(start, end);
            var result = [];

            var rangeIndex = 0;

            for (var i = start; i <= end; i++) {
                if (ranges[rangeIndex].end < i) {
                    rangeIndex ++;
                }

                result.push({ index: i - start, value: ranges[rangeIndex].value });
            }

            return result;
        },

        sortedIndices: function(start, end, valueComparer, indices) {
            var result = this.expandedValues(start, end);

            var comparer = function(a, b) {
                if (a.value === b.value) {
                    return a.index - b.index;
                }

                return valueComparer(a.value, b.value);
            };

            if (indices) {
                comparer = function(a, b) {
                    var x = indices[a.index];
                    var y = indices[b.index];

                    if (x.value === y.value) {
                        return valueComparer(a.value, b.value);
                    }

                    return a.index - b.index;
                };
            }

            result.sort(comparer);

            return result;
        },

        sort: function(start, end, indices) {
            if (this.intersecting(start, end).length === 1) {
                return;
            }

            var values = this.expandedValues(start, end);

            for (var i = 0, len = indices.length; i < len; i++) {
                this.value(i + start, i + start, values[indices[i].index].value);
            }
        },

        copy: function(sourceStart, sourceEnd, targetStart) {
            var values = this.intersecting(sourceStart, sourceEnd);

            var start = targetStart;
            var end;

            for (var i = 0, len = values.length; i < len; i++) {
                var rangeStart = values[i].start;

                if (rangeStart < sourceStart) {
                    rangeStart = sourceStart;
                }

                var rangeEnd = values[i].end;

                if (rangeEnd > sourceEnd) {
                    rangeEnd = sourceEnd;
                }

                end = start + (rangeEnd - rangeStart);

                this.value(start, end, values[i].value);

                start = ++end;
            }
        },

        iterator: function(start, end) {
            return new Iterator(start, end, this.intersecting(start, end));
        },

        getState: function() {
            return this.tree.clone();
        },

        setState: function(state) {
            this.tree = state;
        }
    });

    var Iterator = kendo.Class.extend({
        init: function(start, end, ranges) {
            this.start = start;
            this.end = end;
            this.index = 0;
            this.ranges = ranges;
        },

        unique: function() {
            return this.ranges.map(function(range) {
                return range.value;
            });
        },

        at: function(index) {
            while (this.ranges[this.index].end < index) {
                this.index ++;
            }

            return this.ranges[this.index].value;
        },

        forEach: function(callback) {
            for (var i = this.start; i <= this.end; i++) {
                callback(this.at(i), i);
            }

            this.index = 0;
        }
    });

    var SparseRangeList = RangeList.extend({
        init: function(start, end, value) {
            this.tree = new RangeTree();
            this.range = new Range(start, end, value);
        },

        intersecting: function(start, end) {
            var ranges = this.tree.intersecting(start, end);
            var result = [];
            var range;

            if (!ranges.length) {
                return [this.range];
            }

            for (var i = 0, len = ranges.length; i < len; i++) {
                range = ranges[i];
                if (range.start > start) {
                    result.push(new Range(start, range.start - 1, this.range.value));
                }

                result.push(range);
                start = range.end + 1;
            }

            if (range.end < end) {
                result.push(new Range(range.end + 1, end, this.range.value));
            }

            return result;
        },

        insert: function(start, end, value) {
            if (value !== this.range.value) {
                this.tree.insert(new Range(start, end, value));
            }
        },

        lastRangeStart: function() {
            var node = this.tree.root;

            if (node === NilNode) {
                return this.range.start;
            }

            while(node.right !== NilNode) {
                node = node.right;
            }

            return node.value.end + 1;
        }
    });


    kendo.spreadsheet.RangeTree = RangeTree;
    kendo.spreadsheet.RangeList = RangeList;
    kendo.spreadsheet.SparseRangeList = SparseRangeList;
    kendo.spreadsheet.ValueRange = Range;
})(kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;

    var Property = kendo.Class.extend({
        init: function(list) {
            this.list = list;
        },

        get: function(index) {
            return this.parse(this.list.value(index, index));
        },

        set: function(start, end, value) {
            if (value === undefined) {
                value = end;
                end = start;
            }

            this.list.value(start, end, value);
        },

        parse: function(value) {
            return value;
        },

        copy: function(start, end, dst) {
            this.list.copy(start, end, dst);
        },

        iterator: function(start, end) {
            return this.list.iterator(start, end);
        }
    });

    var JsonProperty = Property.extend({
        set: function(start, end, value) {
            this.list.value(start, end, JSON.stringify(value));
        },

        parse: function(value) {
            return JSON.parse(value);
        }
    });

    var ValueProperty = Property.extend({
        init: function(values, formats, validations) {
            Property.prototype.init.call(this, values);

            this.validations = validations;
            this.formats = formats;
        },

        set: function(start, end, value) {
            //TODO: RESET VALIDATION AS WELL?
            if (value instanceof Date) {
                value = kendo.spreadsheet.dateToNumber(value);
                this.formats.value(start, end, toExcelFormat(kendo.culture().calendar.patterns.d));
            }

            this.list.value(start, end, value);
        }
    });

    function toExcelFormat(format) {
        return format.replace(/M/g, "m").replace(/'/g, '"').replace(/tt/, "am/pm");
    }

    kendo.spreadsheet.PropertyBag = kendo.Class.extend({
        specs: [
            { property: ValueProperty, name: "value", value: null, sortable: true, serializable: true, depends: "format" },
            { property: Property, name: "format", value: null, sortable: true, serializable: true },
            { property: Property, name: "formula", value: null, sortable: true, serializable: true },
            { property: Property, name: "background", value: null, sortable: true, serializable: true },
            { property: JsonProperty, name: "borderBottom", value: null, sortable: false, serializable: true },
            { property: JsonProperty, name: "borderRight", value: null, sortable: false, serializable: true },
            { property: JsonProperty, name: "borderLeft", value: null, sortable: false, serializable: true },
            { property: JsonProperty, name: "borderTop", value: null, sortable: false, serializable: true },
            { property: Property, name: "color", value: null, sortable: true, serializable: true },
            { property: Property, name: "fontFamily", value: null, sortable: true, serializable: true },
            { property: Property, name: "underline", value: null, sortable: true, serializable: true },
            { property: Property, name: "fontSize", value: null, sortable: true, serializable: true },
            { property: Property, name: "italic", value: null, sortable: true, serializable: true },
            { property: Property, name: "bold", value: null, sortable: true, serializable: true },
            { property: Property, name: "textAlign", value: null, sortable: true, serializable: true },
            { property: Property, name: "verticalAlign", value: null, sortable: true, serializable: true },
            { property: Property, name: "wrap", value: null, sortable: true, serializable: true },
            { property: Property, name: "validation", value: null, sortable: false, serializable: true }
        ],

        init: function(cellCount) {
            this.properties = {};

            this.lists = {};

            this.specs.forEach(function(spec) {
               this.lists[spec.name] = new kendo.spreadsheet.SparseRangeList(0, cellCount, spec.value);
            }, this);

            this.specs.forEach(function(spec) {
                this.properties[spec.name] = new spec.property(this.lists[spec.name], this.lists[spec.depends]);
            }, this);
        },

        getState: function() {
            var state = {};

            this.specs.forEach(function(spec) {
               state[spec.name] = this.lists[spec.name].getState();
            }, this);

            return state;
        },

        setState: function(state) {
            this.specs.forEach(function(spec) {
                this.lists[spec.name].setState(state[spec.name]);
            }, this);
        },

        get: function(name, index) {
            if (index === undefined) {
                return this.lists[name];
            }

            return this.properties[name].get(index);
        },

        set: function(name, start, end, value) {
            this.properties[name].set(start, end, value);
        },

        fromJSON: function(index, value) {
            for (var si = 0; si < this.specs.length; si++) {
                var spec = this.specs[si];

                if (spec.serializable) {
                    if (value[spec.name] !== undefined) {
                        this.set(spec.name, index, index, value[spec.name], false);
                    }
                }
            }
        },

        copy: function(sourceStart, sourceEnd, targetStart) {
            this.specs.forEach(function(spec) {
                this.properties[spec.name].copy(sourceStart, sourceEnd, targetStart);
            }, this);
        },

        iterator: function(name, start, end) {
            return this.properties[name].iterator(start, end);
        },

        sortable: function() {
            return this.specs.filter(function(spec) { return spec.sortable; })
                              .map(function(spec) {
                                return this.lists[spec.name];
                              }, this);
        },

        iterators: function(start, end) {
            var specs = this.specs.filter(function(spec) {
                return spec.serializable;
            });

            return specs.map(function(spec) {
                var iterator = this.iterator(spec.name, start, end);

                return {
                    name: spec.name,
                    value: spec.value,
                    at: function (index) {
                        return spec.property.fn.parse(iterator.at(index));
                    }
                };
            }, this);
        },

        forEach: function(start, end, callback) {
            var iterators = this.iterators(start, end);

            for (var index = start; index <= end; index++) {
                var values = {};

                for (var i = 0; i < iterators.length; i++) {
                    var iterator = iterators[i];
                    var value = iterator.at(index);

                    if (value !== iterator.value) {
                        values[iterator.name] = value;
                    }
                }

                callback(values);
            }
        },

        forEachProperty: function(callback) {
            for (var name in this.properties) {
                callback(this.properties[name]);
            }
        }
    });

    kendo.spreadsheet.ALL_PROPERTIES = $.map(kendo.spreadsheet.PropertyBag.prototype.specs, function(spec) {
        return spec.name;
    });

})(window.kendo);

})();

(function(){
    // WARNING: removing the following jshint declaration and turning
    // == into === to make JSHint happy will break functionality.
    /* jshint eqnull:true, laxbreak:true */

    "use strict";

    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var spreadsheet = kendo.spreadsheet;
    var Class = kendo.Class;

    function columnName(colIndex) {
        var letter = Math.floor(colIndex / 26) - 1;
        return (letter >= 0 ? columnName(letter) : "") + String.fromCharCode(65 + (colIndex % 26));
    }

    function displayRef(sheet, row, col, rel) {
        var aa = "";

        ++row;

        if (!isFinite(row)) {
            row = "";
        }
        else if (rel != null && !(rel & 2)) {
            row = "$" + row;
        }

        if (!isFinite(col)) {
            col = "";
        }
        else {
            aa = columnName(col);
            if (rel != null && !(rel & 1)) {
                aa = "$" + aa;
            }
        }

        if (sheet) {
            return sheet + "!" + aa + row;
        } else {
            return aa + row;
        }
    }

    /* -----[ References ]----- */

    var Ref = Class.extend({
        type: "ref",
        hasSheet: function() {
            return this._hasSheet;
        },
        simplify: function() {
            return this;
        },
        setSheet: function(sheet, hasSheet) {
            this.sheet = sheet;
            if (hasSheet != null) {
                this._hasSheet = hasSheet;
            }
            return this;
        },
        absolute: function(){
            return this;
        },
        relative: function(){
            return this;
        },
        adjust: function(){
            return this;
        },
        toString: function() {
            return this.relative(0, 0, 3, 3).print(0, 0);
        },
        forEach: function(callback, obj) {
            callback.call(obj, this);
        },
        map: function(callback, obj) {
            return callback.call(obj, this);
        },
        intersects: function(ref) {
            return this.intersect(ref) !== NULL;
        },
        isCell: function() {
            return false;
        },
        toColumn: function() {
            return this;
        },

        // UnionRef overrides these, to access its subranges.
        first: function() {
            return this;
        },
        lastRange: function() {
            return this;
        },
        size: function() {
            return 1;
        },
        rangeAt: function() {
            return this;
        },
        nextRangeIndex: function() {
            return 0;
        },
        previousRangeIndex: function() {
            return 0;
        },
        eq: function(reference) {
            var r1 = this;
            var r2 = reference;

            if (r1 === NULL || r2 === NULL) {
                return r1 === r2;
            }

            // make positions consistent
            if ((r2 instanceof CellRef) || (r2 instanceof RangeRef && !(r1 instanceof CellRef))) {
               r1 = reference;
               r2 = this;
            }

            if (r1 instanceof CellRef) { // cell eq *
                r2 = r2.simplify();
                return r2 instanceof CellRef && r1.row == r2.row && r1.col == r2.col && r1.sheet == r2.sheet;
            }
            else if (r1 instanceof RangeRef) { // range eq range/union
                if (r2 instanceof RangeRef) {
                    return r2.topLeft.eq(r1.topLeft) && r2.bottomRight.eq(r1.bottomRight);
                }
                if (r2 instanceof UnionRef) {
                    return r2.single() && r1.eq(r2.refs[0]);
                }
            }
            else if (r1 instanceof UnionRef && r2 instanceof UnionRef) { // union eq union
                var refs1 = r1.refs;
                var refs2 = r2.refs;
                if (refs1.length != refs2.length) {
                   return false;
                }

                for (var i = 0, len = refs1.length; i < len; i++) {
                    if (!refs1[i].eq(refs2[i])) {
                        return false;
                    }
                }

                return true;
            }

            return r1 === r2;   // XXX: possibly NameRef when we'll support it.
        },

        concat: function(ref) {
            return new UnionRef([this, ref]);
        },

        replaceAt: function(index, ref) {
            return ref;
        },

        forEachColumnIndex: function(callback) {
            this.forEachAxisIndex('col', callback);
        },

        forEachRowIndex: function(callback) {
            this.forEachAxisIndex('row', callback);
        },

        forEachAxisIndex: function(axis, callback) {
            var sorted = [];

            var method = axis === 'row' ? 'forEachRow' : 'forEachColumn';

            this[method](function(ref) {
                var index = ref.first()[axis];
                if (sorted.indexOf(index) === -1) {
                    sorted.push(index);
                }
            });

            sorted.sort(function(a,b) {
                return a > b ? 1 : (a < b ? -1 : 0);
            }).forEach(callback);
        }
    });

    Ref.display = displayRef;

    /* -----[ Null reference ]----- */

    var NULL = new (Ref.extend({
        init: function NullRef(){},
        print: function() {
            return "#NULL!";
        },
        clone: function() {
            return this;
        },
        eq: function(ref) {
            return ref === this;
        },
        forEach: function() {}
    }))();

    /* -----[ Name reference ]----- */

    var NameRef = Ref.extend({
        ref: "name",
        init: function NameRef(name){
            this.name = name;
        },
        print: function() {
            var ret = this.name;
            if (this.hasSheet()) {
                ret = this.sheet + "!" + ret;
            }
            return ret;
        }
    });

    /* -----[ Cell reference ]----- */

    var CellRef = Ref.extend({
        ref: "cell",
        init: function CellRef(row, col, rel) {
            this.row = row;
            this.col = col;
            this.rel = rel || 0;
        },
        clone: function() {
            return new CellRef(this.row, this.col, this.rel)
                .setSheet(this.sheet, this.hasSheet());
        },
        intersect: function(ref) {
            if (ref instanceof CellRef) {
                if (this.eq(ref)) {
                    return this;
                } else {
                    return NULL;
                }
            }
            return ref.intersect(this);
        },
        print: function(trow, tcol) {
            var col = this.col, row = this.row, rel = this.rel;
            if (trow == null) {
                if (isFinite(col)) {
                    col = rel & 1 ? ("C[" + col + "]") : ("C" + (col + 1));
                } else {
                    col = "";
                }
                if (isFinite(row)) {
                    row = rel & 2 ? ("R[" + row + "]") : ("R" + (row + 1));
                } else {
                    row = "";
                }
                return row + col;
            } else {
                if (rel & 1) {
                    // relative col, add target
                    col += tcol;
                }
                if (rel & 2) {
                    // relative row, add target
                    row += trow;
                }
                if ((isFinite(col) && col < 0) || (isFinite(row) && row < 0)) {
                    return "#REF!";
                }
                return displayRef(this._hasSheet && this.sheet, row, col, rel);
            }
        },
        absolute: function(arow, acol) {
            var ret = this.clone();
            if (ret.rel & 3 === 0) {
                return ret;    // already absolute
            }
            if (ret.rel & 1) {
                // relative col, add anchor
                ret.col += acol;
            }
            if (ret.rel & 2) {
                // relative row, add anchor
                ret.row += arow;
            }
            ret.rel = 0;
            return ret;
        },
        toRangeRef: function() {
            return new RangeRef(this, this);
        },
        relative: function(arow, acol, rel) {
            if (rel == null) {
                rel = this.rel;
            }
            var row = rel & 2 ? this.row - arow : this.row;
            var col = rel & 1 ? this.col - acol : this.col;
            return new CellRef(row, col, rel)
                .setSheet(this.sheet, this.hasSheet());
        },
        height: function() {
            return 1;
        },
        width: function() {
            return 1;
        },
        toString: function() {
            return displayRef(null, this.row, this.col, 3);
        },
        isCell: function() {
            return true;
        },
        leftColumn: function() {
            return this;
        },
        rightColumn: function() {
            return this;
        },
        topRow: function() {
            return this;
        },
        bottomRow: function() {
            return this;
        },
        forEachRow: function(callback) {
            callback(this.toRangeRef());
        },
        forEachColumn: function(callback) {
            callback(this.toRangeRef());
        },
        adjust: function(row, col, trow, tcol, forRow, start, delta) {
            var ref = this.absolute(row, col);
            if (forRow) {
                if (ref.row >= start) {
                    if (delta < 0 && ref.row < start - delta) {
                        return NULL;
                    }
                    ref.row += delta;
                }
            } else {
                if (ref.col >= start) {
                    if (delta < 0 && ref.col < start - delta) {
                        return NULL;
                    }
                    ref.col += delta;
                }
            }
            if (trow != null && tcol != null) {
                ref = ref.relative(trow, tcol, this.rel);
            }
            return ref;
        }
    });

    /* -----[ Range reference ]----- */

    var RangeRef = Ref.extend({
        ref: "range",
        init: function RangeRef(tl, br) {
            if (tl._hasSheet && br._hasSheet && tl.sheet.toLowerCase() != br.sheet.toLowerCase()) {
                // "3D" reference
                this.endSheet = br.sheet;
            }
            // we want to drop any sheet information from the cells here.
            this.topLeft = new CellRef(tl.row, tl.col, tl.rel);
            this.bottomRight = new CellRef(br.row, br.col, br.rel);
            this.normalize();
        },
        clone: function() {
            return new RangeRef(this.topLeft.clone(), this.bottomRight.clone()).setSheet(this.sheet, this.hasSheet());
        },
        _containsRange: function(range) {
            return this._containsCell(range.topLeft)
                && this._containsCell(range.bottomRight);
        },
        _containsCell: function(cell) {
            return cell.sheet == this.sheet
                && cell.row >= this.topLeft.row
                && cell.col >= this.topLeft.col
                && cell.row <= this.bottomRight.row
                && cell.col <= this.bottomRight.col;
        },
        contains: function(ref) {
            if (ref instanceof Array) {
                var that = this;

                return ref.some(function(_ref) { return that.contains(_ref); });
            }
            if (ref instanceof CellRef) {
                return this._containsCell(ref);
            }
            if (ref instanceof RangeRef) {
                return this._containsRange(ref);
            }
            return false;
        },
        _intersectRange: function(ref) {
            if (this.sheet != ref.sheet) {
                return NULL;
            }
            var a_left    = this.topLeft.col;
            var a_top     = this.topLeft.row;
            var a_right   = this.bottomRight.col;
            var a_bottom  = this.bottomRight.row;
            var b_left    = ref.topLeft.col;
            var b_top     = ref.topLeft.row;
            var b_right   = ref.bottomRight.col;
            var b_bottom  = ref.bottomRight.row;
            if (a_left <= b_right &&
                b_left <= a_right &&
                a_top <= b_bottom &&
                b_top <= a_bottom)
            {
                return new RangeRef(
                    // topLeft
                    new CellRef(Math.max(a_top, b_top),
                                Math.max(a_left, b_left)),
                    // bottomRight
                    new CellRef(Math.min(a_bottom, b_bottom),
                                Math.min(a_right, b_right))
                ).setSheet(this.sheet, this.hasSheet());
            } else {
                return NULL;
            }
        },
        intersect: function(ref) {
            if (ref === NULL) {
                return ref;
            }
            if (ref instanceof CellRef) {
                return this._containsCell(ref) ? ref : NULL;
            }
            if (ref instanceof RangeRef) {
                return this._intersectRange(ref).simplify();
            }
            if (ref instanceof UnionRef) {
                return ref.intersect(this);
            }
            throw new Error("Unknown reference");
        },
        simplify: function() {
            if (this.isCell()) {
                return new CellRef(
                    this.topLeft.row,
                    this.topLeft.col,
                    this.topLeft.rel
                ).setSheet(this.sheet, this.hasSheet());
            }
            return this;
        },
        normalize: function() {
            var a = this.topLeft, b = this.bottomRight;
            var r1 = a.row, c1 = a.col, r2 = b.row, c2 = b.col;
            var rr1 = a.rel & 2, rc1 = a.rel & 1;
            var rr2 = b.rel & 2, rc2 = b.rel & 1;
            var tmp, changes = false;
            if (r1 > r2) {
                changes = true;
                tmp = r1; r1 = r2; r2 = tmp;
                tmp = rr1; rr1 = rr2; rr2 = tmp;
            }
            if (c1 > c2) {
                changes = true;
                tmp = c1; c1 = c2; c2 = tmp;
                tmp = rc1; rc1 = rc2; rc2 = tmp;
            }
            if (changes) {
                this.topLeft = new CellRef(r1, c1, rc1 | rr1);
                this.bottomRight = new CellRef(r2, c2, rc2 | rr2);
            }
            return this;
        },
        print: function(trow, tcol) {
            var ret = this.topLeft.print(trow, tcol)
                + ":"
                + this.bottomRight.print(trow, tcol);
            if (this.hasSheet()) {
                ret = this.sheet + "!" + ret;
            }
            return ret;
        },
        absolute: function(arow, acol) {
            return new RangeRef(
                this.topLeft.absolute(arow, acol),
                this.bottomRight.absolute(arow, acol)
            ).setSheet(this.sheet, this.hasSheet());
        },
        relative: function(arow, acol, relTL, relBR) {
            if (relBR == null) {
                relBR = relTL;
            }
            return new RangeRef(
                this.topLeft.relative(arow, acol, relTL),
                this.bottomRight.relative(arow, acol, relBR)
            ).setSheet(this.sheet, this.hasSheet());
        },
        height: function() {
            if (this.topLeft.rel != this.bottomRight.rel) {
                throw new Error("Mixed relative/absolute references");
            }
            return this.bottomRight.row - this.topLeft.row + 1;
        },
        width: function() {
            if (this.topLeft.rel != this.bottomRight.rel) {
                throw new Error("Mixed relative/absolute references");
            }
            return this.bottomRight.col - this.topLeft.col + 1;
        },
        collapse: function() {
            return this.topLeft.toRangeRef();
        },
        leftColumn: function() {
            return new RangeRef(this.topLeft, new CellRef(this.bottomRight.row, this.topLeft.col));
        },
        rightColumn: function() {
            return new RangeRef(new CellRef(this.topLeft.row, this.bottomRight.col), this.bottomRight);
        },
        topRow: function() {
            return new RangeRef(this.topLeft, new CellRef(this.topLeft.row, this.bottomRight.col));
        },
        bottomRow: function() {
            return new RangeRef(new CellRef(this.bottomRight.row, this.topLeft.col), this.bottomRight);
        },
        toRangeRef: function() {
            return this;
        },
        toColumn: function(col) {
            return new RangeRef(
               new CellRef(this.topLeft.row, this.topLeft.col + col),
               new CellRef(this.bottomRight.row, this.topLeft.col + col)
            );
        },
        forEachRow: function(callback) {
            var startRow = this.topLeft.row;
            var endRow = this.bottomRight.row;
            var startCol = this.topLeft.col;
            var endCol = this.bottomRight.col;

            for (var i = startRow; i <= endRow; i++) {
                callback(new RangeRef(
                    new CellRef(i, startCol),
                    new CellRef(i, endCol)
                ));
            }
        },
        forEachColumn: function(callback) {
            var startRow = this.topLeft.row;
            var endRow = this.bottomRight.row;
            var startCol = this.topLeft.col;
            var endCol = this.bottomRight.col;

            for (var i = startCol; i <= endCol; i++) {
                callback(new RangeRef(
                    new CellRef(startRow, i),
                    new CellRef(endRow, i)
                ));
            }
        },
        intersecting: function(refs) {
            return refs.filter(function(ref) {
                return ref.toRangeRef().intersects(this);
            }, this);
        },

        union: function(refs, callback) {
            var intersecting = this.intersecting(refs);

            var topLeftRow = this.topLeft.row;
            var topLeftCol = this.topLeft.col;
            var bottomRightRow = this.bottomRight.row;
            var bottomRightCol = this.bottomRight.col;

            var modified = false;

            intersecting.forEach(function(ref) {
                ref = ref.toRangeRef();
                if (ref.topLeft.row < topLeftRow) {
                    modified = true;
                    topLeftRow = ref.topLeft.row;
                }

                if (ref.topLeft.col < topLeftCol) {
                    modified = true;
                    topLeftCol = ref.topLeft.col;
                }

                if (ref.bottomRight.row > bottomRightRow) {
                    modified = true;
                    bottomRightRow = ref.bottomRight.row;
                }

                if (ref.bottomRight.col > bottomRightCol) {
                    modified = true;
                    bottomRightCol = ref.bottomRight.col;
                }

                if (callback) {
                    callback(ref);
                }
            });

            var result = new RangeRef(
                new CellRef(topLeftRow, topLeftCol),
                new CellRef(bottomRightRow, bottomRightCol)
            );

            if (modified) {
                return result.union(refs, callback);
            } else {
                return result;
            }
        },
        resize: function(options) {
            var limit = Math.max.bind(Math, 0);
            function num(value) { return value || 0; }

            var top = this.topLeft.row + num(options.top);
            var left = this.topLeft.col + num(options.left);
            var bottom = this.bottomRight.row + num(options.bottom);
            var right = this.bottomRight.col + num(options.right);

            if (left < 0 && right < 0 || top < 0 && bottom < 0) {
                return NULL;
            } else if (top <= bottom && left <= right) {
                return new RangeRef(new CellRef(limit(top), limit(left)),
                                    new CellRef(limit(bottom), limit(right)));
            } else {
                return NULL;
            }
        },
        move: function(rows, cols) {
            return new RangeRef(
                new CellRef(this.topLeft.row + rows,
                            this.topLeft.col + cols),
                new CellRef(this.bottomRight.row + rows,
                            this.bottomRight.col + cols)
            );
        },
        first: function() {
            return this.topLeft;
        },
        isCell: function() {
            return !this.endSheet && this.topLeft.eq(this.bottomRight);
        },
        toString: function() {
            return this.topLeft + ":" + this.bottomRight;
        },
        adjust: function(row, col, trow, tcol, forRow, start, delta) {
            var tl = this.topLeft.adjust(row, col, trow, tcol, forRow, start, delta);
            var tr = this.bottomRight.adjust(row, col, trow, tcol, forRow, start, delta);
            if (tl === NULL && tr === NULL) {
                return NULL;
            }
            if (tl === NULL) {
                tl = this.topLeft.absolute(row, col);
                if (forRow) {
                    tl.row = start;
                } else {
                    tl.col = start;
                }
                if (trow != null && tcol != null) {
                    tl = tl.relative(trow, tcol, this.topLeft.rel);
                }
            }
            else if (tr === NULL) {
                tr = this.bottomRight.absolute(row, col);
                if (forRow) {
                    tr.row = start - 1;
                } else {
                    tr.col = start - 1;
                }
                if (trow != null && tcol != null) {
                    tr = tr.relative(trow, tcol, this.bottomRight.rel);
                }
            }
            return new RangeRef(tl, tr)
                .setSheet(this.sheet, this.hasSheet())
                .simplify();
        }
    });

    /* -----[ Union reference ]----- */

    var UnionRef = Ref.extend({
        init: function UnionRef(refs){
            this.refs = refs;
            this.length = this.refs.length;
        },
        intersect: function(ref) {
            var a = [];
            for (var i = 0; i < this.length; ++i) {
                var x = ref.intersect(this.refs[i]);
                if (x !== NULL) {
                    a.push(x);
                }
            }
            if (a.length > 0) {
                return new UnionRef(a).simplify();
            }
            return NULL;
        },
        simplify: function() {
            var u = new UnionRef(this.refs.reduce(function(a, ref){
                ref = ref.simplify();
                if (ref !== NULL) {
                    a.push(ref);
                }
                return a;
            }, []));
            if (u.empty()) {
                return NULL;
            }
            if (u.single()) {
                return u.refs[0];
            }
            return u;
        },
        absolute: function(arow, acol) {
            return new UnionRef(this.refs.map(function(ref){
                return ref.absolute(arow, acol);
            }));
        },
        forEach: function(callback, obj) {
            this.refs.forEach(callback, obj);
        },
        toRangeRef: function() {
            return this.refs[0].toRangeRef();
        },
        contains: function(theRef) {
            return this.refs.some(function(ref) { return ref.contains(theRef); });
        },
        map: function(callback, obj) {
            return new UnionRef(this.refs.map(callback, obj));
        },
        first: function() {
            return this.refs[0].first();
        },
        lastRange: function() {
            return this.refs[this.length - 1];
        },
        size: function() {
            return this.length;
        },
        single: function() {
            return this.length == 1;
        },
        empty: function() {
            return this.length === 0;
        },
        isCell: function() {
            return this.single() && this.refs[0].isCell();
        },
        rangeAt: function(index) {
            return this.refs[index];
        },
        nextRangeIndex: function(index) {
            if (index === this.length - 1) {
                return 0;
            } else {
                return index + 1;
            }
        },
        previousRangeIndex: function(index) {
            if (index === 0) {
                return this.length - 1;
            } else {
                return index - 1;
            }
        },
        concat: function(ref) {
            return new UnionRef(this.refs.concat([ref]));
        },
        print: function() {
            return this.refs.map(function(ref) { return ref.print(); }).join(",");
        },

        replaceAt: function(index, ref) {
            var newRefs = this.refs.slice();
            newRefs.splice(index, 1, ref);
            return new UnionRef(newRefs);
        },

        leftColumn: function() {
            return this.map(function(ref) {
                return ref.leftColumn();
            });
        },
        rightColumn: function() {
            return this.map(function(ref) {
                return ref.rightColumn();
            });
        },
        topRow: function() {
            return this.map(function(ref) {
                return ref.topRow();
            });
        },
        bottomRow: function() {
            return this.map(function(ref) {
                return ref.bottomRow();
            });
        },
        forEachRow: function(callback) {
            this.forEach(function(ref) {
                ref.forEachRow(callback);
            });
        },
        forEachColumn: function(callback) {
            this.forEach(function(ref) {
                ref.forEachColumn(callback);
            });
        },
        adjust: function(row, col, trow, tcol, forRow, start, delta) {
            return this.map(function(ref){
                return ref.adjust(row, col, trow, tcol, forRow, start, delta);
            }).simplify();
        },
        toString: function() {
            return this.refs.map(function(ref){
                return ref.toString();
            }).join(", ");
        }
    });

    /* -----[ exports ]----- */

    spreadsheet.NULLREF = NULL;
    spreadsheet.SHEETREF = new RangeRef(new CellRef(0, 0), new CellRef(Infinity, Infinity));
    spreadsheet.FIRSTREF = new CellRef(0, 0);
    spreadsheet.Ref = Ref;
    spreadsheet.NameRef = NameRef;
    spreadsheet.CellRef = CellRef;
    spreadsheet.RangeRef = RangeRef;
    spreadsheet.UnionRef = UnionRef;

    spreadsheet.SHEETREF.print = function() {
        return "#SHEET";
    };

})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var RangeRef = kendo.spreadsheet.RangeRef;
    var CellRef = kendo.spreadsheet.CellRef;

    var AutoFillCalculator = kendo.Class.extend({
        init: function(grid) {
            this._grid = grid;
        },

        rectIsVertical: function(start, end, x, y) {
            var startRect = this._grid.rectangle(start.toRangeRef());
            var endRect = this._grid.rectangle(end.toRangeRef());
            return Math.abs(endRect[y] - startRect[y]) > Math.abs(startRect[x] - endRect[x]);
        },

        autoFillDest: function(selection, cursor) {
            var topLeft = selection.topLeft;
            var bottomRight = selection.bottomRight;

            var quadrant;
            var lower = cursor.row >= topLeft.row;
            var further = cursor.col >= topLeft.col;

            if (lower) {
                quadrant = further ? 4 : 3;
            } else {
                quadrant = further ? 2 : 1;
            }

            var pivot, opposite, cornerResult, expanding;

            if (quadrant === 4) {
                pivot = topLeft;
                opposite = bottomRight;

                expanding = cursor.row > opposite.row || cursor.col > opposite.col;

                if (expanding) {
                    cursor = new CellRef(Math.max(cursor.row, opposite.row), Math.max(cursor.col, opposite.col));
                }

                if (this.rectIsVertical(opposite, cursor, 'right', 'bottom')) { // vertical
                    cornerResult = new CellRef(cursor.row, opposite.col);
                } else {
                    cornerResult = new CellRef(opposite.row, cursor.col);
                }
            } else if (quadrant === 3) {
                var bottomLeft = new CellRef(topLeft.col, bottomRight.row);

                if (cursor.row > bottomRight.row && this.rectIsVertical(bottomLeft, cursor, 'left', 'bottom')) { // vertical
                    pivot = topLeft;
                    cornerResult = new CellRef(cursor.row, bottomRight.col);
                } else {
                    pivot = bottomRight;
                    cornerResult = new CellRef(topLeft.row, cursor.col);
                }
            } else if (quadrant === 2){
                var topRight = new CellRef(topLeft.row, bottomRight.col);

                if (cursor.col > bottomRight.col && !this.rectIsVertical(topRight, cursor, 'right', 'top')) { // horizontal
                    pivot = topLeft;
                    cornerResult = new CellRef(bottomRight.row, cursor.col);
                } else {
                    pivot = bottomRight;
                    cornerResult = new CellRef(cursor.row, topLeft.col);
                }
            } else {
                pivot = bottomRight;
                if (this.rectIsVertical(topLeft, cursor, 'left', 'top')) { // horizontal
                    cornerResult = new CellRef(cursor.row, topLeft.col);
                } else {
                    cornerResult = new CellRef(topLeft.row, cursor.col);
                }
            }

            return this._grid.normalize(new RangeRef(pivot, cornerResult));
        }
    });

    kendo.spreadsheet.AutoFillCalculator = AutoFillCalculator;
})(kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var RangeRef = kendo.spreadsheet.RangeRef;
    var CellRef = kendo.spreadsheet.CellRef;

    var EdgeNavigator = kendo.Class.extend({
        init: function(field, axis, rangeGetter, union) {
            this.rangeGetter = rangeGetter;

            this.prevLeft = function(index) {
                var current = union(this.range(index));
                var range = this.range(axis.prevVisible(current.topLeft[field]));
                return union(range).topLeft[field];
            };

            this.nextRight = function(index) {
                var current = union(this.range(index));
                var range = this.range(axis.nextVisible(current.bottomRight[field]));
                return union(range).bottomRight[field];
            };

            // these two don't look like the other ones, as they "shrink"
            this.nextLeft = function(index) {
                var range = union(this.range(index));
                return axis.nextVisible(range.bottomRight[field]);
            };

            this.prevRight = function(index) {
                var range = union(this.range(index));
                return axis.prevVisible(range.topLeft[field]);
            };
        },

        boundary: function(top, bottom) {
            this.top = top;
            this.bottom = bottom;
        },

        range: function(index) {
            return this.rangeGetter(index, this.top, this.bottom);
        }
    });

    var SheetNavigator = kendo.Class.extend({
        init: function(sheet) {
            this._sheet = sheet;
            this.columns = this._sheet._grid._columns;
            this.autoFillCalculator = new kendo.spreadsheet.AutoFillCalculator(sheet._grid);

            this.colEdge = new EdgeNavigator("col", this._sheet._grid._columns, this.columnRange.bind(this), this.union.bind(this));
            this.rowEdge = new EdgeNavigator("row", this._sheet._grid._rows, this.rowRange.bind(this), this.union.bind(this));
        },

        height: function(height) {
            this._viewPortHeight = height;
        },

        union: function(ref) {
            return this._sheet.unionWithMerged(ref);
        },

        columnRange: function(col, topRow, bottomRow) {
            return this._sheet._ref(topRow, col, bottomRow - topRow, 1);
        },

        rowRange: function(row, leftCol, rightCol) {
            return this._sheet._ref(row, leftCol, 1, rightCol - leftCol);
        },

        selectionIncludesMergedCells: function() {
            return this._sheet.select().contains(this._sheet._mergedCells);
        },

        setSelectionValue: function(value) {
            var selection = this._sheet.selection();

            setTimeout(function() {
                selection.value(value());
            });
        },

        selectAll: function() {
            this._sheet.select(this._sheet._sheetRef);
        },

        select: function(ref, mode, addToExisting) {
            ref = this.refForMode(ref, mode);

            if (addToExisting) {
                ref = this._sheet.select().concat(ref);
            }

            this._sheet.select(ref);
        },

        refForMode: function(ref, mode) {
            var grid = this._sheet._grid;

            switch(mode) {
                case "range":
                    ref = grid.normalize(ref);
                    break;
                case "row":
                    ref = grid.rowRef(ref.row);
                    break;
                case "column":
                    ref = grid.colRef(ref.col);
                    break;
                case "sheet":
                    ref = this._sheet._sheetRef;
                    break;
            }

            return ref;
        },

        startSelection: function(ref, mode, addToExisting) {
            if (mode == "autofill") {
                this._sheet.startAutoFill();
            } else {
                this._sheet.startSelection();
                this.select(ref, mode, addToExisting);
            }
        },

        completeSelection: function() {
            this._sheet.completeSelection();
        },

        selectForContextMenu: function(ref, mode) {
            var sheet = this._sheet;

            if (!sheet.select().contains(this.refForMode(ref, mode))) {
                this.select(ref, mode);
            }
        },

        modifySelection: function(action) {
            var direction = this.determineDirection(action);

            var sheet = this._sheet;
            var viewPortHeight = this._viewPortHeight;
            var rows = sheet._grid._rows;
            var columns = sheet._grid._columns;

            var originalSelection = sheet.currentOriginalSelectionRange();
            var selection = sheet.select().toRangeRef();
            var activeCell = sheet.activeCell();

            var topLeft = originalSelection.topLeft.clone();
            var bottomRight = originalSelection.bottomRight.clone();

            var scrollInto;

            this.colEdge.boundary(selection.topLeft.row, selection.bottomRight.row);
            this.rowEdge.boundary(selection.topLeft.col, selection.bottomRight.col);

            switch (direction) {
                case "expand-left": // <| |
                    topLeft.col = this.colEdge.prevLeft(topLeft.col);
                    scrollInto = topLeft;
                    break;
                case "shrink-right": // |>|
                    topLeft.col = this.colEdge.nextLeft(topLeft.col);
                    scrollInto = topLeft;
                    break;
                case "expand-right": // | |>
                    bottomRight.col = this.colEdge.nextRight(bottomRight.col);
                    scrollInto = bottomRight;
                    break;
                case "shrink-left": // |<|
                    bottomRight.col = this.colEdge.prevRight(bottomRight.col);
                    scrollInto = bottomRight;
                    break;

                // four actions below mirror the upper ones, on the vertical axis
                case "expand-up":
                    topLeft.row = this.rowEdge.prevLeft(topLeft.row);
                    scrollInto = topLeft;
                    break;
                case "shrink-down":
                    topLeft.row = this.rowEdge.nextLeft(topLeft.row);
                    scrollInto = topLeft;
                    break;
                case "expand-down":
                    bottomRight.row = this.rowEdge.nextRight(bottomRight.row);
                    scrollInto = bottomRight;
                    break;
                case "shrink-up":
                    bottomRight.row = this.rowEdge.prevRight(bottomRight.row);
                    scrollInto = bottomRight;
                    break;

                // pageup/down - may need improvement
                case "expand-page-up":
                    topLeft.row = rows.prevPage(topLeft.row, viewPortHeight);
                    break;
                case "shrink-page-up":
                    bottomRight.row = rows.prevPage(bottomRight.row, viewPortHeight);
                    break;
                case "expand-page-down":
                    bottomRight.row = rows.nextPage(bottomRight.row, viewPortHeight);
                    break;
                case "shrink-page-down":
                    topLeft.row = rows.nextPage(topLeft.row, viewPortHeight);
                    break;

                case "first-col":
                    topLeft.col = columns.firstVisible();
                    bottomRight.col = activeCell.bottomRight.col;
                    scrollInto = topLeft;
                    break;
                case "last-col":
                    bottomRight.col = columns.lastVisible();
                    topLeft.col = activeCell.topLeft.col;
                    scrollInto = bottomRight;
                    break;
                case "first-row":
                    topLeft.row = rows.firstVisible();
                    bottomRight.row = activeCell.bottomRight.row;
                    scrollInto = topLeft;
                    break;
                case "last-row":
                    bottomRight.row = rows.lastVisible();
                    topLeft.row = activeCell.topLeft.row;
                    scrollInto = bottomRight;
                    break;
                case "last":
                    bottomRight.row = rows.lastVisible();
                    bottomRight.col = columns.lastVisible();
                    topLeft = activeCell.topLeft;
                    scrollInto = bottomRight;
                    break;
                case "first":
                    topLeft.row = rows.firstVisible();
                    topLeft.col = columns.firstVisible();
                    bottomRight = activeCell.bottomRight;
                    scrollInto = topLeft;
                    break;
            }

            var newSelection = new RangeRef(topLeft, bottomRight);

            if (!this.union(newSelection).intersects(activeCell)) {
                // throw new Error(newSelection.print() + " does not contain " + activeCell.print());
                this.modifySelection(direction.replace("shrink", "expand"));
                return;
            }

            if (scrollInto) {
                sheet.focus(scrollInto);
            }

            this.updateCurrentSelectionRange(newSelection);
        },

        moveActiveCell: function(direction) {
            var sheet = this._sheet;
            var activeCell = sheet.activeCell();
            var topLeft = activeCell.topLeft;
            var bottomRight = activeCell.bottomRight;

            var cell = sheet.originalActiveCell();
            var rows = sheet._grid._rows;
            var columns = sheet._grid._columns;

            var row = cell.row;
            var column = cell.col;

            switch (direction) {
                case "left":
                    column = columns.prevVisible(topLeft.col);
                    break;
                case "up":
                    row = rows.prevVisible(topLeft.row);
                    break;
                case "right":
                    column = columns.nextVisible(bottomRight.col);
                    break;
                case "down":
                    row = rows.nextVisible(bottomRight.row);
                    break;
                case "first-col":
                    column = columns.firstVisible();
                    break;
                case "last-col":
                    column = columns.lastVisible();
                    break;
                case "first-row":
                    row = rows.firstVisible();
                    break;
                case "last-row":
                    row = rows.lastVisible();
                    break;
                case "last":
                    row = rows.lastVisible();
                    column = columns.lastVisible();
                    break;
                case "first":
                    row = rows.firstVisible();
                    column = columns.firstVisible();
                    break;
                case "next-page":
                    row = rows.nextPage(bottomRight.row, this._viewPortHeight);
                    break;
                case "prev-page":
                    row = rows.prevPage(bottomRight.row, this._viewPortHeight);
                    break;

            }

            sheet.select(new CellRef(row, column));
        },

        navigateInSelection: function(direction) {
            var sheet = this._sheet;
            var activeCell = sheet.activeCell();
            var topLeft = activeCell.topLeft;

            var cell = sheet.originalActiveCell();
            var rows = sheet._grid._rows;
            var columns = sheet._grid._columns;

            var row = cell.row;
            var column = cell.col;

            var selection = sheet.currentNavigationRange();
            var selTopLeft = selection.topLeft;
            var selBottomRight = selection.bottomRight;

            var done = false;

            var topLeftCol = topLeft.col;
            var topLeftRow = topLeft.row;

            while (!done) {
                var current = new CellRef(row, column);

                switch (direction) {
                    case "next":
                        if (selBottomRight.eq(current)) {
                            selection = sheet.nextNavigationRange();
                            row = selection.topLeft.row;
                            column = selection.topLeft.col;
                        } else {
                            column = columns.nextVisible(topLeftCol, true);
                            if (column > selBottomRight.col) {
                                column = selTopLeft.col;
                                row = rows.nextVisible(row, true);
                            }
                        }
                        break;
                    case "previous":
                        if (selTopLeft.eq(current)) {
                            selection = sheet.previousNavigationRange();
                            row = selection.bottomRight.row;
                            column = selection.bottomRight.col;
                        } else {
                            column = columns.prevVisible(topLeftCol, true);
                            if (column < selTopLeft.col) {
                                column = selBottomRight.col;
                                row = rows.prevVisible(row, true);
                            }
                        }
                        break;
                    case "lower":
                        if (selBottomRight.eq(current)) {
                            selection = sheet.nextNavigationRange();
                            row = selection.topLeft.row;
                            column = selection.topLeft.col;
                        } else {
                            row = rows.nextVisible(topLeftRow, true);
                            if (row > selBottomRight.row) {
                                row = selTopLeft.row;
                                column = columns.nextVisible(column, true);
                            }
                        }
                        break;
                    case "upper":
                        if (selTopLeft.eq(current)) {
                            selection = sheet.previousNavigationRange();
                            row = selection.bottomRight.row;
                            column = selection.bottomRight.col;
                        } else {
                            row = rows.prevVisible(topLeftRow, true);
                            if (row < selTopLeft.row) {
                                row = selBottomRight.row;
                                column = columns.prevVisible(column, true);
                            }
                        }
                        break;
                    default:
                        throw new Error("Unknown entry navigation: " + direction);
                }

                done = !this.shouldSkip(row, column);
                // if (!done) { console.log("skipping", row, column); }
                topLeftCol = column;
                topLeftRow = row;
            }

            if (sheet.singleCellSelection()) {
                sheet.select(new CellRef(row, column));
            } else {
                sheet.activeCell(new CellRef(row, column));
            }
        },

        extendSelection: function(ref, mode) {
            var sheet = this._sheet;
            var grid = sheet._grid;

            if (mode === "autofill") {
               this.resizeAutoFill(ref);
               return;
            }
            if (mode === "range") {
                ref = grid.normalize(ref);
            }
            else if (mode === "row") {
                ref = grid.rowRef(ref.row).bottomRight;
            } else if (mode === "column") {
                ref = grid.colRef(ref.col).bottomRight;
            }

            var activeCell = sheet.originalActiveCell().toRangeRef();

            this.updateCurrentSelectionRange(new RangeRef(activeCell.topLeft, ref));
        },

        shouldSkip: function(row, col) {
            var ref = new CellRef(row, col);
            var isMerged = false;
            this._sheet.forEachMergedCell(function(merged) {
                if (merged.intersects(ref) && !merged.collapse().eq(ref)) {
                    isMerged = true;
                }
            });

            return isMerged;
        },

        resizeAutoFill: function(ref) {
            var sheet = this._sheet;
            var selection = sheet.select();
            var origin = sheet._autoFillOrigin;
            var dest = this.autoFillCalculator.autoFillDest(selection, ref);

            var punch = this.punch(selection, dest);
            var hint, direction, row;

            if (!punch) {
                var preview = sheet.range(dest)._previewFillFrom(sheet.range(origin));

                if (preview) {
                    direction = preview.direction;
                    var props = preview.props;

                    if (direction === 0 || direction == 1) {
                        row = props[props.length - 1];
                        hint = row[row.length - 1].value;
                    } else if (direction === 2) {
                        row = props[0];
                        hint = row[row.length - 1].value;
                    } else if (direction === 3) {
                        row = props[props.length - 1];
                        hint = row[0].value;
                    }
                }
            }

            sheet.updateAutoFill(dest, punch, hint, direction);
        },

        determineDirection: function(action) {
            var selection = this._sheet.currentSelectionRange();
            var activeCell = this._sheet.activeCell();

            // There may be a third, indeterminate state, caused by a merged cell.
            // In this state, all key movements are treated as shrinks.
            // The navigator will reverse them if it detects this it will cause the selection to exclude the active cell.
            var leftMode = activeCell.topLeft.col == selection.topLeft.col;
            var rightMode = activeCell.bottomRight.col == selection.bottomRight.col;
            var topMode = activeCell.topLeft.row == selection.topLeft.row;
            var bottomMode = activeCell.bottomRight.row == selection.bottomRight.row;

            switch(action) {
                case "left":
                    action = rightMode ? "expand-left" : "shrink-left";
                    break;
                case "right":
                    action = leftMode ? "expand-right" : "shrink-right";
                    break;
                case "up":
                    action = bottomMode ? "expand-up" : "shrink-up";
                    break;
                case "down":
                    action = topMode ? "expand-down" : "shrink-down";
                    break;
                case "prev-page":
                    action = bottomMode ? "expand-page-up" : "shrink-page-up";
                    break;
                case "next-page":
                    action = topMode ? "expand-page-down" : "shrink-page-down";
                    break;
            }

            return action;
        },

        updateCurrentSelectionRange: function(ref) {
            var sheet = this._sheet;
            sheet.select(sheet.originalSelect().replaceAt(sheet.selectionRangeIndex(), ref), false);
        },

        punch: function(selection, subset) {
            var punch;
            if (subset.topLeft.eq(selection.topLeft)) {
                if (subset.bottomRight.row < selection.bottomRight.row) {
                    var bottomRow = this.rowEdge.nextRight(subset.bottomRight.row);

                    punch = new RangeRef(
                        new CellRef(bottomRow, selection.topLeft.col),
                        selection.bottomRight
                    );
                } else if (subset.bottomRight.col < selection.bottomRight.col) {
                    var bottomCol = this.colEdge.nextRight(subset.bottomRight.col);

                    punch = new RangeRef(
                        new CellRef(selection.topLeft.row, bottomCol),
                        selection.bottomRight
                    );
                }
            }

            return punch;
        }
    });

    kendo.spreadsheet.SheetNavigator = SheetNavigator;
})(kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var AxisManager = kendo.Class.extend({
        init: function(sheet) {
            this._sheet = sheet;
        },

        forEachSelectedColumn: function(callback) {
            var sheet = this._sheet;

            sheet.batch(function() {
                sheet.select().forEachColumnIndex(function(index, i) {
                    callback(sheet, index, i);
                });
            }, { layout: true, recalc: true });
        },

        forEachSelectedRow: function(callback) {
            var sheet = this._sheet;

            sheet.batch(function() {
                sheet.select().forEachRowIndex(function(index, i) {
                    callback(sheet, index, i);
                });
            }, { layout: true, recalc: true });
        },

        includesHiddenColumns: function(ref) {
            return this._sheet._grid._columns.includesHidden(ref.topLeft.col, ref.bottomRight.col);
        },

        includesHiddenRows: function(ref) {
            return this._sheet._grid._rows.includesHidden(ref.topLeft.row, ref.bottomRight.row);
        },

        selectionIncludesHiddenColumns: function() {
            return this.includesHiddenColumns(this._sheet.select());
        },

        selectionIncludesHiddenRows: function() {
            return this.includesHiddenRows(this._sheet.select());
        },

        deleteSelectedColumns: function() {
            this.forEachSelectedColumn(function(sheet, index, i) {
                sheet.deleteColumn(index - i);
            });
        },

        deleteSelectedRows: function() {
            this.forEachSelectedRow(function(sheet, index, i) {
                sheet.deleteRow(index - i);
            });
        },

        hideSelectedColumns: function() {
            this.forEachSelectedColumn(function(sheet, index) {
                sheet.hideColumn(index);
            });
        },

        hideSelectedRows: function() {
            this.forEachSelectedRow(function(sheet, index) {
                sheet.hideRow(index);
            });
        },

        unhideSelectedColumns: function() {
            this.forEachSelectedColumn(function(sheet, index) {
                sheet.unhideColumn(index);
            });
        },

        unhideSelectedRows: function() {
            this.forEachSelectedRow(function(sheet, index) {
                sheet.unhideRow(index);
            });
        },

        addColumnLeft: function() {
            this.forEachSelectedColumn(function(sheet, index, i) {
                sheet.insertColumn(index - i);
            });
        },

        addColumnRight: function() {
            this.forEachSelectedColumn(function(sheet, index, i) {
                sheet.insertColumn(index + (i+1));
            });
        },

        canAddRow: function() {
            var range = this._sheet.select().toRangeRef();
            var rowCount = range.height();

            return this._sheet.canInsertRow(0, rowCount);
        },

        addRowAbove: function() {
            this.forEachSelectedRow(function(sheet, index, i) {
                sheet.insertRow(index - i);
            });
        },

        addRowBelow: function() {
            this.forEachSelectedRow(function(sheet, index, i) {
                sheet.insertRow(index + (i+1));
            });
        }
    });

    kendo.spreadsheet.AxisManager = AxisManager;
})(kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;
    var RangeRef = kendo.spreadsheet.RangeRef;
    var CellRef = kendo.spreadsheet.CellRef;

    var Clipboard = kendo.Class.extend({
        init: function(workbook) {
            this.workbook = workbook;
            this.origin = kendo.spreadsheet.NULLREF;
            this.iframe = document.createElement("iframe");
            this.iframe.className = "k-spreadsheet-clipboard-paste";
            this.menuInvoked = true;
            this._external = {};
            this._uid = kendo.guid();
            document.body.appendChild(this.iframe);
        },

        canCopy: function() {
            var status = {canCopy: true};
            var selection = this.workbook.activeSheet().select();
            if (selection === kendo.spreadsheet.NULLREF) {
                status.canCopy = false;
            }
            if (selection instanceof kendo.spreadsheet.UnionRef) {
                status.canCopy = false;
                status.multiSelection = true;
            }
            if(this.menuInvoked) {
                status.canCopy = false;
                status.menuInvoked = true;
            }
            return status;
        },

        canPaste: function() {
            var sheet = this.workbook.activeSheet();
            var ref = this.pasteRef();
            var status = {canPaste: true};
            if(ref === kendo.spreadsheet.NULLREF) {
                var external = this._external.hasOwnProperty("html") || this._external.hasOwnProperty("plain");
                status.pasteOnMerged = this.intersectsMerged();
                status.canPaste = status.pasteOnMerged ? false : external;
                return status;
            }
            if(!ref.eq(sheet.unionWithMerged(ref))) {
                status.canPaste = false;
                status.pasteOnMerged = true;
            }
            if(this.menuInvoked) {
                status.canPaste = false;
                status.menuInvoked = true;
            }
            return status;
        },

        intersectsMerged: function() {
            var sheet = this.workbook.activeSheet();
            var state = this.parse(this._external);
            this.origin = this.stateRangeRef(state);
            var ref = this.pasteRef();
            return !ref.eq(sheet.unionWithMerged(ref));
        },

        copy: function() {
            var sheet = this.workbook.activeSheet();
            this.origin = sheet.select();
            this.contents = sheet.selection().getState();
        },

        cut: function() {
            var sheet = this.workbook.activeSheet();
            this.copy();
            sheet.range(sheet.select()).clear();
        },

        pasteRef: function() {
            var sheet = this.workbook.activeSheet();
            var destination = sheet.activeCell().first();
            var originActiveCell = this.origin.first();
            var rowDelta = originActiveCell.row - destination.row;
            var colDelta = originActiveCell.col - destination.col;

            return this.origin.relative(rowDelta, colDelta, 3);
        },

        stateRangeRef: function(state) {
            var rows = [];
            var cols = [];
            for (var key in state) {
                if (key === "mergedCells" || key === "ref") {
                    continue;
                }
                var address = key.split(",");
                rows.push(address[0]);
                cols.push(address[1]);
            }
            var topLeft = new CellRef(Math.min.apply(null, rows), Math.min.apply(null, cols));
            var bottomRight = new CellRef(Math.max.apply(null, rows), Math.max.apply(null, cols));
            return new RangeRef(topLeft, bottomRight);
        },

        destroy: function() {
            document.body.removeChild(this.iframe);
        },

        paste: function() {
            var state = {};
            var sheet = this.workbook.activeSheet();
            if(this._isInternal()) {
                state = this.contents;
            } else {
                state = this.parse(this._external);
                this.origin = this.stateRangeRef(state);
            }
            var pasteRef = this.pasteRef();
            sheet.range(pasteRef).clear().setState(state);
            sheet.triggerChange({recalc: true});

        },

        external: function(data) {
            if (data.html || data.plain) {
                this._external = data;
            } else {
                return this._external;
            }
        },

        parse: function(data) {
            var state = {ref:  new CellRef(0,0,0), mergedCells: []};
            if(data.html) {
                var doc = this.iframe.contentWindow.document;
                doc.open();
                doc.write(data.html);
                doc.close();
                var table = $(doc).find("table:first");
                if(table.length) {
                    state = this._parseHTML(table.find("tbody:first"));
                } else {
                    if (!data.plain) {
                        var element = $(doc.body).find(":not(style)");
                        state["0,0"] = this._cellState(element.text());
                    } else {
                        state = this._parseTSV(data.plain);
                    }
                }
            } else {
                state = this._parseTSV(data.plain);
            }
            return state;
        },

        _parseHTML: function(tbody) {
            var that = this;
            var state = {ref:  new CellRef(0,0,0), mergedCells: []};

            tbody.find("tr").each(function(rowIndex, tr) {
                $(tr).find("td").each(function(colIndex, td) {
                    var rowspan = parseInt($(td).attr("rowspan"), 10) -1 || 0;
                    var colspan = parseInt($(td).attr("colspan"), 10) -1 || 0;
                    var blankCell = "<td/>";
                    var ci;
                    if(rowspan){
                        var endRow = rowIndex + rowspan;
                        for(var ri = rowIndex; ri <= endRow; ri++) {
                            var row = tbody.find("tr").eq(ri);
                            if(ri > rowIndex) {
                                blankCell = "<td class='rowspan'></td>";
                                if(colIndex === 0) {
                                    row.find("td").eq(colIndex).after(blankCell);
                                } else {
                                    var last = Math.min(row.find("td").length, colIndex);
                                    row.find("td").eq(last - 1).after(blankCell);
                                }
                            }
                            if(colspan) {
                                for(ci = colIndex; ci < colspan + colIndex; ci++) {
                                    blankCell = "<td class='rowspan colspan'></td>";
                                    row.find("td").eq(ci).after(blankCell);
                                }
                            }
                        }
                    } else {
                        if(colspan) {
                            for(ci = colIndex; ci < colspan + colIndex; ci++) {
                                blankCell = "<td class='colspan'></td>";
                                $(tr).find("td").eq(ci).after(blankCell);
                            }
                        }
                    }
                });
            });

            tbody.find("tr").each(function(rowIndex, tr) {
                $(tr).find("td").each(function(colIndex, td) {
                    var key = rowIndex + "," + colIndex;
                    var rowspan = parseInt($(td).attr("rowspan"), 10) -1 || 0;
                    var colspan = parseInt($(td).attr("colspan"), 10) -1 || 0;
                    var cellState = that._cellState($(td));

                    state[key] = cellState;

                    if(rowspan || colspan) {
                        var startCol = String.fromCharCode(65 + colIndex);
                        var endCol = String.fromCharCode(65 + colIndex + colspan);
                        var address = startCol + (rowIndex + 1) + ":" + endCol + (rowIndex + 1 + rowspan);

                        state.mergedCells.push(address);
                    }
                });
            });
            return state;
        },

        _parseTSV: function(data) {
            var state = {ref:  new CellRef(0,0,0), mergedCells: []};
            if(data.indexOf("\t") === -1 && data.indexOf("\n") == -1) {
                state["0,0"] = {
                    value: data
                };
            } else {
                var rows = data.split("\n");
                for(var ri = 0; ri < rows.length; ri++) {
                    var cols = rows[ri].split("\t");
                    for(var ci = 0; ci < cols.length; ci++) {
                        state[ri + "," + ci] = {value: cols[ci]};
                    }
                }
            }
            return state;
        },

        _isInternal: function() {
            if(this._external.html === undefined) {
                return true;
            }
            var internalHTML = $("<div/>").html(this._external.html).find("table.kendo-clipboard-"+ this._uid).length ? true : false;
            var internalPlain = $("<div/>").html(this._external.plain).find("table.kendo-clipboard-"+ this._uid).length ? true : false;
            if(internalHTML || internalPlain) {
                return true;
            }
            return false;
        },

        _cellState: function(element) {
            var styles = window.getComputedStyle(element[0]);
            var text = element.text();
            var borders = this._borderObject(styles);
            var state = {
                value: text === "" ? null : text,
                borderBottom : borders.borderBottom,
                borderRight : borders.borderRight,
                borderLeft : borders.borderLeft,
                borderTop : borders.borderTop,
                fontSize : parseInt(styles["font-size"], 10)
            };

            if(styles["background-color"] !== "rgb(0, 0, 0)" && styles["background-color"] !== "rgba(0, 0, 0, 0)") {
                state.background = styles["background-color"];
            }
            if(styles.color !== "rgb(0, 0, 0)" && styles.color !== "rgba(0, 0, 0, 0)") {
                state.color = styles.color;
            }
            if(styles["text-decoration"] == "underline") {
                state.underline = true;
            }
            if(styles["font-style"] == "italic") {
                state.italic = true;
            }
            if(styles["font-weight"] == "bold") {
                state.bold = true;
            }
            if(this._strippedStyle(styles["text-align"]) !== "right") {
                state.textAlign = this._strippedStyle(styles["text-align"]);
            }
            if(styles["vertical-align"] !== "middle") {
                state.verticalAlign = styles["vertical-align"];
            }
            if(styles["word-wrap"] !== "normal" ) {
                state.wrap = true;
            }

            return state;
        },

        _strippedStyle: function(style) {
            var prefixes = [
                "-ms-",
                "-moz-",
                "-webkit-"
            ];

            prefixes.forEach(function(prefix) {
                style = style.replace(prefix, "");
            });
            return style;
        },

        _borderObject: function(styles) {
            var borderObject = {};
            var borders = [
                "borderBottom",
                "borderRight",
                "borderLeft",
                "borderTop"
            ];

            borders.forEach(function(key) {
                if(styles[key + "Style"] == "none") {
                    borderObject[key] = null;
                    return;
                }
                borderObject[key] = {
                    size: 1,
                    color: styles[key + "Color"]
                };
            });

            return borderObject;
        }
    });
    kendo.spreadsheet.Clipboard = Clipboard;
})(kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;

    var UnionRef = kendo.spreadsheet.UnionRef;

    var styles = [
        "color", "fontFamily", "underline",
        "italic", "bold", "textAlign",
        "verticalAlign", "background"
    ];

    var borders = {
        borderTop: { complement: "borderBottom", direction: { top: -1, bottom: -1 } },
        borderLeft: { complement: "borderRight", direction: { left: -1, right: -1 } },
        borderRight: { complement: "borderLeft", direction: { left: 1, right: 1 }  },
        borderBottom: { complement: "borderTop", direction: { top: 1, bottom: 1 }  }
    };

    var Range = kendo.Class.extend({
        init: function(ref, sheet) {
            this._sheet = sheet;
            this._ref = ref;
        },

        _normalize: function(ref) {
            return this._sheet._grid.normalize(ref);
        },

        _set: function(name, value, noTrigger) {
            var sheet = this._sheet;
            this._ref.forEach(function(ref) {
                //TODO: set validation formula as OBJECT or JSON STRINGIFY?!!
                sheet._set(ref.toRangeRef(), name, value);
            });
            if (!noTrigger) {
                sheet.triggerChange({
                    recalc : name == "formula" || name == "value" || name == "validation",
                    value  : value,
                    ref    : this._ref
                });
            }
            return this;
        },

        _get: function(name) {
            return this._sheet._get(this._ref.toRangeRef(), name);
        },

        _property: function(name, value) {
            if (value === undefined) {
                return this._get(name);
            } else {
                return this._set(name, value);
            }
        },

        value: function(value) {
            if (value !== undefined) {
                // When value is set through the public API we must clear the
                // formula.  Don't trigger change (third parameter), it'll be
                // done when setting the value below
                this._set("formula", null, true);
            }
            return this._property("value", value);
        },

        resize: function(direction) {
            var ref = this._resizedRef(direction);
            return new Range(ref, this._sheet);
        },

        _resizedRef: function(direction) {
            return this._ref.map(function(ref) {
                return ref.toRangeRef().resize(direction);
            });
        },

        _border: function(property, value) {
            var result;
            var complement = borders[property].complement;
            var direction = borders[property].direction;
            var sheet = this._sheet;

            sheet.batch(function() {
                result = this._property(property, value);

                if (value !== undefined) {
                    this._resizedRef(direction).forEach(function(ref) {
                        if (ref !== kendo.spreadsheet.NULLREF) {
                            new Range(ref, sheet)._property(complement, null);
                        }
                    });
                }
            }.bind(this), {});

            return result;
        },

        _collapsedBorder: function(property) {
            var result = this._property(property);
            var complement = borders[property].complement;
            var direction = borders[property].direction;

            this._resizedRef(direction).forEach(function(ref) {
                if (!result && ref !== kendo.spreadsheet.NULLREF) {
                    var range = new Range(ref, this._sheet);
                    result = range._property(complement);
                }
            }.bind(this));

            return result;
        },

        borderTop: function(value) {
            return this._border("borderTop", value);
        },
        borderRight: function(value) {
            return this._border("borderRight", value);
        },
        borderBottom: function(value) {
            return this._border("borderBottom", value);
        },
        borderLeft: function(value) {
            return this._border("borderLeft", value);
        },

        collapsedBorderTop: function() {
            return this._collapsedBorder("borderTop");
        },
        collapsedBorderRight: function() {
            return this._collapsedBorder("borderRight");
        },
        collapsedBorderBottom: function() {
            return this._collapsedBorder("borderBottom");
        },
        collapsedBorderLeft: function() {
            return this._collapsedBorder("borderLeft");
        },

        input: function(value) {
            if (value !== undefined) {
                var tl = this._ref.toRangeRef().topLeft;
                var x = kendo.spreadsheet.calc.parse(this._sheet.name(), tl.row, tl.col, value);
                this._sheet.batch(function() {
                    var formula = null;
                    if (x.type == "exp") {
                        formula = kendo.spreadsheet.calc.compile(x);
                    } else if (x.type == "date") {
                        this.format(toExcelFormat(kendo.culture().calendar.patterns.d));
                    } else if (x.type == "percent") {
                        this.format(x.value*100 == (x.value*100|0) ? "0%" : "0.00%");
                    }
                    this.formula(formula);
                    if (!formula) {
                        // value() will clear the formula.  Lucky for us,
                        // x.value is undefined so it actually won't, but let's
                        // be explicit and only set value if formula is not
                        // present.
                        this.value(x.value);
                    }
                }.bind(this), { recalc: true, value: value, ref: this._ref });

                return this;
            } else {
                value = this._get("value");
                var format = this._get("format");
                var formula = this._get("formula");
                var type = format && !formula && kendo.spreadsheet.formatting.type(value, format);

                if (formula) {
                    // it's a Formula object which stringifies to the
                    // formula as text (without the starting `=`).
                    value = "=" + formula;
                } else if (type === "date") {
                    value = kendo.toString(kendo.spreadsheet.numberToDate(value), kendo.culture().calendar.patterns.d);
                } else if (type === "percent") {
                    value = (value * 100) + "%";
                } else if (typeof value == "string" &&
                           (/^[=']/.test(value) ||
                            (/^(?:true|false)$/i).test(value) ||
                            looksLikeANumber(value))) {
                    value = "'" + value;
                }

                return value;
            }
        },

        format: function(value) {
            return this._property("format", value);
        },

        formula: function(value) {
            if (value === undefined) {
                var f = this._get("formula");
                return f ? "" + f : null; // stringify if present
            }
            return this._property("formula", value);
        },

        validation: function(value) {
            //TODO: Accept objects only?

            if (value === undefined) {
                var f = this._get("validation");

                return f ? f.toJSON() : null; // stringify if present
            }
            return this._property("validation", value);
        },

        _getValidationState: function() {
            var ref = this._ref.toRangeRef();
            var topLeftRow = ref.topLeft.row;
            var topLeftCol = ref.topLeft.col;
            var bottomRightRow = ref.bottomRight.row;
            var bottomRightCol = ref.bottomRight.col;
            var ci, ri;

            for (ci = topLeftCol; ci <= bottomRightCol; ci ++) {
                for (ri = topLeftRow; ri <= bottomRightRow; ri ++) {
                    var validation = this._sheet._validation(ri, ci);

                    if (validation && validation.type === "reject" && validation.value === false) {
                        return validation;
                    }
                }
            }

            return false;
        },

        merge: function() {
            this._ref = this._sheet._merge(this._ref);
            return this;
        },

        unmerge: function() {
            var mergedCells = this._sheet._mergedCells;

            this._ref.forEach(function(ref) {
                ref.toRangeRef().intersecting(mergedCells).forEach(function(mergedRef) {
                    mergedCells.splice(mergedCells.indexOf(mergedRef), 1);
                });
            });

            this._sheet.triggerChange({});

            return this;
        },

        select: function() {
            this._sheet.select(this._ref);

            return this;
        },

        values: function(values) {
            if (this._ref instanceof UnionRef) {
                throw new Error("Unsupported for multiple ranges.");
            }

            if (this._ref === kendo.spreadsheet.NULLREF) {
                if (values !== undefined) {
                    throw new Error("Unsupported for NULLREF.");
                } else {
                    return [];
                }
            }

            var ref = this._ref.toRangeRef();
            var topLeftRow = ref.topLeft.row;
            var topLeftCol = ref.topLeft.col;
            var bottomRightRow = ref.bottomRight.row;
            var bottomRightCol = ref.bottomRight.col;
            var ci, ri;

            if (values === undefined) {
                values = new Array(ref.height());

                for (var vi = 0; vi < values.length; vi++) {
                    values[vi] = new Array(ref.width());
                }

                for (ci = topLeftCol; ci <= bottomRightCol; ci ++) {
                    for (ri = topLeftRow; ri <= bottomRightRow; ri ++) {
                        values[ri - topLeftRow][ci - topLeftCol] = this._sheet._value(ri, ci);
                    }
                }

                return values;
            } else {
                for (ci = topLeftCol; ci <= bottomRightCol; ci ++) {
                    for (ri = topLeftRow; ri <= bottomRightRow; ri ++) {
                        var row = values[ri - topLeftRow];

                        if (row) {
                            var value = row[ci - topLeftCol];

                            if (value !== undefined) {
                                this._sheet._value(ri, ci, value);
                            }
                        }
                    }
                }

                this._sheet.triggerChange({ recalc: true });

                return this;
            }
        },

        _properties: function(props) {
            if (this._ref instanceof UnionRef) {
                throw new Error("Unsupported for multiple ranges.");
            }

            if (this._ref === kendo.spreadsheet.NULLREF) {
                if (props !== undefined) {
                    throw new Error("Unsupported for NULLREF.");
                } else {
                    return [];
                }
            }

            var ref = this._ref.toRangeRef();
            var topLeftRow = ref.topLeft.row;
            var topLeftCol = ref.topLeft.col;
            var bottomRightRow = ref.bottomRight.row;
            var bottomRightCol = ref.bottomRight.col;
            var ci, ri;
            var sheet = this._sheet;

            if (props === undefined) {
                props = new Array(ref.height());
                sheet.forEach(ref, function(row, col, data){
                    row -= topLeftRow;
                    col -= topLeftCol;
                    var line = props[row] || (props[row] = []);
                    line[col] = data;
                });
                return props;
            }
            else {
                var data;
                ref = ref.clone();
                var setProp = function(propName) {
                    var propValue = data[propName];
                    ref.topLeft.row = ref.bottomRight.row = ri;
                    ref.topLeft.col = ref.bottomRight.col = ci;
                    sheet._set(ref, propName, propValue);
                };
                for (ci = topLeftCol; ci <= bottomRightCol; ci ++) {
                    for (ri = topLeftRow; ri <= bottomRightRow; ri ++) {
                        var row = props[ri - topLeftRow];
                        if (row) {
                            data = row[ci - topLeftCol];
                            if (data) {
                                Object.keys(data).forEach(setProp);
                            }
                        }
                    }
                }
                sheet.triggerChange({ recalc: true });
                return this;
            }
        },

        clear: function(options) {
            var clearAll = !options || !Object.keys(options).length;

            var sheet = this._sheet;

            var reason = {
                recalc: clearAll || (options && options.contentsOnly === true)
            };

            sheet.batch(function() {

                if (reason.recalc) {
                    this.formula(null);
                }

                if (clearAll) {
                    this.validation(null);
                }

                if (clearAll || (options && options.formatOnly === true)) {
                    styles.forEach(function(x) {
                        this[x](null);
                    }.bind(this));
                    this.format(null);
                    this.unmerge();
                }

            }.bind(this), reason);

            return this;
        },

        clearContent: function() {
            return this.clear({ contentsOnly: true });
        },

        clearFormat: function() {
            return this.clear({ formatOnly: true });
        },

        isSortable: function() {
            return !(this._ref instanceof UnionRef || this._ref === kendo.spreadsheet.NULLREF);
        },

        sort: function(spec) {
            if (this._ref instanceof UnionRef) {
                throw new Error("Unsupported for multiple ranges.");
            }

            if (this._ref === kendo.spreadsheet.NULLREF) {
                throw new Error("Unsupported for NULLREF.");
            }

            if (spec === undefined) {
                spec = { column: 0 };
            }

            spec = spec instanceof Array ? spec : [spec];

            this._sheet._sortBy(this._ref.toRangeRef(), spec.map(function(spec, index) {
                if (typeof spec === "number") {
                    spec = { column: spec };
                }

                return {
                    index: spec.column === undefined ? index : spec.column,
                    ascending: spec.ascending === undefined ? true : spec.ascending
                };
            }));

            return this;
        },

        isFilterable: function() {
            return !(this._ref instanceof UnionRef);
        },

        filter: function(spec) {
            if (this._ref instanceof UnionRef) {
                throw new Error("Unsupported for multiple ranges.");
            }

            if (spec === false) {
                this.clearFilters();
            } else {
                spec = spec === true ? [] : spec instanceof Array ? spec : [spec];

                this._sheet._filterBy(this._ref.toRangeRef(), spec.map(function(spec, index) {
                   return {
                       index: spec.column === undefined ? index : spec.column,
                       filter: spec.filter
                   };
                }));
            }

            return this;
        },

        clearFilter: function(spec) {
            this._sheet.clearFilter(spec);
        },

        clearFilters: function() {
            var filter = this._sheet.filter();
            var spec = [];

            if (filter) {
                for (var i = 0; i < filter.columns.length; i++) {
                    spec.push(i);
                }

                this._sheet.batch(function() {
                    this.clearFilter(spec);
                    this._filter = null;
                }, { layout: true, filter: true });
            }
        },

        hasFilter: function() {
            var filter = this._sheet.filter();
            return !!filter;
        },

        leftColumn: function() {
            return new Range(this._ref.leftColumn(), this._sheet);
        },

        rightColumn: function() {
            return new Range(this._ref.rightColumn(), this._sheet);
        },

        topRow: function() {
            return new Range(this._ref.topRow(), this._sheet);
        },

        bottomRow: function() {
            return new Range(this._ref.bottomRow(), this._sheet);
        },

        column: function(column) {
            return new Range(this._ref.toColumn(column), this._sheet);
        },

        forEachRow: function(callback) {
            this._ref.forEachRow(function(ref) {
                callback(new Range(ref, this._sheet));
            }.bind(this));
        },

        forEachColumn: function(callback) {
            this._ref.forEachColumn(function(ref) {
                callback(new Range(ref, this._sheet));
            }.bind(this));
        },

        sheet: function() {
            return this._sheet;
        },

        topLeft: function() {
            return this._ref.toRangeRef().topLeft;
        },

        intersectingMerged: function() {
            var sheet = this._sheet;
            var mergedCells = [];

            sheet._mergedCells.forEach(function(ref) {
                if (ref.intersects(this._ref)) {
                    mergedCells.push(ref.toString());
                }
            }.bind(this));

            return mergedCells;
        },

        getState: function(propertyName) {
            var state = {ref: this._ref.first()};
            var properties;
            if (!propertyName) {
                properties = kendo.spreadsheet.ALL_PROPERTIES;
                state.mergedCells = this.intersectingMerged();
            } else if (propertyName === "input") {
                properties = ["value", "formula"];
            } else if (propertyName === "border") {
                properties = ["borderLeft", "borderTop", "borderRight", "borderBottom"];
            } else {
                properties = [propertyName];
            }

            this.forEachCell(function(row, col, cell) {
                var cellState = state[row + "," + col] = {};

                properties.forEach(function(property) {
                    cellState[property] = cell[property] || null;
                });
            });

            return state;
        },

        setState: function(state) {
            var sheet = this._sheet;
            var origin = this._ref.first();
            var rowDelta = state.ref.row - origin.row;
            var colDelta = state.ref.col - origin.col;

            sheet.batch(function() {
                if (state.mergedCells) {
                    this.unmerge();
                }

                this.forEachCell(function(row, col) {
                    var cellState = state[(row + rowDelta)  + "," + (col + colDelta)];
                    var range = sheet.range(row, col);

                    for (var property in cellState) {
                        if (property != "value") {
                            // make sure value comes last (after the loop),
                            // because if we set value here and get get to
                            // formula later and cellState.formula is null,
                            // it'll clear the value.
                            range._set(property, cellState[property]);
                        }
                    }

                    if (!cellState.formula) {
                        // only need to set the value if we don't have a
                        // formula.  Go through the lower level setter rather
                        // than range.value(...), because range.value will clear
                        // the formula!  chicken and egg issues.
                        range._set("value", cellState.value);
                    }
                });

                if (state.mergedCells) {
                    state.mergedCells.forEach(function(merged) {
                        merged = sheet._ref(merged).relative(rowDelta, colDelta, 3);
                        sheet.range(merged).merge();
                    }, this);
                }
            }.bind(this), { recalc: true });
        },

        forEachCell: function(callback) {
            this._ref.forEach(function(ref) {
                this._sheet.forEach(ref.toRangeRef(), callback.bind(this));
            }.bind(this));
        },

        hasValue: function() {
            var result = false;

            this.forEachCell(function(row, col, cell) {
                if (Object.keys(cell).length !== 0) {
                    result = true;
                }
            });

            return result;
        },

        wrap: function(flag) {
            if (flag === undefined) {
                return !!this._property("wrap");
            }

            this.forEachRow(function(range) {
                var maxHeight = range.sheet().rowHeight(range.topLeft().row);

                range.forEachCell(function(row, col, cell) {
                    var width = this._sheet.columnWidth(col);
                    maxHeight = Math.max(maxHeight, kendo.spreadsheet.util.getTextHeight(cell.value, width, cell.fontSize, true));
                });

                range.sheet().rowHeight(range.topLeft().row, maxHeight);
            }.bind(this));

            this._property("wrap", flag);

            return this;
        },

        fontSize: function(size) {
            if (size === undefined) {
                return this._property("fontSize");
            }

            this.forEachRow(function(range) {
                var maxHeight = range.sheet().rowHeight(range.topLeft().row);

                range.forEachCell(function(row, col, cell) {
                    var width = this._sheet.columnWidth(col);
                    maxHeight = Math.max(maxHeight, kendo.spreadsheet.util.getTextHeight(cell.value, width, size, cell.wrap));
                });

                range.sheet().rowHeight(range.topLeft().row, maxHeight);
            }.bind(this));

            this._property("fontSize", size);

            return this;
        }
    });

    // use $.each instead of forEach to work in oldIE
    $.each(styles, function(i, property) {
        Range.prototype[property] = function(value) {
            return this._property(property, value);
        };
    });

    function toExcelFormat(format) {
        return format.replace(/M/g, "m").replace(/'/g, '"').replace(/tt/, "am/pm");
    }

    function looksLikeANumber(str) {
        // XXX: could do with just a regexp instead of calling parse.
        return !(/^=/.test(str)) && (/number|percent/).test(kendo.spreadsheet.calc.parse(null, 0, 0, str).type);
    }

    var measureBox = $('<div style="position: absolute !important; top: -4000px !important; height: auto !important;' +
                        'padding: 1px !important; margin: 0 !important; border: 1px solid black !important;' +
                        'line-height: normal !important; visibility: hidden !important;' +
                        'white-space: normal !important; word-break: break-all !important;" />'
                     )[0];

    function getTextHeight(text, width, fontSize, wrap) {
        var styles = {
            "baselineMarkerSize" : 0,
            "width" : width + "px",
            "font-size" : (fontSize || 12) + "px",
            "word-break" : (wrap === true) ? "break-all" : "normal",
            "white-space" : (wrap === true) ? "normal" : "nowrap"
        };

        return kendo.util.measureText(text, styles, measureBox).height;
    }

    kendo.spreadsheet.util = { getTextHeight: getTextHeight };
    kendo.spreadsheet.Range = Range;
})(window.kendo);

})();

(function(){
    "use strict";

    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    // WARNING: removing the following jshint declaration and turning
    // == into === to make JSHint happy will break functionality.
    /* jshint eqnull:true, newcap:false, laxbreak:true, shadow:true, validthis:true, -W054, loopfunc: true */
    /* global console */
    /* jshint latedef: nofunc */

    var calc = {};
    var spreadsheet = kendo.spreadsheet;
    spreadsheet.calc = calc;
    var exports = calc.runtime = {};
    var Class = kendo.Class;

    var Ref = spreadsheet.Ref;
    var CellRef = spreadsheet.CellRef;
    var RangeRef = spreadsheet.RangeRef;
    var UnionRef = spreadsheet.UnionRef;
    var NULL = spreadsheet.NULLREF;

    /* -----[ Errors ]----- */

    function CalcError(code) {
        if (code instanceof CalcError) {
            return code;
        }
        this.code = code;
    }

    CalcError.prototype.toString = function() {
        return "#" + this.code + (this.code == "NAME" ? "?" : "!");
    };

    /* -----[ Context ]----- */

    var Context = Class.extend({
        init: function Context(callback, formula, ss, parent) {
            this.callback = callback;
            this.formula = formula;
            this.ss = ss;
            this.parent = parent;
        },

        resolve: function(val) {
            var self = this;
            if (val instanceof Ref) {
                self.resolveCells([ val ], function(){
                    val = self.getRefData(val);
                    if (Array.isArray(val)) {
                        // got a Range, we should return a single value
                        val = val[0];
                    }
                    self._resolve(val);
                });
            } else {
                self._resolve(val);
            }
        },

        _resolve: function(val) {
            if (val === undefined) {
                val = null;
            }
            var f = this.formula;
            f.value = val;
            if (this.ss.onFormula(f) && this.callback) {
                this.callback.call(f, val);
            }
        },

        resolveCells: function(a, f) {
            var context = this, formulas = [];

            (function loop(a){
                for (var i = 0; i < a.length; ++i) {
                    var x = a[i];
                    if (x instanceof Ref) {
                        add(context.getRefCells(x));
                    }
                    if (Array.isArray(x)) {
                        // make sure we resolve cells in literal matrices
                        loop(x);
                    }
                }
            })(a);

            if (!formulas.length) {
                return f.call(context);
            }

            for (var pending = formulas.length, i = 0; i < formulas.length; ++i) {
                fetch(formulas[i]);
            }
            function fetch(cell) { // jshint ignore:line, because you are stupid.
                cell.formula.exec(context.ss, function(){

                    if (!--pending) {
                        f.call(context);
                    }
                }, context);
            }
            function add(a) {
                for (var i = 0; i < a.length; ++i) {
                    var cell = a[i];

                    if (cell.formula) {
                        formulas.push(cell);
                    }
                }
                return true;
            }
        },

        cellValues: function(a, f) {
            var ret = [];
            for (var i = 0; i < a.length; ++i) {
                var val = a[i];
                if (val instanceof Ref) {
                    val = this.getRefData(val);
                    ret = ret.concat(val);
                } else if (Array.isArray(val)) {
                    ret = ret.concat(this.cellValues(val));
                } else if (val instanceof Matrix) {
                    ret = ret.concat(this.cellValues(val.data));
                } else {
                    ret.push(val);
                }
            }
            if (f) {
                return f.apply(this, ret);
            }
            return ret;
        },

        force: function(val) {
            if (val instanceof Ref) {
                return this.getRefData(val);
            }
            return val;
        },

        func: function(fname, callback, args) {
            fname = fname.toLowerCase();
            var f = FUNCS[fname];
            if (f) {
                return f.call(this, callback, args);
            }
            callback(new CalcError("NAME"));
        },

        bool: function(val) {
            if (val instanceof Ref) {
                val = this.getRefData(val);
            }
            if (typeof val == "string") {
                return val.toLowerCase() == "true";
            }
            if (typeof val == "number") {
                return val !== 0;
            }
            if (typeof val == "boolean") {
                return val;
            }
            return val != null;
        },

        asMatrix: function(range) {
            if (range instanceof Matrix) {
                return range;
            }
            var self = this;
            if (range instanceof RangeRef) {
                var tl = range.topLeft;
                var top = tl.row, left = tl.col;
                var cells = self.getRefCells(range);
                var m = new Matrix(self);
                if (isFinite(range.width())) {
                    m.width = range.width();
                }
                if (isFinite(range.height())) {
                    m.height = range.height();
                }
                if (!isFinite(top)) {
                    top = 0;
                }
                if (!isFinite(left)) {
                    left = 0;
                }
                cells.forEach(function(cell){
                    m.set(cell.row - top,
                          cell.col - left,
                          cell.value);
                });
                return m;
            }
            if (Array.isArray(range) && range.length > 0) {
                var m = new Matrix(self), row = 0;
                range.forEach(function(line){
                    var col = 0;
                    var h = 1;
                    line.forEach(function(el){
                        var isRange = el instanceof RangeRef;
                        if (el instanceof Ref && !isRange) {
                            el = self.getRefData(el);
                        }
                        if (isRange || Array.isArray(el)) {
                            el = self.asMatrix(el);
                        }
                        if (el instanceof Matrix) {
                            el.each(function(el, r, c){
                                m.set(row + r, col + c, el);
                            });
                            h = Math.max(h, el.height);
                            col += el.width;
                        } else {
                            m.set(row, col++, el);
                        }
                    });
                    row += h;
                });
                return m;
            }
        },

        getRefCells: function(refs, hiddenInfo) {
            return this.ss.getRefCells(refs, hiddenInfo);
        },

        getRefData: function(ref) {
            return this.ss.getData(ref);
        },

        workbook: function() {
            return this.ss.workbook;
        }
    });

    var Matrix = Class.extend({
        init: function Matrix(context) {
            this.context = context;
            this.height = 0;
            this.width = 0;
            this.data = [];
        },
        clone: function() {
            var m = new Matrix(this.context);
            m.height = this.height;
            m.width = this.width;
            m.data = this.data.map(function(row){ return row.slice(); });
            return m;
        },
        get: function(row, col) {
            var line = this.data[row];
            var val = line ? line[col] : null;
            return val instanceof Ref ? this.context.getRefData(val) : val;
        },
        set: function(row, col, data) {
            var line = this.data[row];
            if (line == null) {
                line = this.data[row] = [];
            }
            line[col] = data;
            if (row >= this.height) {
                this.height = row + 1;
            }
            if (col >= this.width) {
                this.width = col + 1;
            }
        },
        each: function(f, includeEmpty) {
            for (var row = 0; row < this.height; ++row) {
                for (var col = 0; col < this.width; ++col) {
                    var val = this.get(row, col);
                    if (includeEmpty || val != null) {
                        val = f.call(this.context, val, row, col);
                        if (val !== undefined) {
                            return val;
                        }
                    }
                }
            }
        },
        map: function(f, includeEmpty) {
            var m = new Matrix(this.context);
            this.each(function(el, row, col){
                // here `this` is actually the context
                m.set(row, col, f.call(this, el, row, col));
            }, includeEmpty);
            return m;
        },
        eachRow: function(f) {
            for (var row = 0; row < this.height; ++row) {
                var val = f.call(this.context, row);
                if (val !== undefined) {
                    return val;
                }
            }
        },
        eachCol: function(f) {
            for (var col = 0; col < this.width; ++col) {
                var val = f.call(this.context, col);
                if (val !== undefined) {
                    return val;
                }
            }
        },
        mapRow: function(f) {
            var m = new Matrix(this.context);
            this.eachRow(function(row){
                m.set(row, 0, f.call(this.context, row));
            });
            return m;
        },
        mapCol: function(f) {
            var m = new Matrix(this.context);
            this.eachCol(function(col){
                m.set(0, col, f.call(this.context, col));
            });
            return m;
        },
        toString: function() {
            return JSON.stringify(this.data);
        },
        transpose: function() {
            var m = new Matrix(this.context);
            this.each(function(el, row, col){
                m.set(col, row, el);
            });
            return m;
        },
        unit: function(n) {
            this.width = this.height = n;
            var a = this.data = new Array(n);
            for (var i = n; --i >= 0;) {
                var row = a[i] = new Array(n);
                for (var j = n; --j >= 0;) {
                    row[j] = i == j ? 1 : 0;
                }
            }
            return this;
        },
        multiply: function(b) {
            var a = this, m = new Matrix(a.context);
            for (var row = 0; row < a.height; ++row) {
                for (var col = 0; col < b.width; ++col) {
                    var s = 0;
                    for (var i = 0; i < a.width; ++i) {
                        var va = a.get(row, i);
                        var vb = b.get(i, col);
                        if (typeof va != "number" || typeof vb != "number") {
                            throw new CalcError("VALUE");
                        }
                        s += va * vb;
                    }
                    m.set(row, col, s);
                }
            }
            return m;
        },
        adds: function(b, s) {
            var a = this, m = new Matrix(a.context);
            var sign = s ? -1 : 1;
            for (var row = 0; row < a.height; ++row) {
                for (var col = 0; col < a.width; ++col) {
                    var x = a.get(row, col),
                        y = b.get(row, col);
                    m.set(row, col, x + sign * y);
                }
            }
            return m;
        },
        determinant: function() {
            var a = this.clone().data;
            var n = a.length;
            var d = 1, C, L, i, k;
            for (C = 0; C < n; C++) {
                for (L = C; (L < n) && (!a[L][C]); L++) {}
                if (L == n) {
                    return 0;
                }
                if (L != C) {
                    d = -d;
                    for (k = C; k < n; k++) {
                        var t = a[C][k];
                        a[C][k] = a[L][k];
                        a[L][k] = t;
                    }
                }
                for (i = C+1; i < n; i++) {
                    for (k = C+1; k < n; k++) {
                        a[i][k] -= a[C][k] * a[i][C] / a[C][C];
                    }
                }
                d *= a[C][C];
            }
            return d;
        },
        inverse: function() {
            var n = this.width;
            var m = this.augment(new Matrix(this.context).unit(n));
            var a = m.data;
            var tmp;

            // Gaussian elimination
            // https://en.wikipedia.org/wiki/Gaussian_elimination#Finding_the_inverse_of_a_matrix

            // 1. Get zeros below main diagonal
            for (var k = 0; k < n; ++k) {
                var imax = argmax(k, n, function(i){ return a[i][k]; });
                if (!a[imax][k]) {
                    return null; // singular matrix
                }
                if (k != imax) {
                    tmp = a[k];
                    a[k] = a[imax];
                    a[imax] = tmp;
                }
                for (var i = k+1; i < n; ++i) {
                    for (var j = k+1; j < 2*n; ++j) {
                        a[i][j] -= a[k][j] * a[i][k] / a[k][k];
                    }
                    a[i][k] = 0;
                }
            }

            // 2. Get 1-s on main diagonal, dividing by pivot
            for (var i = 0; i < n; ++i) {
                for (var f = a[i][i], j = 0; j < 2*n; ++j) {
                    a[i][j] /= f;
                }
            }

            // 3. Get zeros above main diagonal.  Actually, we only care to compute the right side
            // here (that will be the inverse), so in the inner loop below we go while j >= n,
            // instead of j >= k.
            for (var k = n; --k >= 0;) {
                for (var i = k; --i >= 0;) {
                    if (a[i][k]) {
                        for (var j = 2*n; --j >= n;) {
                            a[i][j] -= a[k][j] * a[i][k];
                        }
                    }
                }
            }

            return m.slice(0, n, n, n);
        },
        augment: function(m) {
            var ret = this.clone(), n = ret.width;
            m.each(function(val, row, col){
                ret.set(row, col + n, val);
            });
            return ret;
        },
        slice: function(row, col, height, width) {
            var m = new Matrix(this.context);
            for (var i = 0; i < height; ++i) {
                for (var j = 0; j < width; ++j) {
                    m.set(i, j, this.get(row + i, col + j));
                }
            }
            return m;
        }

        // XXX: debug
        // dump: function() {
        //     this.data.forEach(function(row){
        //         console.log(row.map(function(val){
        //             var str = val.toFixed(3).replace(/\.?0*$/, function(s){
        //                 return [ "", " ", "  ", "   ", "    " ][s.length];
        //             });
        //             if (val >= 0) { str = " " + str; }
        //             return str;
        //         }).join("  "));
        //     });
        // }
    });

    function argmax(i, end, f) {
        var max = f(i), pos = i;
        while (++i < end) {
            var v = f(i);
            if (v > max) {
                max = v;
                pos = i;
            }
        }
        return pos;
    }

    /* -----[ Formula ]----- */

    var Formula = Class.extend({
        init: function Formula(refs, handler, printer, sheet, row, col){
            this.refs = refs;
            this.handler = handler;
            this.print = printer;
            this.absrefs = null;
            this.sheet = sheet;
            this.row = row;
            this.col = col;
            this.onReady = [];
            this.pending = false;
        },
        clone: function(sheet, row, col) {
            var lcsheet = sheet.toLowerCase();
            var refs = this.refs;
            if (lcsheet != this.sheet.toLowerCase()) {
                refs = refs.map(function(ref){
                    if (!ref.hasSheet() && ref.sheet.toLowerCase() != lcsheet) {
                        ref = ref.clone().setSheet(sheet);
                    }
                    return ref;
                });
            }
            return new Formula(refs, this.handler, this.print, sheet, row, col);
        },
        resolve: function(val) {
            this.pending = false;
            this.onReady.forEach(function(callback){
                callback(val);
            });
        },
        exec: function(ss, callback, parentContext) {
            if ("value" in this) {
                if (callback) {
                    callback(this.value);
                }
            } else {
                if (callback) {
                    this.onReady.push(callback);
                }

                var ctx = new Context(this.resolve, this, ss, parentContext);

                // if the call chain leads back to this same formula, we have a circular dependency.
                while (parentContext) {
                    if (parentContext.formula === this) {
                        this.pending = false;
                        ctx.resolve(new CalcError("CIRCULAR"));
                        return;
                    }
                    parentContext = parentContext.parent;
                }

                // pending is still useful for ASYNC formulas
                if (this.pending) {
                    return;
                }
                this.pending = true;

                // compute and cache the absolute references
                if (!this.absrefs) {
                    this.absrefs = this.refs.map(function(ref){
                        return ref.absolute(this.row, this.col);
                    }, this);
                }

                // finally invoke the handler given to us by the compiler in calc.js
                this.handler.call(ctx);
            }
        },
        reset: function() {
            this.onReady = [];
            this.pending = false;
            delete this.value;
        },
        renameSheet: function(oldSheetName, newSheetName) {
            oldSheetName = oldSheetName.toLowerCase();
            this.absrefs = null;
            if (this.sheet.toLowerCase() == oldSheetName) {
                this.sheet = newSheetName;
            }
            this.refs.forEach(function(ref){
                if (ref.sheet.toLowerCase() == oldSheetName) {
                    ref.sheet = newSheetName;
                }
            });
        },
        adjust: function(affectedSheet, operation, start, delta) {
            affectedSheet = affectedSheet.toLowerCase();
            var formulaRow = this.row;
            var formulaCol = this.col;
            var formulaSheet = this.sheet.toLowerCase();
            var formulaMoves = false;
            if (formulaSheet == affectedSheet) {
                // move formula if it's after the change point
                if (operation == "row" && formulaRow >= start) {
                    this.row += delta;
                    formulaMoves = true;
                }
                if (operation == "col" && formulaCol >= start) {
                    this.col += delta;
                    formulaMoves = true;
                }
            }
            var newFormulaRow = this.row;
            var newFormulaCol = this.col;
            this.absrefs = null;
            this.refs = this.refs.map(function(ref){
                if (ref === NULL) {
                    return ref;
                }
                if (ref.sheet.toLowerCase() != affectedSheet) {
                    if (formulaMoves) {
                        // a reference to another sheet should still point to the same location
                        // after adjustment; thus if row/col was removed before formula, relative
                        // references must be adjusted by delta.
                        if (operation == "row" && formulaRow >= start) {
                            ref = ref.relative(delta, 0);
                        }
                        if (operation == "col" && formulaCol >= start) {
                            ref = ref.relative(0, delta);
                        }
                    }
                    return ref;
                }
                return ref.adjust(
                    formulaRow, formulaCol,
                    newFormulaRow, newFormulaCol,
                    operation == "row",
                    start, delta
                );
            }, this);
        },
        toString: function() {
            return this.print(this.row, this.col);
        }
    });

    // spreadsheet functions --------
    var FUNCS = Object.create(null);

    FUNCS["if"] = function(callback, args) {
        var self = this;
        var co = args[0], th = args[1], el = args[2];
        // XXX: I don't like this resolveCells here.  We should try to declare IF with
        // defineFunction.
        this.resolveCells([ co ], function(){
            var comatrix = self.asMatrix(co);
            if (comatrix) {
                // XXX: calling both branches in this case, since we'll typically need values from
                // both.  We could optimize and call them only when first needed, but oh well.
                th(function(th){
                    el(function(el){
                        var thmatrix = self.asMatrix(th);
                        var elmatrix = self.asMatrix(el);
                        callback(comatrix.map(function(val, row, col){
                            if (self.bool(val)) {
                                return thmatrix ? thmatrix.get(row, col) : th;
                            } else {
                                return elmatrix ? elmatrix.get(row, col) : el;
                            }
                        }));
                    });
                });
            } else {
                if (self.bool(co)) {
                    th(callback);
                } else {
                    el(callback);
                }
            }
        });
    };

    FUNCS["φ"] = function(callback) {
        callback((1+Math.sqrt(5))/2);
    };

    // Lasciate ogni speranza, voi ch'entrate.
    //
    // XXX: document this function.
    function compileArgumentChecks(functionName, args) {
        var arrayArgs = "function arrayArgs(args) { var xargs = [], width = 0, height = 0, arrays = [], i = 0; ";
        var resolve = "function resolve(args, callback) { var toResolve = [], i = 0; ";
        var name, forced, main = "'use strict'; function check(args) { var stack = [], tmp, xargs = [], i = 0, m, err = 'VALUE'; ", haveForced = false;
        var canBeArrayArg = false, hasArrayArgs = false;
        main += args.map(comp).join("");
        main += "if (i < args.length) return new CalcError('N/A'); ";
        main += "return xargs; } ";
        arrayArgs += "return { args: xargs, width: width, height: height, arrays: arrays }; } ";

        var f;
        if (haveForced) {
            resolve += "this.resolveCells(toResolve, callback); } ";
            f = new Function("CalcError", main + resolve + arrayArgs + " return { resolve: resolve, check: check, arrayArgs: arrayArgs };");
        } else {
            f = new Function("CalcError", main + " return { check: check };");
        }
        f = f(CalcError);
        if (!hasArrayArgs) {
            delete f.arrayArgs;
        }
        return f;

        function comp(x) {
            name = x[0];
            var code = "{ ";
            if (Array.isArray(name)) {
                arrayArgs += "while (i < args.length) { ";
                resolve += "while (i < args.length) { ";
                code += "xargs.push(tmp = []); stack.push(xargs); xargs = tmp; ";
                code += "while (i < args.length) { ";
                code += x.map(comp).join("");
                code += "} ";
                code += "xargs = stack.pop(); ";
                resolve += "} ";
                arrayArgs += "} ";
            } else if (name == "+") {
                arrayArgs += "while (i < args.length) { ";
                resolve += "while (i < args.length) { ";
                code += "xargs.push(tmp = []); stack.push(xargs); xargs = tmp; ";
                code += "do { ";
                code += x.slice(1).map(comp).join("");
                code += "} while (i < args.length); ";
                code += "xargs = stack.pop(); ";
                resolve += "} ";
                arrayArgs += "} ";
            } else if (name == "?") {
                // standalone assertion without a new argument
                code += "if (!(" + cond(x[1]) + ")) return new CalcError(err); ";
            } else {
                var type = x[1];
                if (Array.isArray(type) && /^#?collect/.test(type[0])) {
                    var n = type[2];
                    force();
                    code += "try {"
                        + "var $" + name + " = this.cellValues(args.slice(i";
                    if (n) {
                        code += ", i + " + n;
                    }
                    code += ")).filter(function($"+name+"){ ";
                    if (type[0] == "collect") {
                        code += "if ($"+name+" instanceof CalcError) throw $"+name+"; ";
                    }
                    code += "return " + cond(type[1]) + "; }, this); ";
                    if (n) {
                        code += "i += " + n + "; ";
                    } else {
                        code += "i = args.length; ";
                    }
                    code += "xargs.push($"+name+")"
                        + "} catch(ex) { if (ex instanceof CalcError) return ex; throw ex; } ";
                    resolve += "toResolve.push(args.slice(i)); ";
                } else if (type == "rest") {
                    code += "xargs.push(args.slice(i)); i = args.length; ";
                } else {
                    if ((canBeArrayArg = /^\*/.test(name))) {
                        hasArrayArgs = true;
                        name = name.substr(1);
                    }
                    code += "var $" + name + " = args[i++]; ";
                    var allowError = false;
                    if (/!$/.test(type)) {
                        type = type.substr(0, type.length - 1);
                        allowError = true;
                    } else {
                        code += "if ($"+name+" instanceof CalcError) return $"+name+"; ";
                    }
                    code += typeCheck(type, allowError) + "xargs.push($"+name+"); ";
                }
            }
            code += "} ";
            return code;
        }

        function force() {
            if (forced) {
                return "$"+name+"";
            }
            haveForced = true;
            forced = true;
            resolve += "toResolve.push(args[i++]); ";
            return "($"+name+" = this.force($"+name+"))";
        }

        function typeCheck(type, allowError) {
            forced = false;
            var ret = "if (!(" + cond(type) + ")) { ";
            if (forced && !allowError) {
                ret += " if ($" + name + " instanceof CalcError) return $" + name + "; ";
            }
            ret += "return new CalcError(err); } ";
            if (!forced) {
                resolve += "i++; ";
            }
            if (canBeArrayArg) {
                arrayArgs += "var $" + name + " = this.asMatrix(args[i]); "
                    + "if ($" + name + ") { "
                    + "xargs.push($" + name + "); "
                    + "width = Math.max(width, $" + name + ".width); "
                    + "height = Math.max(height, $" + name + ".height); "
                    + "arrays.push(true) } else { "
                    + "xargs.push(args[i]); "
                    + "arrays.push(false); } i++; ";
            } else {
                arrayArgs += "xargs.push(args[i++]); arrays.push(false); ";
            }
            return ret;
        }

        function cond(type) {
            if (Array.isArray(type)) {
                if (type[0] == "or") {
                    return "(" + type.slice(1).map(cond).join(") || (") + ")";
                }
                if (type[0] == "and") {
                    return "(" + type.slice(1).map(cond).join(") && (") + ")";
                }
                if (type[0] == "values") {
                    return "(" + type.slice(1).map(function(val){
                        return force() + " === " + val;
                    }).join(") || (") + ")";
                }
                if (type[0] == "null") {
                    return "(" + cond("null") + " ? (($"+name+" = " + type[1] + "), true) : false)";
                }
                if (type[0] == "between" || type[0] == "[between]") {
                    return "(" + force() + " >= " + type[1] + " && " + "$"+name+" <= " + type[2] + " ? true : ((err = 'NUM'), false))";
                }
                if (type[0] == "(between)") {
                    return "(" + force() + " > " + type[1] + " && " + "$"+name+" < " + type[2] + " ? true : ((err = 'NUM'), false))";
                }
                if (type[0] == "(between]") {
                    return "(" + force() + " > " + type[1] + " && " + "$"+name+" <= " + type[2] + " ? true : ((err = 'NUM'), false))";
                }
                if (type[0] == "[between)") {
                    return "(" + force() + " >= " + type[1] + " && " + "$"+name+" < " + type[2] + " ? true : ((err = 'NUM'), false))";
                }
                if (type[0] == "assert") {
                    var err = type[2] || "N/A";
                    return "((" + type[1] + ") ? true : (err = " + JSON.stringify(err) + ", false))";
                }
                if (type[0] == "not") {
                    return "!(" + cond(type[1]) + ")";
                }
                throw new Error("Unknown array type condition: " + type[0]);
            }
            if (type == "number") {
                return "(typeof " + force() + " == 'number' || typeof $"+name+" == 'boolean')";
            }
            if (type == "integer") {
                return "((typeof " + force() + " == 'number' || typeof $"+name+" == 'boolean') ? ($"+name+" |= 0, true) : false)";
            }
            if (type == "date") {
                return "((typeof " + force() + " == 'number') ? ($"+name+" |= 0, true) : false)";
            }
            if (type == "datetime") {
                return "(typeof " + force() + " == 'number')";
            }
            if (type == "divisor") {
                return "((typeof " + force() + " == 'number' || typeof $"+name+" == 'boolean') && "
                    + "($"+name+" == 0 ? ((err = 'DIV/0'), false) : true))";
            }
            if (type == "number+") {
                return "((typeof " + force() + " == 'number' || typeof $"+name+" == 'boolean') && ($"+name+" >= 0 ? true : ((err = 'NUM'), false)))";
            }
            if (type == "integer+") {
                return "((typeof " + force() + " == 'number' || typeof $"+name+" == 'boolean') && (($"+name+" |= 0) >= 0 ? true : ((err = 'NUM'), false)))";
            }
            if (type == "number++") {
                return "((typeof " + force() + " == 'number' || typeof $"+name+" == 'boolean') && ($"+name+" > 0 ? true : ((err = 'NUM'), false)))";
            }
            if (type == "integer++") {
                return "((typeof " + force() + " == 'number' || typeof $"+name+" == 'boolean') && (($"+name+" |= 0 ) > 0) ? true : ((err = 'NUM'), false))";
            }
            if (type == "string") {
                return "(typeof " + force() + " == 'string')";
            }
            if (type == "boolean") {
                return "(typeof " + force() + " == 'boolean')";
            }
            if (type == "logical") {
                return "(typeof " + force() + " == 'boolean' || (typeof $"+name+" == 'number' ? ($"+name+" = !!$"+name+", true) : false))";
            }
            if (type == "matrix") {
                force();
                return "((m = this.asMatrix($"+name+")) ? ($"+name+" = m) : false)";
            }
            if (type == "#matrix") {
                return "((m = this.asMatrix($"+name+")) ? ($"+name+" = m) : false)";
            }
            if (type == "ref") {
                return "($"+name+" instanceof kendo.spreadsheet.Ref)";
            }
            if (type == "area") {
                return "($"+name+" instanceof kendo.spreadsheet.CellRef || $"+name+" instanceof kendo.spreadsheet.RangeRef)";
            }
            if (type == "cell") {
                return "($"+name+" instanceof kendo.spreadsheet.CellRef)";
            }
            if (type == "null") {
                return "(" + force() + " == null)";
            }
            if (type == "anyvalue") {
                return "(" + force() + " != null && i <= args.length)";
            }
            if (type == "forced") {
                return "(" + force() + ", i <= args.length)";
            }
            if (type == "anything") {
                return "(i <= args.length)";
            }
            if (type == "blank") {
                return "(" + force() + " == null || $"+name+" === '')";
            }
            throw new Error("Can't check for type: " + type);
        }
    }

    function withErrorHandling(obj, f, args) {
        if (args instanceof CalcError) {
            return args;
        }
        try {
            return f.apply(obj, args);
        } catch(ex) {
            if (ex instanceof CalcError) {
                return ex;
            } else {
                throw ex;
            }
        }
    }

    function makeSyncFunction(handler, resolve, check, arrayArgs) {
        return function(callback, args) {
            function doit() {
                if (arrayArgs) {
                    var x = arrayArgs.call(this, args);
                    args = x.args;
                    if (x.width > 0 && x.height > 0) {
                        var result = new Matrix(this);
                        for (var row = 0; row < x.height; ++row) {
                            for (var col = 0; col < x.width; ++col) {
                                var xargs = [];
                                for (var i = 0; i < args.length; ++i) {
                                    if (x.arrays[i]) {
                                        xargs[i] = args[i].get(row, col);
                                    } else {
                                        xargs[i] = args[i];
                                    }
                                }
                                xargs = check.call(this, xargs);
                                result.set(row, col, withErrorHandling(this, handler, xargs));
                            }
                        }
                        return callback(result);
                    }
                }
                var xargs = check.call(this, args);
                callback(withErrorHandling(this, handler, xargs));
            }
            if (resolve) {
                resolve.call(this, args, doit);
            } else {
                doit.call(this);
            }
        };
    }

    function makeAsyncFunction(handler, resolve, check, arrayArgs) {
        return function(callback, args) {
            function doit() {
                if (arrayArgs) {
                    var x = arrayArgs.call(this, args);
                    args = x.args;
                    if (x.width > 0 && x.height > 0) {
                        var result = new Matrix(this);
                        var count = x.width * x.height;
                        var makeCallback = function(row, col) {
                            return function(value) {
                                result.set(row, col, value);
                                --count;
                                if (count === 0) {
                                    return callback(result);
                                }
                            };
                        };
                        for (var row = 0; row < x.height && count > 0; ++row) {
                            for (var col = 0; col < x.width && count > 0; ++col) {
                                var xargs = [];
                                for (var i = 0; i < args.length; ++i) {
                                    if (x.arrays[i]) {
                                        xargs[i] = args[i].get(row, col);
                                    } else {
                                        xargs[i] = args[i];
                                    }
                                }
                                xargs = check.call(this, xargs);
                                if (xargs instanceof CalcError) {
                                    result.set(row, col, xargs);
                                    --count;
                                    if (count === 0) {
                                        return callback(result);
                                    }
                                } else {
                                    xargs.unshift(makeCallback(row, col));
                                    handler.apply(this, xargs);
                                }
                            }
                        }
                        return;
                    }
                }
                var x = check.call(this, args);
                if (x instanceof CalcError) {
                    callback(x);
                } else {
                    x.unshift(callback);
                    handler.apply(this, x);
                }
            }
            if (resolve) {
                resolve.call(this, args, doit);
            } else {
                doit.call(this);
            }
        };
    }

    function defineFunction(name, func) {
        name = name.toLowerCase();
        FUNCS[name] = func;
        return {
            args: function(args, log) {
                var code = compileArgumentChecks(name, args);
                // XXX: DEBUG
                if (log) {
                    if (code.arrayArgs) {console.log(code.arrayArgs.toString());}
                    if (code.resolve) {console.log(code.resolve.toString());}
                    if (code.check) {console.log(code.check.toString());}
                }
                var f = FUNCS[name] = makeSyncFunction(func, code.resolve, code.check, code.arrayArgs);
                f.kendoSpreadsheetArgs = args;
                return this;
            },
            argsAsync: function(args, log) {
                var code = compileArgumentChecks(name, args);
                // XXX: DEBUG
                if (log) {
                    if (code.arrayArgs) {console.log(code.arrayArgs.toString());}
                    if (code.resolve) {console.log(code.resolve.toString());}
                    if (code.check) {console.log(code.check.toString());}
                }
                var f = FUNCS[name] = makeAsyncFunction(func, code.resolve, code.check, code.arrayArgs);
                f.kendoSpreadsheetArgs = args;
                return this;
            }
        };
    }

    /* -----[ date calculations ]----- */

    // Julian days algorithms from http://www.hermetic.ch/cal_stud/jdn.htm#comp

    function dateToJulianDays(y, m, d) {
        m++;
        return ((1461 * (y + 4800 + ((m - 14) / 12 | 0))) / 4 | 0) +
            ((367 * (m - 2 - 12 * ((m - 14) / 12 | 0))) / 12 | 0) -
            ((3 * (((y + 4900 + ((m - 14) / 12 | 0)) / 100 | 0))) / 4 | 0) +
            d - 32075;
    }

    function julianDaysToDate(jd) {
        var l, n, j, i, m, d, y;
        l = jd + 68569;
        n = (4 * l) / 146097 | 0;
        l = l - ((146097 * n + 3) / 4 | 0);
        i = (4000 * (l + 1) / 1461001) | 0;
        l = l - ((1461 * i) / 4 | 0) + 31;
        j = (80 * l) / 2447 | 0;
        d = l - ((2447 * j) / 80 | 0);
        l = j / 11 | 0;
        m = j + 2 - (12 * l);
        y = 100 * (n - 49) + i + l;
        m--;
        return {
            year  : y,
            month : m,
            date  : d,
            day   : (jd + 1) % 7,
            ord   : ORDINAL_ADD_DAYS[isLeapYear(y)][m] + d
        };
    }

    // This uses the Google Spreadsheet approach: treat 1899-12-31 as day 1, allowing to avoid
    // implementing the "Leap Year Bug" yet still be Excel compatible for dates starting 1900-03-01.
    var BASE_DATE = dateToJulianDays(1900, 0, -1);

    var DAYS_IN_MONTH = [ 31, 28, 31,
                          30, 31, 30,
                          31, 31, 30,
                          31, 30, 31 ];

    var ORDINAL_ADD_DAYS = [
        [ 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 ], // non-leap year
        [ 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335 ]  // leap year
    ];

    function isLeapYear(yr) {
        if (yr % 4) {
            return 0;
        }
        if (yr % 100) {
            return 1;
        }
        if (yr % 400) {
            return 0;
        }
        return 1;
    }

    function daysInYear(yr) {
        return isLeapYear(yr) ? 366 : 365;
    }

    function daysInMonth(yr, mo) {
        return (isLeapYear(yr) && mo == 1) ? 29 : DAYS_IN_MONTH[mo];
    }

    function unpackDate(serial) {
        return julianDaysToDate((serial | 0) + BASE_DATE);
    }

    function packDate(year, month, date) {
        return dateToJulianDays(year, month, date) - BASE_DATE;
    }

    var MS_IN_MIN = 60 * 1000;
    var MS_IN_HOUR = 60 * MS_IN_MIN;
    var MS_IN_DAY = 24 * MS_IN_HOUR;

    function unpackTime(serial) {
        var frac = serial - (serial|0);
        if (frac < 0) {
            frac++;
        }
        var ms = Math.round(MS_IN_DAY * frac);
        var hours = Math.floor(ms / MS_IN_HOUR);
        ms -= hours * MS_IN_HOUR;
        var minutes = Math.floor(ms / MS_IN_MIN);
        ms -= minutes * MS_IN_MIN;
        var seconds = Math.floor(ms / 1000);
        ms -= seconds * 1000;
        return {
            hours: hours,
            minutes: minutes,
            seconds: seconds,
            milliseconds: ms
        };
    }

    function serialToDate(serial) {
        var d = unpackDate(serial), t = unpackTime(serial);
        return new Date(d.year, d.month, d.date,
                        t.hours, t.minutes, t.seconds, t.milliseconds);
    }

    function packTime(hours, minutes, seconds, ms) {
        return (hours + minutes/60 + seconds/3600 + ms/3600000) / 24;
    }

    function dateToSerial(date) {
        var time = packTime(date.getHours(),
                            date.getMinutes(),
                            date.getSeconds(),
                            date.getMilliseconds());
        date = packDate(date.getFullYear(),
                        date.getMonth(),
                        date.getDate());
        if (date < 0) {
            return date - 1 + time;
        } else {
            return date + time;
        }
    }

    function parseDate(str) {
        // XXX: this is biased towards US style (when numeric, month must come first).
        return kendo.parseDate(str, [
            "MM/dd/yyyy",
            "MM-dd-yyyy",
            "MM/dd/yy",
            "MM-dd-yy",
            "MMMM dd yyyy",
            "MMMM dd yy",
            "MMM dd yyyy",
            "MMM dd yy",
            "dd MMMM yyyy",
            "dd MMMM yy",
            "dd MMM yyyy",
            "dd MMM yy",
            "MMMM dd, yyyy",
            "MMMM dd, yy",
            "MMM dd, yyyy",
            "MMM dd, yy",
            "MMMM dd",
            "MMM dd",
            "MMMM yyyy",
            "MMM yyyy",
            "dd MMMM",
            "dd MMM",
            "MM-dd",
            "MM/dd"
        ]) || kendo.parseDate(str);
    }

    /* -----[ exports ]----- */

    exports.CalcError = CalcError;
    exports.Formula = Formula;
    exports.Matrix = Matrix;

    exports.packDate = packDate;
    exports.unpackDate = unpackDate;
    exports.packTime = packTime;
    exports.unpackTime = unpackTime;
    exports.serialToDate = serialToDate;
    exports.dateToSerial = dateToSerial;
    exports.daysInMonth = daysInMonth;
    exports.isLeapYear = isLeapYear;
    exports.daysInYear = daysInYear;
    exports.parseDate = parseDate;

    spreadsheet.dateToNumber = dateToSerial;
    spreadsheet.numberToDate = serialToDate;
    spreadsheet.defineFunction = defineFunction;
    spreadsheet.CalcError = CalcError;

    exports.defineFunction = defineFunction;
    exports.defineAlias = function(alias, name) {
        var orig = FUNCS[name];
        if (!orig) {
            throw new Error("Function " + name + " is not yet defined");
        }
        if (!orig.kendoSpreadsheetAliases) {
            orig.kendoSpreadsheetAliases = [ name ];
        }
        orig.kendoSpreadsheetAliases.push(alias);
        FUNCS[alias] = orig;
    };
    exports.FUNCS = FUNCS;

    /* -----[ Excel operators ]----- */

    var ARGS_NUMERIC = [
        [ "*a", "number" ],
        [ "*b", "number" ]
    ];

    var ARGS_ANYVALUE = [
        [ "*a", "anyvalue" ],
        [ "*b", "anyvalue" ]
    ];

    defineFunction("binary+", function(a, b){
        return a + b;
    }).args(ARGS_NUMERIC);

    defineFunction("binary-", function(a, b){
        return a - b;
    }).args(ARGS_NUMERIC);

    defineFunction("binary*", function(a, b){
        return a * b;
    }).args(ARGS_NUMERIC);

    defineFunction("binary/", function(a, b){
        return a / b;
    }).args([
        [ "*a", "number" ],
        [ "*b", "divisor" ]
    ]);

    defineFunction("binary^", function(a, b){
        return Math.pow(a, b);
    }).args(ARGS_NUMERIC);

    defineFunction("binary&", function(a, b){
        if (a == null) { a = ""; }
        if (b == null) { b = ""; }
        return "" + a + b;
    }).args([
        [ "*a", [ "or", "number", "string", "boolean", "null" ] ],
        [ "*b", [ "or", "number", "string", "boolean", "null" ] ]
    ]);

    defineFunction("binary=", function(a, b){
        return a === b;
    }).args(ARGS_ANYVALUE);

    defineFunction("binary<>", function(a, b){
        return a !== b;
    }).args(ARGS_ANYVALUE);

    defineFunction("binary<", binaryCompare(function(a, b){
        return a < b;
    })).args(ARGS_ANYVALUE);

    defineFunction("binary<=", binaryCompare(function(a, b){
        return a <= b;
    })).args(ARGS_ANYVALUE);

    defineFunction("binary>", binaryCompare(function(a, b){
        return a > b;
    })).args(ARGS_ANYVALUE);

    defineFunction("binary>=", binaryCompare(function(a, b){
        return a >= b;
    })).args(ARGS_ANYVALUE);

    defineFunction("unary+", function(a){
        return a;
    }).args([
        [ "*a", "number" ]
    ]);

    defineFunction("unary-", function(a){
        return -a;
    }).args([
        [ "*a", "number" ]
    ]);

    defineFunction("unary%", function(a){
        return a / 100;
    }).args([
        [ "*a", "number" ]
    ]);

    // range operator
    defineFunction("binary:", function(a, b){
        return new RangeRef(a, b)
            .setSheet(a.sheet || this.formula.sheet, a.hasSheet());
    }).args([
        [ "a", "cell" ],
        [ "b", "cell" ]
    ]);

    // union operator
    defineFunction("binary,", function(a, b){
        return new UnionRef([ a, b ]);
    }).args([
        [ "a", "ref" ],
        [ "b", "ref" ]
    ]);

    // intersection operator
    defineFunction("binary ", function(a, b){
        return a.intersect(b);
    }).args([
        [ "a", "ref" ],
        [ "b", "ref" ]
    ]);

    /* -----[ conditionals ]----- */

    defineFunction("not", function(a){
        return !this.bool(a);
    }).args([
        [ "*a", "anyvalue" ]
    ]);

    /* -----[ the IS* functions ]----- */

    defineFunction("isblank", function(val){
        if (val instanceof CellRef) {
            val = this.getRefData(val);
            return val == null;
        }
        return false;
    }).args([
        [ "*value", "anything!" ]
    ]);

    defineFunction("iserror", function(val){
        return val instanceof CalcError;
    }).args([
        [ "*value", "forced!" ]
    ]);

    defineFunction("iserr", function(val){
        return val instanceof CalcError && val.code != "N/A";
    }).args([
        [ "*value", "forced!" ]
    ]);

    defineFunction("isna", function(val){
        return val instanceof CalcError && val.code == "N/A";
    }).args([
        [ "*value", "forced!" ]
    ]);

    defineFunction("islogical", function(val){
        return typeof val == "boolean";
    }).args([
        [ "*value", "forced!" ]
    ]);

    defineFunction("isnontext", function(val){
        return typeof val != "string";
    }).args([
        [ "*value", "forced!" ]
    ]);

    defineFunction("istext", function(val){
        return typeof val == "string";
    }).args([
        [ "*value", "forced!" ]
    ]);

    defineFunction("isnumber", function(val){
        return typeof val == "number";
    }).args([
        [ "*value", "forced!" ]
    ]);

    defineFunction("isref", function(val){
        // apparently should return true only for cell and range
        return val instanceof CellRef || val instanceof RangeRef;
    }).args([
        [ "*value", "anything!" ]
    ]);

    /// utils

    function binaryCompare(func) {
        return function(left, right){
            if (typeof left == "string" && typeof right != "string") {
                right = right == null ? "" : right + "";
            }
            if (typeof left != "string" && typeof right == "string") {
                left = left == null ? "" : left + "";
            }
            if (typeof left == "number" && right == null) {
                right = 0;
            }
            if (typeof right == "number" && left == null) {
                left = 0;
            }
            if (typeof left == "string" && typeof right == "string") {
                // string comparison is case insensitive
                left = left.toLowerCase();
                right = right.toLowerCase();
            }
            if (typeof right == typeof left) {
                return func(left, right);
            } else {
                return new CalcError("VALUE");
            }
        };
    }

})();

(function(){
    "use strict";

    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var spreadsheet = kendo.spreadsheet;

    var exports = {};
    spreadsheet.validation = exports;
    var calc = spreadsheet.calc;
    var Class = kendo.Class;
    var TRANSPOSE_FORMAT = "_matrix({0})";

    calc.runtime.defineFunction("_matrix", function(m){
        return m;
    }).args([
        [ "m", "matrix" ]
    ]);

    function compileValidation(sheet, row, col, validation) {
        var validationHandler;
        var comparer;

        if (typeof validation === "string") {
            validation = JSON.parse(validation);
        }

        if (validation.from) {
            if (validation.dataType === "list") {
                validation.from = kendo.format(TRANSPOSE_FORMAT, validation.from);
            }

            validation.from = calc.compile(calc.parseFormula(sheet, row, col, validation.from));
        }

        if (validation.to) {
            validation.to = calc.compile(calc.parseFormula(sheet, row, col, validation.to));
        }

        if (validation.dataType == "custom") {
            comparer = exports.validationComparers.custom;
        } else if (validation.dataType == "list") {
            comparer = exports.validationComparers.list;
        } else {
            comparer = exports.validationComparers[validation.comparerType];
        }

        if (!comparer) {
            throw kendo.format("'{0}' comparer is not implemented.", validation.comparerType);
        }

        validationHandler = function (valueToCompare) { //add 'valueFormat' arg when add isDate comparer
            var toValue = this.to && this.to.value ? this.to.value : undefined;

            if (this.dataType == "custom") {
                this.value = comparer(valueToCompare, this.from.value,  toValue);
            } else if (this.dataType == "list") {
                var data = this._getListData();

                this.value = comparer(valueToCompare, data, toValue);
            } else if (valueToCompare === null) {
                if (this.allowNulls) {
                    this.value = true;
                } else {
                    this.value = false;
                }
            } else {
                //TODO: TYPE CHECK IS REQUIRED ONLY FOR DATE TYPE WHEN SPECIAL COMPARER (ISDATE) IS USED
                this.value = comparer(valueToCompare, this.from.value,  toValue);
            }

            return this.value;
        };

        return new kendo.spreadsheet.validation.Validation($.extend(validation, {
            handler: validationHandler,
            sheet: sheet,
            row: row,
            col: col
        }));
    }

    var Validation = Class.extend({
        init: function Validation(options){
            this.handler = options.handler;
            this.from = options.from;
            this.to = options.to;
            this.dataType = options.dataType; //date, time etc
            this.comparerType =  options.comparerType; //greaterThan, EqaulTo etc
            this.type = options.type ? options.type : "warning"; //info, warning, reject
            this.allowNulls = options.allowNulls ? true : false;

            //TODO: address to be range / cell ref, and adjust it based on it
            this.sheet = options.sheet;
            this.row = options.row;
            this.col = options.col;

            if (options.tooltipMessageTemplate) {
                this.tooltipMessageTemplate = options.tooltipMessageTemplate;
            }

            if (options.tooltipTitleTemplate) {
                this.tooltipTitleTemplate = options.tooltipTitleTemplate;
            }

            if (options.messageTemplate) {
                this.messageTemplate = options.messageTemplate;
            }

            if (options.titleTemplate) {
                this.titleTemplate = options.titleTemplate;
            }
        },

        _formatMessages: function(format) {
            var from = this.from ? this.from.value : "";
            var to = this.to ? this.to.value : "";

            var fromFormula = this.from ? this.from.toString() : "";
            var toFormula = this.to ? this.to.toString() : "";
            var dataType = this.dataType;
            var type = this.type;
            var comparerType = this.comparerType;

            return kendo.format(format, from, to, fromFormula, toFormula, dataType, type, comparerType);
        },

        _setMessages: function() {
            this.title = "";
            this.message = "";

            if (this.tooltipTitleTemplate) {
                this.tooltipTitle = this._formatMessages(this.tooltipTitleTemplate);
            }

            if (this.tooltipMessageTemplate) {
                this.tooltipMessage = this._formatMessages(this.tooltipMessageTemplate);
            }

            if (this.titleTemplate) {
                this.title = this._formatMessages(this.titleTemplate);
            }

            if (this.messageTemplate) {
                this.message = this._formatMessages(this.messageTemplate);
            }
        },

        _getListData: function() {
            if (!this.from.value || !this.from.value.data) {
                return [];
            }

            var cube = this.from.value.data;
            var i;
            var y;
            var data = [];

            for (i = 0; i < cube.length; i++ ) {
                var array = cube[i];

                if (array) {
                    for (y = 0; y < array.length; y++ ) {
                        data.push(array[y]);
                    }
                }
            }

            return data;
        },

        clone: function(sheet, row, col) {
            var options = this._getOptions();

            if (options.from) {
                options.from = options.from.clone(sheet, row, col);
            }

            if (options.to) {
                options.to = options.to.clone(sheet, row, col);
            }

            return new Validation($.extend(options,
                { handler: this.handler },
                { sheet: sheet, row: row, col: col }
            ));
        },

        exec: function(ss, compareValue, compareFormat, callback) {
            var self = this;

            var calculateFromCallBack = function() {

                self.value = self.handler.call(self, compareValue, compareFormat);
                self._setMessages();
                if (callback) {
                    callback(self.value);
                }
            };

            if (self.to) {
                self.to.exec(ss, function() {
                    self.from.exec(ss, calculateFromCallBack);
                });
            } else {
                self.from.exec(ss, calculateFromCallBack);
            }
        },

        reset: function() {
            if (this.from) {
                this.from.reset();
            }
            if (this.to) {
                this.to.reset();
            }
            delete this.value;
        },

        adjust: function(affectedSheet, operation, start, delta) {
            if (this.from) {
                this.from.adjust(affectedSheet, operation, start, delta);
            }
            if (this.to) {
                this.to.adjust(affectedSheet, operation, start, delta);
            }
            if (this.sheet.toLowerCase() == affectedSheet.toLowerCase()) {
                var formulaRow = this.row;
                var formulaCol = this.col;
                switch (operation) {
                  case "row":
                    if (formulaRow >= start) {
                        this.row += delta;
                    }
                    break;
                  case "col":
                    if (formulaCol >= start) {
                        this.col += delta;
                    }
                    break;
                }
            }
        },

        toJSON: function() {
            var options = this._getOptions();

            if (options.from) {
                options.from = options.from.toString();

                if (options.dataType === "list") {
                    options.from = options.from.replace(/^_matrix\((.*)\)$/i, "$1");
                }
            }

            if (options.to) {
                options.to = options.to.toString();
            }

            return options;
        },

        _getOptions: function () {
            return {
                from: this.from,
                to: this.to,
                dataType: this.dataType,
                type: this.type,
                comparerType: this.comparerType,
                row: this.row,
                col: this.col,
                sheet: this.sheet,
                allowNulls: this.allowNulls,
                tooltipMessageTemplate: this.tooltipMessageTemplate,
                tooltipTitleTemplate: this.tooltipTitleTemplate,
                //TODO: export generated messages instead?
                messageTemplate: this.messageTemplate,
                titleTemplate: this.titleTemplate
            };
        }
    });
    exports.compile = compileValidation;
    exports.validationComparers = {
        greaterThan: function (valueToCompare, from) {
            return valueToCompare > from;
        },

        lessThan: function (valueToCompare, from) {
            return valueToCompare < from;
        },

        between: function (valueToCompare, from, to) {
            return valueToCompare > from && valueToCompare < to;
        },

        equalTo: function (valueToCompare, from) {
            return valueToCompare == from;
        },

        notEqualTo: function (valueToCompare, from) {
            return valueToCompare != from;
        },

        greaterThanOrEqualTo: function (valueToCompare, from) {
            return valueToCompare >= from;
        },

        lessThanOrEqualTo: function (valueToCompare, from) {
            return valueToCompare <= from;
        },

        notBetween: function (valueToCompare, from, to) {
            return valueToCompare < from || valueToCompare > to;
        },

        custom: function (valueToCompare, from) {
            return from;
        },

        list: function (valueToCompare, data) {
            return data.indexOf(valueToCompare) > -1;
        }
    };

    exports.Validation = Validation;


})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var RangeRef = kendo.spreadsheet.RangeRef;
    var CellRef = kendo.spreadsheet.CellRef;
    var Range = kendo.spreadsheet.Range;
    var Color = kendo.Color;

    var Selection = kendo.Class.extend({
        init: function(sheet) {
            this._sheet = sheet;
            this.selection = kendo.spreadsheet.FIRSTREF.toRangeRef();
            this.originalSelection = kendo.spreadsheet.FIRSTREF.toRangeRef();
            this._activeCell = kendo.spreadsheet.FIRSTREF.toRangeRef();
            this.originalActiveCell = kendo.spreadsheet.FIRSTREF;
        },

        currentSelectionRange: function() {
            return this.selection.rangeAt(this.selectionRangeIndex).toRangeRef();
        },

        currentOriginalNavigationRange: function() {
            return this.originalSelection.rangeAt(this.selectionRangeIndex).toRangeRef();
        },

        currentNavigationRange: function() {
            if (this.singleCellSelection()) {
                return this._sheet._sheetRef;
            } else {
                return this.selection.rangeAt(this.selectionRangeIndex).toRangeRef();
            }
        },

        nextNavigationRange: function() {
            if (!this.singleCellSelection()) {
                this.selectionRangeIndex = this.selection.nextRangeIndex(this.selectionRangeIndex);
            }

            return this.currentNavigationRange();
        },

        previousNavigationRange: function() {
            if (!this.singleCellSelection()) {
                this.selectionRangeIndex = this.selection.previousRangeIndex(this.selectionRangeIndex);
            }

            return this.currentNavigationRange();
        },

        activeCell: function(ref) {
            if (ref) {
                this.originalActiveCell = ref;
                this._activeCell = this._sheet.unionWithMerged(ref.toRangeRef());
                this._sheet.focus(ref);
                this._sheet.triggerChange({ activeCell: true, selection: true });
            }

            return this._activeCell;
        },

        select: function(ref, expanded, changeActiveCell) {
            if (ref) {
                if (ref.eq(this.originalSelection)) {
                    return;
                }

                this.originalSelection = ref;

                this.selection = expanded;

                if (changeActiveCell !== false) {

                    if (ref.isCell()) {
                        this.activeCell(ref);
                    } else {
                        this.activeCell(this.selection.lastRange().first());
                    }

                    this.selectionRangeIndex = this.selection.size() - 1;
                } else {
                    this._sheet.triggerChange({ selection: true });
                }
            }

            return this.selection;
        },

        singleCellSelection: function() {
            return this._activeCell.eq(this.selection);
        }
    });

    var Sheet = kendo.Observable.extend({
        init: function(rowCount, columnCount, rowHeight, columnWidth, headerHeight, headerWidth) {
            kendo.Observable.prototype.init.call(this);

            var cellCount = rowCount * columnCount - 1;

            this._rows = new kendo.spreadsheet.Axis(rowCount, rowHeight);
            this._columns = new kendo.spreadsheet.Axis(columnCount, columnWidth);
            this._mergedCells = [];
            this._frozenRows = 0;
            this._frozenColumns = 0;
            this._suspendChanges = false;
            this._filter = null;
            this._grid = new kendo.spreadsheet.Grid(this._rows, this._columns, rowCount, columnCount, headerHeight, headerWidth);
            this._sheetRef = this._grid.normalize(kendo.spreadsheet.SHEETREF);
            this._properties = new kendo.spreadsheet.PropertyBag(cellCount);
            this._sorter = new kendo.spreadsheet.Sorter(this._grid, this._properties.sortable());

            this._viewSelection = new Selection(this);

            this._editSelection = new Selection(this);

            this._formulaSelections = [];
        },

        _selectionState: function() {
            return this._inEdit ? this._editSelection : this._viewSelection;
        },

        navigator: function() {
            if(!this._navigator) {
                 this._navigator = new kendo.spreadsheet.SheetNavigator(this);
            }
            return this._navigator;
        },

        axisManager: function() {
            if(!this._axisManager) {
                 this._axisManager = new kendo.spreadsheet.AxisManager(this);
            }

            return this._axisManager;
        },

        name: function(value) {
            if (!value) {
                return this._name;
            }

            this._name = value;

            return this;
        },

        _property: function(accessor, value, reason) {
            if (value === undefined) {
                return accessor();
            } else {
                accessor(value);

                return this.triggerChange(reason);
            }
        },

        _field: function(name, value, reason) {
            if (value === undefined) {
                return this[name];
            } else {
                this[name] = value;

                return this.triggerChange(reason);
            }
        },

        suspendChanges: function(value) {
            if (value === undefined) {
                return this._suspendChanges;
            }

            this._suspendChanges = value;

            return this;
        },

        triggerChange: function(reason) {
            if (!this._suspendChanges) {
                this.trigger("change", reason);
            }
            return this;
        },

        setDataSource: function(dataSource, columns) {
            if (this.dataSourceBinder) {
                this.dataSourceBinder.destroy();
            }

            this.dataSourceBinder = new kendo.spreadsheet.SheetDataSourceBinder({
                dataSource: dataSource,
                sheet: this,
                columns: columns
            });
        },

        hideColumn: function(columnIndex) {
            return this._property(this._columns.hide.bind(this._columns), columnIndex, { layout: true });
        },

        unhideColumn: function(columnIndex) {
            return this._property(this._columns.unhide.bind(this._columns), columnIndex, { layout: true });
        },

        isHiddenColumn: function(columnIndex) {
            return this._grid._columns.hidden(columnIndex);
        },

        _copyRange: function(sourceRangeRef, targetRef) {
            var grid = this._grid;
            var rowCount = grid.rowCount;

            var nextRefTopLeft = grid.normalize(sourceRangeRef.topLeft);
            var nextRefBottomRight = grid.normalize(sourceRangeRef.bottomRight);

            var nextIndex = nextRefTopLeft.col * rowCount + nextRefTopLeft.row;
            var nextBottomIndex = nextRefBottomRight.col * rowCount + nextRefBottomRight.row;

            var targetIndex = targetRef.col * rowCount + targetRef.row;

            this._properties.copy(nextIndex, nextBottomIndex, targetIndex);
        },

        _adjustReferences: function(operation, start, delta, mergedCells) {
            this._mergedCells = mergedCells.reduce(function(a, ref){
                ref = ref.adjust(null, null, null, null, operation == "row", start, delta);
                if (ref !== kendo.spreadsheet.NULLREF) {
                    a.push(ref);
                }
                return a;
            }, []);
            if (this._workbook) {
                var affectedSheet = this._name;
                this._workbook._sheets.forEach(function(sheet){
                    sheet._forFormulas(function(formula){
                        formula.adjust(affectedSheet, operation, start, delta);
                    });

                    sheet._forValidations(function(validation){
                        validation.adjust(affectedSheet, operation, start, delta);
                    });
                });
            }
            var selection = this.select();
            selection = selection.adjust(null, null, null, null, operation == "row", start, delta);
            if (selection !== kendo.spreadsheet.NULLREF) {
                this.select(selection);
            }
        },

        _forFormulas: function(callback) {
            var props = this._properties;
            props.get("formula").values().forEach(function(f){
                callback.call(this, f.value);
            }, this);
        },

        _forValidations: function(callback) {
            var props = this._properties;
            props.get("validation").values().forEach(function(v){
                callback.call(this, v.value);
            }, this);
        },

        canInsertRow: function(rowIndex, count) {
            count = count || 1;
            var grid = this._grid;
            var range = this.range(grid.rowCount - count, 0, count, grid.columnCount);
            return !range.hasValue();
        },

        insertRow: function(rowIndex) {
            if (!this.canInsertRow(rowIndex)) {
                throw new Error("Shifting nonblank cells off the worksheet is not supported!");
            }

            this.batch(function() {

                var grid = this._grid;
                var columnCount = grid.columnCount;
                var rowCount = grid.rowCount;

                var frozenRows = this.frozenRows();

                if (rowIndex < frozenRows) {
                    this.frozenRows(frozenRows + 1);
                }

                var mergedCells = this._mergedCells.slice();

                for (var ci = 0; ci < columnCount; ci++) {
                    var ref = new RangeRef(new CellRef(rowIndex, ci), new CellRef(rowIndex, ci));

                    var topLeft = grid.normalize(ref.topLeft);
                    var bottomRight = grid.normalize(ref.bottomRight);

                    var nextRef = new RangeRef(
                        new CellRef(topLeft.row, topLeft.col),
                        new CellRef(rowCount - 2, bottomRight.col)
                    );

                    this._copyRange(nextRef, new CellRef(topLeft.row + 1, topLeft.col));

                    new Range(ref, this).clear();
                }

                this._adjustReferences("row", rowIndex, 1, mergedCells);
            }, { recalc: true, layout: true });

            this.trigger("insertRow", { index: rowIndex });

            return this;
        },

        deleteRow: function(rowIndex) {
            this.batch(function() {
                var grid = this._grid;
                var columnCount = grid.columnCount;

                var frozenRows = this.frozenRows();
                if (rowIndex < frozenRows) {
                    this.frozenRows(frozenRows - 1);
                }

                var mergedCells = this._mergedCells.slice();

                for (var ci = 0; ci < columnCount; ci++) {
                    var ref = new RangeRef(new CellRef(rowIndex, ci), new CellRef(rowIndex, ci));

                    new Range(ref, this).clear();

                    var topLeft = grid.normalize(ref.topLeft);
                    var bottomRight = grid.normalize(ref.bottomRight);

                    var nextRef = new RangeRef(
                        new CellRef(topLeft.row + 1, topLeft.col),
                        new CellRef(Infinity, bottomRight.col)
                    );

                    this._copyRange(nextRef, topLeft);

                    var nextRefBottomRight = grid.normalize(nextRef.bottomRight);

                    new Range(new RangeRef(nextRefBottomRight, nextRefBottomRight), this).clear();
                }

                this._adjustReferences("row", rowIndex, -1, mergedCells);
            }, { recalc: true, layout: true });

            this.trigger("deleteRow", { index: rowIndex });

            return this;
        },

        insertColumn: function(columnIndex) {
            this.batch(function() {
                var grid = this._grid;
                var columnCount = grid.columnCount;

                var frozenColumns = this.frozenColumns();

                if (columnIndex < frozenColumns) {
                    this.frozenColumns(frozenColumns + 1);
                }

                var mergedCells = this._mergedCells.slice();

                for (var ci = columnCount; ci >= columnIndex; ci--) {
                    var ref = new RangeRef(new CellRef(0, ci), new CellRef(Infinity, ci));

                    new Range(ref, this).clear();

                    if (ci == columnIndex) {
                        break;
                    }

                    var topLeft = grid.normalize(ref.topLeft);
                    var bottomRight = grid.normalize(ref.bottomRight);

                    var nextRef = new RangeRef(
                        new CellRef(topLeft.row, topLeft.col - 1),
                        new CellRef(bottomRight.row, bottomRight.col - 1)
                    );

                    this._copyRange(nextRef, topLeft);
                }

                this._adjustReferences("col", columnIndex, 1, mergedCells);
            }, { recalc: true, layout: true });

            return this;
        },

        deleteColumn: function(columnIndex) {
            this.batch(function() {
                var grid = this._grid;
                var columnCount = grid.columnCount;

                var frozenColumns = this.frozenColumns();

                if (columnIndex < frozenColumns) {
                    this.frozenColumns(frozenColumns - 1);
                }

                var mergedCells = this._mergedCells.slice();

                for (var ci = columnIndex; ci < columnCount; ci++) {
                    var ref = new RangeRef(new CellRef(0, ci), new CellRef(Infinity, ci));

                    new Range(ref, this).clear();

                    if (ci == columnCount - 1) {
                        break;
                    }

                    var topLeft = grid.normalize(ref.topLeft);
                    var bottomRight = grid.normalize(ref.bottomRight);

                    var nextRef = new RangeRef(
                        new CellRef(topLeft.row, topLeft.col + 1),
                        new CellRef(bottomRight.row, bottomRight.col + 1)
                    );

                    this._copyRange(nextRef, topLeft);
                }

                this._adjustReferences("col", columnIndex, -1, mergedCells);
            }, { recalc: true, layout: true });

            return this;
        },

        hideRow: function(rowIndex) {
            return this._property(this._rows.hide.bind(this._rows), rowIndex, { layout: true });
        },

        unhideRow: function(rowIndex) {
            return this._property(this._rows.unhide.bind(this._rows), rowIndex, { layout: true });
        },

        isHiddenRow: function(rowIndex) {
            return this._grid._rows.hidden(rowIndex);
        },

        columnWidth: function(columnIndex, width) {
            return this._property(this._columns.value.bind(this._columns, columnIndex, columnIndex), width, { layout: true });
        },

        rowHeight: function(rowIndex, height) {
            return this._property(this._rows.value.bind(this._rows, rowIndex, rowIndex), height, { layout: true });
        },

        frozenRows: function(value) {
            return this._field("_frozenRows", value, { layout: true });
        },

        frozenColumns: function(value) {
            return this._field("_frozenColumns", value, { layout: true });
        },

        _ref: function(row, column, numRows, numColumns) {
            var ref = null;

            if (row instanceof kendo.spreadsheet.Ref) {
                return row;
            }

            if (typeof row === "string") {
                ref = kendo.spreadsheet.calc.parseReference(row);
            } else {
                if (!numRows) {
                    numRows = 1;
                }

                if (!numColumns) {
                    numColumns = 1;
                }
                ref = new RangeRef(new CellRef(row, column), new CellRef(row + numRows - 1, column + numColumns - 1));
            }

            return ref;
        },

        range: function(row, column, numRows, numColumns) {
            return new Range(this._ref(row, column, numRows, numColumns), this);
        },

        forEachMergedCell: function(ref, callback) {
            var selectAll = false;

            if (typeof callback === "undefined") {
                callback = ref;
                selectAll = true;
            }

            this._mergedCells.forEach(function(merged) {
                if (selectAll || merged.intersects(ref)) {
                    callback(merged);
                }
            });
        },

        forEachFilterHeader: function(ref, callback) {
            var selectAll = false;

            if (typeof callback === "undefined") {
                callback = ref;
                selectAll = true;
            }

            if (this._filter) {
                var refs = [];

                // get refs of all columns
                this._filter.ref.forEachColumn(function(columnRef) {
                    if (selectAll || columnRef.intersects(ref)) {
                        refs.push(columnRef.topLeft);
                    }
                });

                // filter out merged references
                this._mergedCells.forEach(function(merged) {
                    refs = refs.map(function(ref) {
                        if (merged.intersects(ref)) {
                            return merged;
                        }

                        return ref;
                    });
                });

                // use only unique refs
                refs.reduce(function unique(result, element) {
                    if (result.indexOf(element) < 0) {
                        result.push(element);
                    }

                    return result;
                }, []).forEach(callback);
            }
        },

        forEach: function(ref, callback) {
            var topLeft = this._grid.normalize(ref.topLeft);
            var bottomRight = this._grid.normalize(ref.bottomRight);

            for (var ci = topLeft.col; ci <= bottomRight.col; ci ++) {
                var ri = topLeft.row;

                var startCellIndex = this._grid.index(ri, ci);
                var endCellIndex = this._grid.index(bottomRight.row, ci);

                /* jshint loopfunc: true */
                this._properties.forEach(startCellIndex, endCellIndex, function(value) {
                    callback(ri++, ci, value);
                });
            }
        },

        startResizing: function(initialPosition) {
            this._initialPosition = initialPosition;
            this._resizeInProgress = true;
        },

        startAutoFill: function() {
            this._autoFillInProgress = true;
            var selection = this.select();
            this._autoFillOrigin = selection;
            this._autoFillDest = selection;
            this.triggerChange({ selection: true });
        },

        updateAutoFill: function(dest, punch, hint, direction) {
            this._autoFillDest = dest;
            this._autoFillPunch = punch;
            this._autoFillHint = hint;
            this._autoFillDirection = direction;
            this.triggerChange({ selection: true });
        },

        autoFillRef: function() {
            return this._autoFillDest;
        },

        autoFillPunch: function() {
            return this._autoFillPunch;
        },

        autoFillInProgress: function() {
            return this._autoFillInProgress;
        },

        resizingInProgress: function() {
            return this._resizeInProgress;
        },

        completeResizing: function() {
            if (this._resizeInProgress) {
                this._resizeInProgress = false;
                var hintPosition = this.resizeHintPosition();

                if (this._initialPosition && hintPosition) {
                    var handlePosition = this.resizeHandlePosition();
                    if (handlePosition.col !== -Infinity) {
                        this.columnWidth(handlePosition.col, this.columnWidth(handlePosition.col) - (this._initialPosition.x - hintPosition.x));
                    } else {
                        this.rowHeight(handlePosition.row, this.rowHeight(handlePosition.row) - (this._initialPosition.y - hintPosition.y));
                    }
                } else {
                    this.trigger("change", { resize: true });
                }
            }
        },

        resizeHandlePosition: function() {
            return this._resizeHandlePosition;
        },

        resizeHintPosition: function(location) {
            if (location !== undefined) {
                this._resizeHintPosition = location;
                this.trigger("change", { resize: true });
            }
            return this._resizeHintPosition;
        },

        removeResizeHandle: function() {
            if (this._resizeHandlePosition) {
                this._resizeHintPosition = undefined;
                this._resizeHandlePosition = undefined;
                this._initialPosition = undefined;
                this.trigger("change", { resize: true });
            }
        },

        positionResizeHandle: function(ref) {
            this._resizeHandlePosition = ref;
            this.trigger("change", { resize: true });
        },

        startSelection: function() {
            this._selectionInProgress = true;
        },

        completeSelection: function() {
            if (this._selectionInProgress) {
                this._selectionInProgress = false;
                this._resizeHintPosition = undefined;
                this.trigger("change", { selection: true });
            }
            if (this._autoFillInProgress) {
                this._autoFillInProgress = false;
                var dest = this._autoFillDest;
                var origin = this._autoFillOrigin;

                if (this._autoFillPunch) { // we just clear data here
                    this._workbook.execute({
                        command: "ClearContentCommand", options: { operatingRange: this.range(this._autoFillPunch) }
                    });
                } else {
                    if (!dest.eq(origin)) {
                        this._workbook.execute({
                            command: "AutoFillCommand", options: { operatingRange: this.range(dest), origin: this.range(origin) }
                        });
                    } else {
                        this.triggerChange({ selection: true });
                    }
                }

                this._autoFillDest = null;
                this._autoFillPunch = null;
                this._autoFillOrigin = null;

                this.select(dest);
            }
        },

        selectionInProgress: function() {
            return this._selectionInProgress;
        },

        select: function(ref, changeActiveCell) {
            var selectionState = this._selectionState();

            var expandedRef;

            if (ref) {
                ref = this._ref(ref);
                expandedRef = this._grid.isAxis(ref) ? ref : this.unionWithMerged(ref);
            }

            return selectionState.select(ref, expandedRef, changeActiveCell);
        },

        originalSelect: function() {
            return this._selectionState().originalSelection;
        },

        currentSelectionRange: function() {
            return this._selectionState().currentSelectionRange();
        },

        currentOriginalSelectionRange: function() {
            return this._selectionState().currentOriginalNavigationRange();
        },

        currentNavigationRange: function() {
            return this._selectionState().currentNavigationRange();
        },

        nextNavigationRange: function() {
            return this._selectionState().nextNavigationRange();
        },

        previousNavigationRange: function() {
            return this._selectionState().previousNavigationRange();
        },

        selectionRangeIndex: function() {
            return this._selectionState().selectionRangeIndex;
        },

        activeCell: function(ref) {
            return this._selectionState().activeCell(ref);
        },

        originalActiveCell: function() {
            return this._selectionState().originalActiveCell;
        },

        singleCellSelection: function() {
            return this._selectionState().singleCellSelection();
        },

        unionWithMerged: function(ref) {
            var mergedCells = this._mergedCells;

            return ref.map(function(ref) {
                return ref.toRangeRef().union(mergedCells);
            });
        },

        trim: function(ref) {
            var trims = [];
            var grid = this._grid;
            this._properties.forEachProperty(function(property) {
                trims.push(grid.trim(ref, property.list));
            });
            return this.unionWithMerged(ref.topLeft.toRangeRef().union(trims));
        },

        focus: function(ref) {
            if (ref) {
                this._focus = ref.toRangeRef();
            } else {
                var focus = this._focus;
                this._focus = null;
                return focus;
            }
        },

        activeCellSelection: function() {
            return new Range(this._grid.normalize(this.activeCell()), this);
        },

        selection: function() {
            return new Range(this._grid.normalize(this._selectionState().selection), this);
        },

        selectedHeaders: function() {
            var selection = this.select();

            var rows = {};
            var cols = {};
            var allCols = false;
            var allRows = false;

            selection.forEach(function(ref) {
                var i;
                var rowState = "partial";
                var colState = "partial";
                ref = ref.toRangeRef();

                var bottomRight = ref.bottomRight;

                var rowSelection = bottomRight.col === Infinity;
                var colSelection = bottomRight.row === Infinity;

                if (colSelection) { //column selection
                    allRows = true;
                    colState = "full";
                }

                if (rowSelection) { //row selection
                    allCols = true;
                    rowState = "full";
                }

                if (!colSelection) { //column selection
                    for (i = ref.topLeft.row; i <= bottomRight.row; i++) {
                        if (rows[i] !== "full") {
                            rows[i] = rowState;
                        }
                    }
                }

                if (!rowSelection) {
                    for (i = ref.topLeft.col; i <= bottomRight.col; i++) {
                        if (cols[i] !== "full") {
                            cols[i] = colState;
                        }
                    }
                }
            });

            return {
                rows: rows,
                cols: cols,
                allRows: allRows,
                allCols: allCols,
                all: allRows && allCols
            };
        },

        isInEditMode: function(isInEdit) {
            if (isInEdit === undefined) {
                return this._inEdit;
            }

            this._inEdit = isInEdit;

            if (isInEdit) {
                this._editSelection.selection = this._viewSelection.selection.clone();
                this._editSelection.originalSelection = this._viewSelection.originalSelection.clone();
                this._editSelection._activeCell = this._viewSelection._activeCell.clone();
                this._editSelection.originalActiveCell = this._viewSelection.originalActiveCell.clone();
            }
        },

        _setFormulaSelections: function(selection) {
            this._formulaSelections = (selection || []).slice();
            this.triggerChange({ selection: true });
        },

        _viewActiveCell: function() {
            return this._viewSelection._activeCell.toRangeRef();
        },

        toJSON: function() {
            var positions = {};

            var rows = this._rows.toJSON("height", positions);
            var columns = this._columns.toJSON("width", {});
            var viewSelection = this._viewSelection;

            this.forEach(kendo.spreadsheet.SHEETREF, function(row, col, cell) {
                if (Object.keys(cell).length === 0) {
                    return;
                }

                var position = positions[row];

                if (position === undefined) {
                    position = rows.length;

                    rows.push({ index: row });

                    positions[row] = position;
                }

                row = rows[position];

                cell.index = col;

                if (row.cells === undefined) {
                    row.cells = [];
                }

                if (cell.formula) {
                    // stringify Formula object.
                    cell.formula = cell.formula.toString();
                }

                if (cell.validation) {
                    cell.validation = cell.validation.toJSON();
                }

                if (cell.color) {
                    cell.color = new Color(cell.color).toHex();
                }

                if (cell.background) {
                    cell.background = new Color(cell.background).toHex();
                }

                if (cell.borderTop && cell.borderTop.color) {
                    cell.borderTop.color = new Color(cell.borderTop.color).toHex();
                }

                if (cell.borderBottom && cell.borderBottom.color) {
                    cell.borderBottom.color = new Color(cell.borderBottom.color).toHex();
                }

                if (cell.borderRight && cell.borderRight.color) {
                    cell.borderRight.color = new Color(cell.borderRight.color).toHex();
                }

                if (cell.borderLeft && cell.borderLeft.color) {
                    cell.borderLeft.color = new Color(cell.borderLeft.color).toHex();
                }

                row.cells.push(cell);
            });

            var json = {
                name: this._name,
                rows: rows,
                columns: columns,
                selection: viewSelection.selection.toString(),
                activeCell: viewSelection.activeCell().toString(),
                frozenRows: this.frozenRows(),
                frozenColumns: this.frozenColumns(),
                mergedCells: this._mergedCells.map(function(ref) {
                    return ref.toString();
                })
            };

            if (this._sort) {
               json.sort = {
                   ref: this._sort.ref.toString(),
                   columns: this._sort.columns.map(function(column) {
                       return {
                           index: column.index,
                           ascending: column.ascending
                       };
                   })
               };
            }

            if (this._filter) {
               json.filter = {
                   ref: this._filter.ref.toString(),
                   columns: this._filter.columns.map(function(column) {
                        var filter = column.filter.toJSON();
                        filter.index = column.index;
                        return filter;
                   })
               };
            }

            return json;
        },

        fromJSON: function(json) {
            this.batch(function() {
                if (json.name !== undefined) {
                    this._name = json.name;
                }

                if (json.frozenColumns !== undefined) {
                    this.frozenColumns(json.frozenColumns);
                }

                if (json.frozenRows !== undefined) {
                    this.frozenRows(json.frozenRows);
                }

                if (json.columns !== undefined) {
                    this._columns.fromJSON("width", json.columns);
                }

                if (json.rows !== undefined) {
                    this._rows.fromJSON("height", json.rows);

                    for (var ri = 0; ri < json.rows.length; ri++) {
                        var row = json.rows[ri];
                        var rowIndex = row.index;

                        if (rowIndex === undefined) {
                            rowIndex = ri;
                        }

                        if (row.cells) {
                            for (var ci = 0; ci < row.cells.length; ci++) {
                                var cell = row.cells[ci];
                                var columnIndex = cell.index;

                                if (columnIndex === undefined) {
                                    columnIndex = ci;
                                }

                                if (cell.formula) {
                                    cell.formula = this._compileFormula(rowIndex, columnIndex, cell.formula);
                                }

                                if (cell.validation) {
                                    cell.validation = this._compileValidation(rowIndex, columnIndex, cell.validation);
                                }

                                this._properties.fromJSON(this._grid.index(rowIndex, columnIndex), cell);
                            }
                        }
                    }
                }


                if (json.selection) {
                    this._viewSelection.selection =
                        this._viewSelection.originalSelection = this._ref(json.selection);
                }

                if (json.activeCell) {
                    var activeCellRef = this._ref(json.activeCell);

                    this._viewSelection._activeCell = activeCellRef.toRangeRef();
                    this._viewSelection.originalActiveCell = activeCellRef;
                }


                if (json.mergedCells) {
                    json.mergedCells.forEach(function(ref) {
                       this.range(ref).merge();
                    }, this);
                }

                if (json.sort) {
                    this._sort = {
                        ref: this._ref(json.sort.ref),
                        columns: json.sort.columns.slice(0)
                    };
                }

                if (json.filter) {
                    var ref = json.filter.ref;
                    if (!ref) {
                        kendo.logToConsole("Dropping filter for sheet '" + json.name + "' due to missing ref");
                    } else {
                        this._filter = {
                            ref: this._ref(ref),
                            columns: json.filter.columns.map(function(column) {
                                return {
                                    index: column.index,
                                    filter: kendo.spreadsheet.Filter.create(column)
                                };
                            })
                        };

                        this._refreshFilter();
                    }
                }
            });
        },

        formula: function(ref) {
            return this._properties.get("formula", this._grid.cellRefIndex(ref));
        },

        validation: function(ref) {
            return this._properties.get("validation", this._grid.cellRefIndex(ref));
        },

        // NOTE: resetFormulas should be called first.  We don't do it in this
        // function because it should be done from the Workbook object for all
        // sheets.
        resetFormulas: function() {
            this._forFormulas(function(formula){
                formula.reset();
            });
        },

        resetValidations: function() {
            this._forValidations(function(validation){
                validation.reset();
            });
        },

        recalc: function(context) {
            this._forFormulas(function(formula){
                formula.exec(context);
            });
        },

        revalidate: function(context) {
            var self = this;
            this._forValidations(function(validation){
                var cellRef = new CellRef(validation.row, validation.col);
                var ref =  new RangeRef(cellRef, cellRef);
                validation.exec(context, self._get(ref, "value"), self._get(ref, "format"));
            });
        },

        _value: function(row, col, value) {
            var index = this._grid.index(row, col);

            if (value !== undefined) {
                this._properties.set("value", index, index, value);
            } else {
                return this._properties.get("value", index);
            }
        },

        _validation: function(row, col) {
            var index = this._grid.index(row, col);

            return this._properties.get("validation", index);
        },

        _compileValidation: function(row, col, validation) {
            if (validation.from) {
                validation.from = (validation.from + "").replace(/^=/, "");
            }

            if (validation.to) {
                validation.to = (validation.to + "").replace(/^=/, "");
            }

            return kendo.spreadsheet.validation.compile(this._name, row, col, validation);
        },

        _compileFormula: function(row, col, f) {
            f = f.replace(/^=/, "");

            f = kendo.spreadsheet.calc.parseFormula(this._name, row, col, f);
            return kendo.spreadsheet.calc.compile(f);
        },

        _copyValuesInRange: function (topLeft, bottomRight, value, property) {
            var ci, start, end;

            for (ci = topLeft.col; ci <= bottomRight.col; ci++) {
                start = this._grid.index(topLeft.row, ci);
                end = this._grid.index(bottomRight.row, ci);
                for (var index = start, row = topLeft.row; index <= end; ++index, ++row) {
                    // Even if it's the same formula in multiple cells, we
                    // need to have different Formula objects, hence cloning
                    // it.  Don't worry, clone() is fast.
                    value = value.clone(this._name, row, ci);
                    this._properties.set(property, index, index, value);
                }
            }
            return value;
        },

        _set: function(ref, name, value) {
            var topLeft = this._grid.normalize(ref.topLeft);
            var bottomRight = this._grid.normalize(ref.bottomRight);
            var ci, start, end;

            if (value && name == "formula") {
                if (typeof value == "string") {
                    // get Formula object.  we don't care about handling errors
                    // here since it won't be called interactively.
                    value = this._compileFormula(topLeft.row, topLeft.col, value);
                }

                value = this._copyValuesInRange(topLeft, bottomRight, value, "formula");

            } else if (value && name == "validation") {
                value = this._compileValidation(topLeft.row, topLeft.col, value);
                value = this._copyValuesInRange(topLeft, bottomRight, value, "validation");

            } else {
                for (ci = topLeft.col; ci <= bottomRight.col; ci++) {
                    start = this._grid.index(topLeft.row, ci);
                    end = this._grid.index(bottomRight.row, ci);
                    this._properties.set(name, start, end, value);
                    if (name == "formula") {
                        // removing a formula, must clear value.
                        this._properties.set("value", start, end, null);
                    }
                }
            }
        },

        _get: function(ref, name) {
            var topLeft = this._grid.normalize(ref.topLeft);

            var index = this._grid.index(topLeft.row, topLeft.col);

            return this._properties.get(name, index);
        },

        batch: function(callback, reason) {
            var suspended = this.suspendChanges();

            this.suspendChanges(true);

            callback.call(this);

            return this.suspendChanges(suspended).triggerChange(reason);
        },

        _sortBy: function(ref, columns) {
            var indices = null;

            columns.forEach(function(column) {
                indices = this._sorter.sortBy(ref, column.index, this._properties.get("value"), column.ascending, indices);
            }, this);

            this._sort = {
                ref: ref,
                columns: columns
            };

            this._refreshFilter();

            this.triggerChange({ recalc: true });
        },

        _refreshFilter: function() {
            if (this._filter) {
                this._filterBy(this._filter.ref, this._filter.columns);
            }
        },

        _filterBy: function(ref, columns) {
            this.batch(function() {
                for (var ri = ref.topLeft.row; ri <= ref.bottomRight.row; ri++) {
                    if (this._rows.hidden(ri)) {
                        this._rows.unhide(ri);
                    }
                }

                columns.forEach(function(column) {
                    // do not filter header row
                    var columnRef = ref.resize({ top: 1 }).toColumn(column.index);

                    var cells = [];

                    if (columnRef === kendo.spreadsheet.NULLREF) {
                        return;
                    }

                    this.forEach(columnRef, function(row, col, cell) {
                        cell.row = row;
                        cells.push(cell);
                    });

                    column.filter.prepare(cells);

                    for (var ci = 0; ci < cells.length; ci++) {
                        var cell = cells[ci];
                        var value = column.filter.value(cell);

                        if (column.filter.matches(value) === false) {
                            this.hideRow(cell.row);
                        }
                    }
                }, this);

                this._filter = {
                    ref: ref,
                    columns: columns
                };
            }, { layout: true, filter: true });
        },

        filter: function() {
            return this._filter;
        },

        clearFilter: function(spec) {
            this._clearFilter(spec instanceof Array ? spec : [spec]);
        },

        _clearFilter: function(indices) {
            if (this._filter) {
                this.batch(function() {
                    this._filter.columns = this._filter.columns.filter(function(column) {
                        return indices.indexOf(column.index) < 0;
                    });

                    this._refreshFilter();
                }, { layout: true, filter: true });
            }
        },

        getAxisState: function() {
            return {
                rows: this._rows.getState(),
                columns: this._columns.getState()
            };
        },

        setAxisState: function(state) {
            this._rows.setState(state.rows);
            this._columns.setState(state.columns);
            this.triggerChange({ layout: true });
        },

        getState: function() {
            return {
                rows: this._rows.getState(),
                columns: this._columns.getState(),
                mergedCells: this._mergedCells.map(function(cell) { return cell.clone(); }),
                properties: this._properties.getState()
            };
        },

        setState: function(state) {
            this._rows.setState(state.rows);
            this._columns.setState(state.columns);
            this._mergedCells = state.mergedCells;
            this._properties.setState(state.properties);
            this.triggerChange(kendo.spreadsheet.ALL_REASONS);
        },

        _merge: function(ref) {
            var mergedCells = this._mergedCells;

            var sheet = this;
            var mergedRef;
            this.batch(function() {
                mergedRef = ref.map(function(ref) {
                    if (ref instanceof kendo.spreadsheet.CellRef) {
                        return ref;
                    }

                    var currentRef = ref.toRangeRef().union(mergedCells, function(ref) {
                        mergedCells.splice(mergedCells.indexOf(ref), 1);
                    });

                    var range = new Range(currentRef, sheet);
                    var value = range.value();
                    var format = range.format();
                    var background = range.background();

                    range.value(null);
                    range.format(null);
                    range.background(null);

                    var topLeft = new Range(currentRef.collapse(), sheet);

                    topLeft.value(value);
                    topLeft.format(format);
                    topLeft.background(background);

                    mergedCells.push(currentRef);
                    return currentRef;
                });

                var viewSelection = sheet._viewSelection;

                viewSelection.selection = sheet.unionWithMerged(viewSelection.originalSelection);
                viewSelection._activeCell = sheet.unionWithMerged(viewSelection.originalActiveCell);
            }, { activeCell: true, selection: true });

            return mergedRef;
        }
    });

    kendo.spreadsheet.Sheet = Sheet;
})(kendo);

})();

(function(){

    (function(kendo) {
        if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
            return;
        }

        var $ = kendo.jQuery;
        var DOT = ".";
        var EMPTYCHAR = " ";
        var sheetsBarClassNames = {
            sheetsBarWrapper: "k-widget k-header",
            sheetsBarSheetsWrapper: "k-tabstrip k-floatwrap k-tabstrip-bottom",
            sheetsBarActive: "k-spreadsheet-sheets-bar-active",
            sheetsBarInactive: "k-spreadsheet-sheets-bar-inactive",
            sheetsBarAdd: "k-spreadsheet-sheets-bar-add",
            sheetsBarRemove: "k-spreadsheet-sheets-remove",
            sheetsBarItems: "k-spreadsheet-sheets-items",
            sheetsBarEditor: "k-spreadsheet-sheets-editor",
            sheetsBarScrollable: "k-spreadsheet-sheets-scrollable",
            sheetsBarNext: "k-spreadsheet-sheets-next",
            sheetsBarPrev: "k-spreadsheet-sheets-prev",
            sheetsBarKItem: "k-item k-state-default",
            sheetsBarKActive: "k-state-active k-state-tab-on-top",
            sheetsBarKTextbox: "k-textbox",
            sheetsBarKLink: "k-link",
            sheetsBarKIcon: "k-icon",
            sheetsBarKFontIcon: "k-font-icon",
            sheetsBarKButton: "k-button k-button-icon",
            sheetsBarKButtonBare: "k-button-bare",
            sheetsBarKArrowW: "k-i-arrow-w",
            sheetsBarKArrowE: "k-i-arrow-e",
            sheetsBarKReset: "k-reset k-tabstrip-items",
            sheetsBarKIconX: "k-i-x",
            sheetsBarKSprite: "k-sprite",
            sheetsBarKIconPlus: "k-i-plus",
            sheetsBarHintWrapper: "k-widget k-tabstrip k-tabstrip-bottom k-spreadsheet-sheets-items-hint",
            sheetsBarKResetItems: "k-reset k-tabstrip-items"
        };

        var SheetsBar = kendo.ui.Widget.extend({
            init: function(element, options) {
                var classNames = SheetsBar.classNames;

                kendo.ui.Widget.call(this, element, options);

                element = this.element;

                element.addClass(classNames.sheetsBarWrapper);

                this._tree = new kendo.dom.Tree(element[0]);

                this._tree.render([this._addButton(), this._createSheetsWrapper([])]);

                this._createSortable();

                this._sortable.bind("start", this._onSheetReorderStart.bind(this));

                this._sortable.bind("end", this._onSheetReorderEnd.bind(this));

                element.on("click", DOT + classNames.sheetsBarRemove, this._onSheetRemove.bind(this));

                element.on("click", "li", this._onSheetSelect.bind(this));

                element.on("dblclick", "li" + DOT + classNames.sheetsBarActive, this._createEditor.bind(this));

                element.on("click", DOT + classNames.sheetsBarAdd, this._onAddSelect.bind(this));
            },

            options: {
                name: "SheetsBar",
                scrollable: {
                    distance: 200
                }
            },

            events: [
                "select",
                "reorder",
                "rename"
            ],

            _createEditor: function () {
                if (this._editor) {
                    return;
                }

                this._renderSheets(this._sheets, this._selectedIndex, true);
                this._editor = this.element
                                   .find(kendo.format("input{0}{1}",DOT,SheetsBar.classNames.sheetsBarEditor))
                                   .focus()
                                   .on("keydown", this._onEditorKeydown.bind(this))
                                   .on("blur", this._onEditorBlur.bind(this));
            },

            _destroyEditor: function() {
                this._editor.off();
                this._editor = null;
                this._renderSheets(this._sheets, this._selectedIndex, false);
            },

            renderSheets: function(sheets, selectedIndex) {
                if (!sheets || selectedIndex < 0) {
                    return;
                }

                this._renderSheets(sheets, selectedIndex, false);
            },

            _renderSheets: function(sheets, selectedIndex, isInEditMode) {
                var that = this;
                var wrapperOffsetWidth;
                var sheetsGroupScrollWidth;
                var scrollPrevButton;
                var scrollNextButton;
                var sheetsWrapper = that._sheetsWrapper();
                var sheetsGroup = that._sheetsGroup();
                var options = that.options;
                var classNames = SheetsBar.classNames;

                that._sheets = sheets;
                that._selectedIndex = selectedIndex;

                if (!that._scrollableAllowed()) {
                    that._renderHtml(isInEditMode, false);
                    return;
                }

                sheetsWrapper.addClass(classNames.sheetsBarScrollable + EMPTYCHAR + classNames.sheetsBarSheetsWrapper);

                wrapperOffsetWidth = sheetsWrapper[0].offsetWidth;
                sheetsGroupScrollWidth = sheetsGroup[0].scrollWidth;

                if (sheetsGroupScrollWidth > wrapperOffsetWidth && !that._scrollableModeActive) {

                    that._nowScrollingSheets = false;
                    that._isRtl = kendo.support.isRtl(that.element);

                    that._renderHtml(isInEditMode, true);

                    scrollPrevButton = sheetsWrapper.children(DOT + classNames.sheetsBarPrev);
                    scrollNextButton = sheetsWrapper.children(DOT + classNames.sheetsBarNext);

                    sheetsGroup.css({ marginLeft: scrollPrevButton.outerWidth() + 9, marginRight: scrollNextButton.outerWidth() + 12 });

                    scrollPrevButton.on("mousedown", function () {
                        that._nowScrollingSheets = true;
                        that._scrollSheetsByDelta(options.scrollable.distance * (that._isRtl ? 1 : -1));
                    });

                    scrollNextButton.on("mousedown", function () {
                        that._nowScrollingSheets = true;
                        that._scrollSheetsByDelta(options.scrollable.distance * (that._isRtl ? -1 : 1));
                    });

                    scrollPrevButton.add(scrollNextButton).on("mouseup", function () {
                        that._nowScrollingSheets = false;
                    });

                    that._scrollableModeActive = true;

                    that._toggleScrollButtons();
                } else if (that._scrollableModeActive && sheetsGroupScrollWidth <= wrapperOffsetWidth) {
                    that._scrollableModeActive = false;

                    sheetsWrapper.children(DOT + classNames.sheetsBarPrev).off();
                    sheetsWrapper.children(DOT + classNames.sheetsBarNext).off();

                    that._renderHtml(isInEditMode, false);
                    that._sheetsGroup().css({ marginLeft: "", marginRight: "" });
                } else {
                    that._renderHtml(isInEditMode, false);
                }
            },

            _renderHtml: function(isInEditMode, renderScrollButtons) {
                var idx;
                var sheetElements = [];
                var dom = kendo.dom;
                var element = dom.element;
                var sheets = this._sheets;
                var selectedIndex = this._selectedIndex;
                var classNames = SheetsBar.classNames;

                for (idx = 0; idx < sheets.length; idx++) {
                    var sheet = sheets[idx];
                    var isSelectedSheet = (idx === selectedIndex);
                    var attr = { className: classNames.sheetsBarKItem + EMPTYCHAR };
                    var elementContent = [];

                    if (isSelectedSheet) {
                        attr.className += classNames.sheetsBarKActive + EMPTYCHAR + classNames.sheetsBarActive;
                    } else {
                        attr.className += classNames.sheetsBarInactive;
                    }

                    if (isSelectedSheet && isInEditMode) {
                        elementContent.push(element("input", {
                            type: "text",
                            value: sheet.name(),
                            className: classNames.sheetsBarKTextbox + EMPTYCHAR + classNames.sheetsBarEditor,
                            maxlength: 50
                        }, []));
                    } else {
                        elementContent.push(element("span", {
                            className: classNames.sheetsBarKLink,
                            title: sheet.name()
                        }, [dom.text(sheet.name())]));

                        var deleteIcon = element("span", {
                            className: classNames.sheetsBarKIcon + EMPTYCHAR + classNames.sheetsBarKFontIcon + EMPTYCHAR + classNames.sheetsBarKIconX
                        }, []);

                        elementContent.push(element("span", {
                            className: classNames.sheetsBarKLink + EMPTYCHAR + classNames.sheetsBarRemove
                        }, [deleteIcon]));
                    }

                    sheetElements.push(element("li", attr, elementContent));
                }

                this._tree.render([this._addButton(),  this._createSheetsWrapper(sheetElements, renderScrollButtons)]);
            },

            _createSheetsWrapper: function(sheetElements, renderScrollButtons) {
                var element = kendo.dom.element;
                var classNames = SheetsBar.classNames;
                var childrenElements = [element("ul", {
                    style: { paddingLeft: "50px" },
                    className: classNames.sheetsBarKReset
                }, sheetElements)];

                if (renderScrollButtons) {
                    var baseButtonClass = classNames.sheetsBarKButton + EMPTYCHAR + classNames.sheetsBarKButtonBare + EMPTYCHAR;

                    childrenElements.push(element("span", {className: baseButtonClass + classNames.sheetsBarPrev }, [
                        element("span", {className: classNames.sheetsBarKIcon + EMPTYCHAR + classNames.sheetsBarKArrowW}, [])
                    ]));

                    childrenElements.push(element("span", {className: baseButtonClass + classNames.sheetsBarNext }, [
                        element("span", {className: classNames.sheetsBarKIcon + EMPTYCHAR + classNames.sheetsBarKArrowE}, [])
                    ]));
                }

                return element("div", { className: classNames.sheetsBarItems }, childrenElements);
            },

            _createSortable: function() {
                var classNames = SheetsBar.classNames;
                this._sortable = new kendo.ui.Sortable(this.element, {
                    filter: kendo.format("ul li.{0},ul li.{1}", classNames.sheetsBarActive, classNames.sheetsBarInactive),
                    container: DOT + classNames.sheetsBarItems,
                    axis: "x",
                    animation: false,
                    ignore: "input",
                    hint: function (element) {
                        var hint = $(element).clone();
                        return hint.wrap("<div class='" + classNames.sheetsBarHintWrapper + "'><ul class='" + classNames.sheetsBarKResetItems + "'></ul></div>").closest("div");
                    }
                });
            },

            _onEditorKeydown: function(e) {
                if (this._editor) {
                    if (e.which === 13) {
                        this._destroyEditor();
                        this._onSheetRename($(e.target).val());
                    }

                    if (e.which === 27) {
                        this._destroyEditor();
                        this._onSheetRename();
                    }
                }
            },

            _onEditorBlur: function(e) {
                if (this._editor) {
                    this._destroyEditor();
                    this._onSheetRename($(e.target).val());
                }
            },

            _onSheetReorderEnd: function(e) {
                e.preventDefault();
                this.trigger("reorder", {oldIndex: e.oldIndex, newIndex: e.newIndex});
            },

            _onSheetReorderStart: function(e) {
                if (this._editor) {
                    e.preventDefault();
                }
            },

            _onSheetRemove: function(e) {
                var removedSheetName = $(e.target).closest("li").text();

                if (this._editor) {
                    this._destroyEditor();
                    this._onSheetRename(this._editor.val());
                }

                this.trigger("remove", {name: removedSheetName});
            },

            _onSheetSelect: function(e) {
                var selectedSheetText = $(e.target).text();

                if ($(e.target).is(DOT + SheetsBar.classNames.sheetsBarEditor) || !selectedSheetText) {
                    e.preventDefault();
                    return;
                }

                if (this._editor) {
                    var editorValue = this._editor.val();
                    this._destroyEditor();
                    this._onSheetRename(editorValue);
                }

                this.trigger("select", {name: selectedSheetText, isAddButton: false});
            },

            _onSheetRename: function(newSheetName) {
                this.trigger("rename", {name: newSheetName, sheetIndex: this._selectedIndex });
            },

            _onAddSelect: function() {
                this.trigger("select", {isAddButton: true});
            },

            _addButton: function() {
                var element = kendo.dom.element;
                var classNames = SheetsBar.classNames;
                return element("a", {
                    className: classNames.sheetsBarAdd + EMPTYCHAR + classNames.sheetsBarKButton
                }, [element("span", {className: classNames.sheetsBarKSprite + EMPTYCHAR + classNames.sheetsBarKIcon + EMPTYCHAR + classNames.sheetsBarKFontIcon + EMPTYCHAR + classNames.sheetsBarKIconPlus}, [])]);
            },

            destroy: function() {
                this._sortable.destroy();
            },

            _scrollableAllowed: function() {
                var options = this.options;
                return options.scrollable && !isNaN(options.scrollable.distance);
            },

            _scrollSheetsToItem: function (item) {
                var that = this;
                var sheetsGroup = that._sheetsGroup();
                var currentScrollOffset = sheetsGroup.scrollLeft();
                var itemWidth = item.outerWidth();
                var itemOffset = that._isRtl ? item.position().left : item.position().left - sheetsGroup.children().first().position().left;
                var sheetsGroupWidth = sheetsGroup[0].offsetWidth;
                var sheetsGroupPadding = Math.ceil(parseFloat(sheetsGroup.css("padding-left")));
                var itemPosition;

                if (that._isRtl) {
                    if (itemOffset < 0) {
                        itemPosition = currentScrollOffset + itemOffset - (sheetsGroupWidth - currentScrollOffset) - sheetsGroupPadding;
                    } else if (itemOffset + itemWidth > sheetsGroupWidth) {
                        itemPosition = currentScrollOffset + itemOffset - itemWidth + sheetsGroupPadding * 2;
                    }
                } else {
                    if (currentScrollOffset + sheetsGroupWidth < itemOffset + itemWidth) {
                        itemPosition = itemOffset + itemWidth - sheetsGroupWidth + sheetsGroupPadding * 2;
                    } else if (currentScrollOffset > itemOffset) {
                        itemPosition = itemOffset - sheetsGroupPadding;
                    }
                }

                sheetsGroup.finish().animate({ "scrollLeft": itemPosition }, "fast", "linear", function () {
                    that._toggleScrollButtons();
                });
            },

            _sheetsGroup: function() {
                return this._sheetsWrapper().children("ul");
            },

            _sheetsWrapper: function() {
                return this.element.find(DOT + SheetsBar.classNames.sheetsBarItems);
            },

            _scrollSheetsByDelta: function (delta) {
                var that = this;
                var sheetsGroup = that._sheetsGroup();
                var scrLeft = sheetsGroup.scrollLeft();

                sheetsGroup.finish().animate({ "scrollLeft": scrLeft + delta }, "fast", "linear", function () {
                    if (that._nowScrollingSheets) {
                        that._scrollSheetsByDelta(delta);
                    } else {
                        that._toggleScrollButtons();
                    }
                });
            },

            _toggleScrollButtons: function () {
                var that = this;
                var ul = that._sheetsGroup();
                var wrapper = that._sheetsWrapper();
                var scrollLeft = ul.scrollLeft();

                wrapper.find(DOT + SheetsBar.classNames.sheetsBarPrev).toggle(that._isRtl ? scrollLeft < ul[0].scrollWidth - ul[0].offsetWidth - 1 : scrollLeft !== 0);
                wrapper.find(DOT + SheetsBar.classNames.sheetsBarNext).toggle(that._isRtl ? scrollLeft !== 0 : scrollLeft < ul[0].scrollWidth - ul[0].offsetWidth - 1);
            }
        });

        kendo.spreadsheet.SheetsBar = SheetsBar;
        $.extend(true, SheetsBar, { classNames: sheetsBarClassNames });
    })(window.kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;

    var Workbook = kendo.Observable.extend({
        init: function(options, view) {
            kendo.Observable.fn.init.call(this);

            this.options = options;

            this._view = view;
            this._sheets = [];

            this._sheetsSearchCache = {};

            this._sheet = this.insertSheet({
                rows: this.options.rows,
                columns: this.options.columns,
                rowHeight: this.options.rowHeight,
                columnWidth: this.options.columnWidth,
                headerHeight: this.options.headerHeight,
                headerWidth: this.options.headerWidth,
                dataSource: this.options.dataSource
            });

            this.undoRedoStack = new kendo.util.UndoRedoStack();
            this.undoRedoStack.bind(["undo", "redo"], this._onUndoRedo.bind(this));

            this._context = new kendo.spreadsheet.FormulaContext(this);
            this._validationContext = new kendo.spreadsheet.ValidationFormulaContext(this);

            this.fromJSON(this.options);
        },

        clipboard: function() {
            if(!this._clipboard) {
                 this._clipboard = new kendo.spreadsheet.Clipboard(this);
            }
            return this._clipboard;
        },

        destroy: function() {
            this.unbind();

            if (this._clipboard) {
                this._clipboard.destroy();
            }
        },

        events: [
            "change"
        ],

        _sheetChange: function(e) {
            this.trigger("change", e);
        },

        _inputForRef: function(ref) {
            return new kendo.spreadsheet.Range(ref, this._sheet).input();
        },

        _onUndoRedo: function(e) {
            e.command.range().select();
        },

        execute: function(options) {
            var commandOptions = $.extend({ workbook: this }, options.options);
            var command = new kendo.spreadsheet[options.command](commandOptions);
            var sheet = this.activeSheet();

            if (commandOptions.origin) {
                command.origin(commandOptions.origin);
            }

            if (commandOptions.operatingRange) {
                command.range(commandOptions.operatingRange);
            } else if (commandOptions.editActiveCell) {
                command.range(sheet.activeCellSelection());
            } else {
                command.range(sheet.selection());
            }

            var result = command.exec();

            if (!result || result.reason !== "error") {
                this.undoRedoStack.push(command);
            }

            return result;
        },

        resetFormulas: function() {
            this._sheets.forEach(function(sheet){
                sheet.resetFormulas();
            });
        },

        resetValidations: function() {
            this._sheets.forEach(function(sheet){
                sheet.resetValidations();
            });
        },

        refresh: function(reason) {
            if (reason.recalc) {
                this.resetFormulas();
                this.resetValidations();
                this._sheet.recalc(this._context);
                this._sheet.revalidate(this._validationContext);
            }
        },

        activeSheet: function(sheet) {
            if (sheet === undefined) {
                return this._sheet;
            }

            if (!this.sheetByName(sheet.name())) {
                return;
            }

            this._sheet = sheet;

            //TODO: better way to get all reasons?
            sheet.triggerChange(kendo.spreadsheet.ALL_REASONS);
        },

        moveSheetToIndex: function(sheet, toIndex) {
            var fromIndex = this.sheetIndex(sheet);
            var sheets = this._sheets;

            if (fromIndex === -1) {
                return;
            }

            this._sheetsSearchCache = {};

            sheets.splice(toIndex, 0, sheets.splice(fromIndex, 1)[0]);

            this.trigger("change", { sheetSelection: true });
        },

        insertSheet: function(options) {
            options = options || {};
            var that = this;
            var insertIndex = typeof options.index === "number" ? options.index : that._sheets.length;
            var sheetName;
            var sheets = that._sheets;

            var getUniqueSheetName = function(sheetNameSuffix) {
                sheetNameSuffix = sheetNameSuffix ? sheetNameSuffix : 1;

                var name = "Sheet" + sheetNameSuffix;

                if (!that.sheetByName(name)) {
                    return name;
                }

                return getUniqueSheetName(sheetNameSuffix + 1);
            };

            if (options.name && that.sheetByName(options.name)) {
                return;
            }

            this._sheetsSearchCache = {};

            sheetName = options.name || getUniqueSheetName();

            var sheet = new kendo.spreadsheet.Sheet(
                options.rows || this.options.rows,
                options.columns || this.options.columns,
                options.rowHeight || this.options.rowHeight,
                options.columnWidth || this.options.columnWidth,
                options.headerHeight || this.options.headerHeight,
                options.headerWidth || this.options.headerWidth
            );

            sheet._workbook = this;

            sheet.name(sheetName);

            sheet.bind("change", this._sheetChange.bind(this));

            sheets.splice(insertIndex, 0, sheet);

            if (options.data) {
                sheet.fromJSON(options.data);
            }

            if (options.dataSource) {
                sheet.setDataSource(options.dataSource);
            }

            this.trigger("change", { sheetSelection: true });

            return sheet;
        },

        sheets: function() {
            return this._sheets.slice();
        },

        sheetByName: function (sheetName) {
            return this._sheets[this.sheetIndex(sheetName)];
        },

        sheetByIndex: function(index) {
            return this._sheets[index];
        },

        sheetIndex: function(sheet) {
            var sheets = this._sheets;
            var sheetName = (typeof sheet == "string" ? sheet : sheet.name()).toLowerCase();
            var idx = this._sheetsSearchCache[sheetName];

            if (idx >= 0) {
                return idx;
            }

            for(idx = 0; idx < sheets.length; idx++) {
                var name = sheets[idx].name().toLowerCase();
                this._sheetsSearchCache[name] = idx;

                if (name === sheetName) {
                    return idx;
                }
            }

            return -1;
        },

        renameSheet: function(sheet, newSheetName) {
            var oldSheetName = sheet.name();

            if (!newSheetName ||
                oldSheetName === newSheetName) {
                return;
            }

            sheet = this.sheetByName(oldSheetName);

            if (!sheet) {
                return;
            }

            this._sheetsSearchCache = {};

            // update references
            this._sheets.forEach(function(sheet){
                sheet._forFormulas(function(formula){
                    formula.renameSheet(oldSheetName, newSheetName);
                });
            });

            sheet.name(newSheetName);

            this.trigger("change", { sheetSelection: true });

            return sheet;
        },

        removeSheet: function(sheet) {
            var that = this;
            var sheets = that._sheets;
            var name = sheet.name();
            var index = that.sheetIndex(sheet);

            if (sheets.length === 1) {
                return;
            }

            this._sheetsSearchCache = {};

            if (index > -1) {
                sheet.unbind();

                sheets.splice(index, 1);

                if (that.activeSheet().name() === name) {
                    var newSheet = sheets[index === sheets.length ? index-1 : index];
                    that.activeSheet(newSheet);
                } else {
                    this.trigger("change", { recalc: true,  sheetSelection: true });
                }
            }
        },

        fromJSON: function(json) {
            if (json.sheets) {
                for (var idx = 0; idx < json.sheets.length; idx++) {
                    var sheet = this.sheetByIndex(idx);

                    if (!sheet) {
                        sheet = this.insertSheet();
                    }

                    sheet.fromJSON(json.sheets[idx]);

                    var dataSource = json.sheets[idx].dataSource;

                    if (dataSource) {
                        sheet.setDataSource(dataSource);
                    }
                }
            }

            if (json.activeSheet) {
                this.activeSheet(this.sheetByName(json.activeSheet));
            }
        },

        toJSON: function() {
            this.resetFormulas();
            this.resetValidations();
            return {
                activeSheet: this.activeSheet().name(),
                sheets: this._sheets.map(function(sheet) {
                    sheet.recalc(this._context);
                    return sheet.toJSON();
                }, this)
            };
        },

        saveAsExcel: function(options) {
            options = $.extend({}, this.options.excel, options);
            var data = this.toJSON();

            if (!this.trigger("excelExport", { workbook: data })) {
                var workbook = new kendo.ooxml.Workbook(data);

                kendo.saveAs({
                    dataURI: workbook.toDataURL(),
                    fileName: data.fileName || options.fileName,
                    proxyURL: options.proxyURL,
                    forceProxy: options.forceProxy
                });
            }
        }
    });

    kendo.spreadsheet.Workbook = Workbook;

})(kendo);

})();

(function() {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    /* jshint eqnull:true */

    var spreadsheet = kendo.spreadsheet;
    var CellRef = kendo.spreadsheet.CellRef;
    var RangeRef = kendo.spreadsheet.RangeRef;
    var UnionRef = kendo.spreadsheet.UnionRef;
    var NameRef = kendo.spreadsheet.NameRef;

    var FormulaContext = kendo.Class.extend({
        init: function (workbook) {
            this.workbook = workbook;
        },

        getRefCells: function(ref, hiddenInfo) {
            var sheet, formula, value, i;
            if (ref instanceof CellRef) {
                sheet = this.workbook.sheetByName(ref.sheet);
                if (!sheet) {
                    return [{
                        value: new kendo.spreadsheet.calc.runtime.CalcError("REF")
                    }];
                }
                formula = sheet.formula(ref);
                value = sheet.range(ref.row, ref.col).value();

                if (formula != null || value != null) {
                    return [{
                        formula: formula,
                        value: value,
                        row: ref.row,
                        col: ref.col,
                        sheet: ref.sheet,
                        hidden: hiddenInfo ? (sheet.columnWidth(ref.col) === 0 || sheet.rowHeight(ref.row) === 0) : false
                    }];
                } else {
                    return [];
                }
            }
            if (ref instanceof RangeRef) {
                i = this.workbook.sheetIndex(ref.sheet);
                var states = [], n = i;
                if (ref.endSheet) {
                    // "3D" reference.
                    n = this.workbook.sheetIndex(ref.endSheet);
                    if (i > n) {
                        var tmp = i;
                        i = n;
                        n = tmp;
                    }
                }

                if (i < 0 || n < 0) {
                    return [{
                        value: new kendo.spreadsheet.calc.runtime.CalcError("REF")
                    }];
                }

                // XXX: This is nicer, but significantly slower.
                // Should investigate why, or add some options to make
                // it faster (i.e. probably because it adds all cell
                // properties, while we only need value and formula).
                //
                //     var add = function(row, col, data){
                //         data.row = row;
                //         data.col = col;
                //         data.sheet = sheet.name();
                //         states.push(data);
                //     };
                //     while (i <= n) {
                //         sheet = this.workbook.sheetByIndex(i++);
                //         sheet.forEach(ref, add);
                //     }
                //
                // For now keep doing it "manually".

                while (i <= n) {
                    sheet = this.workbook.sheetByIndex(i++);
                    var tl = sheet._grid.normalize(ref.topLeft);
                    var br = sheet._grid.normalize(ref.bottomRight);

                    var startCellIndex = sheet._grid.cellRefIndex(tl);
                    var endCellIndex = sheet._grid.cellRefIndex(br);

                    var values = sheet._properties.iterator("value", startCellIndex, endCellIndex);

                    for (var col = tl.col; col <= br.col; ++col) {
                        for (var row = tl.row; row <= br.row; ++row) {
                            var index = sheet._grid.index(row, col);
                            formula = sheet._properties.get("formula", index);
                            value = values.at(index);
                            if (formula != null || value != null) {
                                states.push({
                                    formula : formula,
                                    value   : value,
                                    row     : row,
                                    col     : col,
                                    sheet   : sheet.name(),
                                    hidden  : hiddenInfo ? (sheet.columnWidth(col) === 0 || sheet.rowHeight(row) === 0) : false
                                });
                            }
                        }
                    }
                }

                return states;
            }
            if (ref instanceof UnionRef) {
                var a = [];
                for (i = 0; i < ref.refs.length; ++i) {
                    a = a.concat(this.getRefCells(ref.refs[i], hiddenInfo));
                }
                return a;
            }
            if (ref instanceof NameRef) {
                // XXX: NameRef-s not yet supported
                return [{
                    value: new kendo.spreadsheet.calc.runtime.CalcError("NAME")
                }];
            }
            return [];
        },

        getData: function(ref) {
            var data = this.getRefCells(ref).map(function(cell){
                return cell.value;
            });
            return ref instanceof CellRef ? data[0] : data;
        },

        onFormula: function(f) {
            var sheet = this.workbook.sheetByName(f.sheet);
            var row = f.row, col = f.col, value = f.value;
            var currentFormula = sheet.formula({ row: row, col: col });
            if (currentFormula !== f) {
                // could have been deleted or modified in the mean time,
                // if the formula was asynchronous.  ignore this result.
                return false;
            }

            if (value instanceof kendo.spreadsheet.calc.runtime.Matrix) {
                value.each(function(value, r, c) {
                    sheet._value(row + r, col + c, value);
                });
            } else {
                sheet._value(row, col, value);
            }

            return true;
        }
    });

    var ValidationFormulaContext = FormulaContext.extend({
        onFormula: function() {
            return true;
        }
    });

    spreadsheet.FormulaContext = FormulaContext;
    spreadsheet.ValidationFormulaContext = ValidationFormulaContext;

})();

(function(){

(function(kendo) {
    'use strict';

    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;
    var alphaNumRegExp = /:alphanum$/;

    var ACTIONS = {
       "up": "up",
       "down": "down",
       "left": "left",
       "right": "right",
       "home": "first-col",
       "ctrl+left": "first-col",
       "end": "last-col",
       "ctrl+right": "last-col",
       "ctrl+up": "first-row",
       "ctrl+down": "last-row",
       "ctrl+home": "first",
       "ctrl+end": "last",
       "pageup": "prev-page",
       "pagedown": "next-page"
    };

    var ENTRY_ACTIONS = {
        "tab": "next",
        "shift+tab": "previous",
        "enter": "lower",
        "shift+enter": "upper",
        "delete": "clearContents",
        "backspace": "clearContents",
        "shift+:alphanum": "edit",
        ":alphanum": "edit",
        "ctrl+:alphanum": "ctrl",
        ":edit": "edit"
    };

    var CONTAINER_EVENTS = {
        "wheel": "onWheel",
        "*+mousedown": "onMouseDown",
        "contextmenu": "onContextMenu",
        "*+mousedrag": "onMouseDrag",
        "*+mouseup": "onMouseUp",
        "*+dblclick": "onDblClick",
        "mousemove": "onMouseMove"
    };

    var CLIPBOARD_EVENTS = {
        "*+pageup": "onPageUp",
        "*+pagedown": "onPageDown",
        "mouseup": "onMouseUp",
        "*+cut": "onCut",
        "*+paste": "onPaste",
        "*+copy": "onCopy"
    };

    var EDITOR_EVENTS = {
        "esc": "onEditorEsc",
        "enter": "onEditorBlur",
        "shift+enter": "onEditorBlur",
        "tab": "onEditorBlur",
        "shift+tab": "oonEditorBlur"
    };

    var FORMULABAR_EVENTS = $.extend({ focus: "onEditorBarFocus" }, EDITOR_EVENTS);
    var FORMULAINPUT_EVENTS = $.extend({ focus: "onEditorCellFocus" }, EDITOR_EVENTS);

    var SELECTION_MODES = {
       cell: "range",
       rowheader: "row",
       columnheader: "column",
       topcorner: "sheet",
       autofill: "autofill"
    };

    function toActionSelector(selectors) {
        return selectors.map(function(action) {
            return '[data-action="' + action + '"]';
        }).join(",");
    }

    var COMPOSITE_UNAVAILABLE_ACTION_SELECTORS = toActionSelector([ 'cut', 'copy', 'paste', 'insert-left', 'insert-right', 'insert-above', 'insert-below' ]);
    var UNHIDE_ACTION_SELECTORS = toActionSelector([ 'unhide-row', 'unhide-column' ]);

    var ACTION_KEYS = [];
    var SHIFT_ACTION_KEYS = [];
    var ENTRY_ACTION_KEYS = [];

    for (var key in ACTIONS) {
        ACTION_KEYS.push(key);
        SHIFT_ACTION_KEYS.push("shift+" + key);
    }

    for (key in ENTRY_ACTIONS) {
        ENTRY_ACTION_KEYS.push(key);
    }

    CLIPBOARD_EVENTS[ACTION_KEYS] = "onAction";
    CLIPBOARD_EVENTS[SHIFT_ACTION_KEYS] = "onShiftAction";
    CLIPBOARD_EVENTS[ENTRY_ACTION_KEYS] = "onEntryAction";

    FORMULAINPUT_EVENTS[ACTION_KEYS] = "onEditorAction";
    FORMULAINPUT_EVENTS[SHIFT_ACTION_KEYS] = "onEditorShiftAction";

    var Controller = kendo.Class.extend({
        init: function(view, workbook) {
            this.view = view;
            this.workbook(workbook);
            this.container = $(view.container);
            this.clipboardElement = $(view.clipboard);
            this.cellContextMenu = view.cellContextMenu;
            this.rowHeaderContextMenu = view.rowHeaderContextMenu;
            this.colHeaderContextMenu = view.colHeaderContextMenu;
            this.scroller = view.scroller;
            this.tabstrip = view.tabstrip;
            this.sheetsbar = view.sheetsbar;

            this.editor = view.editor;
            this.editor.bind("change", this.onEditorChange.bind(this));
            this.editor.bind("activate", this.onEditorActivate.bind(this));
            this.editor.bind("deactivate", this.onEditorDeactivate.bind(this));
            this.editor.bind("update", this.onEditorUpdate.bind(this));

            $(view.scroller).on("scroll", this.onScroll.bind(this));
            this.listener = new kendo.spreadsheet.EventListener(this.container, this, CONTAINER_EVENTS);
            this.keyListener = new kendo.spreadsheet.EventListener(this.clipboardElement, this, CLIPBOARD_EVENTS);

            this.barKeyListener = new kendo.spreadsheet.EventListener(this.editor.barElement(), this, FORMULABAR_EVENTS);
            this.inputKeyListener = new kendo.spreadsheet.EventListener(this.editor.cellElement(), this, FORMULAINPUT_EVENTS);

            if (this.sheetsbar) {
                this.sheetsbar.bind("select", this.onSheetBarSelect.bind(this));
                this.sheetsbar.bind("reorder", this.onSheetBarReorder.bind(this));
                this.sheetsbar.bind("rename", this.onSheetBarRename.bind(this));
                this.sheetsbar.bind("remove", this.onSheetBarRemove.bind(this));
            }

            this.cellContextMenu.bind("select", this.onContextMenuSelect.bind(this));
            this.rowHeaderContextMenu.bind("select", this.onContextMenuSelect.bind(this));
            this.colHeaderContextMenu.bind("select", this.onContextMenuSelect.bind(this));

            // this is necessary for Windows to catch prevent context menu correctly
            this.cellContextMenu.element.add(this.rowHeaderContextMenu.element).add(this.colHeaderContextMenu.element).on("contextmenu", false);

            if (this.tabstrip) {
                this.tabstrip.bind("action", this.onCommandRequest.bind(this));
                this.tabstrip.bind("dialog", this.onDialogRequest.bind(this));
            }
        },

        _execute: function(command) {
            var result = this._workbook.execute(command);

            if (result) {
                if (result.reason === "error") {
                    this.view.showError(result);
                } else {
                    this.view.openDialog(result.reason);
                }
            }
        },

        _activeTooltip: function() {
            return this._workbook.activeSheet().activeCell().simplify().toString();
        },

        onContextMenuSelect: function(e) {
                var action = $(e.item).data("action");
                var command;
                switch(action) {
                    case "cut":
                        command = { command: "ToolbarCutCommand", options: { workbook: this._workbook } };
                        break;
                    case "copy":
                        command = { command: "ToolbarCopyCommand", options: { workbook: this._workbook } };
                        break;
                    case "paste":
                        command = { command: "ToolbarPasteCommand", options: { workbook: this._workbook } };
                        break;
                    case "unmerge":
                        command = { command: "MergeCellCommand", options: { value: "unmerge" } };
                        break;
                    case "merge":
                        this.view.openDialog("merge");
                        break;
                    case "hide-row":
                        command = { command: "HideLineCommand", options: { axis: "row" } };
                        break;
                    case "hide-column":
                        command = { command: "HideLineCommand", options: { axis: "column" } };
                        break;
                    case "unhide-row":
                        command = { command: "UnHideLineCommand", options: { axis: "row" } };
                        break;
                    case "unhide-column":
                        command = { command: "UnHideLineCommand", options: { axis: "column" } };
                        break;
                    case "delete-row":
                        command = { command: "DeleteRowCommand" };
                        break;
                    case "delete-column":
                        command = { command: "DeleteColumnCommand" };
                        break;
                }

                if (command) {
                    this._execute(command);
                }
        },

        onSheetBarRemove: function(e) {
            var sheet = this._workbook.sheetByName(e.name);

            //TODO: move to model!
            if (!sheet) {
                return;
            }

            this._workbook.removeSheet(sheet);
        },

        destroy: function() {
            this.listener.destroy();
            this.keyListener.destroy();
            this.inputKeyListener.destroy();
        },

        onSheetBarSelect: function(e) {
            var sheet;
            var workbook = this._workbook;

            if (e.isAddButton) {
                sheet = workbook.insertSheet();
            } else {
                sheet = workbook.sheetByName(e.name);
            }

            //TODO: move to model
            if (workbook.activeSheet().name() !== sheet.name()) {
                workbook.activeSheet(sheet);
            }
        },

        onSheetBarReorder: function(e) {
            var sheet = this._workbook.sheetByIndex(e.oldIndex);

            this._workbook.moveSheetToIndex(sheet, e.newIndex);

            this._workbook.activeSheet(sheet);
        },

        onSheetBarRename: function(e) {
            var sheet = this._workbook.sheetByIndex(e.sheetIndex);

            this._workbook.renameSheet(sheet, e.name);

            this.clipboardElement.focus();
        },

        sheet: function(sheet) {
            this.navigator = sheet.navigator();
            this.axisManager = sheet.axisManager();
        },

        workbook: function(workbook) {
            this._workbook = workbook;
            this.clipboard = workbook.clipboard();
        },

        refresh: function() {
            var workbook = this._workbook;

            this._viewPortHeight = this.view.scroller.clientHeight;
            this.navigator.height(this._viewPortHeight);

            if (!this.editor.isActive()) {
                this.editor.value(workbook._inputForRef(workbook.activeSheet().activeCell()));
            }
        },

        onScroll: function() {
            this.view.render();
        },

        onWheel: function(event) {
            var deltaX = event.originalEvent.deltaX;
            var deltaY = event.originalEvent.deltaY;

            if (event.originalEvent.deltaMode === 1) {
                deltaX *= 10;
                deltaY *= 10;
            }

            this.scrollWith(deltaX, deltaY);

            event.preventDefault();
        },

        onAction: function(event, action) {
            this.navigator.moveActiveCell(ACTIONS[action]);
            event.preventDefault();
        },

        onPageUp: function() {
            this.scrollDown(-this._viewPortHeight);
        },

        onPageDown: function() {
            this.scrollDown(this._viewPortHeight);
        },

        onEntryAction: function(event, action) {
            if (event.mod) {
                var shouldPrevent = true;
                var key = String.fromCharCode(event.keyCode);

                switch(key) {
                    case "A":
                        this.navigator.selectAll();
                        break;
                    case "Y":
                        this._workbook.undoRedoStack.redo();
                        break;
                    case "Z":
                        this._workbook.undoRedoStack.undo();
                        break;
                    default:
                        shouldPrevent = false;
                        break;
                }
                if(shouldPrevent) {
                    event.preventDefault();
                }
            } else {
                if (action == "delete" || action == "backspace") {
                    this._execute({ command: "ClearContentCommand" });
                    event.preventDefault();
                } else if (alphaNumRegExp.test(action) || action === ":edit") {
                    if (action !== ":edit") {
                        this.editor.value("");
                    }

                    this.editor
                        .activate({
                            rect: this.view.activeCellRectangle(),
                            tooltip: this._activeTooltip()
                        })
                        .focus();
                } else {
                    this.navigator.navigateInSelection(ENTRY_ACTIONS[action]);
                    event.preventDefault();
                }
            }
        },

        onShiftAction: function(event, action) {
            this.navigator.modifySelection(ACTIONS[action.replace("shift+", "")], this.appendSelection);
            event.preventDefault();
        },

        onMouseMove: function(event) {
            var sheet = this._workbook.activeSheet();

            if (sheet.resizingInProgress() || sheet.selectionInProgress()) {
                return;
            }

            var object = this.objectAt(event);
            if (object.type === "columnresizehandle" || object.type === "rowresizehandle") {
                sheet.positionResizeHandle(object.ref);
            } else {
                sheet.removeResizeHandle();
            }
        },

        onMouseDown: function(event) {
            var object = this.objectAt(event);

            if (object.pane) {
                this.originFrame = object.pane;
            }

            if (this.editor.canInsertRef(false) && object.ref) {
                this._workbook.activeSheet()._setFormulaSelections(this.editor.highlightedRefs());
                this.navigator.startSelection(object.ref, this._selectionMode, this.appendSelection);
                event.preventDefault();
                return;
            } else {
                this.editor.deactivate();
            }

            var sheet = this._workbook.activeSheet();
            if (object.type === "columnresizehandle" || object.type === "rowresizehandle") {
                sheet.startResizing({ x: object.x, y: object.y });
                event.preventDefault();
                return;
            }

            if (object.type === "filtericon") {
                this.openFilterMenu(event.target);
                event.preventDefault();
                return;
            }

            this._selectionMode = SELECTION_MODES[object.type];
            this.appendSelection = event.mod;
            this.navigator.startSelection(object.ref, this._selectionMode, this.appendSelection);
        },

        onContextMenu: function(event) {
            var sheet = this._workbook.activeSheet();

            if (sheet.resizingInProgress()) {
                return;
            }

            event.preventDefault();

            this.cellContextMenu.close();
            this.colHeaderContextMenu.close();
            this.rowHeaderContextMenu.close();

            var menu;

            var location = { pageX: event.pageX, pageY: event.pageY };

            var object = this.objectAt(location);

            if (object.type === "columnresizehandle" || object.type === "rowresizehandle") {
                return;
            }

            this.navigator.selectForContextMenu(object.ref, SELECTION_MODES[object.type]);

            var isComposite = this.navigator._sheet.select() instanceof kendo.spreadsheet.UnionRef;
            var showUnhide = false;
            var showUnmerge = false;

            if (object.type === "cell") {
                menu = this.cellContextMenu;
                showUnmerge = this.navigator.selectionIncludesMergedCells();
            } else if (object.type == "columnheader") {
                menu = this.colHeaderContextMenu;
                showUnhide = !isComposite && this.axisManager.selectionIncludesHiddenColumns();
            } else if (object.type == "rowheader") {
                menu = this.rowHeaderContextMenu;
                showUnhide = !isComposite && this.axisManager.selectionIncludesHiddenRows();
            }

            menu.element.find(COMPOSITE_UNAVAILABLE_ACTION_SELECTORS).toggle(!isComposite);
            menu.element.find(UNHIDE_ACTION_SELECTORS).toggle(showUnhide);
            menu.element.find('[data-action=unmerge]').toggle(showUnmerge);

            // avoid the immediate close
            setTimeout(function() {
                menu.open(event.pageX, event.pageY);
            });
        },

        prevent: function(event) {
            event.preventDefault();
        },

        constrainResize: function(type, ref) {
            var sheet = this._workbook.activeSheet();
            var resizeHandle = sheet.resizeHandlePosition();

            return !resizeHandle || type === "outside" || type === "topcorner" || ref.col < resizeHandle.col || ref.row < resizeHandle.row;
        },

        onMouseDrag: function(event) {
            if (this._selectionMode === "sheet") {
                return;
            }

            var location = { pageX: event.pageX, pageY: event.pageY };
            var object = this.objectAt(location);

            var sheet = this._workbook.activeSheet();
            if (sheet.resizingInProgress()) {

                if (!this.constrainResize(object.type, object.ref)) {
                    sheet.resizeHintPosition({ x: object.x, y: object.y });
                }

                return;
            }

            if (object.type === "outside") {
                this.startAutoScroll(object);
                return;
            }

            if (this.originFrame === object.pane) {
                this.selectToLocation(location);
            } else { // cross frame selection
                var frame = this.originFrame._grid;

                if (object.x > frame.right) {
                    this.scrollLeft();
                }

                if (object.y > frame.bottom) {
                    this.scrollTop();
                }

                if (object.y < frame.top || object.x < frame.left) {
                    this.startAutoScroll(object, location);
                } else {
                    this.selectToLocation(location);
                }
            }

            event.preventDefault();
        },

        onMouseUp: function(event) {
            var sheet = this._workbook.activeSheet();
            sheet.completeResizing();

            this.navigator.completeSelection();
            this.stopAutoScroll();

            var editor = this.editor.activeEditor();
            if (!editor) {
                return;
            }
            var el = event.target;
            while (el) {
                if (el === editor.element[0]) {
                    return;
                }
                el = el.parentNode;
            }

            var object = this.objectAt(event);
            if (object && object.ref && editor.canInsertRef(false)) {
                editor.refAtPoint(sheet.selection()._ref);
                sheet._setFormulaSelections(editor.highlightedRefs());
            }
        },

        onDblClick: function(event) {
            var object = this.objectAt(event);

            if (object.type !== "cell") {
                return;
            }

            this.editor
                .activate({
                    rect: this.view.activeCellRectangle(),
                    tooltip: this._activeTooltip()
                })
                .focus();

            this.onEditorUpdate();
        },

        onCut: function(e) {
            if(e){
                var table = this.clipboardElement.find("table.kendo-clipboard-"+ this.clipboard._uid).detach();
                this.clipboardElement.append(table.clone(false));
                setTimeout(function() {
                    this.clipboardElement.empty().append(table);
                }.bind(this));
            }

            this._execute({
                command: "CutCommand",
                options: { workbook: this.view._workbook }
            });
        },

        clipBoardValue: function() {
            return this.clipboardElement.html();
        },

        onPaste: function(e) {
            var html = "";
            var plain = "";
            this.clipboard.menuInvoked = (e === undefined);
            if(e) {
                if (e.originalEvent.clipboardData && e.originalEvent.clipboardData.getData) {
                    e.preventDefault();
                    var hasHTML = false;
                    var hasPlainText = false;
                    //Firefox uses DOMStringList, needs special handling
                    if(window.DOMStringList && e.originalEvent.clipboardData.types instanceof window.DOMStringList) {
                        hasHTML = e.originalEvent.clipboardData.types.contains("text/html");
                        hasPlainText = e.originalEvent.clipboardData.types.contains("text/plain");
                    } else {
                        hasHTML = (/text\/html/.test(e.originalEvent.clipboardData.types));
                        hasPlainText = (/text\/plain/.test(e.originalEvent.clipboardData.types));
                    }
                    if (hasHTML) {
                        html = e.originalEvent.clipboardData.getData('text/html');
                    }
                    if (hasPlainText) {
                        plain = e.originalEvent.clipboardData.getData('text/plain').trim();
                    }
                } else {
                    //workaround for IE's lack of access to the HTML clipboard data
                    var table = this.clipboardElement.find("table.kendo-clipboard-"+ this.clipboard._uid).detach();
                    this.clipboardElement.empty();
                    setTimeout(function() {
                        this.clipboard.external({html: this.clipboardElement.html(), plain: window.clipboardData.getData("Text").trim()});
                        this.clipboardElement.empty().append(table);
                        this._execute({
                            command: "PasteCommand",
                            options: { workbook: this.view._workbook }
                        });
                        this.clipboard.menuInvoked = true;
                    }.bind(this));
                    return;
                }
            } else {
                if(kendo.support.browser.msie) {
                    this.clipboardElement.focus().select();
                    document.execCommand('paste');
                    return;
                } else {
                    this.clipboard.menuInvoked = true;
                }
            }

            this.clipboard.external({html: html, plain:plain});
            this._execute({
                command: "PasteCommand",
                options: { workbook: this.view._workbook }
            });

        },

        onCopy: function(e) {
            this.clipboard.menuInvoked = (e === undefined);
            this._execute({
                command: "CopyCommand",
                options: { workbook: this.view._workbook }
            });
        },

////////////////////////////////////////////////////////////////////

        scrollTop: function() {
            this.scroller.scrollTop = 0;
        },

        scrollLeft: function() {
            this.scroller.scrollLeft = 0;
        },

        scrollDown: function(value) {
            this.scroller.scrollTop += value;
        },

        scrollRight: function(value) {
            this.scroller.scrollLeft += value;
        },

        scrollWith: function(right, down) {
            this.scroller.scrollTop += down;
            this.scroller.scrollLeft += right;
        },

        objectAt: function(location) {
            var offset = this.container.offset();
            var coordinates = {
                left: location.pageX - offset.left,
                top: location.pageY - offset.top
            };

            return this.view.objectAt(coordinates.left, coordinates.top);
        },

        selectToLocation: function(cellLocation) {
            var object = this.objectAt(cellLocation);

            if (object.pane) { // cell, rowheader or columnheader
                this.extendSelection(object);
                this.lastKnownCellLocation = cellLocation;
                this.originFrame = object.pane;
            }

            this.stopAutoScroll();
        },

        extendSelection: function(object) {
            this.navigator.extendSelection(object.ref, this._selectionMode, this.appendSelection);
        },

        autoScroll: function() {
            var x = this._autoScrollTarget.x;
            var y = this._autoScrollTarget.y;
            var boundaries = this.originFrame._grid;
            var scroller = this.view.scroller;
            var scrollStep = 8;

            var scrollLeft = scroller.scrollLeft;
            var scrollTop = scroller.scrollTop;

            if (x < boundaries.left) {
                this.scrollRight(-scrollStep);
            }
            if (x > boundaries.right) {
                this.scrollRight(scrollStep);
            }
            if (y < boundaries.top) {
                this.scrollDown(-scrollStep);
            }
            if (y > boundaries.bottom) {
                this.scrollDown(scrollStep);
            }

            if (scrollTop === scroller.scrollTop && scrollLeft === scroller.scrollLeft) {
                this.selectToLocation(this.finalLocation);
            } else {
                this.extendSelection(this.objectAt(this.lastKnownCellLocation));
            }
        },

        startAutoScroll: function(viewObject, location) {
            if (!this._scrollInterval) {
                this._scrollInterval = setInterval(this.autoScroll.bind(this), 50);
            }

            this.finalLocation = location || this.lastKnownCellLocation;

            this._autoScrollTarget = viewObject;
        },

        stopAutoScroll: function() {
            clearInterval(this._scrollInterval);
            this._scrollInterval = null;
        },

        openFilterMenu: function(target) {
            var button = $(target).closest(".k-spreadsheet-filter");
            var filterMenu = this.view.createFilterMenu(button.data("column"));

            filterMenu.bind("action", this.onCommandRequest.bind(this));
            filterMenu.bind("action", filterMenu.close.bind(filterMenu));

            filterMenu.openFor(target);
        },

////////////////////////////////////////////////////////////////////

        onEditorChange: function(e) {
            this._workbook.activeSheet().isInEditMode(false);

            this._execute({
                command: "EditCommand",
                options: {
                    editActiveCell: true,
                    value: e.value
                }
            });
        },

        onEditorActivate: function() {
            var workbook = this._workbook;
            var sheet = workbook.activeSheet();

            sheet._setFormulaSelections(this.editor.highlightedRefs());
            sheet.isInEditMode(true);
        },

        onEditorDeactivate: function() {
            var sheet = this._workbook.activeSheet();

            sheet.isInEditMode(false);
            sheet._setFormulaSelections([]);
        },

        onEditorUpdate: function() {
            this._workbook.activeSheet()._setFormulaSelections(this.editor.highlightedRefs());
        },

        onEditorBarFocus: function() {
            this.editor
                .activate({
                    rect: this.view.activeCellRectangle(),
                    tooltip: this._activeTooltip()
                });
        },

        onEditorCellFocus: function() {
            this.editor.scale();
        },

        onEditorEsc: function() {
            this.editor.value(this._workbook._inputForRef(this._workbook.activeSheet()._viewActiveCell()));
            this.editor.deactivate();

            this.clipboardElement.focus();
        },

        onEditorBlur: function(_, action) {
            if (this.editor.isFiltered()) {
                return;
            }

            this.editor.deactivate();
            this.clipboardElement.focus();

            this.navigator.navigateInSelection(ENTRY_ACTIONS[action]);
        },

        onEditorAction: function(event, action) {
            var editor = this.editor;
            var sheet = this._workbook.activeSheet();

            if (editor.canInsertRef(true)) {
                this.navigator.moveActiveCell(ACTIONS[action]);

                editor.activeEditor().refAtPoint(sheet.selection()._ref);
                sheet._setFormulaSelections(editor.highlightedRefs());

                event.preventDefault();
            }
        },

        onEditorShiftAction: function(event, action) {
            var editor = this.editor;
            var sheet = this._workbook.activeSheet();

            if (editor.canInsertRef(true)) {
                this.navigator.modifySelection(ACTIONS[action.replace("shift+", "")], this.appendSelection);

                editor.activeEditor().refAtPoint(sheet.selection()._ref);
                sheet._setFormulaSelections(editor.highlightedRefs());

                event.preventDefault();
            }
        },

////////////////////////////////////////////////////////////////////

        onCommandRequest: function(e) {
            if (e.command) {
                this._execute(e);
            } else {
                this._workbook.undoRedoStack[e.action]();
            }
        },

        onDialogRequest: function(e) {
            this.view.openDialog(e.name, e.options);
        }
    });

    kendo.spreadsheet.Controller = Controller;
})(window.kendo);

})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;
    var CellRef = kendo.spreadsheet.CellRef;
    var DOT = ".";
    var RESIZE_HANDLE_WIDTH = 7;
    var viewClassNames = {
        view: "k-spreadsheet-view",
        fixedContainer: "k-spreadsheet-fixed-container",
        scroller: "k-spreadsheet-scroller",
        viewSize: "k-spreadsheet-view-size",
        clipboard: "k-spreadsheet-clipboard",
        cellEditor: "k-spreadsheet-cell-editor",
        barEditor: "k-spreadsheet-editor",
        topCorner: "k-spreadsheet-top-corner",
        filterHeadersWrapper: "k-filter-wrapper",
        filterRange: "k-filter-range",
        filterButton: "k-spreadsheet-filter",
        filterButtonActive: "k-state-active",
        icon: "k-icon k-font-icon",
        iconFilterDefault: "k-i-arrow-s",
        sheetsBar: "k-spreadsheet-sheets-bar",
        sheetsBarActive: "k-spreadsheet-sheets-bar-active",
        sheetsBarInactive: "k-spreadsheet-sheets-bar-inactive",
        cellContextMenu: "k-spreadsheet-cell-context-menu",
        rowHeaderContextMenu: "k-spreadsheet-row-header-context-menu",
        colHeaderContextMenu: "k-spreadsheet-col-header-context-menu"
    };

    var VIEW_MESAGES = kendo.spreadsheet.messages.view = {
        errors: {
            shiftingNonblankCells: "Cannot insert cells due to data loss possibility. Select another insert location or delete the data from the end of your worksheet.",
            filterRangeContainingMerges: "Cannot create a filter within a range containing merges"
        },
        tabs: {
            home: "Home",
            insert: "Insert",
            data: "Data"
        }
    };

    function selectElementContents(el) {
        var sel = window.getSelection();
        sel.removeAllRanges();

        var range = document.createRange();
        range.selectNodeContents(el);

        sel.addRange(range);
    }

    function cellBefore(table, row) {
        var cells = table.trs[row].children;
        return cells[cells.length - 2];
    }

    function cellAbove(table, row) {
        var prevRow = table.trs[row-1];
        var index = table.trs[row].children.length-1;

        if (prevRow && index >= 0) {
            return prevRow.children[index];
        }
    }

    function cellBorder(value) {
        return [
            "solid",
            (value.size || 1) + "px",
            value.color || "#000"
        ].join(" ");
    }

    function addCell(table, row, cell) {
        var style = {};

        if (cell.background) {
            style.backgroundColor = cell.background;
        }

        if (cell.color) {
            style.color = cell.color;
        }

        if (cell.fontFamily) {
            style.fontFamily = cell.fontFamily;
        }

        if (cell.underline) {
            style.textDecoration = "underline";
        }

        if (cell.italic) {
            style.fontStyle = "italic";
        }

        if (cell.textAlign) {
            style.textAlign = cell.textAlign;
        }

        if (cell.verticalAlign) {
            style.verticalAlign = cell.verticalAlign;
        }

        if (cell.bold) {
            style.fontWeight = "bold";
        }

        if (cell.fontSize) {
            style.fontSize = cell.fontSize + "px";
        }

        if (cell.wrap === true) {
            style.whiteSpace = "normal";
            style.wordBreak = "break-all";
        } else {
            style.whiteSpace = "nowrap";
        }

        if (cell.borderRight) {
            style.borderRight = cellBorder(cell.borderRight);
        } else if (cell.background) {
            style.borderRightColor = cell.background;
        }

        if (cell.borderBottom) {
            style.borderBottom = cellBorder(cell.borderBottom);
        } else if (cell.background) {
            style.borderBottomColor = cell.background;
        }

        var data = cell.value, type = typeof data;
        if (cell.format && data !== null) {
            data = kendo.spreadsheet.formatting.format(data, cell.format);
            if (data.__dataType) {
                type = data.__dataType;
            }
        }

        if (!style.textAlign) {
            switch (type) {
              case "number":
              case "date":
              case "percent":
                style.textAlign = "right";
                break;
              case "boolean":
                style.textAlign = "center";
                break;
            }
        }

        var td = table.addCell(row, data, style, null, cell.validation);

        var border, sibling;

        if (cell.borderLeft) {
            sibling = cellBefore(table, row);
            border = cellBorder(cell.borderLeft);
            if (sibling && border) {
                sibling.attr.style.borderRight = border;
            }
        } else if (cell.background) {
            style.borderLeftColor = cell.background;
        }

        if (cell.borderTop) {
            sibling = cellAbove(table, row);
            border = cellBorder(cell.borderTop);
            if (sibling && border) {
                sibling.attr.style.borderBottom = border;
            }
        } else if (cell.background) {
            style.borderTopColor = cell.background;
        }

        return td;
    }

    var HtmlTable = kendo.Class.extend({
        init: function() {
            this.cols = [];
            this.trs = [];
            this._height = 0;
            this._width = 0;
        },

        addColumn: function(width) {
            this._width += width;

            var col = kendo.dom.element("col", { style: { width: width + "px" } });

            col.visible = width > 0;

            this.cols.push(col);
        },

        addRow: function(height) {
            var attr = null;

            attr = { style: { height: height + "px" } };

            this._height += height;

            var tr = kendo.dom.element("tr", attr);

            tr.visible = height > 0;

            this.trs.push(tr);
        },

        addCell: function(rowIndex, text, style, className, validation) {
            if (text === null || text === undefined) {
                text = "";
            }
            if (!(text instanceof kendo.dom.Node)) {
                text = kendo.dom.text(text);
            }

            var children = [ text ];
            var properties = { style: style };

            if (validation && !validation.value) {
                children.push(kendo.dom.element("span", { className: "k-dirty" }));

                className = (className || "") + (className ? " " : "") + "k-dirty-cell";
                properties.title = validation._getOptions().messageTemplate;
            }

            if (className) {
                properties.className = className;
            }
            var td = kendo.dom.element("td", properties, children);

            this.trs[rowIndex].children.push(td);
            return td;
        },

        toDomTree: function(x, y, className) {
            this.trs = this.trs.filter(function(tr) {
                return tr.visible;
            });

            var offset = 0;
            this.cols = this.cols.filter(function(col, ci) {
                if (!col.visible) {
                    this.trs.forEach(function(tr) {
                        tr.children.splice(ci - offset, 1);
                    });
                    offset++;
                }

                return col.visible;
            }, this);

            return kendo.dom.element("table", { style: { left: x + "px", top: y + "px", height: this._height + "px", width: this._width + "px" }, className: className },
                [
                    kendo.dom.element("colgroup", null, this.cols),
                    kendo.dom.element("tbody", null, this.trs)
                ]);
        }
    });

    var CELL_CONTEXT_MENU = '<ul class="#=classNames.cellContextMenu#">' +
        '<li data-action=cut>Cut</li>' +
        '<li data-action=copy>Copy</li>' +
        '<li data-action=paste>Paste</li>' +
        '<li class="k-separator"></li>' +
        '<li data-action=merge>Merge</li>' +
        '<li data-action=unmerge>Unmerge</li>' +
    '</ul>';

    var ROW_HEADER_CONTEXT_MENU = '<ul class="#=classNames.rowHeaderContextMenu#">' +
        '<li data-action=cut>Cut</li>' +
        '<li data-action=copy>Copy</li>' +
        '<li data-action=paste>Paste</li>' +
        '<li class="k-separator"></li>' +
        '<li data-action="delete-row">Delete</li>'+
        '<li data-action="hide-row">Hide</li>'+
        '<li data-action="unhide-row">Unhide</li>'+
    '</ul>';

    var COL_HEADER_CONTEXT_MENU = '<ul class="#=classNames.colHeaderContextMenu#">' +
        '<li data-action=cut>Cut</li>' +
        '<li data-action=copy>Copy</li>' +
        '<li data-action=paste>Paste</li>' +
        '<li class="k-separator"></li>' +
        '<li data-action="delete-column">Delete</li>'+
        '<li data-action="hide-column">Hide</li>'+
        '<li data-action="unhide-column">Unhide</li>'+
    '</ul>';


    kendo.spreadsheet.ContextMenu = kendo.ui.ContextMenu;

    var VIEW_CONTENTS = kendo.template('<div class="#=classNames.view#"><div class="#=classNames.fixedContainer#"></div><div class="#=classNames.scroller#"><div class="#=classNames.viewSize#"></div></div>' +
        '<div tabindex="0" class="#=classNames.clipboard#" contenteditable=true></div><div class="#=classNames.cellEditor#"></div></div><div class="#=classNames.sheetsBar#"></div>' +
        CELL_CONTEXT_MENU + ROW_HEADER_CONTEXT_MENU + COL_HEADER_CONTEXT_MENU
    );

    function within(value, min, max) {
        return value >= min && value <= max;
    }

    var View = kendo.Class.extend({
        init: function(element, options) {
            var classNames = View.classNames;

            this.element = element;

            this.options = $.extend(true, {}, this.options, options);

            this._chrome();

            this._dialogs = [];

            element.append(VIEW_CONTENTS({ classNames: classNames }));

            this._formulaInput();

            this.wrapper =      element.find(DOT + classNames.view);
            this.container =    element.find(DOT + classNames.fixedContainer)[0];
            this.scroller =     element.find(DOT + classNames.scroller)[0];
            this.clipboard =    element.find(DOT + classNames.clipboard);

            this.viewSize = $(this.scroller.firstChild);

            this.tree = new kendo.dom.Tree(this.container);
            this.clipboardContents = new kendo.dom.Tree(this.clipboard[0]);

            this.editor = new kendo.spreadsheet.SheetEditor(this);

            this._sheetsbar();

            var contextMenuConfig = {
                target: element,
                animation: false,
                showOn: "never" // this is just an invalid event name to prevent the show
            };

            this.cellContextMenu = new kendo.spreadsheet.ContextMenu(element.find(DOT + classNames.cellContextMenu), contextMenuConfig);

            this.colHeaderContextMenu = new kendo.spreadsheet.ContextMenu(element.find(DOT + classNames.colHeaderContextMenu), contextMenuConfig);

            this.rowHeaderContextMenu = new kendo.spreadsheet.ContextMenu(element.find(DOT + classNames.rowHeaderContextMenu), contextMenuConfig);

            var scrollbar = kendo.support.scrollbar();

            $(this.container).css({
                width: this.wrapper[0].clientWidth - scrollbar,
                height: this.wrapper[0].clientHeight - scrollbar
            });
        },

        _resize: function() {
            var tabstripHeight = this.tabstrip ? this.tabstrip.element.outerHeight() : 0;
            var formulaBarHeight = this.formulaBar ? this.formulaBar.element.outerHeight() : 0;
            var sheetsBarHeight = this.sheetsbar ? this.sheetsbar.element.outerHeight() : 0;

            this.wrapper.height(
                this.element.height() -
                    (tabstripHeight + formulaBarHeight + sheetsBarHeight)
            );

            if (this.tabstrip) {
                this.tabstrip.quickAccessAdjust();
            }
        },

        _chrome: function() {
            var formulaBar = $("<div />").prependTo(this.element);
            this.formulaBar = new kendo.spreadsheet.FormulaBar(formulaBar);

            if (this.options.toolbar) {
                this._tabstrip();
            }
        },

        _formulaInput: function() {
            var editor = this.element.find(DOT + View.classNames.cellEditor);

            this.formulaInput = new kendo.spreadsheet.FormulaInput(editor, {
                autoScale: true
            });
        },

        _sheetsbar: function() {
            if (this.options.sheetsbar) {
                this.sheetsbar = new kendo.spreadsheet.SheetsBar(this.element.find(DOT + View.classNames.sheetsBar), $.extend(true, {}, this.options.sheetsbar));
            }
        },

        _tabstrip: function() {
            var messages = VIEW_MESAGES.tabs;
            var options = $.extend(true, { home: true, insert: true, data: true }, this.options.toolbar);
            var tabs = [];

            if (this.tabstrip) {
                this.tabstrip.destroy();
                this.element.children(".k-tabstrip").remove();
            }

            for (var name in options) {
                if (options[name] === true || options[name] instanceof Array) {
                    tabs.push({ id: name, text: messages[name], content: "" });
                }
            }

            this.tabstrip = new kendo.spreadsheet.TabStrip($("<div />").prependTo(this.element), {
                animation: false,
                dataTextField: "text",
                dataContentField: "content",
                dataSource: tabs,
                toolbarOptions: options,
                view: this
            });

            this.tabstrip.select(0);
        },

        _executeCommand: function(e) {
            this._workbook.execute(e);
        },

        workbook: function(workbook) {
            this._workbook = workbook;
        },

        sheet: function(sheet) {
            this._sheet = sheet;
        },

        activeCellRectangle: function() {
            return this.cellRectangle(this._sheet._viewActiveCell());
        },

        _rectangle: function(pane, ref) {
            return pane._grid.boundingRectangle(ref.toRangeRef());
        },

        isColumnResizer: function(x, pane, ref) {
            var rectangle = this._rectangle(pane, ref);

            x -= this._sheet._grid._headerWidth;

            var handleWidth = RESIZE_HANDLE_WIDTH/2;
            var right = rectangle.right - this.scroller.scrollLeft;

            return right - handleWidth <= x && x <= right + handleWidth;
        },

        isRowResizer: function(y, pane, ref) {
            var rectangle = this._rectangle(pane, ref);

            y -= this._sheet._grid._headerHeight;

            var handleWidth = RESIZE_HANDLE_WIDTH/2;
            var bottom = rectangle.bottom - this.scroller.scrollTop;

            return bottom - handleWidth <= y && y <= bottom + handleWidth;
        },

        isFilterIcon: function(x, y, pane, ref) {
            var result = false;

            x -= this._sheet._grid._headerWidth;
            y -= this._sheet._grid._headerHeight;

            this._sheet.forEachFilterHeader(ref, function(ref) {
                var rect = this._rectangle(pane, ref);
                result = result || pane.filterIconRect(rect).intersects(x, y);
            }.bind(this));

            return result;
        },

        isAutoFill: function(x, y, pane) {
            var selection = this._sheet.select();

            if (selection.size > 1) {
                return false;
            }

            x -= this._sheet._grid._headerWidth;
            y -= this._sheet._grid._headerHeight;

            if (!pane._grid.columns.frozen) {
                x += this.scroller.scrollLeft;
            }

            if (!pane._grid.rows.frozen) {
                y += this.scroller.scrollTop;
            }

            var rectangle = this._rectangle(pane, selection);

            return Math.abs(rectangle.right - x) < 8 && Math.abs(rectangle.bottom - y) < 8;
        },

        objectAt: function(x, y) {
            var grid = this._sheet._grid;

            var object, pane;

            if (x < 0 || y < 0 || x > this.scroller.clientWidth || y > this.scroller.clientHeight) {
                object = { type: "outside" };
            } else if (x < grid._headerWidth && y < grid._headerHeight) {
                object = { type: "topcorner" };
            } else {
                pane = this.paneAt(x, y);

                var row = pane._grid.rows.index(y, this.scroller.scrollTop);
                var column = pane._grid.columns.index(x, this.scroller.scrollLeft);

                var type = "cell";
                var ref = new CellRef(row, column);

                if (this.isAutoFill(x, y, pane)) {
                    type = "autofill";
                } else if (this.isFilterIcon(x, y, pane, ref)) {
                    type = "filtericon";
                } else if (x < grid._headerWidth) {
                    ref = new CellRef(row, -Infinity);
                    type = this.isRowResizer(y, pane, ref) ? "rowresizehandle" : "rowheader";
                } else if (y < grid._headerHeight) {
                    ref = new CellRef(-Infinity, column);
                    type = this.isColumnResizer(x, pane, ref) ? "columnresizehandle" : "columnheader";
                }

                object = { type: type, ref: ref };
            }

            object.pane = pane;
            object.x = x;
            object.y = y;
            return object;
        },

        paneAt: function(x, y) {
            return this.panes.filter(function paneLocationWithin(pane) {
                var grid = pane._grid;
                return within(y, grid.top, grid.bottom) && within(x, grid.left, grid.right);
            })[0];
        },

        containingPane: function(cell) {
            return this.panes.filter(function(pane) {
                if (pane._grid.contains(cell)) {
                    return true;
                }
                return false;
            })[0];
        },

        cellRectangle: function(cell) {
            var theGrid = this.containingPane(cell)._grid;
            var rectangle = this._sheet._grid.rectangle(cell);

            return rectangle.offset(
                theGrid.headerWidth - this.scroller.scrollLeft,
                theGrid.headerHeight - this.scroller.scrollTop
            );
        },

        refresh: function(reason) {
            var sheet = this._sheet;

            if (this.tabstrip) {
                this.tabstrip.refreshTools(sheet.range(sheet.activeCell()));
            }

            if (reason.sheetSelection && this.sheetsbar) {
                this.sheetsbar.renderSheets(this._workbook.sheets(), this._workbook.sheetIndex(this._sheet));
            }

            this._resize();

            //TODO: refresh sheets list on sheetSelection
            this.viewSize[0].style.height = sheet._grid.totalHeight() + "px";
            this.viewSize[0].style.width = sheet._grid.totalWidth() + "px";

            if (reason.layout) {
                var frozenColumns = sheet.frozenColumns();
                var frozenRows = sheet.frozenRows();

                // main or bottom or right pane
                this.panes = [ this._pane(frozenRows, frozenColumns) ];

                // left pane
                if (frozenColumns > 0) {
                    this.panes.push(this._pane(frozenRows, 0, null, frozenColumns));
                }

                // top pane
                if (frozenRows > 0) {
                    this.panes.push(this._pane(0, frozenColumns, frozenRows, null));
                }

                // left-top "fixed" pane
                if (frozenRows > 0 && frozenColumns > 0) {
                    this.panes.push(this._pane(0, 0, frozenRows, frozenColumns));
                }
            }

            if (reason.filter) {
                this._destroyFilterMenu();
            }

            if (reason.activeCell) {
                this._focus = sheet.activeCell().toRangeRef();
            }
        },

        createFilterMenu: function(column) {
            if (this._filterMenu && this._filterMenu.options.column == column) {
                return this._filterMenu;
            }

            var sheet = this._sheet;
            var ref = sheet.filter().ref;
            var range = new kendo.spreadsheet.Range(ref, sheet);
            var filterMenu = new kendo.spreadsheet.FilterMenu({ column: column, range: range });

            this._destroyFilterMenu();

            this._filterMenu = filterMenu;

            return filterMenu;
        },

        selectClipBoardContents: function() {
                this.clipboard.focus();
                selectElementContents(this.clipboard[0]);
        },

        scrollIntoView: function(cell) {
            var willScroll = false;
            var theGrid = this.containingPane(cell)._grid;

            var boundaries = theGrid.scrollBoundaries(cell);

            var scroller = this.scroller;
            var scrollTop = theGrid.rows.frozen ? 0 : scroller.scrollTop;
            var scrollLeft = theGrid.columns.frozen ? 0 : scroller.scrollLeft;

            if (boundaries.top < scrollTop) {
                willScroll = true;
                scroller.scrollTop = boundaries.scrollTop;
            }

            if (boundaries.bottom > scrollTop) {
                willScroll = true;
                scroller.scrollTop = boundaries.scrollBottom;
            }

            if (boundaries.left < scrollLeft) {
                willScroll = true;
                scroller.scrollLeft = boundaries.scrollLeft;
            }

            if (boundaries.right > scrollLeft) {
                willScroll = true;
                scroller.scrollLeft = boundaries.scrollRight;
            }

            return willScroll;
        },

        openDialog: function(name, options) {
            var sheet = this._sheet;
            var ref = sheet.activeCell();
            var range = new kendo.spreadsheet.Range(ref, sheet);
            var dialog = kendo.spreadsheet.dialogs.create(name, options);

            if (dialog) {
                dialog.bind("action", this._executeCommand.bind(this));
                this._dialogs.push(dialog);
                dialog.open(range);
                return dialog;
            }
        },

        showError: function(options) {
            var errorMessages = VIEW_MESAGES.errors;
            this.openDialog("message", {
                title : options.title || "Error",
                text  : options.type ? errorMessages[options.type] : options.body
            });
        },

        destroy: function() {
            this._dialogs.forEach(function(dialog) {
                dialog.destroy();
            });
            this.cellContextMenu.destroy();
            this.rowHeaderContextMenu.destroy();
            this.colHeaderContextMenu.destroy();

            if (this.tabstrip) {
                this.tabstrip.destroy();
            }

            this._destroyFilterMenu();
        },

        _destroyFilterMenu: function() {
            if (this._filterMenu) {
                this._filterMenu.destroy();
                this._filterMenu = undefined;
                this._filterMenuColumn = undefined;
            }
        },

        render: function() {
            if (!this.element.is(":visible")) {
                return;
            }
            var sheet = this._sheet;
            var focus = sheet.focus();

            if (focus && this.scrollIntoView(focus)) {
                return;
            }

            var grid = sheet._grid;

            var scrollTop = this.scroller.scrollTop;
            var scrollLeft = this.scroller.scrollLeft;

            if (scrollTop < 0) {
                scrollTop = 0;
            }

            if (scrollLeft < 0) {
                scrollLeft = 0;
            }

            var result = this.panes.map(function(pane) {
                return pane.render(scrollLeft, scrollTop);
            }, this);

            var merged = [];
            merged = Array.prototype.concat.apply(merged, result);

            var topCorner = kendo.dom.element("div", {
                style: { width: grid._headerWidth + "px", height: grid._headerHeight + "px" },
                className: View.classNames.topCorner
            });

            merged.push(topCorner);

            if (sheet.resizeHandlePosition() && sheet.resizeHintPosition()) {
                merged.push(this.renderResizeHint());
            }

            this.tree.render(merged);

            if (this.editor.isActive()) {
                this.editor.toggleTooltip(this.activeCellRectangle());
            } else if (!sheet.selectionInProgress() && !sheet.resizingInProgress() && !sheet.isInEditMode()) {
                this.renderClipboardContents();
            }
        },

        renderResizeHint: function() {
            var sheet = this._sheet;
            var ref = sheet.resizeHandlePosition();

            var horizontal = ref.col !== -Infinity;

            var style;
            if (horizontal) {
                style = {
                    height: this.scroller.clientHeight + "px",
                    width: RESIZE_HANDLE_WIDTH + "px",
                    left: sheet.resizeHintPosition().x + "px"
                };
            } else {
                style = {
                    height: RESIZE_HANDLE_WIDTH + "px",
                    width: this.scroller.clientWidth + "px",
                    top: sheet.resizeHintPosition().y + "px"
                };
            }

            var classNames = Pane.classNames;

            return kendo.dom.element("div", {
                className: classNames.resizeHint + (!horizontal ? " " + classNames.resizeHintVertical : ""),
                style: style
            },[
                kendo.dom.element("div", { className: classNames.resizeHintHandle }),
                kendo.dom.element("div", { className: classNames.resizeHintMarker })
            ]);
        },


        renderClipboardContents: function() {
            var sheet = this._sheet;
            var grid = sheet._grid;

            var selection = sheet.select();
            var status = this._workbook.clipboard().canCopy();
            if(status.canCopy === false && status.multiSelection) {
                this.clipboardContents.render([]);
                this.selectClipBoardContents();
                return;
            }

            selection = sheet.trim(selection);

            var table = new HtmlTable();

            var selectionView = grid.rangeDimensions(selection);

            selectionView.rows.forEach(function(height) {
                table.addRow(height);
            });

            selectionView.columns.forEach(function(width) {
                table.addColumn(width);
            });

            var primaryMergedCells = {};
            var secondaryMergedCells = {};

            sheet.forEachMergedCell(selection, function(ref) {
                var topLeft = ref.topLeft;

                grid.forEach(ref, function(cellRef) {
                    if (topLeft.eq(cellRef)) {
                        primaryMergedCells[cellRef.print()] = ref;
                    } else {
                        secondaryMergedCells[cellRef.print()] = true;
                    }
                });
            });

            sheet.forEach(selection, function(row, col, cell) {
                var location = new CellRef(row, col).print();

                if (!secondaryMergedCells[location]) {
                    var td = addCell(table, row - selection.topLeft.row, cell);

                    var mergedCell = primaryMergedCells[location];

                    if (mergedCell) {
                       td.attr.colspan = mergedCell.width();
                       td.attr.rowspan = mergedCell.height();
                    }
                }
            });
            this.clipboardContents.render([ table.toDomTree(0, 0, "kendo-clipboard-" + this._workbook.clipboard()._uid) ]);

            this.selectClipBoardContents();
        },

        _pane: function(row, column, rowCount, columnCount) {
            var pane = new Pane(this._sheet, this._sheet._grid.pane({ row: row, column: column, rowCount: rowCount, columnCount: columnCount }));
            pane.refresh(this.scroller.clientWidth, this.scroller.clientHeight);
            return pane;
        }
    });

    var paneClassNames = {
        rowHeader: "k-spreadsheet-row-header",
        columnHeader: "k-spreadsheet-column-header",
        pane: "k-spreadsheet-pane",
        data: "k-spreadsheet-data",
        mergedCell: "k-spreadsheet-merged-cell",
        mergedCellsWrapper: "k-merged-cells-wrapper",
        activeCell: "k-spreadsheet-active-cell",
        selection: "k-spreadsheet-selection",
        selectionWrapper: "k-selection-wrapper",
        autoFillWrapper: "k-auto-fill-wrapper",
        single: "k-single",
        top: "k-top",
        right: "k-right",
        bottom: "k-bottom",
        left: "k-left",
        resizeHandle: "k-resize-handle",
        resizeHint: "k-resize-hint",
        resizeHintHandle: "k-resize-hint-handle",
        resizeHintMarker: "k-resize-hint-marker",
        resizeHintVertical: "k-resize-hint-vertical",
        selectionHighlight: "k-spreadsheet-selection-highlight",
        series: [
            "k-series-a",
            "k-series-b",
            "k-series-c",
            "k-series-d",
            "k-series-e",
            "k-series-f"
        ]
    };

    var Pane = kendo.Class.extend({
        init: function(sheet, grid) {
            this._sheet = sheet;
            this._grid = grid;
        },

        refresh: function(width, height) {
            this._grid.refresh(width, height);
        },

        isVisible: function(scrollLeft, scrollTop, ref) {
            return this._grid.view(scrollLeft, scrollTop).ref.intersects(ref);
        },

        render: function(scrollLeft, scrollTop) {
            var classNames = Pane.classNames;
            var sheet = this._sheet;
            var grid = this._grid;

            var view = grid.view(scrollLeft, scrollTop);
            this._currentView = view;
            this._selectedHeaders = sheet.selectedHeaders();

            var children = [];

            children.push(this.renderData());

            children.push(this.renderMergedCells());

            children.push(this.renderSelection());

            children.push(this.renderAutoFill());

            children.push(this.renderEditorSelection());

            children.push(this.renderFilterHeaders());

            if (grid.hasRowHeader) {
                var rowHeader = new HtmlTable();
                rowHeader.addColumn(grid.headerWidth);

                view.rows.values.forEach(function(height) {
                    rowHeader.addRow(height);
                });

                sheet.forEach(view.ref.leftColumn(), function(row) {
                    var text = row + 1;
                    rowHeader.addCell(row - view.ref.topLeft.row, text, {}, this.headerClassName(row, "row"));
                }.bind(this));

                children.push(rowHeader.toDomTree(0, view.rowOffset, classNames.rowHeader));
            }

            if (grid.hasColumnHeader) {
                var columnHeader = new HtmlTable();

                view.columns.values.forEach(function(width) {
                    columnHeader.addColumn(width);
                });

                columnHeader.addRow(grid.headerHeight);

                sheet.forEach(view.ref.topRow(), function(row, col) {
                    var text = kendo.spreadsheet.Ref.display(null, Infinity, col);
                    columnHeader.addCell(0, text, {}, this.headerClassName(col, "col"));
                }.bind(this));

                children.push(columnHeader.toDomTree(view.columnOffset, 0, classNames.columnHeader));
            }

            if (sheet.resizeHandlePosition() && (grid.hasColumnHeader || grid.hasRowHeader)) {
                var ref = sheet._grid.normalize(sheet.resizeHandlePosition());

                if (view.ref.intersects(ref)) {
                    if (!sheet.resizeHintPosition()) {
                        children.push(this.renderResizeHandler());
                    }
                }
            }

            var paneClasses = [classNames.pane];

            if (grid.hasColumnHeader) {
                paneClasses.push(classNames.top);
            }

            if (grid.hasRowHeader) {
                paneClasses.push(classNames.left);
            }

            return kendo.dom.element("div", {
                style: grid.style,
                className: paneClasses.join(" ")
            }, children);
        },

        headerClassName: function(index, type) {
            var selectedHeaders = this._selectedHeaders;

            var itemSelection;
            var allHeaders;

            if (type === "row") {
                itemSelection = selectedHeaders.rows[index];
                allHeaders = selectedHeaders.allRows;
            } else {
                itemSelection = selectedHeaders.cols[index];
                allHeaders = selectedHeaders.allCols;
            }

            var className = itemSelection || (selectedHeaders.all ? "full" : (allHeaders ? "partial" : "none"));

            if (className) {
                className = "k-selection-" + className;
            }

            return className;
        },

        renderData: function() {
            var table = new HtmlTable();
            var view = this._currentView;

            view.rows.values.forEach(function(height) {
                table.addRow(height);
            });

            view.columns.values.forEach(function(width) {
                table.addColumn(width);
            });

            this._sheet.forEach(view.ref, function(row, col, cell) {
                addCell(table, row - view.ref.topLeft.row, cell);
            });

            return table.toDomTree(view.columnOffset, view.rowOffset, Pane.classNames.data);
        },

        renderMergedCells: function() {
            var classNames = Pane.classNames;
            var mergedCells = [];
            var sheet = this._sheet;

            sheet.forEachMergedCell(function(ref) {
                this._addTable(mergedCells, ref, classNames.mergedCell);
            }.bind(this));

            return kendo.dom.element("div", { className: classNames.mergedCellsWrapper }, mergedCells);
        },

        renderResizeHandler: function() {
            var sheet = this._sheet;
            var ref = sheet.resizeHandlePosition();
            var rectangle = this._rectangle(ref);

            var style;
            if (ref.col !== -Infinity) {
                style = {
                    height: this._grid.headerHeight + "px",
                    width: RESIZE_HANDLE_WIDTH + "px",
                    left: rectangle.right - RESIZE_HANDLE_WIDTH/2  + "px"
                };
            } else {
                style = {
                    height: RESIZE_HANDLE_WIDTH + "px",
                    width:  this._grid.headerWidth + "px",
                    top: rectangle.bottom - RESIZE_HANDLE_WIDTH/2  + "px"
                };
            }
            return kendo.dom.element("div", {
                className: Pane.classNames.resizeHandle,
                style: style
            });
        },

        filterIconRect: function(rect) {
            var BUTTON_SIZE = 16;
            var BUTTON_OFFSET = 3;

            return new kendo.spreadsheet.Rectangle(
                rect.right - BUTTON_SIZE - BUTTON_OFFSET,
                rect.top + BUTTON_OFFSET,
                BUTTON_SIZE,
                BUTTON_SIZE
            );
        },

        renderFilterHeaders: function() {
            var sheet = this._sheet;
            var children = [];
            var classNames = View.classNames;
            var index = 0;
            var filter = sheet.filter();

            function icon(className) {
                return kendo.dom.element("span", {
                    className: classNames.icon + " " + className
                });
            }

            function filterButton(classNames, position, index) {
                var style = {
                    left: position.left + "px",
                    top: position.top + "px"
                };
                var filtered = filter && filter.columns.some(function(c) {
                    return c.index === index;
                });
                var classes = classNames.filterButton;

                if (filtered) {
                    classes += " " + classNames.filterButtonActive;
                }

                var button = kendo.dom.element(
                    "span",
                    { className: classes, style: style, "data-column": index },
                    [ icon(classNames.iconFilterDefault) ]
                );

                return button;
            }

            if (filter) {
                this._addDiv(children, filter.ref, classNames.filterRange);
            }

            sheet.forEachFilterHeader(this._currentView.ref, function(ref) {
                var rect = this._rectangle(ref);
                var position = this.filterIconRect(rect);
                var button = filterButton(classNames, position, index);
                index++;

                children.push(button);
            }.bind(this));

            return kendo.dom.element("div", {
                className: classNames.filterHeadersWrapper
            }, children);

        },

        renderEditorSelection: function() {
            var classNames = Pane.classNames;
            var sheet = this._sheet;
            var selections = [];

            sheet._formulaSelections.forEach(function(range) {
                var ref = range.ref;

                if (ref === kendo.spreadsheet.NULLREF) {
                    return;
                }

                this._addDiv(selections, ref, classNames.selectionHighlight + " " + range.colorClass);
            }.bind(this));

            return kendo.dom.element("div", { className: classNames.selectionWrapper }, selections);

        },

        renderSelection: function() {
            var classNames = Pane.classNames;
            var selections = [];
            var activeCellClasses = [classNames.activeCell];
            var selectionClasses = [classNames.selection];
            var sheet = this._sheet;
            var activeCell = sheet.activeCell().toRangeRef();
            var activeFormulaColor = this._activeFormulaColor();
            var selection = sheet.select();

            activeCellClasses = activeCellClasses.concat(activeFormulaColor, this._directionClasses(activeCell));
            selectionClasses = selectionClasses.concat(activeFormulaColor);

            if (sheet.singleCellSelection()) {
                activeCellClasses.push(classNames.single);
            }

            if (selection.size() === 1) {
                selectionClasses.push("k-single-selection");
            }

            if (this._sheet.autoFillPunch()) {
               selectionClasses.push("k-dim-auto-fill-handle");
            }

            selection.forEach(function(ref) {
                if (ref !== kendo.spreadsheet.NULLREF) {
                    this._addDiv(selections, ref, selectionClasses.join(" "));
                }
            }.bind(this));

            this._addTable(selections, activeCell, activeCellClasses.join(" "));

            return kendo.dom.element("div", { className: classNames.selectionWrapper }, selections);
        },

        renderAutoFill: function() {
            var autoFillRectangle = [];

            if (this._sheet.autoFillInProgress()) {
                var autoFillRef = this._sheet.autoFillRef();
                var punch = this._sheet.autoFillPunch();
                var direction = this._sheet._autoFillDirection;

                this._addDiv(autoFillRectangle, autoFillRef, "k-auto-fill");

                if (punch) { // collapsing, add overlay
                    this._addDiv(autoFillRectangle, punch, "k-auto-fill-punch");
                } else if (direction !== undefined) { // expanding - add hint
                    var ref, cssClass;

                    switch(direction) {
                        case 0:
                            ref = autoFillRef.bottomRight;
                            cssClass = "k-auto-fill-br-hint";
                            break;
                        case 1:
                            ref = autoFillRef.bottomRight;
                            cssClass = "k-auto-fill-br-hint";
                            break;
                        case 2:
                            ref = new CellRef(autoFillRef.topLeft.row, autoFillRef.bottomRight.col);
                            cssClass = "k-auto-fill-tr-hint";
                            break;
                        case 3:
                            ref = new CellRef(autoFillRef.bottomRight.row, autoFillRef.topLeft.col);
                            cssClass = "k-auto-fill-bl-hint";
                            break;
                    }

                    var hint = kendo.dom.element("span", { className: "k-tooltip" }, [ kendo.dom.text(this._sheet._autoFillHint) ]);

                    var rectangle = this._addDiv(autoFillRectangle, ref, cssClass);
                    if (rectangle) {
                        rectangle.children.push(hint);
                    }
                }
            }

            return kendo.dom.element("div", { className: Pane.classNames.autoFillWrapper }, autoFillRectangle);
        },

        _addDiv: function(collection, ref, className) {
            var view = this._currentView, div;

            if (view.ref.intersects(ref)) {
                div = this._rectangle(ref).resize(1, 1).toDiv(className);
                collection.push(div);
            }
            return div;
        },

        _addTable: function(collection, ref, className) {
            var sheet = this._sheet;
            var view = this._currentView;

            if (view.ref.intersects(ref)) {
                sheet.forEach(ref.collapse(), function(row, col, cell) {
                    var rectangle = this._rectangle(ref);

                    var table = new HtmlTable();
                    table.addColumn(rectangle.width);
                    table.addRow(rectangle.height);
                    addCell(table, 0, cell);

                    collection.push(table.toDomTree(rectangle.left, rectangle.top, className));
                }.bind(this));
            }
        },

        _activeFormulaColor: function() {
            var activeFormulaSelection;
            var colorClasses = [];

            if (this._sheet.isInEditMode()) {
                activeFormulaSelection = this._sheet._formulaSelections.filter(function(sel) { return sel.active && sel.type == "ref"; })[0];

                if (activeFormulaSelection) {
                    colorClasses.push(activeFormulaSelection.colorClass);
                }
            }

            return colorClasses;
        },

        _directionClasses: function(cell) {
            var cellClasses = [];
            var classNames = Pane.classNames;
            var view = this._currentView.ref;

            if (!cell.move(0, -1).intersects(view)) {
                cellClasses.push(classNames.left);
            }

            if (!cell.move(-1, 0).intersects(view)) {
                cellClasses.push(classNames.top);
            }

            if (!cell.move(0, 1).intersects(view)) {
                cellClasses.push(classNames.right);
            }

            if (!cell.move(1, 0).intersects(view)) {
                cellClasses.push(classNames.bottom);
            }

            return cellClasses;
        },

        _rectangle: function(ref) {
            return this._grid.boundingRectangle(ref.toRangeRef()).offset(-this._currentView.mergedCellLeft, -this._currentView.mergedCellTop);
        }
    });

    kendo.spreadsheet.View = View;
    kendo.spreadsheet.Pane = Pane;
    kendo.spreadsheet.addCell = addCell;

    $.extend(true, View, { classNames: viewClassNames });
    $.extend(true, Pane, { classNames: paneClassNames });

})(window.kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var CellRef = kendo.spreadsheet.CellRef;
    var RangeRef = kendo.spreadsheet.RangeRef;
    var UnionRef = kendo.spreadsheet.UnionRef;

    var Rectangle = kendo.Class.extend({
        init: function(left, top, width, height) {
            this.left = left;
            this.width = width;
            this.right = left + width;

            this.top = top;
            this.height = height;
            this.bottom = top + height;
        },

        offset: function(left, top) {
            return new Rectangle(this.left + left, this.top + top, this.width, this.height);
        },

        resize: function(width, height) {
            return new Rectangle(this.left, this.top, this.width + width, this.height + height);
        },

        intersects: function(x, y) {
            return this.left < x && x < this.left + this.width &&
                   this.top < y && y < this.top + this.height;
        },

        toDiv: function(className) {
            return kendo.dom.element("div", {
                className: className,
                style: {
                    width:  this.width + "px",
                    height: this.height + "px",
                    top:    this.top + "px",
                    left:   this.left + "px"
                }
            });
        }
    });

    var Grid = kendo.Class.extend({
        init: function(rows, columns, rowCount, columnCount, headerHeight, headerWidth) {
            this.rowCount = rowCount;
            this.columnCount = columnCount;
            this._columns = columns;
            this._rows = rows;
            this._headerHeight = headerHeight;
            this._headerWidth = headerWidth;
        },

        isAxis: function(ref) {
            ref = ref.toRangeRef();
            var topLeft = ref.topLeft;
            var bottomRight = ref.bottomRight;
            return (topLeft.row === 0 && bottomRight.row === this.rowCount - 1) || (topLeft.col === 0 && bottomRight.col === this.columnCount - 1);
        },

        width: function(start, end) {
            return this._columns.sum(start, end);
        },

        height: function(start, end) {
            return this._rows.sum(start, end);
        },

        totalHeight: function() {
            return this._rows.total + this._headerHeight;
        },

        totalWidth: function() {
            return this._columns.total + this._headerWidth;
        },

        index: function(row, column) {
            return column * this.rowCount + row;
        },

        cellRef: function(index) {
            return new CellRef(index % this.rowCount, (index / this.rowCount) >> 0);
        },

        rowRef: function(row) {
            return new RangeRef(new CellRef(row, 0), new CellRef(row, this.columnCount - 1));
        },

        colRef: function(col) {
            return new RangeRef(new CellRef(0, col), new CellRef(this.rowCount - 1, col));
        },

        cellRefIndex: function(ref) {
            return this.index(ref.row, ref.col);
        },

        normalize: function(ref) {
            if (ref instanceof RangeRef) {
                return new RangeRef(
                    this.normalize(ref.topLeft),
                    this.normalize(ref.bottomRight)
                ).setSheet(ref.sheet, ref.hasSheet());
            }

            if (ref instanceof UnionRef) {
                return ref.map(function(ref) {
                    return this.normalize(ref);
                }, this);
            }

            var clone = ref.clone();
            clone.col = Math.max(0, Math.min(this.columnCount - 1, ref.col));
            clone.row = Math.max(0, Math.min(this.rowCount - 1, ref.row));

            return clone;
        },

        rectangle: function(ref) {
            var topLeft = this.normalize(ref.topLeft);
            var bottomRight = this.normalize(ref.bottomRight);
            return new Rectangle(
                this.width(0, topLeft.col - 1),
                this.height(0, topLeft.row - 1),
                this.width(topLeft.col, bottomRight.col),
                this.height(topLeft.row, bottomRight.row)
            );
        },

        pane: function(options) {
            return new PaneGrid(
                new kendo.spreadsheet.PaneAxis(this._rows, options.row, options.rowCount, this._headerHeight),
                new kendo.spreadsheet.PaneAxis(this._columns, options.column, options.columnCount, this._headerWidth),
                this
            );
        },

        rangeDimensions: function(rangeRef) {
            return {
                rows: this._rows.values.iterator(rangeRef.topLeft.row, rangeRef.bottomRight.row),
                columns: this._columns.values.iterator(rangeRef.topLeft.col, rangeRef.bottomRight.col)
            };
        },

        forEach: function(ref, callback) {
            var topLeft = this.normalize(ref.topLeft);
            var bottomRight = this.normalize(ref.bottomRight);

            for (var ci = topLeft.col; ci <= bottomRight.col; ci ++) {
                for (var ri = topLeft.row; ri <= bottomRight.row; ri ++) {
                    callback(new CellRef(ri, ci));
                }
            }
        },

        trim: function(ref, property) {
            var topLeft = ref.topLeft;
            var bottomRight = ref.bottomRight;
            var bottomRightRow = topLeft.row;
            var bottomRightCol = topLeft.col;

            for (var ci = topLeft.col; ci <= bottomRight.col; ci ++) {
                var start = this.index(topLeft.row, ci);
                var end = this.index(bottomRight.row, ci);
                var values = property.tree.intersecting(start, end);
                if(values.length) {
                    var cell = this.cellRef(values[values.length - 1].end);
                    bottomRightRow = Math.max(bottomRightRow, cell.row);
                    bottomRightCol = ci;
                }
            }
            return new RangeRef(ref.topLeft, new CellRef(Math.min(bottomRightRow, ref.bottomRight.row), bottomRightCol));
        }
    });

    var PaneGrid = kendo.Class.extend({
        init: function(rows, columns, grid) {
            this.rows = rows;
            this.columns = columns;
            this._grid = grid;

            this.headerHeight = rows.headerSize;
            this.headerWidth = columns.headerSize;
            this.hasRowHeader = columns.hasHeader;
            this.hasColumnHeader = rows.hasHeader;
        },

        refresh: function(width, height) {
            this.columns.viewSize(width);
            this.rows.viewSize(height);

            var x = this.columns.paneSegment();
            var y = this.rows.paneSegment();

            this.left = x.offset;
            this.top = y.offset;
            this.right = x.offset + x.length;
            this.bottom = y.offset + y.length;

            this.style = {
                top: y.offset  + "px",
                left: x.offset + "px",
                height: y.length + "px",
                width: x.length + "px"
            };
        },

        view: function(left, top) {
            var rows = this.rows.visible(top);
            var columns = this.columns.visible(left);

            return {
                rows: rows,
                columns: columns,

                rowOffset: rows.offset,
                columnOffset: columns.offset,

                mergedCellLeft: columns.start,
                mergedCellTop: rows.start,

                ref: new RangeRef(
                    new CellRef(rows.values.start, columns.values.start),
                    new CellRef(rows.values.end, columns.values.end)
                )
            };
        },

        contains: function(ref) {
            return this.rows.contains(ref.topLeft.row, ref.bottomRight.row) &&
                this.columns.contains(ref.topLeft.col, ref.bottomRight.col);
        },

        index: function(row, column) {
            return this._grid.index(row, column);
        },

        boundingRectangle: function(ref) {
            return this._grid.rectangle(ref);
        },

        cellRefIndex: function(ref) {
            return this._grid.cellRefIndex(ref);
        },

        scrollBoundaries: function(cell) {
            var position = this.boundingRectangle(cell);

            var boundaries = {
                top: Math.max(0, position.top - this.top + (this.hasColumnHeader ? 0 : this.headerHeight)),
                left: Math.max(0, position.left - this.left + (this.hasRowHeader ? 0 : this.headerWidth)),
                right: position.right - this.columns._viewSize + this.headerWidth,
                bottom: position.bottom - this.rows._viewSize + this.headerHeight
            };

            var widthCompensation = this.columns.defaultValue / 2;
            var heightCompensation = this.rows.defaultValue / 2;

            boundaries.scrollTop = boundaries.top - heightCompensation;
            boundaries.scrollBottom = boundaries.bottom + heightCompensation;

            boundaries.scrollLeft = boundaries.left - widthCompensation;
            boundaries.scrollRight = boundaries.right + widthCompensation;
            return boundaries;
        }
    });

    kendo.spreadsheet.Grid = Grid;
    kendo.spreadsheet.PaneGrid = PaneGrid;
    kendo.spreadsheet.Rectangle = Rectangle;
})(kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var Axis = kendo.Class.extend({
        init: function(count, value) {
            this._value = value;
            this._count = count;
            this.values = new kendo.spreadsheet.RangeList(0, count - 1, value);
            this._hidden = new kendo.spreadsheet.RangeList(0, count - 1, 0);

            this.scrollBarSize = kendo.support.scrollbar();
            this._refresh();
        },

        toJSON: function(field, positions) {
            var values = [];

            var iterator = this.values.iterator(0, this._count - 1);

            for (var idx = 0; idx < this._count; idx++) {
                var value = iterator.at(idx);

                if (value === this._value) {
                    continue;
                }

                var position = positions[idx];

                if (position === undefined) {
                    position = values.length;

                    var item = { index: idx };

                    item[field] = value;

                    values.push(item);

                    positions[idx] = position;
                }
            }

            return values;
        },

        fromJSON: function(field, values) {
            for (var idx = 0; idx < values.length; idx++) {
                var value = values[idx][field];
                var index = values[idx].index;

                if (index === undefined) {
                    index = idx;
                }

                this.value(index, index, value);
            }
        },

        hide: function(index) {
            var value = this.value(index, index);
            this._hidden.value(index, index, value);
            this.value(index, index, 0);
        },

        hidden: function(index) {
            return this._hidden.value(index, index) !== 0;
        },

        includesHidden: function(start, end) {
             return this._hidden.intersecting(start, end).length > 1;
        },

        nextVisible: function(index, overflow) {
            var end = this._count - 1;

            if (index === end) {
                return overflow ? index + 1 : index;
            }

            index += 1;
            var range = this._hidden.intersecting(index, index)[0];
            if (range.value !== 0) { // hidden
                if (range.end === end) { // reached the last visible
                    return index - 1;
                } else {
                    return range.end + 1;
                }
            } else {
                return index;
            }
        },

        nextPage: function(index, pageSize) {
            return this.index(this.sum(0, index - 1) + pageSize);
        },

        prevPage: function(index, pageSize) {
            return this.index(this.sum(0, index) - pageSize);
        },

        firstVisible: function() {
            var firstHidden = this._hidden.first();
            if (firstHidden.value === 0) {
                return 0;
            } else {
                return firstHidden.end + 1;
            }
        },

        lastVisible: function() {
            var lastHidden = this._hidden.last();
            if (lastHidden.value === 0) {
                return this._count - 1;
            } else {
                return lastHidden.start - 1;
            }
        },

        prevVisible: function(index, overflow) {
            if (index === 0) {
                return overflow ? -1 : 0;
            }

            index -= 1;
            var range = this._hidden.intersecting(index, index)[0];
            if (range.value !== 0) { // hidden
                if (range.start === 0) { // reached the last visible
                    return index + 1;
                } else {
                    return range.start - 1;
                }
            } else {
                return index;
            }
        },

        unhide: function(index) {
            if (this.hidden(index)) {
                var value = this._hidden.value(index, index);
                this._hidden.value(index, index, 0);
                this.value(index, index, value);
            }
        },

        value: function(start, end, value) {
            if (value !== undefined) {
                this.values.value(start, end, value);
                this._refresh();
            } else {
                return this.values.iterator(start, end).at(0);
            }
        },

        sum: function(start, end) {
            var values = this.values.iterator(start, end);

            var sum = 0;

            for (var idx = start; idx <= end; idx ++) {
                sum += values.at(idx);
            }

            return sum;
        },

        visible: function(start, end) {
            var startSegment = null;
            var endSegment = null;
            var lastPage = false;

            if (end >= this.total + this.scrollBarSize) {
                lastPage = true;
            }

            var ranges = this._pixelValues.intersecting(start, end);

            startSegment = ranges[0];
            endSegment = ranges[ranges.length - 1];

            var startOffset = start - startSegment.start;

            var startIndex = ((startOffset / startSegment.value.value) >> 0) + startSegment.value.start;

            var offset = startOffset - (startIndex - startSegment.value.start) * startSegment.value.value;

            var endOffset = end - endSegment.start;
            var endIndex = ((endOffset / endSegment.value.value) >> 0) + endSegment.value.start;

            if (endIndex > endSegment.value.end) {
                endIndex = endSegment.value.end;
            }

            if (lastPage) {
                offset += endSegment.value.value - (endOffset - (endIndex - endSegment.value.start) * endSegment.value.value);
            }

            offset = Math.min(-offset, 0);

            return {
                values: this.values.iterator(startIndex, endIndex),
                offset: offset
            };
        },

        index: function(value) {
            var index = 0;
            var iterator = this.values.iterator(0, this._count - 1);
            var current = iterator.at(0);

            while (current < value && index < this._count - 1) {
                current += iterator.at(++index);
            }

            return index;
        },

        _refresh: function() {
            var current = 0;
            this._pixelValues = this.values.map(function(range) {
                var start = current;

                current += (range.end - range.start + 1) * range.value;

                var end = current - 1;

                return new kendo.spreadsheet.ValueRange(start, end, range);
            });

            this.total = current;
        },

        getState: function() {
            return {
                values: this.values.getState(),
                hidden: this._hidden.getState()
            };
        },

        setState: function(state) {
            this.values.setState(state.values);
            this._hidden.setState(state.hidden);
            this._refresh();
        }
    });

    var PaneAxis = kendo.Class.extend({
        init: function(axis, start, count, headerSize) {
           this._axis = axis;
           this._start = start;
           this._count = count;
           this.hasHeader = start === 0;
           this.headerSize = headerSize;
           this.defaultValue = axis._value;
           this.frozen = count > 0;
        },

        viewSize: function(viewSize) {
            this._viewSize = viewSize;
        },

        sum: function(start, end) {
            return this._axis.sum(start, end - 1);
        },

        start: function() {
            return this.sum(0, this._start);
        },

        size: function() {
            return this.sum(this._start, this._start + this._count);
        },

        index: function(value, offset) {
            return this._axis.index(value + (this.frozen ? 0 : offset) - this.headerSize);
        },

        //XXX: rename this method
        paneSegment: function() {
            var offset = this.start();
            var length;

            if (!this.hasHeader) {
                offset += this.headerSize;
            }

            if (this.frozen) {
                length = this.size();
                if (this.hasHeader) {
                    length += this.headerSize;
                } else {
                    length -= this.headerSize;
                }
            } else {
                length = this._viewSize - offset;
            }

            return {
                offset: offset,
                length: length
            };
        },

        visible: function(offset) {
            var start = this.start();
            var size;

            if (this.frozen) {
                size = this.size();
                if (!this.hasHeader) {
                    size -= this.headerSize;
                }
            } else {
                size = this._viewSize - start - this.headerSize;
                start += offset;
            }

            var result = this._axis.visible(start, start + size - 1);

            if (this.frozen) {
                result.offset = 0;
            }

            result.start = start;

            if (this.hasHeader) {
                result.offset += this.headerSize;
                result.start -= this.headerSize;
            }

            return result;
        },

        contains: function(start, end) {
            if (this.frozen) {
                if (start > this._start + this._count) {
                    return false;
                }
                if (end < this._start) {
                    return false;
                }
                return true;
            } else {
                return end >= this._start;
            }
        }
    });

    kendo.spreadsheet.Axis = Axis;
    kendo.spreadsheet.PaneAxis = PaneAxis;

})(kendo);
})();

(function(){
(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    /*jshint evil: true */
    var Filter = kendo.spreadsheet.Filter = kendo.Class.extend({
        prepare: function() {
        },
        value: function(cell) {
            return cell.value;
        },
        matches: function() {
            throw new Error("The 'matches' method is not implemented.");
        },
        toJSON: function() {
            throw new Error("The 'toJSON' method is not implemented.");
        }
    });

    Filter.create = function(options) {
        var filter = options.filter;

        if (!filter) {
            throw new Error("Filter type not specified.");
        }

        var constructor = kendo.spreadsheet[filter.charAt(0).toUpperCase() + filter.substring(1) + "Filter"];

        if (!constructor) {
            throw new Error("Filter type not recognized.");
        }

        return new constructor(options);
    };

    kendo.spreadsheet.ValueFilter = Filter.extend({
        _values: [],

        _dates: [],

        _blanks: false,

        init: function(options) {
            if (options.values !== undefined) {
                this._values = options.values;
            }

            if (options.blanks !== undefined) {
                this._blanks = options.blanks;
            }

            if (options.dates !== undefined) {
                this._dates = options.dates;
            }
        },

        value: function(cell) {
            var value = cell.value;

            if (this._dates.length > 0 && cell.format && typeof value === "number") {
                var type = kendo.spreadsheet.formatting.type(value, cell.format);

                if (type === "date") {
                    value = kendo.spreadsheet.numberToDate(value);
                }
            }

            return value;
        },

        matches: function(value) {
            if (value === null || value === undefined) {
                return this._blanks;
            }

            if (value instanceof Date) {
                return this._dates.some(function(date) {
                    return date.year === value.getFullYear() &&
                        (date.month === undefined || date.month === value.getMonth()) &&
                        (date.day === undefined || date.day === value.getDate()) &&
                        (date.hours === undefined || date.hours === value.getHours()) &&
                        (date.minutes === undefined || date.minutes === value.getMinutes()) &&
                        (date.seconds === undefined || date.seconds === value.getSeconds());
                });
            }

            return this._values.indexOf(value) >= 0;
        },
        toJSON: function() {
            return {
                filter: "value",
                values: this._values.slice(0)
            };
        }
    });

    kendo.spreadsheet.CustomFilter = Filter.extend({
        _logic: "and",
        init: function(options) {
            if (options.logic !== undefined) {
                this._logic = options.logic;
            }

            if (options.criteria === undefined) {
                throw new Error("Must specify criteria.");
            }

            this._criteria = options.criteria;

            var expression = kendo.data.Query.filterExpr({
                logic: this._logic,
                filters: this._criteria
            }).expression;

            this._matches = new Function("d", "return " + expression);
        },
        matches: function(value) {
            if (value === null) {
                return false;
            }

            return this._matches(value);
        },
        value: function(cell) {
            var value = cell.value;
            var criterionType = this._criteria[0].type || typeof this._criteria[0].value;
            var valueType = typeof value;
            var dom;

            if (cell.format) {
                valueType = kendo.spreadsheet.formatting.type(value, cell.format);
            }

            if (valueType != criterionType) {
                if (criterionType == "string") {
                    if (cell.format) {
                        dom = kendo.spreadsheet.formatting.format(value, cell.format);
                        value = dom.children[0].nodeValue;
                    }
                    value = value + "";
                }
            } else if (valueType == "date") {
                value = kendo.spreadsheet.numberToDate(value);
            }

            return value;
        },
        toJSON: function() {
            return {
                filter: "custom",
                logic: this._logic,
                criteria: this._criteria
            };
        }
    });

    kendo.spreadsheet.TopFilter = Filter.extend({
        init: function(options) {
            this._type = options.type;
            this._value = options.value;
            this._values = [];
        },

        prepare: function(cells) {
            var values = cells.map(this.value)
            .sort()
            .filter(function(value, index, array) {
                return index === 0 || value !== array[index - 1];
            });

            if (this._type === "topNumber" || this._type == "topPercent") {
                values.sort(function(x, y) {
                    return y - x;
                });
            } else {
                values.sort(function(x, y) {
                    return x - y;
                });
            }

            var count = this._value;

            if (this._type === "topPercent" || this._type === "bottomPercent") {
                count = (values.length * count / 100) >> 0;
            }

            this._values = values.slice(0, count);
        },
        matches: function(value) {
            return this._values.indexOf(value) >= 0;
        },
        toJSON: function() {
            return {
                filter: "top",
                type: this._type,
                value: this._value
            };
        }
    });

    kendo.spreadsheet.DynamicFilter = Filter.extend({
        init: function(options) {
            this._type = options.type;

            this._predicate = this[options.type];

            if (typeof this._predicate !== "function") {
                throw new Error("DynamicFilter type '" + options.type + "' not recognized.");
            }
        },
        value: function(cell) {
            var value = cell.value;

            if (cell.format) {
                var type = kendo.spreadsheet.formatting.type(value, cell.format);

                if (type === "date") {
                    value = kendo.spreadsheet.numberToDate(value);
                }
            }

            return value;
        },
        prepare: function(cells) {
            var sum = 0;
            var count = 0;

            for (var ci = 0; ci < cells.length; ci++) {
                var value = this.value(cells[ci]);

                if (typeof value === "number") {
                    sum += value;
                    count ++;
                }
            }

            if (count > 0) {
                this._average = sum / count;
            } else {
                this._average = 0;
            }
        },
        matches: function(value) {
            return this._predicate(value);
        },
        aboveAverage: function(value) {
            if (value instanceof Date) {
                value = kendo.spreadsheet.dateToNumber(value);
            }

            if (typeof value !== "number") {
                return false;
            }

            return value > this._average;
        },
        belowAverage: function(value) {
            if (value instanceof Date) {
                value = kendo.spreadsheet.dateToNumber(value);
            }

            if (typeof value !== "number") {
                return false;
            }

            return value < this._average;
        },
        tomorrow: function(value) {
            if (value instanceof Date) {
                var tomorrow = kendo.date.addDays(kendo.date.today(), 1);

                return kendo.date.getDate(value).getTime() === tomorrow.getTime();
            }

            return false;
        },
        today: function(value) {
            if (value instanceof Date) {
                return kendo.date.isToday(value);
            }

            return false;
        },
        yesterday: function(value) {
            if (value instanceof Date) {
                var yesterday = kendo.date.addDays(kendo.date.today(), -1);

                return kendo.date.getDate(value).getTime() === yesterday.getTime();
            }

            return false;
        },
        nextWeek: function(value) {
            return sameWeek(kendo.date.addDays(kendo.date.today(), 7), value);
        },
        thisWeek: function(value) {
            return sameWeek(kendo.date.today(), value);
        },
        lastWeek: function(value) {
            return sameWeek(kendo.date.addDays(kendo.date.today(), -7), value);
        },
        nextMonth: function(value) {
            return sameMonth(value, 1);
        },
        thisMonth: function(value) {
            return sameMonth(value, 0);
        },
        lastMonth: function(value) {
            return sameMonth(value, -1);
        },
        nextQuarter: function(value) {
            if (value instanceof Date) {
                var today = kendo.date.today();

                var diff = quarter(value) - quarter(today);

                return (diff === 1 && today.getFullYear() === value.getFullYear()) ||
                       (diff == -3 && today.getFullYear() + 1 === value.getFullYear());
            }

            return false;
        },
        thisQuarter: function(value) {
            if (value instanceof Date) {
                var today = kendo.date.today();

                var diff = quarter(value) - quarter(today);

                return diff === 0 && today.getFullYear() === value.getFullYear();
            }

            return false;
        },
        lastQuarter: function(value) {
            if (value instanceof Date) {
                var today = kendo.date.today();

                var diff = quarter(today) - quarter(value);

                return (diff === 1 && today.getFullYear() === value.getFullYear()) ||
                       (diff == -3 && today.getFullYear() - 1 === value.getFullYear());
            }

            return false;
        },
        nextYear: function(value) {
            return sameYear(value, 1);
        },
        thisYear: function(value) {
            return sameYear(value, 0);
        },
        lastYear: function(value) {
            return sameYear(value, -1);
        },
        yearToDate: function(value) {
            if (value instanceof Date) {
                var today = kendo.date.today();

                return value.getFullYear() === today.getFullYear() && value <= today;
            }

            return false;
        },
        toJSON: function() {
            return {
                filter: "dynamic",
                type: this._type
            };
        }
    });

    [1, 2, 3, 4].forEach(function(target) {
        kendo.spreadsheet.DynamicFilter.prototype["quarter" + target] = function(value) {
           if (value instanceof Date) {
              return quarter(value) === target;
           }

           return false;
        };
    });

    kendo.cultures["en-US"].calendar.months.names.forEach(function(month, index) {
        kendo.spreadsheet.DynamicFilter.prototype[month.toLowerCase()] = function(value) {
           if (value instanceof Date) {
               return value.getMonth() === index;
           }

           return false;
        };
    });

    function quarter(value) {
        var month = value.getMonth() + 1;

        if (month >= 1 && month <= 3) {
            return 1;
        } else if(month >= 4 && month <= 6) {
            return 2;
        } else if (month >= 7 && month <= 9) {
            return 3;
        } else {
            return 4;
        }
    }

    function sameYear(value, offset) {
        if (value instanceof Date) {
            var today = kendo.date.today();

            today.setFullYear(today.getFullYear() + offset);

            return today.getFullYear() === value.getFullYear();
        }

        return false;
    }

    function sameMonth(value, offset) {
        if (value instanceof Date) {
            var today = kendo.date.firstDayOfMonth(kendo.date.today());

            today.setMonth(today.getMonth() + offset, 1);

            return today.getTime() === kendo.date.firstDayOfMonth(value).getTime();
        }

        return false;
    }

    function sameWeek(a, b) {
        if (b instanceof Date) {
            var firstWeek = kendo.date.dayOfWeek(kendo.date.getDate(a), 1);

            var secondWeek = kendo.date.dayOfWeek(kendo.date.getDate(b), 1);

            return firstWeek.getTime() === secondWeek.getTime();
        }

        return false;
    }

})(kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var Sorter = kendo.Class.extend({
        init: function(grid, lists) {
            this._grid = grid;
            this._lists = lists;
        },

        indices: function(rangeRef, list, ascending, indices) {
            var comparer = Sorter.ascendingComparer;

            if (ascending === false) {
                comparer = Sorter.descendingComparer;
            }

            return list.sortedIndices(this._grid.cellRefIndex(rangeRef.topLeft),
                this._grid.cellRefIndex(rangeRef.bottomRight), comparer, indices);
        },

        sortBy: function(ref, column, list, ascending, indices) {
            var sortedIndices = this.indices(ref.toColumn(column), list, ascending, indices);

            for (var ci = ref.topLeft.col; ci <= ref.bottomRight.col; ci++) {
                var start = this._grid.index(ref.topLeft.row, ci);
                var end = this._grid.index(ref.bottomRight.row, ci);

                for (var li = 0; li < this._lists.length; li++) {
                    if (start < this._lists[li].lastRangeStart()) {
                        this._lists[li].sort(start, end, sortedIndices);
                    }
                }
            }

            return sortedIndices;
        }
    });

    Sorter.ascendingComparer = function(a, b) {
        if (a === null && b === null) {
            return 0;
        }

        if (a === null) {
            return 1;
        }

        if (b === null) {
            return -1;
        }

        var typeA = typeof a;
        var typeB = typeof b;

        if (typeA === "number") {
            if (typeB === "number") {
                return a - b;
            } else {
               return -1;
            }
        }

        if (typeA === "string") {
            switch (typeB) {
                case "number":
                    return 1;
                case "string":
                    return a.localeCompare(b);
                default:
                    return -1;
            }
        }

        if (typeA === "boolean") {
            switch (typeB) {
                case "number":
                    return 1;
                case "string":
                    return 1;
                case "boolean":
                    return a - b;
                default:
                    return -1;
            }
        }

        if (a instanceof kendo.spreadsheet.calc.runtime.CalcError) {
            if (b instanceof kendo.spreadsheet.calc.runtime.CalcError) {
                return 0;
            } else {
                return 1;
            }
        }

        throw new Error("Cannot compare " + a + " and " + b);
    };

    Sorter.descendingComparer = function(a, b) {
        if (a === null && b === null) {
            return 0;
        }

        if (a === null) {
            return 1;
        }

        if (b === null) {
            return -1;
        }

        return Sorter.ascendingComparer(b, a);
    };

    kendo.spreadsheet.Sorter = Sorter;
})(kendo);
})();

(function(){
    "use strict";

    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    // WARNING: removing the following jshint declaration and turning
    // == into === to make JSHint happy will break functionality.
    /* jshint eqnull:true, newcap:false, laxbreak:true, shadow:true, -W054 */
    /* jshint latedef: nofunc */

    var spreadsheet = kendo.spreadsheet;
    var exports = spreadsheet.calc;
    var runtime = exports.runtime;

    // Excel formula parser and compiler to JS.
    // some code adapted from http://lisperator.net/pltut/

    var OPERATORS = Object.create(null);

    var ParseError = kendo.Class.extend({
        init: function ParseError(message, pos) {
            this.message = message;
            this.pos = pos;
        },
        toString: function() {
            return this.message;
        }
    });

    (function(ops){
        ops.forEach(function(cls, i){
            cls.forEach(function(op){
                OPERATORS[op] = ops.length - i;
            });
        });
    })([
        [ ":" ],
        [ " " ],
        [ "," ],
        [ "%" ],
        [ "^" ],
        [ "*", "/" ],
        [ "+", "-" ],
        [ "&" ],
        [ "=", "<", ">", "<=", ">=", "<>" ]
    ]);

    var TRUE = { type: "bool", value: true };
    var FALSE = { type: "bool", value: false };

    function getcol(str) {
        str = str.toUpperCase();
        for (var col = 0, i = 0; i < str.length; ++i) {
            col = col * 26 + str.charCodeAt(i) - 64;
        }
        return col - 1;
    }

    function getrow(str) {
        return parseFloat(str) - 1;
    }

    // XXX: rewrite this with the TokenStream.
    function parseReference(name, noThrow) {
        if (name.toLowerCase() == "#sheet") {
            return spreadsheet.SHEETREF;
        }
        if (name.indexOf(",") >= 0) {
            return new spreadsheet.UnionRef(name.split(/\s*,\s*/g).map(parseReference));
        }
        var m;
        // Sheet!A1
        if ((m = /^(([A-Z0-9]+)!)?\$?([A-Z]+)\$?([0-9]+)$/i.exec(name))) {
            return new spreadsheet.CellRef(getrow(m[4]), getcol(m[3]), 0)
                .setSheet(m[2], !!m[2]);
        }
        // Sheet!R1C1
        if ((m = /^(([A-Z0-9]+)!)?R([0-9]+)C([0-9]+)$/i.exec(name))) {
            return new spreadsheet.CellRef(getrow(m[3]), getrow(m[4]))
                .setSheet(m[2], !!m[2]);
        }
        // Sheet1!R1C1:Sheet2!R2C2
        if ((m = /^(([A-Z0-9]+)!)?R([0-9]+)C([0-9]+):(([A-Z0-9]+)!)?R([0-9]+)C([0-9]+)$/i.exec(name))) {
            return new spreadsheet.RangeRef(
                new spreadsheet.CellRef(getrow(m[3]), getrow(m[4]))
                    .setSheet(m[2], !!m[2]),
                new spreadsheet.CellRef(getrow(m[7]), getrow(m[8]))
                    .setSheet(m[6], !!m[6])
            ).setSheet(m[2], !!m[2]);
        }
        // Sheet!A1:B2, Sheet!$A$1:$B$2, Sheet1!A1:Sheet2:B2 etc.
        if ((m = /^(([A-Z0-9]+)!)?\$?([A-Z]+)\$?([0-9]+):(([A-Z0-9]+)!)?\$?([A-Z]+)\$?([0-9]+)$/i.exec(name))) {
            return new spreadsheet.RangeRef(
                new spreadsheet.CellRef(getrow(m[4]), getcol(m[3]), 0)
                    .setSheet(m[2], !!m[2]),
                new spreadsheet.CellRef(getrow(m[8]), getcol(m[7]), 0)
                    .setSheet(m[6], !!m[6])
            ).setSheet(m[2], !!m[2]);
        }
        // Sheet1!A:Sheet2!B
        if ((m = /^(([A-Z0-9]+)!)?\$?([A-Z]+):(([A-Z0-9]+)!)?\$?([A-Z]+)$/i.exec(name))) {
            return new spreadsheet.RangeRef(
                new spreadsheet.CellRef(-Infinity, getcol(m[3]), 0)
                    .setSheet(m[2], !!m[2]),
                new spreadsheet.CellRef(+Infinity, getcol(m[6]), 0)
                    .setSheet(m[5], !!m[5])
            ).setSheet(m[2], !!m[2]);
        }
        // Sheet1!5:Sheet2!6
        if ((m = /^(([A-Z0-9]+)!)?\$?([0-9]+):(([A-Z0-9]+)!)?\$?([0-9]+)$/i.exec(name))) {
            return new spreadsheet.RangeRef(
                new spreadsheet.CellRef(getrow(m[3]), -Infinity, 0)
                    .setSheet(m[2], !!m[2]),
                new spreadsheet.CellRef(getrow(m[6]), +Infinity, 0)
                    .setSheet(m[5], !!m[5])
            ).setSheet(m[2], !!m[2]);
        }
        if (!noThrow) {
            throw new Error("Cannot parse reference: " + name);
        }
    }

    function parseFormula(sheet, row, col, input) {
        var refs = [];

        if (typeof input == "string") {
            input = TokenStream(InputStream(input));
        }
        return {
            type: "exp",
            ast: parseExpression(true),
            refs: refs,
            sheet: sheet,
            row: row,
            col: col
        };

        function addReference(ref) {
            ref.index = refs.length;
            refs.push(ref);
            return ref;
        }

        function skip(type, value) {
            if (is(type, value)) {
                return input.next();
            } else {
                var tok = input.peek();
                if (tok) {
                    input.croak("Expected " + type + " «" + value + "» but found " + tok.type + " «" + tok.value + "»");
                } else {
                    input.croak("Expected " + type + " «" + value + "»");
                }
            }
        }

        function is(type, value) {
            var tok = input.peek();
            return tok != null
                && (type == null || tok.type === type)
                && (value == null || tok.value === value)
                ? tok : null;
        }

        function parseExpression(commas) {
            return maybeBinary(maybeIntersect(parseAtom(commas)), 0, commas);
        }

        function maybeRef(name) {
            var m;
            if ((m = /^((.+)!)?(\$)?([A-Z]+)(\$)?([0-9]+)$/i.exec(name))) {
                var thesheet  = m[1] && m[2];
                var relcol = m[3] ? 0 : 1, thecol = getcol(m[4]);
                var relrow = m[5] ? 0 : 2, therow = getrow(m[6]);
                if (relcol) {
                    thecol -= col;
                }
                if (relrow) {
                    therow -= row;
                }
                return new spreadsheet.CellRef(therow, thecol, relcol | relrow)
                    .setSheet(thesheet || sheet, !!thesheet);
            }
            if ((m = /^((.*)!)?(.+)$/i.exec(name))) {
                var thesheet  = m[1] && m[2];
                var name = m[3];
                return new spreadsheet.NameRef(name)
                    .setSheet(thesheet || sheet, !!thesheet);
            }
        }

        function parseSymbol(tok, addRef) {
            if (tok.upper == "TRUE" || tok.upper == "FALSE") {
                return tok.upper == "TRUE" ? TRUE : FALSE;
            }
            var ref = maybeRef(tok.value);
            if (ref) {
                if (addRef) {
                    addReference(ref);
                }
                return ref;
            }
            return tok;
        }

        function parseAtom(commas) {
            var exp = maybeRange() || maybeCall();
            if (!exp) {
                if (is("punc", "(")) {
                    input.next();
                    exp = parseExpression(true);
                    skip("punc", ")");
                }
                else if (is("num") || is("str")) {
                    exp = input.next();
                }
                else if (is("sym")) {
                    exp = parseSymbol(input.next(), true);
                }
                else if (is("op", "+") || is("op", "-")) {
                    exp = {
                        type: "prefix",
                        op: input.next().value,
                        exp: parseExpression(commas)
                    };
                }
                else if (is("punc", "{")) {
                    input.next();
                    exp = parseArray();
                }
                else if (!input.peek()) {
                    input.croak("Incomplete expression");
                }
                else {
                    input.croak("Parse error");
                }
            }
            return maybePercent(exp);
        }

        function parseArray() {
            var row = [], value = [ row ], first = true;
            while (!input.eof() && !is("punc", "}")) {
                if (first) {
                    first = false;
                } else if (is("punc", ";")) {
                    value.push(row = []);
                    input.next();
                } else {
                    skip("op", ",");
                }
                row.push(parseExpression(false));
            }
            skip("punc", "}");
            return {
                type: "matrix",
                value: value
            };
        }

        function maybeIntersect(exp) {
            if (is("punc", "(") || is("sym") || is("num")) {
                return {
                    type: "binary",
                    op: " ",
                    left: exp,
                    right: parseExpression(false)
                };
            } else {
                return exp;
            }
        }

        function maybeCall() {
            return input.ahead(2, function(fname, b){
                if (fname.type == "sym" && b.type == "punc" && b.value == "(") {
                    fname = fname.value;
                    input.skip(2);
                    var args = [];
                    if (!is("punc", ")")) {
                        while (1) {
                            if (is("op", ",")) {
                                args.push({ type: "null" });
                                input.next();
                                continue;
                            }
                            args.push(parseExpression(false));
                            if (input.eof() || is("punc", ")")) {
                                break;
                            }
                            skip("op", ",");
                        }
                    }
                    skip("punc", ")");
                    return {
                        type: "func",
                        func: fname,
                        args: args
                    };
                }
            });
        }

        function maybePercent(exp) {
            if (is("op", "%")) {
                input.next();
                return maybePercent({
                    type: "postfix",
                    op: "%",
                    exp: exp
                });
            } else {
                return exp;
            }
        }

        function maybeBinary(left, my_prec, commas) {
            var tok = is("op");
            if (tok && (commas || tok.value != ",")) {
                var his_prec = OPERATORS[tok.value];
                if (his_prec > my_prec) {
                    input.next();
                    var right = maybeBinary(parseAtom(commas), his_prec, commas);
                    return maybeBinary({
                        type: "binary",
                        op: tok.value,
                        left: left,
                        right: right
                    }, my_prec, commas);
                }
            }
            return left;
        }

        // Attempt to resolve constant ranges at parse time.  This helps handling row or column
        // ranges (i.e. A:A), printing ranges in normalized form, determining a formula's
        // dependencies.  However, the following can also be a valid range, and we can't analyze it
        // statically:
        //
        // ( INDIRECT(A1) : INDIRECT(A2) )
        //
        // therefore support for the range operators must also be present at run-time.
        //
        // This function will only deal with constant ranges like:
        //
        // - A1:B3
        // - A:A
        // - 2:2
        function maybeRange() {
            return input.ahead(4, function(a, b, c, d){
                if (looksLikeRange(a, b, c, d)) {
                    var topLeft = getref(a, true);
                    var bottomRight = getref(c, false);
                    if (topLeft != null && bottomRight != null) {
                        if (bottomRight.hasSheet() && topLeft.sheet.toLowerCase() != bottomRight.sheet.toLowerCase()) {
                            input.croak("Invalid range");
                        } else {
                            input.skip(3);
                            return addReference(
                                new spreadsheet.RangeRef(topLeft, bottomRight)
                                    .setSheet(topLeft.sheet, topLeft.hasSheet())
                            );
                        }
                    }
                }
            });
        }

        function getref(tok, isFirst) {
            if (tok.type == "num" && tok.value == tok.value|0) {
                return new spreadsheet.CellRef(
                    getrow(tok.value) - row,
                    isFirst ? -Infinity : +Infinity,
                    2
                ).setSheet(sheet, false);
            }
            var ref = parseSymbol(tok);
            if (ref.type == "ref") {
                if (ref.ref == "name") {
                    var name = ref.name;
                    var abs = name.charAt(0) == "$";
                    if (abs) {
                        name = name.substr(1);
                    }
                    if (/^[0-9]+$/.test(name)) {
                        // row ref
                        return new spreadsheet.CellRef(
                            getrow(name) - (abs ? 0 : row),
                            isFirst ? -Infinity : +Infinity,
                            (abs ? 0 : 2)
                        ).setSheet(ref.sheet || sheet, ref.hasSheet());
                    } else {
                        // col ref
                        return new spreadsheet.CellRef(
                            isFirst ? -Infinity : +Infinity,
                            getcol(name) - (abs ? 0 : col),
                            (abs ? 0 : 1)
                        ).setSheet(ref.sheet || sheet, ref.hasSheet());
                    }
                }
                return ref;
            }
        }
    }

    function makePrinter(exp) {
        return makeClosure("function(row, col){return(" + print(exp.ast, 0) + ")}");
        function print(node, prec) { // jshint ignore:line, because you are stupid.
            switch (node.type) {
              case "num":
              case "bool":
                return JSON.stringify(node.value);
              case "str":
                return JSON.stringify(JSON.stringify(node.value));
              case "ref":
                return "this.refs[" + node.index + "].print(row, col)";
              case "prefix":
                return withParens(node.op, prec, function(){
                    return JSON.stringify(node.op) + " + " + print(node.exp, OPERATORS[node.op]);
                });
              case "postfix":
                return withParens(node.op, prec, function(){
                    return print(node.exp, OPERATORS[node.op]) + " + " + JSON.stringify(node.op);
                });
              case "binary":
                return withParens(node.op, prec, function(){
                    var left = parenthesize(
                        print(node.left, OPERATORS[node.op]),
                        node.left instanceof spreadsheet.NameRef && node.op == ":"
                    );
                    var right = parenthesize(
                        print(node.right, OPERATORS[node.op]),
                        node.right instanceof spreadsheet.NameRef && node.op == ":"
                    );
                    return left + " + " + JSON.stringify(node.op) + " + " + right;
                });
              case "func":
                return JSON.stringify(node.func + "(") + " + "
                    + (node.args.length > 0
                       ? node.args.map(function(arg){
                           return print(arg, 0);
                       }).join(" + ', ' + ")
                       : "''")
                    + " + ')'";
              case "matrix":
                return "'{ ' + " + node.value.map(function(el){
                    return el.map(function(el){
                        return print(el, 0);
                    }).join(" + ', ' + ");
                }).join(" + '; ' + ") + "+ ' }'";
              case "null":
                return "''";
            }
            throw new Error("Cannot make printer for node " + node.type);
        }
        function parenthesize(code, cond) {
            return cond ? "'(' + " + code + " + ')'" : code;
        }
        function withParens(op, prec, f) {
            var needParens = (OPERATORS[op] < prec || (!prec && op == ","));
            return parenthesize(f(), needParens);
        }
    }

    function toCPS(ast, k) {
        var GENSYM = 0;
        return cps(ast, k);

        function cps(node, k){
            switch (node.type) {
              case "ref"     :
              case "num"     :
              case "str"     :
              case "null"    :
              case "bool"    : return cpsAtom(node, k);
              case "prefix"  :
              case "postfix" : return cpsUnary(node, k);
              case "binary"  : return cpsBinary(node, k);
              case "func"    : return cpsFunc(node, k);
              case "lambda"  : return cpsLambda(node, k);
              case "matrix"  : return cpsMatrix(node.value, k, true);
            }
            throw new Error("Cannot CPS " + node.type);
        }

        function cpsAtom(node, k) {
            return k(node);
        }

        function cpsUnary(node, k) {
            return cps({
                type: "func",
                func: "unary" + node.op,
                args: [ node.exp ]
            }, k);
        }

        function cpsBinary(node, k) {
            return cps({
                type: "func",
                func: "binary" + node.op,
                args: [ node.left, node.right ]
            }, k);
        }

        function cpsIf(co, th, el, k) {
            return cps(co, function(co){
                // compile THEN and ELSE into a lambda which takes a callback to invoke with the
                // result of the branches, and the IF itself will become a call the internal "if"
                // function.
                var rest = makeContinuation(k);
                var thenK = gensym("T");
                var elseK = gensym("E");
                return {
                    type: "func",
                    func: "if",
                    args: [
                        rest,
                        co, // condition
                        { // then
                            type: "lambda",
                            vars: [ thenK ],
                            body: cps(th || TRUE, function(th){
                                return {
                                    type: "call",
                                    func: { type: "var", name: thenK },
                                    args: [ th ]
                                };
                            })
                        },
                        { // else
                            type: "lambda",
                            vars: [ elseK ],
                            body: cps(el || FALSE, function(el){
                                return {
                                    type: "call",
                                    func: { type: "var", name: elseK },
                                    args: [ el ]
                                };
                            })
                        }
                    ]
                };
            });
        }

        function cpsAnd(args, k) {
            if (args.length === 0) {
                return cpsAtom(TRUE, k);
            }
            return cps({
                type: "func",
                func: "IF",
                args: [
                    // first item
                    args[0],
                    // if true, apply AND for the rest
                    {
                        type: "func",
                        func: "AND",
                        args: args.slice(1)
                    },
                    // otherwise return false
                    FALSE
                ]
            }, k);
        }

        function cpsOr(args, k) {
            if (args.length === 0) {
                return cpsAtom(FALSE, k);
            }
            return cps({
                type: "func",
                func: "IF",
                args: [
                    // first item
                    args[0],
                    // if true, return true
                    TRUE,
                    // otherwise apply OR for the rest
                    {
                        type: "func",
                        func: "OR",
                        args: args.slice(1)
                    }
                ]
            }, k);
        }

        function cpsFunc(node, k) {
            switch (node.func.toLowerCase()) {
              case "if":
                return cpsIf(node.args[0], node.args[1], node.args[2], k);
              case "and":
                return cpsAnd(node.args, k);
              case "or":
                return cpsOr(node.args, k);
              case "true":
                return k(TRUE);
              case "false":
                return k(FALSE);
            }
            // actual function
            return (function loop(args, i){
                if (i == node.args.length) {
                    return {
                        type : "func",
                        func : node.func,
                        args : args
                    };
                }
                else {
                    return cps(node.args[i], function(value){
                        return loop(args.concat([ value ]), i + 1);
                    });
                }
            })([ makeContinuation(k) ], 0);
        }

        function cpsLambda(node, k) {
            var cont = gensym("K");
            var body = cps(node.body, function(body){
                return { type: "call",
                         func: { type: "var", value: cont },
                         args: [ body ] };
            });
            return k({ type: "lambda",
                       vars: [ cont ].concat(node.vars),
                       body: body });
        }

        function cpsMatrix(elements, k, isMatrix) {
            var a = [];
            return (function loop(i){
                if (i == elements.length) {
                    return k({
                        type: "matrix",
                        value: a
                    });
                } else {
                    return (isMatrix ? cpsMatrix : cps)(elements[i], function(val){
                        a[i] = val;
                        return loop(i + 1);
                    });
                }
            })(0);
        }

        function makeContinuation(k) {
            var cont = gensym("R");
            return {
                type : "lambda",
                vars : [ cont ],
                body : k({ type: "var", name: cont })
            };
        }

        function gensym(name) {
            if (!name) {
                name = "";
            }
            name = "_" + name;
            return name + (++GENSYM);
        }
    }

    var makeClosure = (function(cache){
        return function(code) {
            var f = cache[code];
            if (!f) {
                f = cache[code] = new Function("'use strict';return(" + code + ")")();
            }
            return f;
        };
    })(Object.create(null));

    var FORMULA_CACHE = Object.create(null);

    function makeFormula(exp) {
        var printer = makePrinter(exp);
        var hash = printer.call(exp); // needs .refs
        var formula = FORMULA_CACHE[hash];
        if (formula) {
            // we need to clone because formulas cache the result; even if the formula is the same,
            // its value will depend on its location, hence we need different objects.  Still, using
            // this cache is a good idea because we'll reuse the same refs array, handler and
            // printer instead of allocating new ones (and we skip compiling it).
            return formula.clone(exp.sheet, exp.row, exp.col);
        }
        var code = js(toCPS(exp.ast, function(ret){
            return {
                type: "return",
                value: ret
            };
        }));

        code = [
            "function(){",
            "var context = this, refs = context.formula.absrefs",
            code,
            "}"
        ].join(";\n");

        formula = new runtime.Formula(exp.refs, makeClosure(code), printer, exp.sheet, exp.row, exp.col);
        FORMULA_CACHE[hash] = formula;
        return formula;

        function js(node){
            var type = node.type;
            if (type == "num") {
                return node.value + "";
            }
            else if (type == "str") {
                return JSON.stringify(node.value);
            }
            else if (type == "return") {
                return "context.resolve(" + js(node.value) + ")";
            }
            else if (type == "func") {
                return "context.func(" + JSON.stringify(node.func) + ", "
                    + js(node.args[0]) + ", " // the callback
                    + jsArray(node.args.slice(1)) // the arguments
                    + ")";
            }
            else if (type == "call") {
                return js(node.func) + "(" + node.args.map(js).join(", ") + ")";
            }
            else if (type == "ref") {
                return "refs[" + node.index + "]";
            }
            else if (type == "bool") {
                return "" + node.value;
            }
            else if (type == "if") {
                return "(context.bool(" + js(node.co) + ") ? " + js(node.th) + " : " + js(node.el) + ")";
            }
            else if (type == "lambda") {
                return "(function("
                    + node.vars.join(", ")
                    + "){ return(" + js(node.body) + ") })";
            }
            else if (type == "var") {
                return node.name;
            }
            else if (type == "matrix") {
                return jsArray(node.value);
            }
            else if (type == "null") {
                return "null";
            }
            else {
                throw new Error("Cannot compile expression " + type);
            }
        }

        function jsArray(a) {
            return "[ " + a.map(js).join(", ") + " ]";
        }
    }

    function TokenStream(input, forEditor) {
        var tokens = [], index = 0;
        var readWhile = input.readWhile;
        var addPos = forEditor ? function(thing) {
            var begin = input.pos();
            thing = thing();
            thing.begin = begin;
            thing.end = input.pos();
            return thing;
        } : function(thing) {
            return thing();
        };

        return {
            next  : next,
            peek  : peek,
            eof   : eof,
            croak : input.croak,
            ahead : ahead,
            skip  : skip
        };

        function isDigit(ch) {
            return (/[0-9]/i.test(ch));
        }
        function isIdStart(ch) {
            return (/[a-z$_]/i.test(ch) || ch.toLowerCase() != ch.toUpperCase());
        }
        function isId(ch) {
            return isIdStart(ch) || isDigit(ch) || ch == "!" || ch == ".";
        }
        function isOpChar(ch) {
            return ch in OPERATORS;
        }
        function isPunc(ch) {
            return ";(){}[]".indexOf(ch) >= 0;
        }
        function isWhitespace(ch) {
            return " \t\n\xa0".indexOf(ch) >= 0;
        }
        function readNumber() {
            // XXX: TODO: exponential notation
            var has_dot = false;
            var number = readWhile(function(ch){
                if (ch == ".") {
                    if (has_dot) {
                        return false;
                    }
                    has_dot = true;
                    return true;
                }
                return isDigit(ch);
            });
            return { type: "num", value: parseFloat(number) };
        }
        function readSymbol() {
            var id = readWhile(isId);
            return {
                type  : "sym",
                value : id,
                upper : id.toUpperCase(),
                space : isWhitespace(input.peek())
            };
        }
        function readString() {
            input.next();
            return { type: "str", value: input.readEscaped('"') };
        }
        function readOperator() {
            return {
                type  : "op",
                value : readWhile(function(ch, op){
                    return (op + ch) in OPERATORS;
                })
            };
        }
        function readPunc() {
            return {
                type  : "punc",
                value : input.next()
            };
        }
        function readNext() {
            readWhile(isWhitespace);
            if (input.eof()) {
                return null;
            }
            var ch = input.peek(), m;
            if (ch == '"') {
                return addPos(readString);
            }
            if (isDigit(ch)) {
                return addPos(readNumber);
            }
            if (isIdStart(ch)) {
                return addPos(readSymbol);
            }
            if (isOpChar(ch)) {
                return addPos(readOperator);
            }
            if (isPunc(ch)) {
                return addPos(readPunc);
            }
            if ((m = input.lookingAt(/^#([a-z\/]+)[?!]/i))) {
                return addPos(function(){
                    input.skip(m);
                    return { type: "error", value: m[1] };
                });
            }
            if (!forEditor) {
                input.croak("Can't handle character: " + ch);
            }
            return addPos(function(){
                return { type: "error", value: input.next() };
            });
        }
        function peek() {
            while (tokens.length <= index) {
                tokens.push(readNext());
            }
            return tokens[index];
        }
        function next() {
            var tok = peek();
            if (tok) {
                index++;
            }
            return tok;
        }
        function ahead(n, f) {
            var pos = index, a = [], eof = { type: "eof" };
            while (n-- > 0) {
                a.push(next() || eof);
            }
            index = pos;
            return f.apply(a, a);
        }
        function skip(n) {
            index += n;
        }
        function eof() {
            return peek() == null;
        }
    }

    function InputStream(input) {
        var pos = 0, line = 1, col = 0;
        return {
            next        : next,
            peek        : peek,
            eof         : eof,
            croak       : croak,
            readWhile   : readWhile,
            readEscaped : readEscaped,
            lookingAt   : lookingAt,
            skip        : skip,
            forward     : forward,
            pos         : location
        };
        function location() { // jshint ignore:line, :-(
            return pos;
        }
        function next() {
            var ch = input.charAt(pos++);
            if (ch == "\n") {
                line++;
                col = 0;
            } else {
                col++;
            }
            return ch;
        }
        function peek() {
            return input.charAt(pos);
        }
        function eof() {
            return peek() === "";
        }
        function croak(msg) {
            throw new ParseError(msg, pos);
        }
        function skip(ch) {
            if (typeof ch == "string") {
                if (input.substr(pos, ch.length) != ch) {
                    croak("Expected " + ch);
                }
                forward(ch.length);
            } else if (ch instanceof RegExp) {
                var m = ch.exec(input.substr(pos));
                if (m) {
                    forward(m[0].length);
                    return m;
                }
            } else {
                // assuming RegExp match data
                forward(ch[0].length);
            }
        }
        function forward(n) {
            while (n-- > 0) {
                next();
            }
        }
        function readEscaped(end) {
            var escaped = false, str = "";
            while (!eof()) {
                var ch = next();
                if (escaped) {
                    str += ch;
                    escaped = false;
                } else if (ch == "\\") {
                    escaped = true;
                } else if (ch == end) {
                    break;
                } else {
                    str += ch;
                }
            }
            return str;
        }
        function readWhile(predicate) {
            var str = "";
            while (!eof() && predicate(peek(), str)) {
                str += next();
            }
            return str;
        }
        function lookingAt(rx) {
            return rx.exec(input.substr(pos));
        }
    }

    //// exports

    exports.parse = function(sheet, row, col, input) {
        if (input instanceof Date) {
            return { type: "date", value: runtime.dateToSerial(input) };
        }
        if (typeof input == "number") {
            return { type: "number", value: input };
        }
        if (typeof input == "boolean") {
            return { type: "boolean", value: input };
        }
        input += "";
        if (/^'/.test(input)) {
            return {
                type: "string",
                value: input.substr(1)
            };
        }
        if (/^[0-9.]+%$/.test(input)) {
            var str = input.substr(0, input.length - 1);
            var num = parseFloat(str);
            if (!isNaN(num) && num == str) {
                return {
                    type: "percent",
                    value: num / 100
                };
            }
        }
        if (/^=/.test(input)) {
            input = input.substr(1);
            if (/\S/.test(input)) {
                return parseFormula(sheet, row, col, input);
            } else {
                return {
                    type: "string",
                    value: "=" + input
                };
            }
        }
        if (input.toLowerCase() == "true") {
            return { type: "boolean", value: true };
        }
        if (input.toLowerCase() == "false") {
            return { type: "boolean", value: false };
        }
        var date = runtime.parseDate(input);
        if (date) {
            return { type: "date", value: runtime.dateToSerial(date) };
        }
        var num = parseFloat(input);
        if (!isNaN(num) && input.length > 0 && num == input) {
            return {
                type: "number",
                value: num
            };
        }
        return {
            type: "string",
            value: input
        };
    };

    function looksLikeRange(a, b, c, d) {
        // We need c.space here to resolve an ambiguity:
        //
        //   - A1:C3 (A2, A3) -- parse as intersection between range and union
        //
        //   - A1:CHOOSE(2, A1, A2, A3) -- parse as range operator where the
        //     bottom-right side is returned by the CHOOSE function
        //
        // note no space between CHOOSE and the paren in the second example.
        // I believe this is the Right Way™.
        return ((a.type == "sym" || a.type == "num") &&
                (b.type == "op" && b.value == ":") &&
                (c.type == "sym" || c.type == "num") &&
                !(d.type == "punc" && d.value == "(" && !c.space));
    }

    function tokenize(input) {
        var tokens = [];
        input = TokenStream(InputStream(input), true);
        while (!input.eof()) {
            tokens.push(input.ahead(4, maybeRange) ||
                        input.ahead(2, maybeCall) ||
                        next());
        }
        var tok = tokens[0];
        if (tok.type == "op" && tok.value == "=") {
            tok.type = "startexp";
        }
        return tokens;

        function maybeRange(a, b, c, d) {
            if (looksLikeRange(a, b, c, d)) {
                var ref = parseReference(a.value + ":" + c.value, true);
                if (ref) {
                    input.skip(3);
                    return {
                        type: "ref",
                        ref: ref,
                        begin: a.begin,
                        end: c.end
                    };
                }
            }
        }
        function next() {
            var tok = input.next();
            if (tok.type == "sym") {
                var ref = parseReference(tok.value, true);
                if (ref) {
                    tok.type = "ref";
                    tok.ref = ref;
                } else if (tok.upper == "TRUE") {
                    tok.type = "bool";
                    tok.value = true;
                } else if (tok.upper == "FALSE") {
                    tok.type = "bool";
                    tok.value = false;
                }
            }
            return tok;
        }
        function maybeCall(fname, b) {
            if (fname.type == "sym" && b.type == "punc" && b.value == "(") {
                input.skip(1);
                fname.type = "func";
                return fname;
            }
        }
    }


    exports.parseFormula = parseFormula;
    exports.parseReference = parseReference;
    exports.compile = makeFormula;

    exports.InputStream = InputStream;
    exports.ParseError = ParseError;
    exports.tokenize = tokenize;

})();

(function(){
    "use strict";

    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    // WARNING: removing the following jshint declaration and turning
    // == into === to make JSHint happy will break functionality.
    /* jshint eqnull:true, newcap:false, laxbreak:true, shadow:true, -W054 */
    /* jshint latedef: nofunc */

    var calc = kendo.spreadsheet.calc;

    var RX_COLORS = /^\[(black|green|white|blue|magenta|yellow|cyan|red)\]/i;
    var RX_CONDITION = /^\[(<=|>=|<>|<|>|=)(-?[0-9.]+)\]/;

    /* The proper amount of suffering for whoever designed the Excel
       Custom Number Format would be to have him implement the
       formatter himself. */

    function parse(input) {
        input = calc.InputStream(input);
        var sections = [], haveTimePart = false, haveConditional = false, decimalPart;

        while (!input.eof()) {
            var sec = readSection();
            sections.push(sec);
            if (sec.cond) {
                haveConditional = true;
            }
        }

        // From https://support.office.com/en-us/article/Create-or-delete-a-custom-number-format-78f2a361-936b-4c03-8772-09fab54be7f4:
        //
        //    A number format can have up to four sections of code, separated by semicolons. These
        //    code sections define the format for positive numbers, negative numbers, zero values,
        //    and text, in that order.
        //
        //    You do not have to include all code sections in your custom number format. If you
        //    specify only two code sections for your custom number format, the first section is
        //    used for positive numbers and zeros, and the second section is used for negative
        //    numbers. If you specify only one code section, it is used for all numbers. If you want
        //    to skip a code section and include a code section that follows it, you must include
        //    the ending semicolon for the section that you skip.
        //
        // However, if sections have conditionals, it is not clear if:
        //
        //    - still at most four are allowed
        //    - is the last section still for text
        //
        // We will assume that if no sections have conditionals, then there must be at most 4, and
        // they will be interpreted in the order above.  If the first section contains a
        // conditional, then there can be any number of them; if the last one is not conditional
        // then it will be interpreted as text format.

        function addPlainText() {
            sections.push({
                cond: "text",
                body: [ { type: "text" } ]
            });
        }

        if (haveConditional) {
            addPlainText();
        }
        else if (sections.length == 1) {
            sections[0].cond = "num";
            addPlainText();
        }
        else if (sections.length == 2) {
            sections[0].cond = { op: ">=", value: 0 };
            sections[1].cond = { op: "<", value: 0 };
            addPlainText();
        }
        else if (sections.length >= 3) {
            sections[0].cond = { op: ">", value: 0 };
            sections[1].cond = { op: "<", value: 0 };
            sections[2].cond = { op: "=", value: 0 };
            addPlainText();
            if (sections.length > 3) {
                sections[3].cond = "text";
                sections = sections.slice(0, 4);
            }
        }

        return sections;

        function maybeColor() {
            var m = input.skip(RX_COLORS);
            if (m) {
                return m[1].toLowerCase();
            }
        }

        function maybeCondition() {
            var m = input.skip(RX_CONDITION);
            if (m) {
                var val = parseFloat(m[2]);
                if (!isNaN(val)) {
                    return { op: m[1], value: val };
                }
            }
        }

        function readFormat() {
            var format = [], tok;
            LOOP: while (!input.eof() && (tok = readNext())) {
                format.push(tok);
            }
            return format;
        }

        function readNext() {
            var ch, m;
            // numbers
            if ((m = input.skip(/^([#0?]+),([#0?]+)/))) {
                // thousands separator.  seems convenient to treat
                // this as a single token.
                return { type: "digit", sep: true, format: m[1] + m[2], decimal: decimalPart };
            }
            if ((m = input.skip(/^[#0?]+/))) {
                return { type: "digit", sep: false, format: m[0], decimal: decimalPart };
            }
            // XXX: handle this one!
            if ((m = input.skip(/^(e)([+-])/i))) {
                return { type: "exp", ch: m[1], sign: m[2] };
            }
            // dates
            if ((m = input.skip(/^(d{1,4}|m{1,5}|yyyy|yy)/i))) {
                // Disambiguate between month/minutes.  This means
                // there is no way to display minutes before hours or
                // seconds.  Dear whoever-invented-this format, you
                // are an idiot.
                var type = "date", m = m[1].toLowerCase();
                if (haveTimePart && (m == "m" || m == "mm")) {
                    type = "time";
                }
                haveTimePart = false;
                return { type: type, part: m.charAt(0), format: m.length };
            }
            // time (interpret as a date)
            if ((m = input.skip(/^(hh?|ss?)/i))) { // m and mm are handled above
                haveTimePart = true;
                m = m[1].toLowerCase();
                return { type: "time", part: m.charAt(0), format: m.length };
            }
            // elapsed time (interpret as interval of days)
            if ((m = input.skip(/^\[(hh?|mm?|ss?)\]/i))) {
                haveTimePart = true;
                m = m[1].toLowerCase();
                return { type: "eltime", part: m.charAt(0), format: m.length };
            }
            if ((m = input.skip(/^(am\/pm|a\/p)/i))) {
                m = m[1].split("/");
                return { type: "ampm", am: m[0], pm: m[1] };
            }
            switch ((ch = input.next())) { // JSHint sadness
              case ";":
                return null;
              case "\\":
                // quotes next character
                return { type: "str", value: input.next() };
              case '"':
                return { type: "str", value: input.readEscaped(ch) };
              case "@":
                return { type: "text" };
              case "_":
                // whitespace of the width of following character
                return { type: "space", value: input.next() };
              case "*":
                // fills cell width with the following character
                return { type: "fill", value: input.next() };
              case ".":
                if (input.lookingAt(/^\s*[#0?]/)) {
                    decimalPart = true;
                    return { type: "dec" };
                }
                return { type: "str", value: "." };
              case "%":
                return { type: "percent" };
              case ",":
                return { type: "comma" };
            }

            // whatever we can't make sense of, output literally.
            return { type: "str", value: ch };
        }

        function readSection() {
            decimalPart = false;
            var color = maybeColor(), cond = maybeCondition();
            if (!color && cond) {
                color = maybeColor();
            }
            return {
                color : color,
                cond  : cond,
                body  : readFormat()
            };
        }
    }

    function print(sections) {
        return sections.map(printSection).join(";");

        function printSection(sec) {
            var out = "";
            if (sec.color) {
                out += "[" + sec.color + "]";
            }
            if (sec.cond) {
                if (!(sec.cond == "text" || sec.cond == "num")) {
                    out += "[" + sec.cond.op + sec.cond.value + "]";
                }
            }
            out += sec.body.map(printToken).join("");
            return out;
        }

        function printToken(tok) {
            if (tok.type == "digit") {
                if (tok.sep) {
                    return tok.format.charAt(0) + "," + tok.format.substr(1);
                } else {
                    return tok.format;
                }
            }
            else if (tok.type == "exp") {
                return tok.ch + tok.sign;
            }
            else if (tok.type == "date" || tok.type == "time") {
                return padLeft("", tok.format, tok.part);
            }
            else if (tok.type == "ampm") {
                return tok.am + "/" + tok.pm;
            }
            else if (tok.type == "str") {
                return JSON.stringify(tok.value);
            }
            else if (tok.type == "text") {
                return "@";
            }
            else if (tok.type == "space") {
                return "_" + tok.value;
            }
            else if (tok.type == "fill") {
                return "*" + tok.value;
            }
            else if (tok.type == "dec") {
                return ".";
            }
            else if (tok.type == "percent") {
                return "%";
            }
            else if (tok.type == "comma") {
                return ",";
            }
        }
    }

    function adjustDecimals(sections, x) {
        sections.forEach(function(sec) {
            var diff = x;
            if (sec.cond == "text") {
                return;
            }
            var body = sec.body, adjusted = false, i = body.length;
            while (diff !== 0 && --i >= 0) {
                var tok = body[i];
                if (tok.type == "digit") {
                    if (tok.decimal) {
                        adjusted = true;
                        if (diff > 0) {
                            tok.format += padLeft("", diff, "0");
                        } else if (diff < 0) {
                            var tmp = tok.format.length;
                            tok.format = tok.format.substr(0, tmp + diff);
                            diff += tmp - tok.format.length;
                        }
                        if (tok.format.length === 0) {
                            body.splice(i, 1);
                            while (--i >= 0) {
                                tok = body[i];
                                if (tok.type == "digit" && tok.decimal) {
                                    ++i;
                                    break;
                                }
                                if (tok.type == "dec") {
                                    body.splice(i, 1);
                                    break;
                                }
                            }
                        }
                    }
                    if (diff > 0) {
                        break;
                    }
                }
            }
            if (!adjusted && diff > 0) {
                // no decimal part was found, insert one after the last digit token.
                body.splice(
                    i + 1, 0, {
                        type    : "dec"
                    }, {
                        type    : "digit",
                        sep     : false,
                        decimal : true,
                        format  : padLeft("", diff, "0")
                    }
                );
            }
        });
    }

    function TokenStream(parts) {
        var index = 0;
        return {
            next: function() {
                return parts[index++];
            },
            eof: function() {
                return index >= parts.length;
            },
            ahead: function(n, f) {
                if (index + n <= parts.length) {
                    var val = f.apply(null, parts.slice(index, index + n));
                    if (val) {
                        index += n;
                    }
                    return val;
                }
            },
            restart: function() {
                index = 0;
            }
        };
    }

    function compileFormatPart(format) {
        var input = TokenStream(format.body);
        var hasDate = false;
        var hasTime = false;
        var hasAmpm = false;
        var percentCount = 0;
        var scaleCount = 0;
        var code = "";
        var separeThousands = false;
        var declen = 0;
        var intFormat = [], decFormat = [];
        var condition = format.cond;
        var preamble = "";

        if (condition == "text") {
            preamble = "if (typeof value == 'string') { ";
        }
        else if (condition == "num") {
            preamble = "if (typeof value == 'number') { ";
        }
        else if (condition) {
            var op = condition.op == "=" ? "==" : condition.op;
            preamble = "if (typeof value == 'number' && value "
                + op + " " + condition.value + ") { ";
            code += "value = Math.abs(value); ";
        }

        if (format.color) {
            code += "element.attr.style = { color: " + JSON.stringify(format.color) + "}; ";
        }

        function checkComma(a, b) {
            if ((a.type == "digit" && b.type == "comma") ||
                (a.type == "comma" && a.hidden && b.type == "comma"))
            {
                b.hidden = true;
                scaleCount++;
            }
        }

        while (!input.eof()) {
            input.ahead(2, checkComma);
            var tok = input.next();
            if (tok.type == "percent") {
                percentCount++;
            }
            else if (tok.type == "digit") {
                if (tok.decimal) {
                    declen += tok.format.length;
                    decFormat.push(tok.format);
                } else {
                    intFormat.push(tok.format);
                    if (tok.sep) {
                        separeThousands = true;
                    }
                }
            }
            else if (tok.type == "time") {
                hasTime = true;
            }
            else if (tok.type == "date") {
                hasDate = true;
            }
            else if (tok.type == "ampm") {
                hasAmpm = hasTime = true;
            }
        }

        if (percentCount > 0) {
            code += "value *= " + Math.pow(100, percentCount) + "; ";
        }
        if (scaleCount > 0) {
            code += "value /= " + Math.pow(1000, scaleCount) + "; ";
        }
        if (intFormat.length) {
            code += "var intPart = runtime.formatInt(culture, value, " + JSON.stringify(intFormat) + ", " + declen + ", " + separeThousands + "); ";
        }
        if (decFormat.length) {
            code += "var decPart = runtime.formatDec(culture, value, " + JSON.stringify(decFormat) + ", " + declen + "); ";
        }
        if (intFormat.length || decFormat.length) {
            code += "type = 'number'; ";
        }
        if (hasDate) {
            code += "var date = runtime.unpackDate(value); ";
        }
        if (hasTime) {
            code += "var time = runtime.unpackTime(value); ";
        }
        if (hasDate || hasTime) {
            code += "type = 'date'; ";
        }

        if (percentCount > 0 || scaleCount > 0 || intFormat.length || decFormat.length || hasDate || hasTime) {
            if (!preamble) {
                preamble = "if (typeof value == 'number') { ";
            }
        }

        input.restart();
        while (!input.eof()) {
            var tok = input.next();
            if (tok.type == "dec") {
                code += "output += culture.numberFormat['.']; ";
            }
            else if (tok.type == "comma" && !tok.hidden) {
                code += "output += ','; ";
            }
            else if (tok.type == "percent") {
                code += "type = 'percent'; ";
                code += "output += culture.numberFormat.percent.symbol; ";
            }
            else if (tok.type == "str") {
                code += "output += " + JSON.stringify(tok.value) + "; ";
            }
            else if (tok.type == "text") {
                code += "type = 'text'; ";
                code += "output += value; ";
            }
            else if (tok.type == "space") {
                code += "element.children.push(dom.text(output)); ";
                code += "output = ''; ";
                code += "element.children.push(dom.element('span', { style: { visibility: 'hidden' }}, [ dom.text(" + JSON.stringify(tok.value) + ") ])); ";
            }
            else if (tok.type == "fill") {
                code += "output += runtime.fill(" + JSON.stringify(tok.value) + "); ";
            }
            else if (tok.type == "digit") {
                code += "output += " + (tok.decimal ? "decPart" : "intPart") + ".shift(); ";
            }
            else if (tok.type == "date") {
                code += "output += runtime.date(culture, date, "
                    + JSON.stringify(tok.part) + ", " + tok.format + "); ";
            }
            else if (tok.type == "time") {
                code += "output += runtime.time(culture, time, "
                    + JSON.stringify(tok.part) + ", " + tok.format + ", " + hasAmpm + "); ";
            }
            else if (tok.type == "eltime") {
                code += "output += runtime.eltime(culture, value, "
                    + JSON.stringify(tok.part) + ", " + tok.format + "); ";
            }
            else if (tok.type == "ampm") {
                // XXX: should use culture?  As per the "spec", Excel
                // displays whatever the token was (AM/PM, a/p etc.)
                code += "output += time.hours < 12 ? " + JSON.stringify(tok.am) + " : " + JSON.stringify(tok.pm) + "; ";
            }
        }

        code += "element.children.push(dom.text(output)); ";
        code += "element.__dataType = type; ";
        code += "return element; ";

        if (preamble) {
            code = preamble + code + "}";
        }

        return code;
    }

    var CACHE = Object.create(null);

    function compile(format) {
        var f = CACHE[format];
        if (!f) {
            var tree = parse(format);
            var code = tree.map(compileFormatPart).join("\n");
            code = "return function(value, culture){ "
                + "'use strict'; "
                + "if (!culture) culture = kendo.culture(); "
                + "var output = '', type = null, element = dom.element('span'); " + code + "; return element; };";
            f = CACHE[format] = new Function("runtime", "dom", code)(runtime, kendo.dom);
        }
        return f;
    }

    var runtime = {

        unpackDate: calc.runtime.unpackDate,
        unpackTime: calc.runtime.unpackTime,

        space: function(str) {
            return "<span style='visibility: hidden'>"
                + kendo.htmlEncode(str) + "</span>";
        },

        date: function(culture, d, part, length) {
            switch (part) {
              case "d":
                switch (length) {
                  case 1: return d.date;
                  case 2: return padLeft(d.date, 2, "0");
                  case 3: return culture.calendars.standard.days.namesAbbr[d.day];
                  case 4: return culture.calendars.standard.days.names[d.day];
                }
                break;
              case "m":
                switch (length) {
                  case 1: return d.month + 1;
                  case 2: return padLeft(d.month + 1, 2, "0");
                  case 3: return culture.calendars.standard.months.namesAbbr[d.month];
                  case 4: return culture.calendars.standard.months.names[d.month];
                  case 5: return culture.calendars.standard.months.names[d.month].charAt(0);
                }
                break;
              case "y":
                switch (length) {
                  case 2: return d.year % 100;
                  case 4: return d.year;
                }
                break;
            }
            return "##";
        },

        time: function(culture, t, part, length, ampm) {
            switch (part) {
              case "h":
                var h = ampm ? t.hours % 12 || 12 : t.hours;
                switch (length) {
                  case 1: return h;
                  case 2: return padLeft(h, 2, "0");
                }
                break;
              case "m":
                switch (length) {
                  case 1: return t.minutes;
                  case 2: return padLeft(t.minutes, 2, "0");
                }
                break;
              case "s":
                switch (length) {
                  case 1: return t.seconds;
                  case 2: return padLeft(t.seconds, 2, "0");
                }
                break;
            }
            return "##";
        },

        eltime: function(culture, value, part, length) {
            switch (part) {
              case "h":
                value = value * 24;
                break;
              case "m":
                value = value * 24 * 60;
                break;
              case "s":
                value = value * 24 * 60 * 60;
                break;
            }
            value |= 0;
            switch (length) {
              case 1: return value;
              case 2: return padLeft(value, 2, "0");
            }
            return "##";
        },

        fill: function(ch) {
            // XXX: how to implement this?
            return ch;
        },

        // formatting integer part is slightly different than decimal
        // part, so they're implemented in two functions.  For the
        // integer part we need to walk the value and the format
        // backwards (right-to-left).

        formatInt: function(culture, value, parts, declen, sep) {
            // toFixed is perfect for rounding our value; if there is
            // no format for decimals, for example, we want the number
            // rounded up.
            value = value.toFixed(declen).replace(/\..*$/, "");

            if (declen > 0) {
                // if the rounded number is zero and we have decimal
                // format, consider it a non-significant digit (Excel
                // won't display the leading zero for 0.2 in format
                // #.#).
                if (value === "0") { value = ""; }
                else if (value === "-0") { value = "-"; }
            }

            var iv = value.length - 1;
            var result = [];
            var len = 0, str;

            function add(ch) {
                if (sep && len && len % 3 === 0 && ch != " ") {
                    str = culture.numberFormat[","] + str;
                }
                str = ch + str;
                len++;
            }

            for (var j = parts.length; --j >= 0;) {
                var format = parts[j];
                str = "";
                for (var k = format.length; --k >= 0;) {
                    var chf = format.charAt(k);
                    if (iv < 0) {
                        if (chf == "0") {
                            add("0");
                        } else if (chf == "?") {
                            add(" ");
                        }
                    } else {
                        add(value.charAt(iv--));
                    }
                }
                if (j === 0) {
                    while (iv >= 0) {
                        add(value.charAt(iv--));
                    }
                }
                result.unshift(str);
            }

            return result;
        },

        // for decimal part we walk in normal direction and pad on the
        // right if required (for '0' or '?' chars).

        formatDec: function(culture, value, parts, declen) {
            value = value.toFixed(declen);
            var pos = value.indexOf(".");
            if (pos >= 0) {
                value = value.substr(pos + 1).replace(/0+$/, "");
            } else {
                value = "";
            }

            var iv = 0;
            var result = [];

            for (var j = 0; j < parts.length; ++j) {
                var format = parts[j];
                var str = "";
                for (var k = 0; k < format.length; ++k) {
                    var chf = format.charAt(k);
                    if (iv < value.length) {
                        str += value.charAt(iv++);
                    } else if (chf == "0") {
                        str += "0";
                    } else if (chf == "?") {
                        str += " ";
                    }
                }
                result.push(str);
            }

            return result;
        }
    };

    function padLeft(val, width, ch) {
        val += "";
        while (val.length < width) {
            val = ch + val;
        }
        return val;
    }

    /* -----[ exports ]----- */

    kendo.spreadsheet.formatting = {
        compile : compile,
        parse: parse,
        format: function(value, format, culture) {
            return compile(format)(value, culture);
        },
        type: function(value, format) {
            return compile(format)(value).__dataType;
        },
        adjustDecimals: function(format, diff) {
            var ast = parse(format);
            adjustDecimals(ast, diff);
            return print(ast);
        }
    };

})();

(function(){
    "use strict";

    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    // WARNING: removing the following jshint declaration and turning
    // == into === to make JSHint happy will break functionality.
    /* jshint eqnull:true, newcap:false, laxbreak:true, validthis:true */

    var util = kendo.util;
    var spreadsheet = kendo.spreadsheet;
    var calc = spreadsheet.calc;
    var runtime = calc.runtime;
    var defineFunction = runtime.defineFunction;
    var defineAlias = runtime.defineAlias;
    var CalcError = runtime.CalcError;
    var RangeRef = spreadsheet.RangeRef;
    var CellRef = spreadsheet.CellRef;
    var UnionRef = spreadsheet.UnionRef;
    var Matrix = runtime.Matrix;
    var Ref = spreadsheet.Ref;
    var daysInMonth = runtime.daysInMonth;
    var packDate = runtime.packDate;
    var unpackDate = runtime.unpackDate;
    var daysInYear = runtime.daysInYear;

    /* -----[ Math functions ]----- */

    [ "abs", "cos", "sin", "acos", "asin", "tan", "atan", "exp", "sqrt" ].forEach(function(name){
        defineFunction(name, Math[name]).args([
            [ "*n", "number" ]
        ]);
    });

    defineFunction("ln", Math.log).args([
        [ "*n", "number" ]
    ]);

    defineFunction("log", function(num, base){
        return Math.log(num) / Math.log(base);
    }).args([
        [ "*num", "number++" ],
        [ "*base", [ "or", "number++", [ "null", 10 ] ] ],
        [ "?", [ "assert", "$base != 1", "DIV/0" ] ]
    ]);

    defineFunction("log10", function(num){
        return Math.log(num) / Math.log(10);
    }).args([
        [ "*num", "number++" ]
    ]);

    defineFunction("pi", function(){
        return Math.PI;
    }).args([]);

    defineFunction("sqrtpi", function(n){
        return Math.sqrt(n * Math.PI);
    }).args([
        [ "*num", "number+" ]
    ]);

    defineFunction("degrees", function(rad){
        return ((180 * rad) / Math.PI) % 360;
    }).args([
        [ "*radians", "number" ]
    ]);

    defineFunction("radians", function(deg){
        return Math.PI * deg / 180;
    }).args([
        [ "*degrees", "number" ]
    ]);

    function _cosh(n){
        return (Math.exp(n) + Math.exp(-n)) / 2;
    }

    defineFunction("cosh", _cosh).args([
        [ "*num", "number" ]
    ]);

    defineFunction("acosh", function(n){
        return Math.log(n + Math.sqrt(n - 1) * Math.sqrt(n + 1));
    }).args([
        [ "*num", "number" ],
        [ "?", [ "assert", "$num >= 1" ] ]
    ]);

    function _sinh(n){
        return (Math.exp(n) - Math.exp(-n)) / 2;
    }

    defineFunction("sinh", _sinh).args([
        [ "*num", "number" ]
    ]);

    defineFunction("asinh", function(n){
        return Math.log(n + Math.sqrt(n * n + 1));
    }).args([
        [ "*num", "number" ]
    ]);

    defineFunction("sec", function(n){
        return 1 / Math.cos(n);
    }).args([
        [ "*num", "number" ]
    ]);

    defineFunction("sech", function(n){
        return 1 / _cosh(n);
    }).args([
        [ "*num", "number" ]
    ]);

    defineFunction("csc", function(n){
        return 1 / Math.sin(n);
    }).args([
        [ "*num", "number" ]
    ]);

    defineFunction("csch", function(n){
        return 1 / _sinh(n);
    }).args([
        [ "*num", "number" ]
    ]);

    defineFunction("atan2", function(x, y){
        return Math.atan(y / x);
    }).args([
        [ "*x", "divisor" ],
        [ "*y", "number" ]
    ]);

    function _tanh(n) {
        return _sinh(n) / _cosh(n);
    }

    defineFunction("tanh", _tanh).args([
        [ "*num", "number" ]
    ]);

    defineFunction("atanh", function(n){
        return Math.log(Math.sqrt(1 - n*n) / (1 - n));
    }).args([
        [ "*num", [ "and", "number", [ "(between)", -1, 1 ] ] ]
    ]);

    defineFunction("cot", function(n){
        return 1 / Math.tan(n);
    }).args([
        [ "*num", "divisor" ]
    ]);

    defineFunction("coth", function(n){
        return 1 / _tanh(n);
    }).args([
        [ "*num", "divisor" ]
    ]);

    defineFunction("acot", function(n){
        return Math.PI / 2 - Math.atan(n);
    }).args([
        [ "*num", "number" ]
    ]);

    defineFunction("acoth", function(n){
        return Math.log((n + 1) / (n - 1)) / 2;
    }).args([
        [ "*num", "number" ],
        [ "?", [ "or",
                 [ "assert", "$num < -1"],
                 [ "assert", "$num > 1" ] ] ]
    ]);

    defineFunction("power", function(a, b){
        return Math.pow(a, b);
    }).args([
        [ "*a", "number" ],
        [ "*b", "number" ]
    ]);

    defineFunction("mod", function(a, b){
        return a % b;
    }).args([
        [ "*a", "number" ],
        [ "*b", "divisor" ]
    ]);

    defineFunction("quotient", function(a, b){
        return Math.floor(a / b);
    }).args([
        [ "*a", "number" ],
        [ "*b", "divisor" ]
    ]);

    defineFunction("ceiling", function(num, s){
        return s ? s * Math.ceil(num / s) : 0;
    }).args([
        [ "*number", "number" ],
        [ "*significance", "number" ],
        [ "?", [ "assert", "$significance >= 0 || $number < 0" ] ]
    ]);

    defineFunction("ceiling.precise", function(num, s){
        s = Math.abs(s);
        return s ? s * Math.ceil(num / s) : 0;
    }).args([
        [ "*number", "number" ],
        [ "*significance", [ "or", "number", [ "null", 1 ] ] ]
    ]);

    defineAlias("iso.ceiling", "ceiling.precise");

    // XXX: how do we know if this function is correct?
    //
    // https://support.office.com/en-gb/article/CEILING-MATH-function-80f95d2f-b499-4eee-9f16-f795a8e306c8
    //
    // “There are many combinations of Significance and Mode values that affect rounding of negative
    // numbers in different ways.” — right, thanks for the info. :-\
    defineFunction("ceiling.math", function(num, s, mode){
        if (!s || !num) {
            return 0;
        }
        if (num < 0 && ((!mode && s < 0) || (mode && s > 0))) {
            s = -s;
        }
        return s ? s * Math.ceil(num / s) : 0;
    }).args([
        [ "*number", "number" ],
        [ "*significance", [ "or", "number", [ "null", "$number < 0 ? -1 : 1" ] ] ],
        [ "*mode", [ "or", "logical", [ "null", 0 ] ] ]
    ]);

    defineFunction("floor", function(num, s){
        return s ? s * Math.floor(num / s) : 0;
    }).args([
        [ "*number", "number" ],
        [ "*significance", "number" ],
        [ "?", [ "assert", "$significance >= 0 || $number < 0" ] ]
    ]);

    defineFunction("floor.precise", function(num, s){
        s = Math.abs(s);
        return s ? s * Math.floor(num / s) : 0;
    }).args([
        [ "*number", "number" ],
        [ "*significance", [ "or", "number", [ "null", 1 ] ] ]
    ]);

    // XXX: check this
    defineFunction("floor.math", function(num, s, mode){
        if (!s || !num) {
            return 0;
        }
        if (num < 0 && ((!mode && s < 0) || (mode && s > 0))) {
            s = -s;
        }
        return s ? s * Math.floor(num / s) : 0;
    }).args([
        [ "*number", "number" ],
        [ "*significance", [ "or", "number", [ "null", "$number < 0 ? -1 : 1" ] ] ],
        [ "*mode", [ "or", "logical", [ "null", 0 ] ] ]
    ]);

    defineFunction("int", Math.floor).args([
        [ "*number", "number" ]
    ]);

    defineFunction("mround", function(num, mult){
        return mult ? mult * Math.round(num / mult) : 0;
    }).args([
        [ "*number", "number" ],
        [ "*multiple", "number" ]
    ]);

    defineFunction("even", function(num){
        var n = num < 0 ? Math.floor(num) : Math.ceil(num);
        return n % 2 ? n + (n < 0 ? -1 : 1) : n;
    }).args([
        [ "*number", "number" ]
    ]);

    defineFunction("odd", function(num){
        var n = num < 0 ? Math.floor(num) : Math.ceil(num);
        return n % 2 ? n : n + (n < 0 ? -1 : 1);
    }).args([
        [ "*number", "number" ]
    ]);

    defineFunction("sign", function(num){
        return num < 0 ? -1 : num > 0 ? 1 : 0;
    }).args([
        [ "*number", "number" ]
    ]);

    function _gcd(a, b) {
        while (b) {
            var r = a % b;
            a = b;
            b = r;
        }
        return a;
    }

    function _lcm(a, b) {
        return Math.abs(a * b) / _gcd(a, b);
    }

    defineFunction("gcd", function(args){
        var a = args[0];
        for (var i = 1; i < args.length; ++i) {
            a = _gcd(a, args[i]);
        }
        return a;
    }).args([
        [ "numbers", [ "collect", "number" ] ]
    ]);

    defineFunction("lcm", function(args){
        var a = args[0];
        for (var i = 1; i < args.length; ++i) {
            a = _lcm(a, args[i]);
        }
        return a;
    }).args([
        [ "numbers", [ "collect", "number" ] ]
    ]);

    defineFunction("sum", function(numbers){
        return numbers.reduce(function(sum, num){
            return sum + num;
        }, 0);
    }).args([
        [ "numbers", [ "collect", "number" ] ]
    ]);

    defineFunction("product", function(numbers){
        return numbers.reduce(function(prod, num){
            return prod * num;
        }, 1);
    }).args([
        [ "numbers", [ "collect", "number" ] ]
    ]);

    defineFunction("sumproduct", function(a) {
        var sum = 0;
        a[0].each(function(p, row, col){
            if (typeof p == "number") {
                for (var i = 1; i < a.length; ++i) {
                    var v = a[i].get(row, col);
                    if (typeof v != "number") {
                        return;
                    }
                    p *= v;
                }
                sum += p;
            }
        });
        return sum;
    }).args([
        [ "a1", "matrix" ],
        [ "+",
          [ "a2", [ "and", "matrix",
                    [ "assert", "$a2.width == $a1.width" ],
                    [ "assert", "$a2.height == $a1.height" ] ] ] ]
    ]);

    defineFunction("sumsq", function(numbers){
        return numbers.reduce(function(sum, num){
            return sum + num * num;
        }, 0);
    }).args([
        [ "numbers", [ "collect", "number" ] ]
    ]);

    defineFunction("sumx2my2", function(a, b){
        var sum = 0;
        a.each(function(x, row, col){
            var y = b.get(row, col);
            if (typeof x == "number" && typeof y == "number") {
                sum += x*x - y*y;
            }
        });
        return sum;
    }).args([
        [ "a", "matrix" ],
        [ "b", [ "and", "matrix",
                 [ "assert", "$b.width == $a.width" ],
                 [ "assert", "$b.height == $a.height" ] ] ]
    ]);

    defineFunction("sumx2py2", function(a, b){
        var sum = 0;
        a.each(function(x, row, col){
            var y = b.get(row, col);
            if (typeof x == "number" && typeof y == "number") {
                sum += x*x + y*y;
            }
        });
        return sum;
    }).args([
        [ "a", "matrix" ],
        [ "b", [ "and", "matrix",
                 [ "assert", "$b.width == $a.width" ],
                 [ "assert", "$b.height == $a.height" ] ] ]
    ]);

    defineFunction("sumxmy2", function(a, b){
        var sum = 0;
        a.each(function(x, row, col){
            var y = b.get(row, col);
            if (typeof x == "number" && typeof y == "number") {
                sum += (x - y) * (x - y);
            }
        });
        return sum;
    }).args([
        [ "a", "matrix" ],
        [ "b", [ "and", "matrix",
                 [ "assert", "$b.width == $a.width" ],
                 [ "assert", "$b.height == $a.height" ] ] ]
    ]);

    defineFunction("seriessum", function(x, n, m, a){
        var sum = 0;
        a.each(function(coef){
            if (typeof coef != "number") {
                throw new CalcError("VALUE");
            }
            sum += coef * Math.pow(x, n);
            n += m;
        });
        return sum;
    }).args([
        [ "x", "number" ],
        [ "y", "number" ],
        [ "m", "number" ],
        [ "a", "matrix" ]
    ]);

    defineFunction("min", function(numbers){
        return Math.min.apply(Math, numbers);
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length > 0", "N/A" ] ]
    ]);

    defineFunction("max", function(numbers){
        return Math.max.apply(Math, numbers);
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length > 0", "N/A" ] ]
    ]);

    defineFunction("counta", function(values){
        return values.length;
    }).args([
        [ "values", [ "#collect", "anyvalue" ] ]
    ]);

    defineFunction("count", function(numbers){
        return numbers.length;
    }).args([
        [ "numbers", [ "#collect", "number" ] ]
    ]);

    defineFunction("countunique", function(values){
        var count = 0, seen = [];
        values.forEach(function(val){
            if (seen.indexOf(val) < 0) {
                count++;
                seen.push(val);
            }
        });
        return count;
    }).args([
        [ "values", [ "#collect", "anyvalue" ] ]
    ]);

    defineFunction("countblank", function(a){
        var count = 0;
        function add(val) {
            if (val == null || val === "") {
                count++;
            }
        }
        function loop(args){
            for (var i = 0; i < args.length; ++i) {
                var x = args[i];
                if (x instanceof Matrix) {
                    x.each(add, true);
                } else {
                    add(x);
                }
            }
        }
        loop(a);
        return count;
    }).args([
        [ "+", [ "args", [ "or", "matrix", "anyvalue" ] ] ]
    ]);

    defineFunction("iseven", function(num){
        return num % 2 === 0;
    }).args([
        [ "*number", "number" ]
    ]);

    defineFunction("isodd", function(num){
        return num % 2 !== 0;
    }).args([
        [ "*number", "number" ]
    ]);

    defineFunction("n", function(val){
        if (typeof val == "boolean") {
            return val ? 1 : 0;
        }
        if (typeof val == "number") {
            return val;
        }
        return 0;
    }).args([
        [ "*value", "anyvalue" ]
    ]);

    defineFunction("na", function(){
        return new CalcError("N/A");
    }).args([]);

    /* -----[ the "*IFS" functions ]----- */

    // helper function: take `args` like COUNTIFS (see Excel docs) and
    // calls `f` for each cell matching all criteria.  `f` receives
    // `chunks` (parsed args containing matrix and predicate) and
    // row,col of matching cells.
    function forIFS(args, f) {
        var chunks = [], i = 0, matrix = args[0];
        while (i < args.length) {
            chunks.push({
                matrix: args[i++],
                pred: parseCriteria(args[i++])
            });
        }
        ROW: for (var row = 0; row < matrix.height; ++row) {
            COL: for (var col = 0; col < matrix.width; ++col) {
                for (i = 0; i < chunks.length; ++i) {
                    var val = chunks[i].matrix.get(row, col);
                    if (!chunks[i].pred(val == null || val === "" ? 0 : val)) {
                        continue COL;
                    }
                }
                f(row, col);
            }
        }
    }

    var ARGS_COUNTIFS = [
        [ "m1", "matrix" ],
        [ "c1", "anyvalue" ],
        [ [ "m2", [ "and", "matrix",
                    [ "assert", "$m1.width == $m2.width" ],
                    [ "assert", "$m1.height == $m2.height" ] ] ],
          [ "c2", "anyvalue" ] ]
    ];

    defineFunction("countifs", function(m1, c1, rest){
        var count = 0;
        rest.unshift(m1, c1);
        forIFS(rest, function(){ count++; });
        return count;
    }).args(ARGS_COUNTIFS);

    var ARGS_SUMIFS = [
        [ "range", "matrix" ]
    ].concat(ARGS_COUNTIFS);

    defineFunction("sumifs", function(range, m1, c1, args){
        // hack: insert a predicate that filters out non-numeric
        // values; should also accept blank cells.  it's safe to
        // modify args.
        args.unshift(range, numericPredicate, m1, c1);
        var sum = 0;
        forIFS(args, function(row, col){
            var val = range.get(row, col);
            if (val) {
                sum += val;
            }
        });
        return sum;
    }).args(ARGS_SUMIFS);

    // similar to sumifs, but compute average of matching cells
    defineFunction("averageifs", function(range, m1, c1, args){
        args.unshift(range, numericPredicate, m1, c1);
        var sum = 0, count = 0;
        forIFS(args, function(row, col){
            var val = range.get(row, col);
            if (val == null || val === "") {
                val = 0;
            }
            sum += val;
            count++;
        });
        return count ? sum / count : new CalcError("DIV/0");
    }).args(ARGS_SUMIFS);

    defineFunction("countif", function(matrix, criteria){
        criteria = parseCriteria(criteria);
        var count = 0;
        matrix.each(function(val){
            if (criteria(val)) {
                count++;
            }
        });
        return count;
    }).args([
        [ "range", "matrix" ],
        [ "*criteria", "anyvalue" ]
    ]);

    var ARGS_SUMIF = [
        [ "range", "matrix" ],
        [ "*criteria", "anyvalue" ],
        [ "sumRange", [ "or",
                        [ "and", "matrix",
                          [ "assert", "$sumRange.width == $range.width" ],
                          [ "assert", "$sumRange.height == $range.height" ] ],
                        [ "null", "$range" ] ] ]
    ];

    defineFunction("sumif", function(range, criteria, sumRange){
        var sum = 0;
        criteria = parseCriteria(criteria);
        range.each(function(val, row, col){
            if (criteria(val)) {
                var v = sumRange.get(row, col);
                if (numericPredicate(v)) {
                    sum += v || 0;
                }
            }
        });
        return sum;
    }).args(ARGS_SUMIF);

    defineFunction("averageif", function(range, criteria, sumRange){
        var sum = 0, count = 0;
        criteria = parseCriteria(criteria);
        range.each(function(val, row, col){
            if (criteria(val)) {
                var v = sumRange.get(row, col);
                if (numericPredicate(v)) {
                    sum += v || 0;
                    count++;
                }
            }
        });
        return count ? sum / count : new CalcError("DIV/0");
    }).args(ARGS_SUMIF);

    (function(def){
        def("large", function(numbers, nth){
            return numbers.sort(descending)[nth];
        });
        def("small", function(numbers, nth){
            return numbers.sort(ascending)[nth];
        });
    })(function(name, handler){
        defineFunction(name, function(matrix, nth){
            var numbers = [];
            var error = matrix.each(function(val){
                if (val instanceof CalcError) {
                    return val;
                }
                if (typeof val == "number") {
                    numbers.push(val);
                }
            });
            if (error) {
                return error;
            }
            if (nth > numbers.length) {
                return new CalcError("NUM");
            }
            return handler(numbers, nth - 1);
        }).args([
            [ "array", "matrix" ],
            [ "*nth", "number++" ]
        ]);
    });

    function _avg(numbers) {
        return numbers.reduce(function(sum, num){
            return sum + num;
        }, 0) / numbers.length;
    }

    function _var_sp(numbers, divisor, avg) {
        if (avg == null) {
            avg = _avg(numbers);
        }
        return numbers.reduce(function(sum, num){
            return sum + Math.pow(num - avg, 2);
        }, 0) / divisor;
    }

    function _stdev_sp(numbers, divisor) {
        return Math.sqrt(_var_sp(numbers, divisor));
    }

    // https://support.office.com/en-sg/article/STDEV-S-function-7d69cf97-0c1f-4acf-be27-f3e83904cc23
    defineFunction("stdev.s", function(numbers){
        return _stdev_sp(numbers, numbers.length - 1);
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length >= 2", "NUM" ] ]
    ]);

    // https://support.office.com/en-sg/article/STDEV-P-function-6e917c05-31a0-496f-ade7-4f4e7462f285
    defineFunction("stdev.p", function(numbers){
        return _stdev_sp(numbers, numbers.length);
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length >= 2", "NUM" ] ]
    ]);

    defineFunction("var.s", function(numbers){
        return _var_sp(numbers, numbers.length - 1);
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length >= 2", "NUM" ] ]
    ]);

    defineFunction("var.p", function(numbers){
        return _var_sp(numbers, numbers.length);
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length >= 2", "NUM" ] ]
    ]);

    defineFunction("median", function(numbers){
        var n = numbers.length;
        numbers.sort(ascending);
        if (n % 2) {
            return numbers[n >> 1];
        }
        return (numbers[n >> 1] + numbers[n >> 1 + 1]) / 2;
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length > 0", "N/A" ] ]
    ]);

    defineFunction("mode.sngl", function(numbers){
        numbers.sort(ascending);
        var prev = null, count = 0, max = 1, mode = null;
        for (var i = 0; i < numbers.length; ++i) {
            var n = numbers[i];
            if (n != prev) {
                count = 1;
                prev = n;
            } else {
                count++;
            }
            if (count > max) {
                max = count;
                mode = n;
            }
        }
        return mode == null ? new CalcError("N/A") : mode;
    }).args([
        [ "numbers", [ "collect", "number" ] ]
    ]);

    defineFunction("mode.mult", function(numbers){
        var seen = Object.create(null), max = 2, res = [];
        numbers.forEach(function(num){
            var s = seen[num] || 0;
            seen[num] = ++s;
            if (s == max) {
                res.push(num);
            } else if (s > max) {
                max = s;
                res = [ num ];
            }
        });
        var m = new Matrix(this);
        res.forEach(function(num, i){
            m.set(i, 0, num);
        });
        return m;
    }).args([
        [ "numbers", [ "collect", "number" ] ]
    ]);

    defineFunction("geomean", function(numbers){
        var n = numbers.length;
        var p = numbers.reduce(function(p, num){
            if (num < 0) {
                throw new CalcError("NUM");
            }
            return p * num;
        }, 1);
        return Math.pow(p, 1/n);
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length > 0", "NUM" ] ]
    ]);

    defineFunction("harmean", function(numbers){
        var n = numbers.length;
        var s = numbers.reduce(function(s, num){
            if (!num) {
                throw new CalcError("DIV/0");
            }
            return s + 1 / num;
        }, 0);
        return n / s;
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length > 0", "NUM" ] ]
    ]);

    defineFunction("trimmean", function(numbers, p){
        var n = numbers.length;
        numbers.sort(ascending);
        var discard = Math.floor(n * p);
        if (discard % 2) {
            --discard;
        }
        discard /= 2;
        var sum = 0;
        for (var i = discard; i < n-discard; ++i) {
            sum += numbers[i];
        }
        return sum / (n - discard * 2);
    }).args([
        [ "numbers", [ "collect", "number", 1 ] ],
        [ "percent", [ "and", "number", [ "[between)", 0, 1 ] ] ],
        [ "?", [ "assert", "$numbers.length > 0", "NUM" ] ]
    ]);

    defineFunction("frequency", function(data, bins){
        // apparently this always returns a vertical matrix in Excel, so we collect all numbers in
        // bins instead of receiving it as a Matrix and try to mimic its shape.
        data.sort(ascending);
        bins.sort(ascending);
        var prev = -Infinity;
        var i = 0;
        function count(max) {
            var n = 0;
            while (i < data.length && data[i] > prev && data[i] <= max) {
                ++n; ++i;
            }
            return n;
        }
        var m = new Matrix(this);
        bins.forEach(function(val, i){
            var n = count(val);
            prev = val;
            m.set(i, 0, n);
        });
        m.set(m.height, 0, data.length - i);
        return m;
    }).args([
        [ "data", [ "collect", "number", 1 ] ],
        [ "bins", [ "collect", "number", 1 ] ]
    ]);

    defineFunction("rank.eq", function(val, numbers, asc) {
        numbers.sort(asc ? ascending : descending);
        var pos = numbers.indexOf(val);
        return pos < 0 ? new CalcError("N/A") : pos + 1;
    }).args([
        [ "value", "number" ],
        [ "numbers", [ "collect", "number" ] ],
        [ "order", [ "or", "logical", [ "null", false ] ] ]
    ]);

    defineAlias("rank", "rank.eq");

    defineFunction("rank.avg", function(val, numbers, asc) {
        numbers.sort(asc ? ascending : descending);
        var pos = numbers.indexOf(val);
        if (pos < 0) {
            return new CalcError("N/A");
        }
        for (var i = pos; numbers[i] == val; ++i){}
        return (pos + i + 1) / 2;
    }).args([
        [ "value", "number" ],
        [ "numbers", [ "collect", "number" ] ],
        [ "order", [ "or", "logical", [ "null", false ] ] ]
    ]);

    // formula available at https://support.office.microsoft.com/en-us/article/KURT-function-cbbc2312-dfa6-4cc4-b5c0-1b3c59cc9377
    defineFunction("kurt", function(numbers){
        var n = numbers.length;
        var avg = _avg(numbers);
        var variance = _var_sp(numbers, n-1, avg);
        var stddev = Math.sqrt(variance);
        var sum = numbers.reduce(function(sum, num){
            return sum + Math.pow((num - avg) / stddev, 4);
        }, 0);
        return n*(n+1)/((n-1)*(n-2)*(n-3)) * sum
            - 3*Math.pow(n-1, 2)/((n-2)*(n-3));
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length >= 4", "NUM" ] ]
    ]);

    function _percentrank(numbers, x, exc) {
        var nlt = 0, ngt = 0, left = null, right = null, found = false;
        numbers.forEach(function(num){
            if (num < x) {
                nlt++;
                left = left == null ? num : Math.max(left, num);
            } else if (num > x) {
                ngt++;
                right = right == null ? num : Math.min(right, num);
            } else {
                found = true;
            }
        });
        if (!nlt && !ngt) {
            return new CalcError("N/A");
        }
        if (found) {
            if (exc) {
                return (nlt + 1) / (numbers.length + 1);
            }
            return nlt / (nlt + ngt);
        }
        return ((right - x) * _percentrank(numbers, left, exc) +
                (x - left) * _percentrank(numbers, right, exc)) / (right - left);
    }

    var ARGS_PERCENTRANK = [
        [ "array", [ "collect", "number", 1 ] ],
        [ "x", "number" ],
        [ "significance", [ "or", [ "null", 3 ], "integer++" ] ],
        [ "?", [ "assert", "$array.length > 0", "NUM" ] ]
    ];

    defineFunction("percentrank.inc", function(numbers, x, significance) {
        var p = _percentrank(numbers, x, 0);
        p = p.toFixed(significance + 1);
        return parseFloat(p.substr(0, p.length - 1));
    }).args(ARGS_PERCENTRANK);

    defineFunction("percentrank.exc", function(numbers, x, significance) {
        var p = _percentrank(numbers, x, 1);
        p = p.toFixed(significance + 1);
        return parseFloat(p.substr(0, p.length - 1));
    }).args(ARGS_PERCENTRANK);

    defineAlias("percentrank", "percentrank.inc");

    function _covariance(x, y, divisor) {
        var sum = 0;
        var ax = _avg(x);
        var ay = _avg(y);
        var n = x.length;
        for (var i = 0; i < n; ++i) {
            sum += (x[i] - ax) * (y[i] - ay);
        }
        return sum / divisor;
    }

    defineFunction("covariance.p", function(x, y){
        return _covariance(x, y, x.length);
    }).args([
        [ "array1", [ "collect", "number", 1 ] ],
        [ "array2", [ "collect", "number", 1 ] ],
        [ "?", [ "assert", "$array1.length == $array2.length", "N/A" ] ],
        [ "?", [ "assert", "$array1.length > 0", "DIV/0" ] ]
    ]);

    defineFunction("covariance.s", function(x, y){
        return _covariance(x, y, x.length - 1);
    }).args([
        [ "array1", [ "collect", "number", 1 ] ],
        [ "array2", [ "collect", "number", 1 ] ],
        [ "?", [ "assert", "$array1.length == $array2.length", "N/A" ] ],
        [ "?", [ "assert", "$array1.length > 1", "DIV/0" ] ]
    ]);

    defineAlias("covar", "covariance.p");

    /* -----[ Factorials ]----- */

    var _fact = util.memoize(function(n){
        for (var i = 2, fact = 1; i <= n; ++i) {
            fact *= i;
        }
        return fact;
    });

    defineFunction("fact", _fact).args([
        [ "*n", "integer+" ]
    ]);

    defineFunction("factdouble", function(n){
        for (var i = 2 + (n&1), fact = 1; i <= n; i += 2) {
            fact *= i;
        }
        return fact;
    }).args([
        [ "*n", "integer+" ]
    ]);

    defineFunction("multinomial", function(numbers){
        var div = 1, sum = 0;
        numbers.forEach(function(n){
            if (n < 0) {
                throw new CalcError("NUM");
            }
            sum += n;
            div *= _fact(n);
        });
        return _fact(sum) / div;
    }).args([
        [ "numbers", [ "collect", "number" ] ]
    ]);

    var _combinations = util.memoize(function (n, k){
        for (var f1 = k + 1, f2 = 1, p1 = 1, p2 = 1; f2 <= n - k; ++f1, ++f2) {
            p1 *= f1;
            p2 *= f2;
        }
        return p1/p2;
    });

    defineFunction("combin", _combinations).args([
        [ "*n", "integer++" ],
        [ "*k", [ "and", "integer", [ "[between]", 0, "$n" ] ] ]
    ]);

    defineFunction("combina", function(n, k){
        return _combinations(n + k - 1, n - 1);
    }).args([
        [ "*n", "integer++" ],
        [ "*k", [ "and", "integer", [ "[between]", 1, "$n" ] ] ]
    ]);

    /* -----[ Statistical functions ]----- */

    defineFunction("average", function(numbers){
        var sum = numbers.reduce(function(sum, num){
            return sum + num;
        }, 0);
        return sum / numbers.length;
    }).args([
        // most numeric functions must treat booleans as numbers (1 for TRUE
        // and 0 for FALSE), but AVERAGE shouldn't.
        [ "numbers", [ "collect", [ "and", "number",
                                    [ "not", "boolean" ] ] ] ],
        [ "?", [ "assert", "$numbers.length > 0", "DIV/0" ] ]
    ]);

    defineFunction("averagea", function(values){
        var sum = 0, count = 0;
        values.forEach(function(num){
            if (typeof num != "string") {
                sum += num;
            }
            ++count;
        });
        return count ? sum / count : new CalcError("DIV/0");
    }).args([
        [ "values", [ "collect", "anyvalue" ] ]
    ]);

    function _percentile(numbers, rank) {
        numbers.sort(ascending);
        var n = numbers.length;
        var k = rank | 0, d = rank - k;
        if (k === 0) {
            return numbers[0];
        }
        if (k >= n) {
            return numbers[n - 1];
        }
        --k;
        return numbers[k] + d * (numbers[k + 1] - numbers[k]);
    }

    function _percentile_inc(numbers, p){
        // algorithm from https://en.wikipedia.org/wiki/Percentile#Microsoft_Excel_method
        var rank = p * (numbers.length - 1) + 1;
        return _percentile(numbers, rank);
    }

    function _percentile_exc(numbers, p){
        // https://en.wikipedia.org/wiki/Percentile#NIST_method
        var rank = p * (numbers.length + 1);
        return _percentile(numbers, rank);
    }

    defineFunction("percentile.inc", _percentile_inc).args([
        [ "numbers", [ "collect", "number", 1 ] ],
        [ "p", [ "and", "number", [ "[between]", 0, 1 ] ] ]
    ]);

    defineFunction("percentile.exc", _percentile_exc).args([
        [ "numbers", [ "collect", "number", 1 ] ],
        [ "p", [ "and", "number", [ "(between)", 0, 1 ] ] ]
    ]);

    defineFunction("quartile.inc", function(numbers, quarter){
        return _percentile_inc(numbers, quarter / 4);
    }).args([
        [ "numbers", [ "collect", "number", 1 ] ],
        [ "quarter", [ "values", 0, 1, 2, 3, 4 ] ]
    ]);

    defineFunction("quartile.exc", function(numbers, quarter){
        return _percentile_exc(numbers, quarter / 4);
    }).args([
        [ "numbers", [ "collect", "number", 1 ] ],
        [ "quarter", [ "values", 0, 1, 2, 3, 4 ] ]
    ]);

    defineAlias("quartile", "quartile.inc");
    defineAlias("percentile", "percentile.inc");

    // AGGREGATE function
    //
    // https://support.office.com/en-SG/article/aggregate-function-c8caed56-07df-4aeb-9741-23693ffbe525
    //
    // we can only partially type-check this function.  also, we need to use the async version in
    // order to resolve references and delegate values to the function to aggregate.
    defineFunction("aggregate", function(callback, funcId, options, args){
        // options is a bit field.  that makes sense; it's the documentation which doesn't.
        var self = this;
        var opt_ignore_hidden_rows = 1;
        var opt_ignore_errors = 2;
        var opt_use_aggregates = 4;
        var fname = [
            "AVERAGE", "COUNT", "COUNTA", "MAX", "MIN", "PRODUCT",
            "STDEV.S", "STDEV.P", "SUM", "VAR.S", "VAR.P", "MEDIAN",
            "MODE.SNGL", "LARGE", "SMALL", "PERCENTILE.INC",
            "QUARTILE.INC", "PERCENTILE.EXC", "QUARTILE.EXC"
        ][funcId - 1];
        var values = [];
        function fetchValues(args) {
            if (args instanceof Ref) {
                self.getRefCells(args, true).forEach(function(cell){
                    var value = cell.value;
                    if ((options & opt_ignore_hidden_rows) && cell.hidden) {
                        return;
                    }
                    if (cell.formula) {
                        // XXX: formula.print is fast, but still, can't we do any better here?
                        //      perhaps access the input string directly somehow?
                        var str = cell.formula.print(cell.row, cell.col);
                        if (/^\s*(?:aggregate|subtotal)\s*\(/i.test(str)) {
                            if (!(options & opt_use_aggregates)) {
                                return;
                            }
                        }
                        if ("value" in cell.formula) {
                            value = cell.formula.value;
                        }
                    }
                    if ((options & opt_ignore_errors) && value instanceof CalcError) {
                        return;
                    }
                    if (typeof value == "number" || value instanceof CalcError) {
                        values.push(value);
                    }
                });
            } else if (Array.isArray(args)) {
                for (var i = 0; i < args.length; ++i) {
                    fetchValues(args[i]);
                }
            } else if (args instanceof Matrix) {
                args.each(fetchValues);
            } else if (typeof args == "number") {
                values.push(args);
            } else if (args instanceof CalcError && !(options & opt_ignore_errors)) {
                values.push(args);
            }
        }
        self.resolveCells(args, function(){
            if (funcId > 12) {
                // "array form"
                fetchValues(args[0]);
                var k = args[1];
                if (k instanceof CellRef) {
                    k = self.getRefData(k);
                }
                if (typeof k != "number") {
                    return callback(new CalcError("VALUE"));
                }
            } else {
                fetchValues(args);
            }
            self.func(fname, callback, values);
        });
    }).argsAsync([
        [ "funcId", [ "values", 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                      11, 12, 13, 14, 15, 16, 17, 18, 19 ] ],
        [ "options", [ "or",
                       [ "null", 0 ],
                       [ "values", 0, 1, 2, 3, 4, 5, 6, 7  ] ] ],
        [ "args", "rest" ]
    ]);

    // https://support.office.com/en-sg/article/AVEDEV-function-ec78fa01-4755-466c-9a2b-0c4f9eacaf6d
    defineFunction("avedev", function(numbers){
        var avg = numbers.reduce(function(sum, num){
            return sum + num;
        }, 0) / numbers.length;
        return numbers.reduce(function(sum, num){
            return sum + Math.abs(num - avg);
        }, 0) / numbers.length;
    }).args([
        [ "numbers", [ "collect", "number" ] ],
        [ "?", [ "assert", "$numbers.length >= 2", "NUM" ] ]
    ]);

    function _binom_dist(x, n, p, cumulative) {
        if (!cumulative) {
            return _combinations(n, x) * Math.pow(p, x) * Math.pow(1-p, n-x);
        } else {
            var sum = 0;
            for (var j = 0; j <= x; ++j) {
                sum += _combinations(n, j) * Math.pow(p, j) * Math.pow(1-p, n-j);
            }
            return sum;
        }
    }

    defineFunction("binom.dist", _binom_dist).args([
        [ "successes", "integer+" ],
        [ "trials", [ "and", "integer", [ "assert", "$trials >= $successes" ] ] ],
        [ "probability", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "cumulative", "logical" ]
    ]);

    defineAlias("binomdist", "binom.dist");

    defineFunction("binom.inv", function(n, p, alpha){
        // XXX: could a binary search be faster?
        for (var x = 0; x <= n; ++x) {
            if (_binom_dist(x, n, p, true) >= alpha) {
                return x;
            }
        }
        return new CalcError("N/A"); // XXX: is this right?
    }).args([
        [ "trials", "integer+" ],
        [ "probability", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "alpha", [ "and", "number", [ "[between]", 0, 1 ] ] ]
    ]);

    defineAlias("critbinom", "binom.inv");

    defineFunction("binom.dist.range", function(n, p, s, s2){
        var sum = 0;
        for (var k = s; k <= s2; ++k) {
            sum += _combinations(n, k) * Math.pow(p, k) * Math.pow(1-p, n-k);
        }
        return sum;
    }).args([
        [ "trials", "integer+" ],
        [ "probability", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "successes_min", [ "and", "integer", [ "[between]", 0, "$trials" ] ] ],
        [ "successes_max", [ "or",
                             [ "and", "integer",
                               [ "[between]", "$successes_min", "$trials" ] ],
                             [ "null", "$successes_min" ] ] ]
    ]);

    defineFunction("negbinom.dist", function(x, k, p, cumulative){
        if (cumulative) {
            var sum = 0;
            while (x >= 0) {
                sum += _combinations(x+k-1, x) * Math.pow(p, k) * Math.pow(1-p, x);
                x--;
            }
            return sum;
        }
        return _combinations(x+k-1, x) * Math.pow(p, k) * Math.pow(1-p, x);
    }).args([
        [ "number_f", "integer+" ],
        [ "number_s", "integer+" ],
        [ "probability_s", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "cumulative", "logical" ]
    ]);

    defineAlias("negbinomdist", "negbinom.dist");

    /* -----[ lookup functions ]----- */

    defineFunction("address", function(row, col, abs, a1, sheet){
        // by some lucky coincidence, we get the corret `rel` value by just subtracting 1 from the
        // abs argument
        var cell = new CellRef(row - 1, col - 1, abs - 1);
        if (sheet) {
            cell.setSheet(sheet, true);
        }
        return a1 ? cell.print(0, 0) : cell.print();
    }).args([
        [ "row", "integer++" ],
        [ "col", "integer++" ],
        [ "abs", [ "or", [ "null", 1 ], [ "values", 1, 2, 3, 4 ]]],
        [ "a1", [ "or", [ "null", true ], "logical" ]],
        [ "sheet", [ "or", "null", "string" ]]
    ]);

    defineFunction("areas", function(ref){
        var count = 0;
        (function loop(x){
            if (x instanceof CellRef || x instanceof RangeRef) {
                count++;
            } else if (x instanceof UnionRef) {
                x.refs.forEach(loop);
            }
            // XXX: NameRef if we add support
        })(ref);
        return count;
    }).args([
        [ "ref", "ref" ]
    ]);

    defineFunction("choose", function(index, args){
        if (index > args.length) {
            return new CalcError("N/A");
        } else {
            return args[index - 1];
        }
    }).args([
        [ "*index", "integer" ],
        [ "+", [ "value", "anything" ] ]
    ]);

    defineFunction("column", function(ref){
        if (!ref) {
            return this.formula.col + 1;
        }
        if (ref instanceof CellRef) {
            return ref.col + 1;
        }
        return this.asMatrix(ref).mapCol(function(col){
            return col + ref.topLeft.col + 1;
        });
    }).args([
        [ "ref", [ "or", "area", "null" ]]
    ]);

    defineFunction("columns", function(m){
        return m instanceof Ref ? m.width() : m.width;
    }).args([
        [ "ref", [ "or", "area", "#matrix" ] ]
    ]);

    defineFunction("formulatext", function(ref){
        var cell = this.getRefCells(ref)[0]; // XXX: overkill, but oh well.
        if (!cell.formula) {
            return new CalcError("N/A");
        }
        return cell.formula.print(cell.row, cell.col);
    }).args([
        [ "ref", "ref" ]
    ]);

    defineFunction("hlookup", function(value, m, row, approx){
        var resultCol = null;
        m.eachCol(function(col){
            var data = m.get(0, col);
            if (approx) {
                if (data > value) {
                    return true;
                }
                resultCol = col;
            } else if (data === value) {
                resultCol = col;
                return true;
            }
        });
        if (resultCol == null) {
            return new CalcError("N/A");
        }
        return m.get(row - 1, resultCol);
    }).args([
        [ "value", "anyvalue" ],
        [ "range", "matrix" ],
        [ "row", "integer++" ],
        [ "approx", [ "or", "logical", [ "null", true ]]]
    ]);

    defineFunction("index", function(m, row, col){
        if (row == null && col == null) {
            return new CalcError("N/A");
        }
        if (m.width > 1 && m.height > 1) {
            if (row != null && col != null) {
                return m.get(row - 1, col - 1);
            }
            if (row == null) {
                return m.mapRow(function(row){
                    return m.get(row, col - 1);
                });
            }
            if (col == null) {
                return m.mapCol(function(col){
                    return m.get(row - 1, col);
                });
            }
        }
        if (m.width == 1) {
            return m.get(row - 1, 0);
        }
        if (m.height == 1) {
            return m.get(0, col - 1);
        }
        return new CalcError("REF");
    }).args([
        [ "range", "matrix" ],
        [ "row", [ "or", "integer++", "null" ]],
        [ "col", [ "or", "integer++", "null" ]]
    ]);

    defineFunction("indirect", function(thing){
        try {
            // XXX: does more work than needed.  we could go for parseReference, but that one
            // doesn't (yet?) support "SheetName!" prefix.
            var f = this.formula;
            var exp = calc.parseFormula(f.sheet, f.row, f.col, thing);
            if (!(exp.ast instanceof Ref)) {
                throw 1;
            }
            return exp.ast.absolute(f.row, f.col);
        } catch(ex) {
            return new CalcError("REF");
        }
    }).args([
        [ "thing", "string" ]
    ]);

    // XXX: LOOKUP.  seems to be deprecated in favor of HLOOKUP/VLOOKUP

    // XXX: double-check this one.
    defineFunction("match", function(val, m, type){
        var index = 1, cmp;
        if (type === 0) {
            cmp = parseCriteria(val);
        } else if (type === -1) {
            cmp = parseCriteria("<=" + val);
        } else if (type === 1) {
            cmp = parseCriteria(">=" + val);
        }
        if (m.each(function(el){
            if (el != null && cmp(el)) {
                if (type !== 0 && val != el) {
                    --index;
                }
                return true;
            }
            index++;
        }, true) && index > 0) {
            return index;
        } else {
            return new CalcError("N/A");
        }
    }).args([
        [ "value", "anyvalue" ],
        [ "range", "matrix" ],
        [ "type", [ "or",
                    [ "values", -1, 0, 1 ],
                    [ "null", 1 ]]]
    ]);

    defineFunction("offset", function(ref, rows, cols, height, width){
        var topLeft = (ref instanceof CellRef ? ref : ref.topLeft).clone();
        topLeft.row += rows;
        topLeft.col += cols;
        if (topLeft.row < 0 || topLeft.col < 0) {
            return new CalcError("VALUE");
        }
        if (height > 1 || width > 1) {
            return new RangeRef(topLeft, new CellRef(topLeft.row + height - 1,
                                                     topLeft.col + width - 1))
                .setSheet(ref.sheet, ref.hasSheet());
        }
        return topLeft;
    }).args([
        [ "ref", "area" ],
        [ "*rows", "integer" ],
        [ "*cols", "integer" ],
        [ "*height", [ "or", "integer++", [ "null", "$ref.height()" ]]],
        [ "*width", [ "or", "integer++", [ "null", "$ref.width()" ]]]
    ]);

    defineFunction("row", function(ref){
        if (!ref) {
            return this.formula.row + 1;
        }
        if (ref instanceof CellRef) {
            return ref.row + 1;
        }
        return this.asMatrix(ref).mapRow(function(row){
            return row + ref.topLeft.row + 1;
        });
    }).args([
        [ "ref", [ "or", "area", "null" ]]
    ]);

    defineFunction("rows", function(m){
        return m instanceof Ref ? m.height() : m.height;
    }).args([
        [ "ref", [ "or", "area", "#matrix" ] ]
    ]);

    defineFunction("vlookup", function(value, m, col, approx){
        var resultRow = null;
        m.eachRow(function(row){
            var data = m.get(row, 0);
            if (approx) {
                if (data > value) {
                    return true;
                }
                resultRow = row;
            } else if (data === value) {
                resultRow = row;
                return true;
            }
        });
        if (resultRow == null) {
            return new CalcError("N/A");
        }
        return m.get(resultRow, col - 1);
    }).args([
        [ "value", "anyvalue" ],
        [ "range", "matrix" ],
        [ "col", "integer++" ],
        [ "approx", [ "or", "logical", [ "null", true ]]]
    ]);

    /* -----[ Date and time functions ]----- */

    defineFunction("date", function(year, month, date){
        return packDate(year, month-1, date);
    }).args([
        [ "*year", "integer" ],
        [ "*month", "integer" ],
        [ "*date", "integer" ]
    ]);

    defineFunction("day", function(date){
        return unpackDate(date).date;
    }).args([
        [ "*date", "date" ]
    ]);

    defineFunction("month", function(date){
        return unpackDate(date).month + 1;
    }).args([
        [ "*date", "date" ]
    ]);

    defineFunction("year", function(date){
        return unpackDate(date).year;
    }).args([
        [ "*date", "date" ]
    ]);

    defineFunction("weekday", function(date){
        // XXX: TODO type
        return unpackDate(date).day + 1;
    }).args([
        [ "*date", "date" ]
    ]);

    // https://support.office.com/en-GB/article/WEEKNUM-function-e5c43a03-b4ab-426c-b411-b18c13c75340
    // XXX: this is a mess.
    defineFunction("weeknum", function(date, type){
        var fw = packDate(unpackDate(date).year, 0, 1);
        var sy = unpackDate(fw);
        var diff;
        if (type == 21) {
            // Monday-based weeks, first week is the one containing the first Thursday of the year
            // we want to place in fw and sy the first Thursday
            diff = 3 - (sy.day + 6) % 7;
            if (diff < 0) {
                diff += 7;
            }
            fw += diff;
            sy.date += diff;
            sy.day = 4;         // Thursday.
            type = 1;
        } else {
            if (type == 1) {
                type = 0;
            } else if (type == 2) {
                type = 1;
            } else {
                type = (type - 10) % 7;
            }
            // finally compatible with what we got:
            // type == 0 means week starts on Sunday
            //         1                      Monday
            //         2                      Tuesday
            // ...
        }
        diff = sy.day - type;
        if (diff < 0) {
            diff += 7;
        }
        fw -= diff;
        return Math.ceil((date + 1 - fw) / 7);
    }).args([
        [ "*date", "date" ],
        [ "*type", [ "or", [ "null", 1 ],
                     [ "values", 1, 2, 11, 12, 13, 14, 15, 16, 17, 21 ] ] ]
    ]);

    function weeksInYear(year) {
        var d = unpackDate(packDate(year, 0, 1));
        if ((d.day == 4) || (d.day == 3 && runtime.isLeapYear(year))) {
            // long year
            return 53;
        }
        return 52;
    }

    defineFunction("isoweeknum", function isoweeknum(date){
        // https://en.wikipedia.org/wiki/ISO_week_date#Calculating_the_week_number_of_a_given_date
        var d = unpackDate(date);
        var dow = d.day || 7;
        var wk = Math.floor((d.ord - dow + 10) / 7);
        if (wk < 1) {
            return weeksInYear(d.year - 1);
        } else if (wk == 53 && wk > weeksInYear(d.year)) {
            return 1;
        }
        return wk;
    }).args([
        [ "*date", "date" ]
    ]);

    defineFunction("now", function(){
        return runtime.dateToSerial(new Date());
    }).args([]);

    defineFunction("today", function(){
        return runtime.dateToSerial(new Date()) | 0;
    }).args([]);

    defineFunction("time", function(hh, mm, ss){
        return runtime.packTime(hh, mm, ss, 0);
    }).args([
        [ "*hours", "integer" ],
        [ "*minutes", "integer" ],
        [ "*seconds", "integer" ]
    ]);

    defineFunction("hour", function(time){
        return runtime.unpackTime(time).hours;
    }).args([
        [ "*time", "datetime" ]
    ]);

    defineFunction("minute", function(time){
        return runtime.unpackTime(time).minutes;
    }).args([
        [ "*time", "datetime" ]
    ]);

    defineFunction("second", function(time){
        return runtime.unpackTime(time).seconds;
    }).args([
        [ "*time", "datetime" ]
    ]);

    defineFunction("edate", function(base, months){
        var d = unpackDate(base);
        var m = d.month + months;
        var y = d.year + Math.floor(m/12);
        m %= 12;
        if (m < 0) {
            m += 12;
        }
        d = Math.min(d.date, daysInMonth(y, m));
        return packDate(y, m, d);
    }).args([
        [ "*start_date", "date" ],
        [ "*months", "integer" ]
    ]);

    defineFunction("eomonth", function(base, months){
        var d = unpackDate(base);
        var m = d.month + months;
        var y = d.year + Math.floor(m/12);
        m %= 12;
        if (m < 0) {
            m += 12;
        }
        d = daysInMonth(y, m);
        return packDate(y, m, d);
    }).args([
        [ "*start_date", "date" ],
        [ "*months", "integer" ]
    ]);

    defineFunction("workday", function(date, n, holidays){
        // XXX: the algorithm here is pretty dumb, can we do better?
        var inc = n > 0 ? 1 : -1;
        n = Math.abs(n);
        var dow = unpackDate(date).day;
        while (n > 0) {
            date += inc;
            dow = (dow + inc) % 7;
            if (dow > 0 && dow < 6 && holidays.indexOf(date) < 0) {
                --n;
            }
        }
        return date;
    }).args([
        [ "start_date", "date" ],
        [ "days", "integer" ],
        [ "holidays", [ "collect", "date" ] ]
    ]);

    defineFunction("networkdays", function(date, end, holidays){
        // XXX: the algorithm here is pretty dumb, can we do better?
        if (date > end) {
            var tmp = date;
            date = end;
            end = tmp;
        }
        var count = 0;
        var dow = unpackDate(date).day;
        while (date <= end) {
            if (dow > 0 && dow < 6 && holidays.indexOf(date) < 0) {
                count++;
            }
            date++;
            dow = (dow + 1) % 7;
        }
        return count;
    }).args([
        [ "start_date", "date" ],
        [ "end_date", "date" ],
        [ "holidays", [ "collect", "date" ] ]
    ]);

    defineFunction("days", function(start, end){
        return end - start;
    }).args([
        [ "*start_date", "date" ],
        [ "*end_date", "date" ]
    ]);

    function _days_360(start, end, method) {
        var d1 = unpackDate(start);
        var d2 = unpackDate(end);

        // https://en.wikipedia.org/wiki/360-day_calendar
        // humanity is a mess.
        if (method) {
            // EU method
            if (d1.date == 31) {
                d1.date = 30;
            }
            if (d2.date == 31) {
                d2.date = 30;
            }
        } else {
            // US method
            if (d1.month == 1 && d2.month == 1
                && d1.date == daysInMonth(d1.year, 1)
                && d2.date == daysInMonth(d2.year, 1)) {
                d2.date = 30;
            }
            if (d1.date == daysInMonth(d1.year, d1.month)) {
                d1.date = 30;
                if (d2.date == 31) {
                    d2.date = 30;
                }
            }
        }

        return (360 * (d2.year - d1.year)
                + 30 * (d2.month - d1.month)
                + (d2.date - d1.date));
    }

    defineFunction("days360", _days_360).args([
        [ "*start_date", "date" ],
        [ "*end_date", "date" ],
        [ "*method", [ "or", "logical", [ "null", "false" ] ] ]
    ]);

    defineFunction("yearfrac", function(start, end, method){
        switch (method) {
          case 0:
            return _days_360(start, end, false) / 360;
          case 1:
            return (end - start) / daysInYear(unpackDate(start).year);
          case 2:
            return (end - start) / 360;
          case 3:
            return (end - start) / 365;
          case 4:
            return _days_360(start, end, true) / 360;
        }
    }).args([
        [ "*start_date", "date" ],
        [ "*end_date", "date" ],
        [ "*method", [ "or", [ "null", 0 ], [ "values", 0, 1, 2, 3, 4 ] ] ]
    ]);

    defineFunction("datevalue", function(text){
        var date = runtime.parseDate(text);
        if (date) {
            return runtime.dateToSerial(date);
        }
        return new CalcError("VALUE");
    }).args([
        [ "*text", "string" ]
    ]);

    defineFunction("timevalue", function(text){
        var m = text.toLowerCase().match(/(\d+):(\d+)(:(\d+)(\.(\d+))?)?\s*(am?|pm?)?/);
        if (m) {
            var hh = parseFloat(m[1]);
            var mm = parseFloat(m[2]);
            var ss = m[3] ? parseFloat(m[4]) : 0;
            //var ms = m[5] ? parseFloat(m[6]) : 0;
            var ampm = m[7];
            if (ampm && (hh > 12 || hh < 1)) {
                return new CalcError("VALUE");
            }
            if (/^p/.test(ampm)) {
                hh += 12;
            }
            return runtime.packTime(hh, mm, ss, 0);
        }
        return new CalcError("VALUE");
    }).args([
        [ "*text", "string" ]
    ]);

    /* -----[ Matrix functions ]----- */

    defineFunction("mdeterm", function(m){
        var error = m.each(function(val){
            if (typeof val != "number") {
                return new CalcError("VALUE");
            }
        }, true);
        return error || m.determinant();
    }).args([
        [ "m", [ "and", "matrix",
                 [ "assert", "$m.width == $m.height" ] ] ]
    ]);

    defineFunction("transpose", function(m){
        return m.transpose();
    }).args([
        [ "range", "matrix" ]
    ]);

    defineFunction("mmult", function(a, b){
        return a.multiply(b);
    }).args([
        [ "a", "matrix" ],
        [ "b", [ "and", "matrix",
                 [ "assert", "$b.height == $a.width" ] ] ]
    ]);

    defineFunction("munit", function(n){
        return new Matrix(this).unit(n);
    }).args([
        [ "n", "integer+" ]
    ]);

    defineFunction("minverse", function(m){
        var error = m.each(function(val){
            if (typeof val != "number") {
                return new CalcError("VALUE");
            }
        }, true);
        return error || m.inverse() || new CalcError("VALUE");
    }).args([
        [ "m", [ "and", "matrix",
                 [ "assert", "$m.width == $m.height" ] ] ]
    ]);

    /* -----[ Other ]----- */

    defineFunction("rand", function() {
        return Math.random();
    }).args([]);

    defineFunction("randbetween", function(min, max){
        return min + Math.floor((max - min + 1) * Math.random());
    }).args([
        [ "min", "integer" ],
        [ "max", [ "and", "integer", [ "assert", "$max >= $min" ] ] ]
    ]);

    defineFunction("true", function(){
        return true;
    }).args([]);

    defineFunction("false", function(){
        return true;
    }).args([]);

    defineFunction("roman", function(num){
        return util.arabicToRoman(num).toUpperCase();
    }).args([
        [ "*number", "integer" ]
    ]);

    defineFunction("arabic", function(rom){
        var num = util.romanToArabic(rom);
        return num == null ? new CalcError("VALUE") : num;
    }).args([
        [ "*roman", "string" ]
    ]);

    defineFunction("base", function(number, radix, minLen){
        var str = number.toString(radix).toUpperCase();
        while (str.length < minLen) {
            str = "0" + str;
        }
        return str;
    }).args([
        [ "*number", "integer" ],
        [ "*radix", [ "and", "integer", [ "[between]", 2, 36 ] ] ],
        [ "*minLen", [ "or", "integer+", [ "null", 0 ] ] ]
    ]);

    defineFunction("decimal", function(text, radix){
        text = text.toUpperCase();
        var val = 0;
        for (var i = 0; i < text.length; ++i) {
            var d = text.charCodeAt(i);
            if (d >= 48 && d <= 57) {
                d -= 48;
            } else if (d >= 65 && d < (55 + radix)) {
                d -= 55;
            } else {
                return new CalcError("VALUE");
            }
            val = val * radix + d;
        }
        return val;
    }).args([
        [ "*text", "string" ],
        [ "*radix", [ "and", "integer", [ "[between]", 2, 36 ] ] ]
    ]);

    /* -----[ String functions ]----- */

    defineFunction("char", function(code){
        return String.fromCharCode(code);
    }).args([
        [ "*code", "integer+" ]
    ]);

    // From XRegExp
    var RX_NON_PRINTABLE = /[\0-\x1F\x7F-\x9F\xAD\u0378\u0379\u037F-\u0383\u038B\u038D\u03A2\u0528-\u0530\u0557\u0558\u0560\u0588\u058B-\u058E\u0590\u05C8-\u05CF\u05EB-\u05EF\u05F5-\u0605\u061C\u061D\u06DD\u070E\u070F\u074B\u074C\u07B2-\u07BF\u07FB-\u07FF\u082E\u082F\u083F\u085C\u085D\u085F-\u089F\u08A1\u08AD-\u08E3\u08FF\u0978\u0980\u0984\u098D\u098E\u0991\u0992\u09A9\u09B1\u09B3-\u09B5\u09BA\u09BB\u09C5\u09C6\u09C9\u09CA\u09CF-\u09D6\u09D8-\u09DB\u09DE\u09E4\u09E5\u09FC-\u0A00\u0A04\u0A0B-\u0A0E\u0A11\u0A12\u0A29\u0A31\u0A34\u0A37\u0A3A\u0A3B\u0A3D\u0A43-\u0A46\u0A49\u0A4A\u0A4E-\u0A50\u0A52-\u0A58\u0A5D\u0A5F-\u0A65\u0A76-\u0A80\u0A84\u0A8E\u0A92\u0AA9\u0AB1\u0AB4\u0ABA\u0ABB\u0AC6\u0ACA\u0ACE\u0ACF\u0AD1-\u0ADF\u0AE4\u0AE5\u0AF2-\u0B00\u0B04\u0B0D\u0B0E\u0B11\u0B12\u0B29\u0B31\u0B34\u0B3A\u0B3B\u0B45\u0B46\u0B49\u0B4A\u0B4E-\u0B55\u0B58-\u0B5B\u0B5E\u0B64\u0B65\u0B78-\u0B81\u0B84\u0B8B-\u0B8D\u0B91\u0B96-\u0B98\u0B9B\u0B9D\u0BA0-\u0BA2\u0BA5-\u0BA7\u0BAB-\u0BAD\u0BBA-\u0BBD\u0BC3-\u0BC5\u0BC9\u0BCE\u0BCF\u0BD1-\u0BD6\u0BD8-\u0BE5\u0BFB-\u0C00\u0C04\u0C0D\u0C11\u0C29\u0C34\u0C3A-\u0C3C\u0C45\u0C49\u0C4E-\u0C54\u0C57\u0C5A-\u0C5F\u0C64\u0C65\u0C70-\u0C77\u0C80\u0C81\u0C84\u0C8D\u0C91\u0CA9\u0CB4\u0CBA\u0CBB\u0CC5\u0CC9\u0CCE-\u0CD4\u0CD7-\u0CDD\u0CDF\u0CE4\u0CE5\u0CF0\u0CF3-\u0D01\u0D04\u0D0D\u0D11\u0D3B\u0D3C\u0D45\u0D49\u0D4F-\u0D56\u0D58-\u0D5F\u0D64\u0D65\u0D76-\u0D78\u0D80\u0D81\u0D84\u0D97-\u0D99\u0DB2\u0DBC\u0DBE\u0DBF\u0DC7-\u0DC9\u0DCB-\u0DCE\u0DD5\u0DD7\u0DE0-\u0DF1\u0DF5-\u0E00\u0E3B-\u0E3E\u0E5C-\u0E80\u0E83\u0E85\u0E86\u0E89\u0E8B\u0E8C\u0E8E-\u0E93\u0E98\u0EA0\u0EA4\u0EA6\u0EA8\u0EA9\u0EAC\u0EBA\u0EBE\u0EBF\u0EC5\u0EC7\u0ECE\u0ECF\u0EDA\u0EDB\u0EE0-\u0EFF\u0F48\u0F6D-\u0F70\u0F98\u0FBD\u0FCD\u0FDB-\u0FFF\u10C6\u10C8-\u10CC\u10CE\u10CF\u1249\u124E\u124F\u1257\u1259\u125E\u125F\u1289\u128E\u128F\u12B1\u12B6\u12B7\u12BF\u12C1\u12C6\u12C7\u12D7\u1311\u1316\u1317\u135B\u135C\u137D-\u137F\u139A-\u139F\u13F5-\u13FF\u169D-\u169F\u16F1-\u16FF\u170D\u1715-\u171F\u1737-\u173F\u1754-\u175F\u176D\u1771\u1774-\u177F\u17DE\u17DF\u17EA-\u17EF\u17FA-\u17FF\u180F\u181A-\u181F\u1878-\u187F\u18AB-\u18AF\u18F6-\u18FF\u191D-\u191F\u192C-\u192F\u193C-\u193F\u1941-\u1943\u196E\u196F\u1975-\u197F\u19AC-\u19AF\u19CA-\u19CF\u19DB-\u19DD\u1A1C\u1A1D\u1A5F\u1A7D\u1A7E\u1A8A-\u1A8F\u1A9A-\u1A9F\u1AAE-\u1AFF\u1B4C-\u1B4F\u1B7D-\u1B7F\u1BF4-\u1BFB\u1C38-\u1C3A\u1C4A-\u1C4C\u1C80-\u1CBF\u1CC8-\u1CCF\u1CF7-\u1CFF\u1DE7-\u1DFB\u1F16\u1F17\u1F1E\u1F1F\u1F46\u1F47\u1F4E\u1F4F\u1F58\u1F5A\u1F5C\u1F5E\u1F7E\u1F7F\u1FB5\u1FC5\u1FD4\u1FD5\u1FDC\u1FF0\u1FF1\u1FF5\u1FFF\u200B-\u200F\u202A-\u202E\u2060-\u206F\u2072\u2073\u208F\u209D-\u209F\u20BB-\u20CF\u20F1-\u20FF\u218A-\u218F\u23F4-\u23FF\u2427-\u243F\u244B-\u245F\u2700\u2B4D-\u2B4F\u2B5A-\u2BFF\u2C2F\u2C5F\u2CF4-\u2CF8\u2D26\u2D28-\u2D2C\u2D2E\u2D2F\u2D68-\u2D6E\u2D71-\u2D7E\u2D97-\u2D9F\u2DA7\u2DAF\u2DB7\u2DBF\u2DC7\u2DCF\u2DD7\u2DDF\u2E3C-\u2E7F\u2E9A\u2EF4-\u2EFF\u2FD6-\u2FEF\u2FFC-\u2FFF\u3040\u3097\u3098\u3100-\u3104\u312E-\u3130\u318F\u31BB-\u31BF\u31E4-\u31EF\u321F\u32FF\u4DB6-\u4DBF\u9FCD-\u9FFF\uA48D-\uA48F\uA4C7-\uA4CF\uA62C-\uA63F\uA698-\uA69E\uA6F8-\uA6FF\uA78F\uA794-\uA79F\uA7AB-\uA7F7\uA82C-\uA82F\uA83A-\uA83F\uA878-\uA87F\uA8C5-\uA8CD\uA8DA-\uA8DF\uA8FC-\uA8FF\uA954-\uA95E\uA97D-\uA97F\uA9CE\uA9DA-\uA9DD\uA9E0-\uA9FF\uAA37-\uAA3F\uAA4E\uAA4F\uAA5A\uAA5B\uAA7C-\uAA7F\uAAC3-\uAADA\uAAF7-\uAB00\uAB07\uAB08\uAB0F\uAB10\uAB17-\uAB1F\uAB27\uAB2F-\uABBF\uABEE\uABEF\uABFA-\uABFF\uD7A4-\uD7AF\uD7C7-\uD7CA\uD7FC-\uF8FF\uFA6E\uFA6F\uFADA-\uFAFF\uFB07-\uFB12\uFB18-\uFB1C\uFB37\uFB3D\uFB3F\uFB42\uFB45\uFBC2-\uFBD2\uFD40-\uFD4F\uFD90\uFD91\uFDC8-\uFDEF\uFDFE\uFDFF\uFE1A-\uFE1F\uFE27-\uFE2F\uFE53\uFE67\uFE6C-\uFE6F\uFE75\uFEFD-\uFF00\uFFBF-\uFFC1\uFFC8\uFFC9\uFFD0\uFFD1\uFFD8\uFFD9\uFFDD-\uFFDF\uFFE7\uFFEF-\uFFFB\uFFFE\uFFFF]/g;

    defineFunction("clean", function(text){
        return text.replace(RX_NON_PRINTABLE, "");
    }).args([
        [ "*text", "string" ]
    ]);

    defineFunction("code", function(text){
        return text.charAt(0);
    }).args([
        [ "*text", "string" ]
    ]);

    defineAlias("unichar", "char");
    defineAlias("unicode", "code");

    defineFunction("concatenate", function(){
        var out = "";
        for (var i = 0; i < arguments.length; ++i) {
            out += arguments[i];
        }
        return out;
    }).args([
        [ "+",
          [ "*text", "string" ] ]
    ]);

    defineFunction("dollar", function(number, decimals){
        var format = "$#,##0.DECIMALS;($#,##0.DECIMALS)";
        var dec = "";
        while (decimals-- > 0) { dec += "0"; }
        format = format.replace(/DECIMALS/g, dec);
        return spreadsheet.formatting.format(number, format).text();
    }).args([
        [ "*number", "number" ],
        [ "*decimals", [ "or", "integer++", [ "null", 2 ] ] ]
    ]);

    defineFunction("exact", function(a, b){
        return a === b;
    }).args([
        [ "*text1", "string" ],
        [ "*text2", "string" ]
    ]);

    defineFunction("find", function(substring, string, start){
        var pos = string.indexOf(substring, start - 1);
        return pos < 0 ? new CalcError("VALUE") : pos + 1;
    }).args([
        [ "*substring", "string" ],
        [ "*string", "string" ],
        [ "*start", [ "or", "integer++", [ "null", 1 ] ] ]
    ]);

    defineFunction("fixed", function(number, decimals, noCommas){
        var format = noCommas ? "0.DECIMALS" : "#,##0.DECIMALS";
        var dec = "";
        while (decimals-- > 0) { dec += "0"; }
        format = format.replace(/DECIMALS/g, dec);
        return spreadsheet.formatting.format(number, format).text();
    }).args([
        [ "*number", "number" ],
        [ "*decimals", [ "or", "integer++", [ "null", 2 ] ] ],
        [ "*noCommas", [ "or", "boolean", [ "null", false ] ] ]
    ]);

    defineFunction("left", function(text, length){
        return text.substr(0, length);
    }).args([
        [ "*text", "string" ],
        [ "*length", [ "or", "integer+", [ "null", 1 ] ] ]
    ]);

    defineFunction("right", function(text, length){
        return text.substr(-length);
    }).args([
        [ "*text", "string" ],
        [ "*length", [ "or", "integer+", [ "null", 1 ] ] ]
    ]);

    defineFunction("len", function(text){
        return text.length;
    }).args([
        [ "*text", "string" ]
    ]);

    defineFunction("lower", function(text){
        return text.toLowerCase();
    }).args([
        [ "*text", "string" ]
    ]);

    defineFunction("upper", function(text){
        return text.toUpperCase();
    }).args([
        [ "*text", "string" ]
    ]);

    defineFunction("ltrim", function(text){
        return text.replace(/^\s+/, "");
    }).args([
        [ "*text", "string" ]
    ]);

    defineFunction("rtrim", function(text){
        return text.replace(/\s+$/, "");
    }).args([
        [ "*text", "string" ]
    ]);

    defineFunction("trim", function(text){
        return text.replace(/^\s+|\s+$/, "");
    }).args([
        [ "*text", "string" ]
    ]);

    defineFunction("mid", function(text, start, length){
        return text.substr(start - 1, length);
    }).args([
        [ "*text", "string" ],
        [ "*start", "integer++" ],
        [ "*length", "integer+" ]
    ]);

    defineFunction("proper", function(text){
        return text.toLowerCase().replace(/\b./g, function(s){
            return s.toUpperCase();
        });
    }).args([
        [ "*text", "string" ]
    ]);

    defineFunction("replace", function(text, start, length, newText){
        return text.substr(0, --start) + newText + text.substr(start + length);
    }).args([
        [ "*text", "string" ],
        [ "*start", "integer++" ],
        [ "*length", "integer+" ],
        [ "*newText", "string" ]
    ]);

    defineFunction("rept", function(text, number){
        var out = "";
        while (number-- > 0) { out += text; }
        return out;
    }).args([
        [ "*text", "string" ],
        [ "*number", "integer+" ]
    ]);

    defineFunction("search", function(substring, string, start){
        var pos = string.toLowerCase().indexOf(substring.toLowerCase(), start - 1);
        return pos < 0 ? new CalcError("VALUE") : pos + 1;
    }).args([
        [ "*substring", "string" ],
        [ "*string", "string" ],
        [ "*start", [ "or", "integer++", [ "null", 1 ] ] ]
    ]);

    defineFunction("substitute", function(text, oldText, newText, nth){
        if (oldText === newText) {
            return text;
        }
        var pos = -1;
        function replace() {
            text = text.substring(0, pos) + newText + text.substring(pos + oldText.length);
        }
        while ((pos = text.indexOf(oldText, pos + 1)) >= 0) {
            if (nth == null) {
                replace();
            } else if (--nth === 0) {
                replace();
                break;
            }
        }
        return text;
    }).args([
        [ "*text", "string" ],
        [ "*oldText", "string" ],
        [ "*newText", "string" ],
        [ "*nth", [ "or", "integer++", "null" ] ]
    ]);

    defineFunction("t", function(value){
        return typeof value == "string" ? value : "";
    }).args([
        [ "*value", "anyvalue" ]
    ]);

    defineFunction("text", function(value, format){
        return spreadsheet.formatting.format(value, format).text();
    }).args([
        [ "*value", "anyvalue" ],
        [ "*format", "string" ]
    ]);

    defineFunction("value", function(value){
        if (typeof value == "number") {
            return value;
        }
        if (typeof value == "boolean") {
            return +value;
        }
        // XXX: this is dirty.  we need it so we can parse i.e. "$12,345.50"
        value = (value+"").replace(/[$€,]/g, "");
        value = parseFloat(value);
        return isNaN(value) ? new CalcError("VALUE") : value;
    }).args([
        [ "*value", "anyvalue" ]
    ]);

    //// utils

    var parseCriteria = (function(){
        var RXCACHE = Object.create(null);

        function makeComparator(cmp, x) {
            if (typeof x == "string") {
                var num = parseFloat(x);
                if (!isNaN(num)) {
                    x = num;
                }
            }
            return function(a) {
                var b = x;
                if (typeof a == "string" && typeof b == "string") {
                    a = a.toLowerCase();
                    b = b.toLowerCase();
                }
                return cmp(a, b);
            };
        }

        function lc(a) {
            if (typeof a == "string") {
                return a.toLowerCase();
            }
            return a;
        }

        function compLT(a, b) { return lc(a) < lc(b); }
        function compLTE(a, b) { return lc(a) <= lc(b); }
        function compGT(a, b) { return lc(a) > lc(b); }
        function compGTE(a, b) { return lc(a) >= lc(b); }
        function compNE(a, b) { return lc(a) != lc(b); }
        function compEQ(a, b) {
            if (b instanceof RegExp) {
                return b.test(a);
            }
            return lc(a) == lc(b);
        }

        return function(cmp) {
            if (typeof cmp == "function") {
                return cmp;
            }
            var m;
            if ((m = /^=(.*)$/.exec(cmp))) {
                return makeComparator(compEQ, m[1]);
            }
            if ((m = /^<>(.*)$/.exec(cmp))) {
                return makeComparator(compNE, m[1]);
            }
            if ((m = /^<=(.*)$/.exec(cmp))) {
                return makeComparator(compLTE, m[1]);
            }
            if ((m = /^<(.*)$/.exec(cmp))) {
                return makeComparator(compLT, m[1]);
            }
            if ((m = /^>=(.*)$/.exec(cmp))) {
                return makeComparator(compGTE, m[1]);
            }
            if ((m = /^>(.*)$/.exec(cmp))) {
                return makeComparator(compGT, m[1]);
            }
            if (/[?*]/.exec(cmp)) {
                // has wildchars
                var rx = RXCACHE[cmp];
                if (!rx) {
                    rx = cmp.replace(/(~\?|~\*|[\]({\+\.\|\^\$\\})\[]|[?*])/g, function(s){
                        switch (s) {
                          case "~?" : return "\\?";
                          case "~*" : return "\\*";
                          case "?"  : return ".";
                          case "*"  : return ".*";
                          default   : return "\\" + s;
                        }
                    });
                    rx = RXCACHE[cmp] = new RegExp("^" + rx + "$", "i");
                }
                return makeComparator(compEQ, rx);
            }
            return makeComparator(compEQ, cmp);
        };
    })();

    function numericPredicate(val) {
        return typeof val == "number"
            || typeof val == "boolean"
            || val == null
            || val === "";
    }

    function ascending(a, b) {
        return a === b ? 0 : a < b ? -1 : 1;
    }

    function descending(a, b) {
        return a === b ? 0 : a < b ? 1 : -1;
    }

    // var m = new Matrix();
    // m.data = [[1, 2, 3], [4, 1, 6], [7, 8, 9]];
    // //m.data = [[0, 2, 1], [2, 6, 1], [1, 1, 4]];
    // //m.data = [[1,4,1,1], [1,4,0,1], [2,3,1,2], [3,2,6,4]];
    // m.width = m.height = 3;
    // var x = m.inverse();
    // x.dump();

})();

(function(){
    "use strict";

    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    // WARNING: removing the following jshint declaration and turning
    // == into === to make JSHint happy will break functionality.
    /* jshint eqnull:true, newcap:false, laxbreak:true, validthis:true */
    /* jshint latedef: nofunc */

    var spreadsheet = kendo.spreadsheet;
    var calc = spreadsheet.calc;
    var runtime = calc.runtime;
    var defineFunction = runtime.defineFunction;
    var CalcError = runtime.CalcError;

    /* -----[ Spreadsheet API ]----- */

    defineFunction("ERF", function(ll, ul) {
        if (ul == null) {
            return ERF(ll);
        }
        return ERF(ul) - ERF(ll);
    }).args([
        [ "lower_limit", "number" ],
        [ "upper_limit", [ "or", "number", "null" ] ]
    ]);

    defineFunction("ERFC", ERFC).args([
        [ "x", "number" ]
    ]);

    defineFunction("GAMMALN", GAMMALN).args([
        [ "x", "number++" ]
    ]);

    defineFunction("GAMMA", GAMMA).args([
        [ "x", "number" ]
    ]);

    defineFunction("GAMMA.DIST", GAMMA_DIST).args([
        [ "x", "number+" ],
        [ "alpha", "number++" ],
        [ "beta", "number++" ],
        [ "cumulative", "logical" ]
    ]);

    defineFunction("GAMMA.INV", GAMMA_INV).args([
        [ "p", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "alpha", "number++" ],
        [ "beta", "number++" ]
    ]);

    defineFunction("NORM.S.DIST", NORM_S_DIST).args([
        [ "z", "number" ],
        [ "cumulative", "logical" ]
    ]);

    defineFunction("NORM.S.INV", NORM_S_INV).args([
        [ "p", [ "and", "number", [ "[between]", 0, 1 ] ] ]
    ]);

    defineFunction("NORM.DIST", NORM_DIST).args([
        [ "x", "number" ],
        [ "mean", "number" ],
        [ "stddev", "number++" ],
        [ "cumulative", "logical" ]
    ]);

    defineFunction("NORM.INV", NORM_INV).args([
        [ "p", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "mean", "number" ],
        [ "stddev", "number++" ]
    ]);

    defineFunction("BETADIST", BETADIST).args([
        [ "x", "number" ],
        [ "alpha", "number++" ],
        [ "beta", "number++" ],
        [ "A", [ "or", "number", [ "null", 0 ] ] ],
        [ "B", [ "or", "number", [ "null", 1 ] ] ],
        [ "?", [ "assert", "$x >= $A", "NUM" ] ],
        [ "?", [ "assert", "$x <= $B", "NUM" ] ],
        [ "?", [ "assert", "$A < $B", "NUM" ] ]
    ]);

    defineFunction("BETA.DIST", BETA_DIST).args([
        [ "x", "number" ],
        [ "alpha", "number++" ],
        [ "beta", "number++" ],
        [ "cumulative", "logical" ],
        [ "A", [ "or", "number", [ "null", 0 ] ] ],
        [ "B", [ "or", "number", [ "null", 1 ] ] ],
        [ "?", [ "assert", "$x >= $A", "NUM" ] ],
        [ "?", [ "assert", "$x <= $B", "NUM" ] ],
        [ "?", [ "assert", "$A < $B", "NUM" ] ]
    ]);

    defineFunction("BETA.INV", BETA_INV).args([
        [ "p", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "alpha", "number++" ],
        [ "beta", "number++" ],
        [ "A", [ "or", "number", [ "null", 0 ] ] ],
        [ "B", [ "or", "number", [ "null", 1 ] ] ]
    ]);

    defineFunction("CHISQ.DIST", chisq_left).args([
        [ "x", "number+" ],
        [ "deg_freedom", "integer++" ],
        [ "cumulative", "logical" ]
    ]);

    defineFunction("CHISQ.DIST.RT", chisq_right).args([
        [ "x", "number+" ],
        [ "deg_freedom", "integer++" ]
    ]);

    defineFunction("CHISQ.INV", chisq_left_inv).args([
        [ "p", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "deg_freedom", "integer++" ]
    ]);

    defineFunction("CHISQ.INV.RT", chisq_right_inv).args([
        [ "p", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "deg_freedom", "integer++" ]
    ]);

    defineFunction("CHISQ.TEST", function(ac, ex){
        return chisq_test(ac.data, ex.data);
    }).args([
        [ "actual_range", "matrix" ],
        [ "expected_range", "matrix" ],
        [ "?", [ "assert", "$actual_range.width == $expected_range.width" ] ],
        [ "?", [ "assert", "$actual_range.height == $expected_range.height" ] ]
    ]);

    defineFunction("EXPON.DIST", expon).args([
        [ "x", "number+" ],
        [ "lambda", "number++" ],
        [ "cumulative", "logical" ]
    ]);

    defineFunction("POISSON.DIST", poisson).args([
        [ "x", "integer+" ],
        [ "mean", "number+" ],
        [ "cumulative", "logical" ]
    ]);

    defineFunction("F.DIST", Fdist).args([
        [ "x", "number+" ],
        [ "deg_freedom1", "integer++" ],
        [ "deg_freedom2", "integer++" ],
        [ "cumulative", "logical" ]
    ]);

    defineFunction("F.DIST.RT", Fdist_right).args([
        [ "x", "number+" ],
        [ "deg_freedom1", "integer++" ],
        [ "deg_freedom2", "integer++" ]
    ]);

    defineFunction("F.INV", Finv).args([
        [ "p", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "deg_freedom1", "integer++" ],
        [ "deg_freedom2", "integer++" ]
    ]);

    defineFunction("F.INV.RT", Finv_right).args([
        [ "p", [ "and", "number", [ "[between]", 0, 1 ] ] ],
        [ "deg_freedom1", "integer++" ],
        [ "deg_freedom2", "integer++" ]
    ]);

    defineFunction("F.TEST", Ftest).args([
        [ "array1", [ "collect", "number", 1 ] ],
        [ "array2", [ "collect", "number", 1 ] ],
        [ "?", [ "assert", "$array1.length >= 2", "DIV/0" ] ],
        [ "?", [ "assert", "$array2.length >= 2", "DIV/0" ] ]
    ]);

    defineFunction("FISHER", fisher).args([
        [ "x", [ "and", "number", [ "(between)", -1, 1 ] ] ]
    ]);

    defineFunction("FISHERINV", fisherinv).args([
        [ "y", "number" ]
    ]);

    defineFunction("T.DIST", Tdist).args([
        [ "x", "number" ],
        [ "deg_freedom", "integer++" ],
        [ "cumulative", "logical" ]
    ]);

    defineFunction("T.DIST.RT", Tdist_right).args([
        [ "x", "number" ],
        [ "deg_freedom", "integer++" ]
    ]);

    defineFunction("T.DIST.2T", Tdist_2tail).args([
        [ "x", "number+" ],
        [ "deg_freedom", "integer++" ]
    ]);

    defineFunction("T.INV", Tdist_inv).args([
        [ "p", [ "and", "number", [ "(between]", 0, 1 ] ] ],
        [ "deg_freedom", "integer++" ]
    ]);

    defineFunction("T.INV.2T", Tdist_2tail_inv).args([
        [ "p", [ "and", "number", [ "(between]", 0, 1 ] ] ],
        [ "deg_freedom", "integer++" ]
    ]);

    defineFunction("T.TEST", Tdist_test).args([
        [ "array1", [ "collect", "number", 1 ] ],
        [ "array2", [ "collect", "number", 1 ] ],
        [ "tails", [ "and", "integer", [ "values", 1, 2 ] ] ],
        [ "type", [ "and", "integer", [ "values", 1, 2, 3 ] ] ],
        [ "?", [ "assert", "$type != 1 || $array1.length == $array2.length", "N/A" ] ],
        [ "?", [ "assert", "$array1.length >= 2", "DIV/0" ] ],
        [ "?", [ "assert", "$array2.length >= 2", "DIV/0" ] ]
    ]);

    defineFunction("CONFIDENCE.T", confidence_t).args([
        [ "alpha", [ "and", "number", [ "(between)", 0, 1 ] ] ],
        [ "standard_dev", "number++" ],
        [ "size", [ "and", "integer++",
                    [ "assert", "$size != 1", "DIV/0" ] ] ]
    ]);

    defineFunction("CONFIDENCE.NORM", confidence_norm).args([
        [ "alpha", [ "and", "number", [ "(between)", 0, 1 ] ] ],
        [ "standard_dev", "number++" ],
        [ "size", [ "and", "integer++" ] ]
    ]);

    defineFunction("GAUSS", gauss).args([
        [ "z", "number" ]
    ]);

    defineFunction("PHI", phi).args([
        [ "x", "number" ]
    ]);

    defineFunction("LOGNORM.DIST", lognorm_dist).args([
        [ "x", "number++" ],
        [ "mean", "number" ],
        [ "standard_dev", "number++" ],
        [ "cumulative", "logical" ]
    ]);

    defineFunction("LOGNORM.INV", lognorm_inv).args([
        [ "probability", [ "and", "number", [ "(between)", 0, 1 ] ] ],
        [ "mean", "number" ],
        [ "standard_dev", "number++" ]
    ]);

    defineFunction("PROB", prob).args([
        [ "x_range", [ "collect", "number", 1 ] ],
        [ "prob_range", [ "collect", "number", 1 ] ],
        [ "lower_limit", "number" ],
        [ "upper_limit", [ "or", "number", [ "null", "$lower_limit" ] ] ],
        [ "?", [ "assert", "$prob_range.length == $x_range.length", "N/A" ] ]
    ]);

    defineFunction("SLOPE", slope).args([
        [ "known_y", [ "collect", "number", 1 ] ],
        [ "known_x", [ "collect", "number", 1 ] ],
        [ "?", [ "assert", "$known_x.length == $known_y.length", "N/A" ] ],
        [ "?", [ "assert", "$known_x.length > 0 && $known_y.length > 0", "N/A" ] ]
    ]);

    defineFunction("INTERCEPT", intercept).args([
        [ "known_y", [ "collect", "number", 1 ] ],
        [ "known_x", [ "collect", "number", 1 ] ],
        [ "?", [ "assert", "$known_x.length == $known_y.length", "N/A" ] ],
        [ "?", [ "assert", "$known_x.length > 0 && $known_y.length > 0", "N/A" ] ]
    ]);

    defineFunction("PEARSON", pearson).args([
        [ "array1", [ "collect", "number", 1 ] ],
        [ "array2", [ "collect", "number", 1 ] ],
        [ "?", [ "assert", "$array2.length == $array1.length", "N/A" ] ],
        [ "?", [ "assert", "$array2.length > 0 && $array1.length > 0", "N/A" ] ]
    ]);

    defineFunction("RSQ", rsq).args([
        [ "known_y", [ "collect", "number", 1 ] ],
        [ "known_x", [ "collect", "number", 1 ] ],
        [ "?", [ "assert", "$known_x.length == $known_y.length", "N/A" ] ],
        [ "?", [ "assert", "$known_x.length > 0 && $known_y.length > 0", "N/A" ] ],
        [ "?", [ "assert", "$known_x.length != 1 && $known_y.length != 1", "N/A" ] ]
    ]);

    defineFunction("STEYX", steyx).args([
        [ "known_y", [ "collect", "number", 1 ] ],
        [ "known_x", [ "collect", "number", 1 ] ],
        [ "?", [ "assert", "$known_x.length == $known_y.length", "N/A" ] ],
        [ "?", [ "assert", "$known_x.length >= 3 && $known_y.length >= 3", "DIV/0" ] ]
    ]);

    defineFunction("FORECAST", forecast).args([
        [ "x", "number" ],
        [ "known_y", [ "collect", "number", 1 ] ],
        [ "known_x", [ "collect", "number", 1 ] ],
        [ "?", [ "assert", "$known_x.length == $known_y.length", "N/A" ] ],
        [ "?", [ "assert", "$known_x.length > 0 && $known_y.length > 0", "N/A" ] ]
    ]);

    defineFunction("LINEST", linest).args([
        [ "known_y", "matrix" ],
        [ "known_x", [ "or", "matrix", "null" ] ],
        [ "const", [ "or", "logical", [ "null", true ] ] ],
        [ "stats", [ "or", "logical", [ "null", false ] ] ]
    ]);

    defineFunction("LOGEST", logest).args([
        [ "known_y", "matrix" ],
        [ "known_x", [ "or", "matrix", "null" ] ],
        [ "const", [ "or", "logical", [ "null", true ] ] ],
        [ "stats", [ "or", "logical", [ "null", false ] ] ]
    ]);

    defineFunction("TREND", trend).args([
        [ "known_y", "matrix" ],
        [ "known_x", [ "or", "matrix", "null" ] ],
        [ "new_x", [ "or", "matrix", "null" ] ],
        [ "const", [ "or", "logical", [ "null", true ] ] ]
    ]);

    defineFunction("GROWTH", growth).args([
        [ "known_y", "matrix" ],
        [ "known_x", [ "or", "matrix", "null" ] ],
        [ "new_x", [ "or", "matrix", "null" ] ],
        [ "const", [ "or", "logical", [ "null", true ] ] ]
    ]);

    defineFunction("FV", FV).args([
        [ "rate", "number" ],
        [ "nper", "number" ],
        [ "pmt", [ "or", "number", [ "null", 0 ] ] ],
        [ "pv", [ "or", "number", [ "null", 0 ] ] ],
        [ "type", [ "or", [ "values", 0, 1 ], [ "null", 0 ] ] ],
        [ "?", [ "assert", "$pmt || $pv" ] ]
    ]);

    defineFunction("PV", PV).args([
        [ "rate", "number" ],
        [ "nper", "number" ],
        [ "pmt", [ "or", "number", [ "null", 0 ] ] ],
        [ "fv", [ "or", "number", [ "null", 0 ] ] ],
        [ "type", [ "or", [ "values", 0, 1 ], [ "null", 0 ] ] ],
        [ "?", [ "assert", "$pmt || $fv" ] ]
    ]);

    defineFunction("PMT", PMT).args([
        [ "rate", "number" ],
        [ "nper", "number" ],
        [ "pmt", "number" ],
        [ "fv", [ "or", "number", [ "null", 0 ] ] ],
        [ "type", [ "or", [ "values", 0, 1 ], [ "null", 0 ] ] ]
    ]);

    defineFunction("NPER", NPER).args([
        [ "rate", "number" ],
        [ "pmt", "number" ],
        [ "pv", "number" ],
        [ "fv", [ "or", "number", [ "null", 0 ] ] ],
        [ "type", [ "or", [ "values", 0, 1 ], [ "null", 0 ] ] ]
    ]);

    defineFunction("RATE", RATE).args([
        [ "nper", "number" ],
        [ "pmt", [ "or", "number", [ "null", 0 ] ] ],
        [ "pv", "number" ],
        [ "fv", [ "or", "number", [ "null", 0 ] ] ],
        [ "type", [ "or", [ "values", 0, 1 ], [ "null", 0 ] ] ],
        [ "guess", [ "or", "number++", [ "null", 0.01 ] ] ],
        [ "?", [ "assert", "$pmt || $fv" ] ]
    ]);

    defineFunction("IPMT", IPMT).args([
        [ "rate", "number" ],
        [ "per", "number++" ],
        [ "nper", "number++" ],
        [ "pv", "number" ],
        [ "fv", [ "or", "number", [ "null", 0 ] ] ],
        [ "type", [ "or", [ "values", 0, 1 ], [ "null", 0 ] ] ],
        [ "?", [ "assert", "$per >= 1 && $per <= $nper" ] ]
    ]);

    defineFunction("PPMT", PPMT).args([
        [ "rate", "number" ],
        [ "per", "number++" ],
        [ "nper", "number++" ],
        [ "pv", "number" ],
        [ "fv", [ "or", "number", [ "null", 0 ] ] ],
        [ "type", [ "or", [ "values", 0, 1 ], [ "null", 0 ] ] ],
        [ "?", [ "assert", "$per >= 1 && $per <= $nper" ] ]
    ]);

    defineFunction("CUMPRINC", CUMPRINC).args([
        [ "rate", "number++" ],
        [ "nper", "number++" ],
        [ "pv", "number++" ],
        [ "start_period", "number++" ],
        [ "end_period", "number++" ],
        [ "type", [ "or", [ "values", 0, 1 ], [ "null", 0 ] ] ],
        [ "?", [ "assert", "$end_period >= $start_period", "NUM" ] ]
    ]);

    defineFunction("CUMIPMT", CUMIPMT).args([
        [ "rate", "number++" ],
        [ "nper", "number++" ],
        [ "pv", "number++" ],
        [ "start_period", "number++" ],
        [ "end_period", "number++" ],
        [ "type", [ "or", [ "values", 0, 1 ], [ "null", 0 ] ] ],
        [ "?", [ "assert", "$end_period >= $start_period", "NUM" ] ]
    ]);

    defineFunction("NPV", NPV).args([
        [ "rate", "number" ],
        [ "values", [ "collect", "number" ] ],
        [ "?", [ "assert", "$values.length > 0", "N/A" ] ]
    ]);

    defineFunction("IRR", IRR).args([
        [ "values", [ "collect", "number", 1 ] ],
        [ "guess", [ "or", "number", [ "null", 0.1 ] ] ]
    ]);

    defineFunction("EFFECT", EFFECT).args([
        [ "nominal_rate", "number++" ],
        [ "npery", "integer++" ]
    ]);

    defineFunction("NOMINAL", NOMINAL).args([
        [ "effect_rate", "number++" ],
        [ "npery", "integer++" ]
    ]);

    defineFunction("XNPV", XNPV).args([
        [ "rate", "number" ],
        [ "values", [ "collect", "number", 1 ] ],
        [ "dates", [ "collect", "date", 1 ] ],
        [ "?", [ "assert", "$values.length == $dates.length", "NUM" ] ]
    ]);

    defineFunction("XIRR", XIRR).args([
        [ "values", [ "collect", "number", 1 ] ],
        [ "dates", [ "collect", "date", 1 ] ],
        [ "guess", [ "or", "number", [ "null", 0.1 ] ] ],
        [ "?", [ "assert", "$values.length == $dates.length", "NUM" ] ]
    ]);

    defineFunction("ISPMT", ISPMT).args([
        [ "rate", "number" ],
        [ "per", "number++" ],
        [ "nper", "number++" ],
        [ "pv", "number" ],
        [ "?", [ "assert", "$per >= 1 && $per <= $nper" ] ]
    ]);

    defineFunction("DB", DB).args([
        [ "cost", "number" ],
        [ "salvage", "number" ],
        [ "life", "number++" ],
        [ "period", "number++" ],
        [ "month", [ "or", "number", [ "null", 12 ] ] ]
    ]);

    defineFunction("DDB", DDB).args([
        [ "cost", "number" ],
        [ "salvage", "number" ],
        [ "life", "number++" ],
        [ "period", "number++" ],
        [ "factor", [ "or", "number", [ "null", 2 ] ] ]
    ]);

    defineFunction("SLN", SLN).args([
        [ "cost", "number" ],
        [ "salvage", "number" ],
        [ "life", "number++" ]
    ]);

    defineFunction("SYD", SYD).args([
        [ "cost", "number" ],
        [ "salvage", "number" ],
        [ "life", "number++" ],
        [ "per", "number++" ]
    ]);

    defineFunction("VDB", VDB).args([
        [ "cost", "number+" ],
        [ "salvage", "number+" ],
        [ "life", "number++" ],
        [ "start_period", "number+" ],
        [ "end_period", "number+" ],
        [ "factor", [ "or", "number+", [ "null", 2 ] ] ],
        [ "no_switch", [ "or", "logical", [ "null", false ] ] ],
        [ "?", [ "assert", "$end_period >= $start_period", "NUM" ] ]
    ]);

    /* -----[ utils ]----- */

    // function resultAsMatrix(f) {
    //     return function() {
    //         var a = f.apply(this, arguments);
    //         return this.asMatrix(a);
    //     };
    // }

    /* -----[ definitions: statistical functions ]----- */

    var MAX_IT = 300,     // Maximum allowed number of iterations
        EPS = 2.2204e-16, // Relative accuracy; 1-3*(4/3-1) = 2.220446049250313e-16
        FP_MIN = 1.0e-30, // Near the smallest representable as floating-point, number.
        f_abs = Math.abs;

    function ERF(x) {
        if (f_abs(x) >= 3.3) {
            return 1 - ERFC(x);
        }
        var S = x > 0 ? 1 : -1;
        if (S == -1) {
            x = -x;
        }
        var m = 0, an = 1;
        for (var n = 1; n < 100; n++) {
            m += an;
            an *= 2*x*x/(2*n+1);
        }
        return S*2/Math.sqrt(Math.PI)*x*Math.exp(-x*x)*m;
    }

    function ERFC(x) {
        if (f_abs(x) < 3.3) {
            return 1 - ERF(x);
        }
        var s = 1;
        if (x < 0) {
            s = -1;
            x = -x;
        }
        var frac = x;
        for (var n = 8; n >= 1; n -= 0.5) {
            frac = x + n/frac;
        }
        frac = 1 / (x + frac);
        return s == 1
            ? Math.exp(-x*x)/Math.sqrt(Math.PI)*frac
            : 2 - Math.exp(-x*x)/Math.sqrt(Math.PI)*frac;
    }

    function GAMMALN(x) { // Returns the value ln[Γ(x)] for x > 0.
        var cof = [
            1.000000000190015, 76.18009172947146, -86.50532032941677,
            24.01409824083091, -1.231739572450155, 0.1208650973866179e-2, -0.5395239384953e-5
        ];
        var y = x, tmp = x + 5.5, ser = cof[0];
        tmp -= (x + 0.5) * Math.log(tmp);
        for (var j = 1; j <= 6; j++) {
            y += 1;
            ser += cof[j] / y;
        }
        return -tmp + Math.log(Math.sqrt(2*Math.PI) * ser / x); // log(Γ(x)) = log(Γ(x+1)) - log(x)
    }

    function GAMMA(x) { // returns Infinity for 0 or negative _integer argument.
        if (x > 0) {
            return Math.exp(GAMMALN(x));
        }
        var pi = Math.PI, y = -x; // For x<0 we use the reflection formula: Γ(x)Γ(1-x) = PI / sin(PI*x)
        return -pi / (y*GAMMA(y)*Math.sin(pi*y));
    }

    function BETALN(a, b) {
        return GAMMALN(a) + GAMMALN(b) - GAMMALN(a+b);
    }

    function BETA(a, b) {
        return Math.exp(BETALN(a, b));
    }

    function gamma_inc(a, x) { // returns the normalized incomplete gamma function P(a, x); x > 0.
        return x < a+1.0 ? g_series(a, x) : 1 - g_contfrac(a, x);
    }

    function g_series(a, x) { // evaluate P(a, x) by its series representation (converges quickly for x < a+1).
        var sum = 1/a,
            frac = sum,
            ap = a;
        var gln = GAMMALN(a), n;
        for (n = 1; n <= MAX_IT; n++) {
            ap++;
            frac *= x/ap;
            sum += frac;
            if (f_abs(frac) < f_abs(sum)*EPS) {
                break; // already the last frac is too small versus the current sum value
            }
        }
        return sum * Math.exp(-x + a*Math.log(x) - gln); // e^{-x} * x^a * Γ(a) * sum
    }

    function g_contfrac(a, x) { // Q(a, x) by its continued fraction representation (converges quickly for x > a + 1); modified Lentz’s method (Numerical Recipes (The Art of Scientific Computing), 2rd Edition $5.2)
        var f = FP_MIN, c = f, d = 0, aj = 1, bj = x + 1 - a;
        var gln = GAMMALN(a);
        for (var i = 1; i <= MAX_IT; i++) {
            d = bj + aj * d;
	    if (f_abs(d) < FP_MIN) {
                d = FP_MIN;
            }
	    c = bj + aj / c;
	    if (f_abs(c) < FP_MIN) {
                c = FP_MIN;
            }
	    d = 1 / d;
	    var delta = c * d;
	    f *= delta;
    	    if (f_abs(delta - 1) < EPS) {
                break;
            }
	    bj += 2;
	    aj = -i * (i - a);
	}
	return f * Math.exp(-x - gln + a * Math.log(x));
    }

    function GAMMA_DIST(x, a, b, cumulative) { // a > 0, b > 0; x >= 0
        if (!cumulative) {
            return Math.pow(x/b, a-1)*Math.exp(-x/b)/(b*GAMMA(a)); // the PDF of the Gamma distribution
        }
        return gamma_inc(a, x/b); // (else) compute the CDF (using the incomplete Gamma function)
    }

    function GAMMA_INV(p, a, b) { // the quantile function of the Gamma distribution
	if (p === 0) {
            return 0;
        }
	if (p == 1) {
            return Infinity;
        }
	var m = 0, M = 10, x = 0, ab = a*b;
	if (ab > 1) {
            M *= ab;
        }
	for (var i = 0; i < MAX_IT; i++) {
	    x = 0.5*(m + M); // console.log(x);
	    var q = GAMMA_DIST(x, a, b, true);
	    if (f_abs(p - q) < 1e-16) {
                break;
            }
	    if (q > p) {
                M = x;
            } else {
                m = x;
            }
	}
	return x;
    }

    function NORM_S_DIST(x, cumulative) {
        if (!cumulative) {
            return Math.exp(-x*x/2)/Math.sqrt(2*Math.PI);
        }
        return 0.5 + 0.5*ERF(x/Math.sqrt(2));
    }

    function NORM_S_INV(p) { // see [1] $26.2.3 and http://home.online.no/~pjacklam/notes/invnorm/#References
        // Coefficients in rational approximations.
        var a = [-3.969683028665376e+01,  2.209460984245205e+02,
                 -2.759285104469687e+02,  1.383577518672690e+02,
                 -3.066479806614716e+01,  2.506628277459239e+00],
            b = [-5.447609879822406e+01,  1.615858368580409e+02,
                 -1.556989798598866e+02,  6.680131188771972e+01,
                 -1.328068155288572e+01],
            c = [-7.784894002430293e-03, -3.223964580411365e-01,
                 -2.400758277161838e+00, -2.549732539343734e+00,
                 4.374664141464968e+00,  2.938163982698783e+00],
            d = [ 7.784695709041462e-03,  3.224671290700398e-01,
                  2.445134137142996e+00,  3.754408661907416e+00];
        // Define break-points.
        var plow  = 0.02425,
            phigh = 1 - plow;
        var q, r;
        // Rational approximation for lower region:
        if (p < plow) {
            q = Math.sqrt(-2*Math.log(p));
            return (((((c[0]*q+c[1])*q+c[2])*q+c[3])*q+c[4])*q+c[5]) / ((((d[0]*q+d[1])*q+d[2])*q+d[3])*q+1);
        }
        // Rational approximation for upper region:
        if (phigh < p) {
            q = Math.sqrt(-2*Math.log(1-p));
            return -(((((c[0]*q+c[1])*q+c[2])*q+c[3])*q+c[4])*q+c[5]) / ((((d[0]*q+d[1])*q+d[2])*q+d[3])*q+1);
        }
        // Rational approximation for central region:
        q = p - 0.5;
        r = q*q;
        return (((((a[0]*r+a[1])*r+a[2])*r+a[3])*r+a[4])*r+a[5])*q / (((((b[0]*r+b[1])*r+b[2])*r+b[3])*r+b[4])*r+1);
    }

    function NORM_DIST(x, m, s, cumulative) {
        if (!cumulative) {
            return Math.exp(-(x-m)*(x-m)/(2*s*s))/(s*Math.sqrt(2*Math.PI)); // NORM_S_DIST((x-m)/s)/s;
        }
        return NORM_S_DIST((x-m)/s, true);
    }

    function NORM_INV(p, m, s) {
        return m + s*NORM_S_INV(p);
    }

    function betastd_pdf(x, a, b) {
        return Math.exp((a-1)*Math.log(x) + (b-1)*Math.log(1-x) - BETALN(a, b));
    }

    function betastd_cdf(x, a, b) {
        var k = Math.exp(a*Math.log(x) + b*Math.log(1-x) - BETALN(a, b));
        return x < (a+1)/(a+b+2) ? k*beta_lentz(a, b, x)/a : 1 - k*beta_lentz(b, a, 1-x)/b;
    }

    function beta_lentz(a, b, x) { // estimates continued fraction by modified Lentz’s method ([2] $8.17.22)
        var m, m2;
        var aa, c, d, del, h, qab, qam, qap;
        qab = a + b; // These q’s will be used in factors that occur in the coefficients d_n
        qap = a + 1;
        qam = a - 1;
        c = 1; // First step of Lentz’s method.
        d = 1 - qab * x / qap;
        if (f_abs(d) < FP_MIN) {
            d = FP_MIN;
        }
        d = 1/d;
        h = d;
        for (m = 1; m <= MAX_IT; m++) {
            m2 = 2*m;
            aa = m*(b - m)*x / ((qam + m2)*(a + m2));
            d = 1 + aa*d; // One step (the even one) of the recurrence.
            if (f_abs(d) < FP_MIN) {
                d = FP_MIN;
            }
            c = 1 + aa/c;
            if (f_abs(c) < FP_MIN) {
                c = FP_MIN;
            }
            d = 1/d;
            h *= d*c;
            aa = -(a + m)*(qab + m)*x / ((a + m2)*(qap + m2));
            d = 1 + aa*d; // Next step of the recurrence (the odd one).
            if (f_abs(d) < FP_MIN) {
                d = FP_MIN;
            }
            c = 1 + aa/c;
            if (f_abs(c) < FP_MIN) {
                c = FP_MIN;
            }
            d = 1/d;
            del = d*c;
            h *= del;
            if (f_abs(del - 1) < EPS) {
                break;
            }
        }
        return h; // if(m > MAX_IT) throw new Error("a or b too big, or MAX_IT too small");
    }

    function betastd_inv(p, a, b) { // the quantile function of the standard Beta distribution
	var m = 0, M = 1, x = 0;
	for (var i = 0; i < MAX_IT; i++) {
	    x = 0.5*(m + M);
	    var q = betastd_cdf(x, a, b);
	    if (f_abs(p - q) < EPS) {
                break;
            }
	    if (q > p) {
                M = x;
            } else {
                m = x;
            }
	}
	return x;
    }

    function BETADIST(x, a, b, m, M) {
        return betastd_cdf((x-m)/(M-m), a, b);
    }

    function BETA_DIST(x, a, b, cdf, m, M) {
        if (cdf) {
            return betastd_cdf((x-m)/(M-m), a, b);
        }
        return betastd_pdf((x-m)/(M-m), a, b) / (M-m);
    }

    function BETA_INV(p, a, b, m, M) {
        return m + (M-m)*betastd_inv(p, a, b);
    }

    function chisq_left(x, n, cds) {  // CHISQ.DIST(x,deg_freedom,cumulative)
        return GAMMA_DIST(x, n/2, 2, cds);
    }

    function chisq_right(x, n) { // CHISQ.DIST.RT(x,deg_freedom)
        return 1 - chisq_left(x, n, true);
    }

    function chisq_left_inv(p, n) { // CHISQ.INV( probability, degrees_freedom )
        return GAMMA_INV(p, n/2, 2);
    }

    function chisq_right_inv(p, n) { // CHISQ.INV.RT(probability,deg_freedom)
        return chisq_left_inv(1-p, n);
    }

    function chisq_test(obsv, expect) {
        var rows = obsv.length, cols = obsv[0].length;
        var x = 0, i, j;
        for (i = 0; i < rows; i++) {
            for (j = 0; j < cols; j++) {
                var eij = expect[i][j];
                var delta = obsv[i][j] - eij;
                delta *= delta;
                x += delta/eij;
            }
        }
        var n = (rows - 1)*(cols - 1);
        return chisq_right(x, n);
    }

    function expon(x, r, cdf) { // EXPON.DIST(x, lambda, cumulative)
        if (cdf) {
            return 1 - Math.exp(-r*x);
        }
        return r * Math.exp(-r*x);
    }

    function poisson(k, m, cdf) { // POISSON.DIST(x, mean, cumulative)
        if (cdf) {
            return 1 - chisq_left(2*m, 2*(k+1), true);
        }
        //return chisq_left(2*m, 2*k, true) - chisq_left(2*m, 2*(k+1), true);
        var lnf = 0;
        for (var i = 2; i <= k; i++) {
            lnf += Math.log(i); // compute log(k!)
        }
        return Math.exp(k*Math.log(m) - m - lnf);
    }

    function Fdist(x, n, d, cdf) { //F.DIST(x,deg_freedom1,deg_freedom2,cumulative)
        if (cdf) {
            return betastd_cdf(n*x/(d+n*x), n/2, d/2);
        }
        var u = n/d;
        n /= 2; d /= 2;
        return u/BETA(n, d) * Math.pow(u*x, n-1) / Math.pow(1+u*x, n+d);
    }

    function Fdist_right(x, n, d) { // F.DIST.RT(x,deg_freedom1,deg_freedom2)
        return 1 - Fdist(x, n, d, true);
    }

    function Finv_right(p, n, d) { // F.INV.RT(probability,deg_freedom1,deg_freedom2
        return d/n*(1/BETA_INV(p, d/2, n/2, 0, 1) - 1);
    }

    function Finv(p, n, d) { // F.INV(probability,deg_freedom1,deg_freedom2
        return d/n*(1/BETA_INV(1-p, d/2, n/2, 0, 1) - 1);
    }

    function _mean(arr) {
        var me = 0, n = arr.length;
        for (var i = 0; i < n; i++) {
            me += arr[i];
        }
        return me / n;
    }

    function _var_sq(arr, m) { // returns the (n-1)-part of the sum of the squares of deviations from m (= VAR)
        var v = 0, n = arr.length;
        for (var i = 0; i < n; i++) {
            var delta = arr[i] - m;
            v += delta*delta;
        }
        return v / (n-1);
    }

    function Ftest(arr1, arr2) { // F.TEST(array1,array2)
        var n1 = arr1.length - 1, n2 = arr2.length - 1;
        var va1 = _var_sq(arr1, _mean(arr1)),
            va2 = _var_sq(arr2, _mean(arr2));
        if (!va1 || !va2) {
            throw new CalcError("DIV/0");
        }
        return 2*Fdist(va1 / va2, n1, n2, true);
    }

    function fisher(x) { // FISHER(x)
        return 0.5*Math.log((1+x)/(1-x));
    }

    function fisherinv(x) { // FISHERINV(x)
        var e2 = Math.exp(2*x);
        return (e2 - 1)/(e2 + 1);
    }

    function Tdist(x, n, cdf) { // T.DIST(x,deg_freedom, cumulative)
        if (cdf) {
            return 1 - 0.5*betastd_cdf(n/(x*x+n), n/2, 0.5);
        }
        return 1/(Math.sqrt(n)*BETA(0.5, n/2)) * Math.pow(1 + x*x/n, -(n+1)/2);
    }

    function Tdist_right(x, n) { // T.DIST.RT(x,deg_freedom)
        return 1 - Tdist(x, n, true);
    }

    function Tdist_2tail(x, n) { // T.DIST.2T(x,deg_freedom)
        if (x < 0) {
            x = -x;
        }
        return 2*Tdist_right(x, n);
    }

    function Tdist_inv(p, n) { // T.INV(probability,deg_freedom)
        var x = betastd_inv(2*Math.min(p, 1-p), n/2, 0.5); // ibetainv();
        x = Math.sqrt(n * (1 - x) / x);
        return (p > 0.5) ? x : -x;
    }

    function Tdist_2tail_inv(p, n) { // T.INV.2T(probability,deg_freedom)
        // T2 = 2T_r = p => T_r(x,n) = p/2 => 1 - T(x,n,true) = p/2 => x = T^-1(1-p/2, n)
        return Tdist_inv(1-p/2, n);
    }

    function Tdist_test(gr1, gr2, tail, type) { // T.TEST(array1,array2,tails,type)
        var n1 = gr1.length, n2 = gr2.length;
        var t_st, df; // the t-statistic and the "degree of freedom"
        if (type == 1) { // paired (dependent) samples
            var d = 0, d2 = 0;
            for (var i = 0; i < n1; i++) {
                var delta = gr1[i] - gr2[i];
                d += delta;
                d2 += delta*delta;
            }
            var md = d/n1; //, md2 = d2 / n1;
            t_st = md / Math.sqrt((d2 - d*md)/(n1*(n1-1))); // has a "Student T" distribution
            return tail == 1 ? Tdist_right(t_st, n1-1) : Tdist_2tail(t_st, n1-1);
        }
        // unpaired (independent) samples
        var m1 = _mean(gr1), m2 = _mean(gr2),
            v1 = _var_sq(gr1, m1), v2 = _var_sq(gr2, m2);
        if (type == 3) { // unpaired, unequal variances
            var u1 = v1/n1, u2 = v2/n2, u = u1 + u2;
            var q1 = u1/u, q2 = u2/u; // u==0 must be invalidated
            df = 1/(q1*q1/(n1-1) + q2*q2/(n2-1));
            t_st =  f_abs(m1-m2)/Math.sqrt(u);
            return tail == 1 ? Tdist_right(t_st, df) : Tdist_2tail(t_st, df);
        }
        else { // (type == 2) unpaired, equal variances ("equal" in the sense that there is no significant difference in variance in both groups - a prealable F-test could revealed that)
            df = n1 + n2 - 2;
            t_st = f_abs(m1-m2)*Math.sqrt(df*n1*n2/((n1+n2)*((n1-1)*v1+(n2-1)*v2)));
            return tail == 1 ? Tdist_right(t_st, df) : Tdist_2tail(t_st, df);
        }
    }

    function confidence_t(alpha, stddev, size) { // CONFIDENCE.T(alpha,standard_dev,size)
        return -Tdist_inv(alpha/2, size-1)*stddev/Math.sqrt(size);
    }

    function confidence_norm(alpha, stddev, size) { // CONFIDENCE.NORM(alpha,standard_dev,size)
        return -NORM_S_INV(alpha/2)*stddev/Math.sqrt(size);
    }

    function gauss(z) { // GAUSS(z)
        return NORM_S_DIST(z, true) - 0.5;
    }

    function phi(x) { // PHI(x)
        return NORM_S_DIST(x);
    }

    function lognorm_dist(x, m, s, cumulative) { // LOGNORM.DIST(x,mean,standard_dev,cumulative)
        if (cumulative) {
            return 0.5 + 0.5*ERF((Math.log(x)-m)/(s*Math.sqrt(2)));
        }
        var t = Math.log(x)-m;
        return Math.exp(-t*t/(2*s*s))/(x*s*Math.sqrt(2*Math.PI));
    }

    function lognorm_inv(p, m, s) { //LOGNORM.INV(probability, mean, standard_dev)
        return Math.exp(NORM_INV(p, m, s));
    }

    function prob(x_, p_, lw, up) { //PROB(x_range, prob_range, [lower_limit], [upper_limit])
        var n = x_.length;
        var s = 0, i;
        for (i = 0; i < n; i++) {
            if (p_[i] <= 0 || p_[i] > 1) {
                throw new CalcError("NUM");
            }
            s += p_[i];
        }
        if (s != 1) {
            throw new CalcError("NUM");
        }
        var res = 0;
        for (i = 0; i < n; i++) {
            var x = x_[i];
            if (x >= lw && x <= up) {
                res += p_[i];
            }
        }
        return res;
    }

    function slope(y_, x_) { // SLOPE(known_y's, known_x's)
        var mx = _mean(x_), my = _mean(y_), b1 = 0, b2 = 0;
        for (var i = 0, n = y_.length; i < n; i++) {
            var t = x_[i] - mx;
            b1 += t*(y_[i] - my);
            b2 += t*t;
        }
        return b1/b2;
    }

    function intercept(y_, x_) { // INTERCEPT(known_y's, known_x's)
        var mx = _mean(x_), my = _mean(y_);
        // return my - mx*slope(y_, x_);  //but repeating the calls for _mean()
        var b1 = 0, b2 = 0;
        for (var i = 0, n = y_.length; i < n; i++) {
            var t = x_[i] - mx;
            b1 += t*(y_[i] - my);
            b2 += t*t;
        }
        return my - b1*mx/b2;
    }

    function pearson(x_, y_) { // PEARSON(array1, array2)
        var mx = _mean(x_), my = _mean(y_);
        var s1 = 0, s2 = 0, s3 = 0;
        for(var i = 0, n = x_.length; i < n; i++) {
            var t1 = x_[i] - mx, t2 = y_[i] - my;
            s1 += t1*t2;
            s2 += t1*t1;
            s3 += t2*t2;
        }
        return s1/Math.sqrt(s2*s3);
    }

    function rsq(x_, y_) { // RSQ(known_y's,known_x's)
        var r = pearson(x_, y_);
        return r*r;
    }

    function steyx(y_, x_) { //STEYX(known_y's, known_x's)
        var n = x_.length;
        var mx = _mean(x_), my = _mean(y_);
        var s1 = 0, s2 = 0, s3 = 0;
        for (var i = 0; i < n; i++) {
            var t1 = x_[i] - mx, t2 = y_[i] - my;
            s1 += t2*t2;
            s2 += t1*t2;
            s3 += t1*t1;
        }
        return Math.sqrt((s1 - s2*s2/s3)/(n-2));
    }

    function forecast(x, y_, x_) { //FORECAST(x, known_y's, known_x's)
        var mx = _mean(x_), my = _mean(y_);
        var s1 = 0, s2 = 0;
        for (var i = 0, n = x_.length; i < n; i++) {
            var t1 = x_[i] - mx, t2 = y_[i] - my;
            s1 += t1*t2;
            s2 += t1*t1;
        }
        if (s2 === 0) {
            throw new CalcError("N/A");
        }
        var b = s1/s2, a = my - b*mx;
        return a + b*x;
    }

    function _mat_mean(Mat) { // returns the mean value of a Matrix(n, 1)
        var n = Mat.height, sum = 0;
        for (var i=0; i < n; i++) {
            sum += Mat.data[i][0];
        }
        return sum/n;
    }

    function _mat_devsq(Mat, mean) { // returns the sum of squares of deviations for a Matrix(n, 1)
        var n = Mat.height, sq = 0;
        for (var i=0; i < n; i++) {
            var x = Mat.data[i][0] - mean;
            sq += x*x;
        }
        return sq;
    }

    function linest(Y, X, konst, stats) { // LINEST(known_y's, [known_x's], [const], [stats])
        var i = 0;

        if (!X) {
            // if not passed, X should default to array {1, 2, 3, ...} (same size as Y)
            X = Y.map(function(){ return ++i; });
        }

        if (konst) { // adding 1's column is unnecessary when const==false (meaning that y_intercept==0)
            X = X.clone();
            X.eachRow(function(row){
                X.data[row].unshift(1);
            });
            ++X.width;
        }

        var Xt = X.transpose();
        var B = Xt.multiply(X).inverse().multiply(Xt).multiply(Y); // the last square estimate of the coefficients
        var line_1 = [];
        for (i = B.height-1; i >= 0; i--) {
            line_1.push(B.data[i][0]); // regression coefficients ('slopes') and the y_intercept
        }
        if (!konst) {
            line_1.push(0); // display 0 for y_intercept, when const==false
        }
        if (!stats) {
            return this.asMatrix([ line_1 ]); // don't display statistics about the regression, when stats==false
        }

        var Y1 = X.multiply(B); // the predicted Y values
        var y_y1 = Y.adds(Y1, true); // the errors of the predictions (= Y - Y1)
        var mp = !konst? 0 : _mat_mean(Y1);
        var SSreg = _mat_devsq(Y1, mp); // The regression sum of squares
        var me = !konst? 0 : _mat_mean(y_y1);
        var SSresid = _mat_devsq(y_y1, me); // The residual sum of squares
        var line_5 = [];
        line_5.push(SSreg, SSresid);
        var R2 = SSreg / (SSreg + SSresid); // The coefficient of determination
        var degfre = Y.height - X.width; // The degrees of freedom
        var err_est = Math.sqrt(SSresid / degfre); // The standard error for the y estimate
        var line_3 = [];
        line_3.push(R2, err_est);
        var F_sta = !konst ? (R2/X.width)/((1-R2)/(degfre)) : (SSreg/(X.width-1))/(SSresid/degfre); // The F statistic
        var line_4 = [];
        line_4.push(F_sta, degfre);
        var SCP = Xt.multiply(X).inverse();
        var line_2 = [];
        for (i=SCP.height-1; i >= 0; i--) { // The standard errors (of coefficients an y-intercept)
            line_2.push(Math.sqrt(SCP.data[i][i]*SSresid/degfre));
        }
        return this.asMatrix([line_1, line_2, line_3, line_4, line_5]);
    }

    function logest(Y, X, konst, stats) { // LOGEST(known_y's, [known_x's], [const], [stats])
        return linest.call(this, Y.map(Math.log), X, konst, stats).map(Math.exp);
    }

    function trend(Y, X, W, konst) { // TREND(known_y's, [known_x's], [new_x's], [const])
        var i = 0;

        if (!X) {
            // if not passed, X should default to array {1, 2, 3, ...} (same size as Y)
            X = Y.map(function(){ return ++i; });
        }

        if (konst) { // adding 1's column is unnecessary when const==false (meaning that y_intercept==0)
            X = X.clone();
            X.eachRow(function(row){
                X.data[row].unshift(1);
            });
            ++X.width;
        }

        var Xt = X.transpose();
        var B = Xt.multiply(X).inverse().multiply(Xt).multiply(Y); // the last square estimate of the coefficients

        if (!W) {
            W = X;
        } else {
            if (konst) { // for non-zero y_intercept
                W = W.clone();
                W.eachRow(function(row){
                    W.data[row].unshift(1);
                });
                ++W.width;
            }
        }
        return W.multiply(B); // the predicted Y values for the W values
    }

    function growth(Y, X, new_X, konst) { // GROWTH(known_y's, [known_x's], [new_x's], [const])
        // = EXP(TREND(LN(Y_), X_, new_X, const))
        return trend.call(this, Y.map(Math.log), X, new_X, konst).map(Math.exp);
    }

    /*
      [1] Handbook of Mathematical Functions (NIST, 1964-2010):
      https://en.wikipedia.org/wiki/Abramowitz_and_Stegun
      http://dlmf.nist.gov/
      http://www.aip.de/groups/soe/local/numres/

      [2] https://en.wikibooks.org/wiki/Statistics/Numerical_Methods/Numerics_in_Excel
    */

    /* -----[ financial functions ]----- */

    //// find the root of a function known an initial guess (Newton's method) ////
    function root_newton(func, guess, max_it, eps) { // func(x) must return [value_F(x), value_F'(x)]
        var MAX_IT = max_it || 20, // maximum number of iterations
            EPS = eps || 1E-7; // accuracy
        var root = guess;
        for (var j = 1; j <= MAX_IT; j++) {
            var f_d = func(root),
                f = f_d[0], // the value of the function
                df = f_d[1]; // the value of the derivative
            var dx = f / df;
            root -= dx;
            if (Math.abs(dx) < EPS) {
                return root;
            }
        }
        return new CalcError("NUM");
    }


    /* https://support.office.com/en-us/article/PV-function-23879d31-0e02-4321-be01-da16e8168cbd
       if(rate==0):
       PMT * nper + PV + FV = 0
       else: //the basic equation (with six variables) implied in financial problems
       PV * (1+rate)^nper + PMT * (1+rate*type) * ((1+rate)^nper-1) / rate + FV = 0         [1]
    */



    //// FV (final or future value) ////
    /* I initially invest £1000 in a saving scheme and then at the end of each month I invest an
       extra £50. If the interest rate is 0.5% per month and I continue this process for two year,
       how much will my saving be worth: =FV(0.005, 24, -50, -1000, 0) */
    function FV(rate, nper, pmt, pv, type) { // FV(rate,nper,pmt,[pv],[type])
        var h1 = Math.pow(1+rate, nper);
        var h2 = rate ? (h1 - 1)/rate : nper;
        return -(pv * h1 + pmt * h2 * (1 + rate*type));
    }

    //// PV (present value of investment) ////
    /* If I wish to accumulate £5000 in four years time by depositing £75 per month in a fixed
       rate account with interest rate of 0.4% per month, what initial investment must I also
       make: =PV(0.004, 4*12, -75, 5000, 0) */
    function PV(rate, nper, pmt, fv, type) { // PV(rate, nper, pmt, [fv], [type])
        if (!rate) {
            return -fv - pmt*nper;
        }
        var h1 = Math.pow(1+rate, nper);
        return -(fv + pmt * (h1 - 1)/rate * (1 + rate*type)) / h1;
    }

    //// PMT monthly payments (= principal part PPMT + interest part IPMT) ////
    /* How much will the monthly repayments be if I borrow £100,000 over 20 years with an
       effective monthly interest rate is 0.5%: =PMT(0.005, 12*20, 100000, 0, 0) */
    function PMT(rate, nper, pv, fv, type) { // PMT(rate, nper, pv, [fv], [type])
        if (!rate) {
            return -(fv + pv)/nper;
        }
        var h1 = Math.pow(1+rate, nper);
        return -rate*(fv + pv*h1)/((1 + rate*type)*(h1 - 1));
    }

    //// NPER (number of periods for an investment) ////
    /* How long would it take me to pay off a loan of £10,000 at a rate of 0.5% per month if I
       can afford to pay £100 per month: =NPER(0.5%, -100, 10000, 0, 0) */
    function NPER(rate, pmt, pv, fv, type) { // NPER(rate,pmt,pv,[fv],[type])
        if (!rate) {
            return -(fv + pv) / pmt;
        }
        var h1 = pmt*(1 + rate*type);
        return Math.log((h1 - fv*rate)/(h1 + pv*rate)) / Math.log(1 + rate);
    }

    //// RATE (the interest rate per period) ////
    /* I borrow £1000 over 1 year making payments of £100 per month at the end of each
       month. What is the monthly interest rate: =RATE(12, −100, 1000, 0, 0, 0) */

    function RATE (nper, pmt, pv, fv, type, guess) { // RATE(nper, pmt, pv, [fv], [type], [guess])
        function xfd(x) { // returns F(x) and F'(x), where F is given by the equation [1]
            var h2 = Math.pow(1+x, nper-1), h1 = h2*(1+x);
            return [ pv*h1 + pmt*(1/x + type)*(h1 - 1) + fv,
                     nper*pv*h2 + pmt*(-(h1 - 1)/(x*x) + (1/x + type)*nper*h2) ];
        }
        return root_newton(xfd, guess); // a root of the equation F(x)=0
    }

    //// IPMT (interest part of a loan or investment) ////
    //// PPMT (principal part of a loan) ////

    function IPMT(rate, per, nper, pv, fv, type) { // IPMT(rate, per, nper, pv, [fv], [type])
        if(type==1 && per==1) { // interest before beginnig of the payments... = ZERO
            return 0;
        }
        var pmt = PMT(rate, nper, pv, fv, type);
        var ipmt = FV(rate, per - 1, pmt, pv, type) * rate;
        return type ? ipmt/(1 + rate) : ipmt;
    }

    function PPMT(rate, per, nper, pv, fv, type) { // PPMT(rate, per, nper, pv, [fv], [type])
        var pmt = PMT(rate, nper, pv, fv, type);
        return pmt - IPMT(rate, per, nper, pv, fv, type);
    }

    //// CUMPRINC (cumulative principal paid) ////
    /* The amount financed is $200,000 at an interest rate of 7.25% for 30 years. How much is the amount of principal
       and the amount of interest paid in the first year: CUMPRINC(0.0725/12, 12*30, 200000, 1, 12, 0)
       and CUMIPMT(0.0725/12, 12*30, 200000, 1, 12, 0) */
    function CUMPRINC(rate, nper, pv, start, end, type) { // CUMPRINC(rate, nper, pv, start_period, end_period, type)
        if(type == 1) { // start >= 1 (as in Excel), but if pay at beginning of the period (type==1),
            start --;    // then periods must be counted from Zero (decreasing given start and end parameters)
            end --;
        }
        var tn = Math.pow(1 + rate, nper),
            ts = Math.pow(1 + rate, start-1),
            te = Math.pow(1 + rate, end);
        var monthlyPayment = rate * pv * tn / (tn - 1);
        var remainingBalanceAtStart = ts * pv - ((ts - 1) / rate) * monthlyPayment;
        var remainingBalanceAtEnd = te * pv - ((te - 1) / rate) * monthlyPayment;
        return remainingBalanceAtEnd - remainingBalanceAtStart;
    }

    //// CUMIPMT (cumulative  interest paid) ////
    function CUMIPMT(rate, nper, pv, start, end, type) { // CUMIPMT(rate, nper, pv, start_period, end_period, type)
        var cip = 0;
        for(var i=start; i<=end; i++) {
            cip += IPMT(rate, i, nper, pv, 0, type);
        }
        return cip;
    }

    //// NPV (Net Present Value of an investment based on a series of periodic cash flows and a discount rate) ////
    function NPV(rate, flows) { // NPV(rate,value1,[value2],...)
        var npv = 0;
        for(var i=0, n=flows.length; i < n; i++) {
            npv += flows[i]*Math.pow(1 + rate, -i-1);
        }
        return npv;
    }

    //// IRR (Internal Rate of Return on an investment based on a series of periodic cash flows) ////
    function IRR(flows, guess) { // IRR(values, [guess])
        function xfd(x) {
            var npv = 0, npv1 = 0;
            for(var j=0, n=flows.length; j < n; j++) {
                npv += flows[j]*Math.pow(1 + x, -j-1); // construct the NPV(x) value,
                npv1 += -j*flows[j]*Math.pow(1+x, -j-2); // the value in x of the NPV()-derivative
            }
            return [npv, npv1];
        }
        return root_newton(xfd, guess);
    }

    //// EFFECT (effective annual interest rate) ////
    /* which investment option is better - one that pays 5 percent after one year, or a
       savings account that pays a monthly interest of 4.75 percent:
       = (5% - EFFECT(4.75%, 12)) * 10000 */
    function EFFECT(nominal_rate, npery) { // EFFECT(nominal_rate, npery)
        return Math.pow(1 + nominal_rate/npery, npery) - 1;
    }

    //// NOMINAL (nominal annual interest rate) ////
    function NOMINAL(effect_rate, npery) { // NOMINAL(effect_rate, npery)
        return npery*(Math.pow(effect_rate + 1, 1/npery) - 1);
    }

    //// XNPV (Net Present Value of a series of cashflows at irregular intervals) ////
    function XNPV(rate, values, dates) { // XNPV(rate, values, dates)
        var npv = 0;
        for(var i=0, n=values.length; i < n; i++) {
            npv += values[i]*Math.pow(1 + rate, (dates[0]-dates[i])/365);
        }
        return npv;
    }

    //// XIRR (Internal Rate of Return of a series of cashflows at irregular intervals) ////
    function XIRR(values, dates, guess) { // XIRR(values, dates, [guess])
        function xfd(x) {
            var npv = values[0], npv1 = 0;
            for(var j=1, n=values.length; j < n; j++) {
                var delta = (dates[0] - dates[j]) / 365;
                npv += values[j]*Math.pow(1 + x, delta); // construct the XNPV(x) value,
                npv1 += delta*values[j]*Math.pow(1+x, delta - 1); // the value in x of the XNPV()-derivative
            }
            return [npv, npv1];
        }
        return root_newton(xfd, guess); // , 100, 0.1);
    }

    //// ISPMT (Interest paid during a Specific Period of an investment) ////
    function ISPMT(rate, per, nper, pv) { // ISPMT(rate, per, nper, pv)
        var tmp = -pv*rate;
        return tmp*(1 - per/nper);
    }

    //// DB (Declining Balance depreciation) ////
    function DB(cost, salvage, life, period, month) { // DB(cost, salvage, life, period, [month])
        var rate = 1 - Math.pow(salvage/cost, 1/life);
        rate = Math.floor(rate*1000 + 0.5) / 1000; // rounded to three decimals
        var db = cost * rate * month / 12;
        if(period == 1) {
            return db;
        }
        for(var i=1; i < life; i++) {
            if(i == period - 1) {
                return (cost - db) * rate;
            }
            db += (cost - db) * rate;
        }
        return (cost - db) * rate * (12 - month) / 12;
    }

    //// DDB (Double Declining Balance depreciation) ////
    function DDB(cost, salvage, life, period, factor) { // DDB(cost, salvage, life, period, [factor])
        var f = factor / life;
        var prior = -cost * (Math.pow(1-f, period-1) - 1);
        var dep = (cost - prior) * f;
        /* Depreciation cannot exceed book value.  */
        dep = Math.min(dep, Math.max(0, cost - prior - salvage));
        return dep;
    }

    //// SLN (straight-line depreciation) ////
    function SLN(cost, salvage, life) { // SLN(cost, salvage, life)
        return (cost - salvage) / life;
    }

    //// SYD (Sum-of-Years' digits Depreciation) ////
    function SYD(cost, salvage, life, per) { // SYD(cost, salvage, life, per)
        return (cost - salvage) * (life - per + 1) * 2 / (life * (life + 1));
    }

    //// VDB (Variable Declining Balance) ////
    //
    // Code adapted from Gnumeric, which in turn took it from OpenOffice.  The original code is
    // available under GNU Lesser General Public License (LGPL).
    // https://github.com/GNOME/gnumeric/blob/master/plugins/fn-financial/sc-fin.c
    function VDB (cost, salvage, life, start, end, factor, no_switch) {
        var interest = factor >= life ? 1 : factor / life;

        function _getGDA(value, period) {
            var gda, oldValue, newValue;
            if (interest == 1) {
                oldValue = period == 1 ? value : 0;
            } else {
                oldValue = value * Math.pow(1 - interest, period - 1);
            }
            newValue = value * Math.pow(1 - interest, period);
            gda = newValue < salvage ? oldValue - salvage : oldValue - newValue;
            return gda < 0 ? 0 : gda;
        }

        function _interVDB(cost, life1, period) {
            var remValue = cost - salvage;
            var intEnd = Math.ceil(period);
            var term, lia = 0, vdb = 0, nowLia = false;
            for (var i = 1; i <= intEnd; i++) {
                if (!nowLia) {
                    var gda = _getGDA(cost, i);
                    lia = remValue / (life1 - i + 1);
                    if (lia > gda) {
                        term = lia;
                        nowLia = true;
                    } else {
                        term = gda;
                        remValue -= gda;
                    }
                } else {
                    term = lia;
                }
                if (i == intEnd) {
                    term *= period + 1 - intEnd;
                }
                vdb += term;
            }
            return vdb;
        }

        var intStart = Math.floor(start), intEnd = Math.ceil(end);
        var vdb = 0;
        if (no_switch) {
            for (var i = intStart + 1; i <= intEnd; i++) {
                var term = _getGDA(cost, i);
                if (i == intStart + 1) {
                    term *= Math.min(end, intStart + 1) - start;
                } else {
                    if (i == intEnd) {
                        term *= end + 1 - intEnd;
                    }
                }
                vdb += term;
            }
        } else {
            var life1 = life;
            if (start != Math.floor(start)) {
                if (factor > 1) {
                    if (start >= life / 2) {
                        var part = start - life / 2;
                        start = life / 2;
                        end -= part;
                        life1 += 1;
                    }
                }
            }
            cost -= _interVDB(cost, life1, start);
            vdb = _interVDB(cost, life - start, end - start);
        }
        return vdb;
    }

})();

(function(){

    (function(kendo) {
        if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
            return;
        }

        var $ = kendo.jQuery;
        var BORDER_TYPES = [ "allBorders", "insideBorders", "insideHorizontalBorders", "insideVerticalBorders", "outsideBorders", "leftBorder", "topBorder", "rightBorder", "bottomBorder", "noBorders" ];

        var BORDER_PALETTE_MESSAGES = kendo.spreadsheet.messages.borderPalette = {
            allBorders: "All borders",
            insideBorders: "Inside borders",
            insideHorizontalBorders: "Inside horizontal borders",
            insideVerticalBorders: "Inside vertical borders",
            outsideBorders: "Outside borders",
            leftBorder: "Left border",
            topBorder: "Top border",
            rightBorder: "Right border",
            bottomBorder: "Bottom border",
            noBorders: "No border"
        };

        var BorderPalette = kendo.ui.Widget.extend({
            init: function(element, options) {
                kendo.ui.Widget.call(this, element, options);

                this.element = element;
                this.color = "#000";

                this.element.addClass("k-spreadsheet-border-palette");

                this._borderTypePalette();
                this._borderColorPalette();

                this.element.on("click", ".k-spreadsheet-border-type-palette .k-button", this._click.bind(this));
            },

            options: {
                name: "BorderPalette"
            },

            events: [
                "change"
            ],

            _borderTypePalette: function() {
                var messages = BORDER_PALETTE_MESSAGES;
                var buttons = BORDER_TYPES.map(function(type) {
                    return '<a title="' + messages[type] + '" href="#" data-border-type="' + type + '" class="k-button k-button-icon">' +
                                '<span class="k-sprite k-font-icon k-icon k-i-' + kendo.toHyphens(type) + '"></span>' +
                           '</a>';
                }).join("");

                var element = $("<div />", {
                    "class": "k-spreadsheet-border-type-palette",
                    "html": buttons
                });

                element.appendTo(this.element);
            },

            _borderColorPalette: function() {
                var element = $("<div />", {
                    "class": "k-spreadsheet-border-style-palette"
                });

                var colorPalette = this.colorPalette = $("<div />").kendoColorPalette({
                    palette: [ //metro palette
                        "#ffffff", "#000000", "#d6ecff", "#4e5b6f", "#7fd13b", "#ea157a", "#feb80a", "#00addc", "#738ac8", "#1ab39f",
                        "#f2f2f2", "#7f7f7f", "#a7d6ff", "#d9dde4", "#e5f5d7", "#fad0e4", "#fef0cd", "#c5f2ff", "#e2e7f4", "#c9f7f1",
                        "#d8d8d8", "#595959", "#60b5ff", "#b3bcca", "#cbecb0", "#f6a1c9", "#fee29c", "#8be6ff", "#c7d0e9", "#94efe3",
                        "#bfbfbf", "#3f3f3f", "#007dea", "#8d9baf", "#b2e389", "#f272af", "#fed46b", "#51d9ff", "#aab8de", "#5fe7d5",
                        "#a5a5a5", "#262626", "#003e75", "#3a4453", "#5ea226", "#af0f5b", "#c58c00", "#0081a5", "#425ea9", "#138677",
                        "#7f7f7f", "#0c0c0c", "#00192e", "#272d37", "#3f6c19", "#750a3d", "#835d00", "#00566e", "#2c3f71", "#0c594f"
                    ],
                    value: this.color,
                    change: this._colorChange.bind(this)
                }).data("kendoColorPalette");

                element
                    .append(colorPalette.wrapper)
                    .appendTo(this.element);
            },

            _colorChange: function(e) {
                this.color = e.value;
                if (this.type) {
                    this.trigger("change", { type: this.type, color: this.color });
                }
            },

            _click: function(e) {
                this.type = $(e.currentTarget).data("borderType");
                this.trigger("change", { type: this.type, color: this.color });
            },

            destroy: function() {
                this.colorPalette.destroy();
                this.element.off("click");
            }
        });

        kendo.spreadsheet.BorderPalette = BorderPalette;

    })(window.kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;

    var ToolBar = kendo.ui.ToolBar;

    var MESSAGES = kendo.spreadsheet.messages.toolbar = {
        addColumnLeft: "Add column left",
        addColumnRight: "Add column right",
        addRowAbove: "Add row above",
        addRowBelow: "Add row below",
        alignment: "Alignment",
        alignmentButtons: {
            justtifyLeft: "Align left",
            justifyCenter: "Center",
            justifyRight: "Align right",
            justifyFull: "Justify",
            alignTop: "Align top",
            alignMiddle: "Align middle",
            alignBottom: "Align bottom"
        },
        backgroundColor: "Background",
        bold: "Bold",
        borders: "Borders",
        copy: "Copy",
        cut: "Cut",
        deleteColumn: "Delete column",
        deleteRow: "Delete row",
        excelExport: "Export to Excel...",
        filter: "Filter",
        fontFamily: "Font",
        fontSize: "Font size",
        format: "Custom format...",
        formatTypes: {
            automatic: "Automatic",
            number: "Number",
            percent: "Percent",
            financial: "Financial",
            currency: "Currency",
            date: "Date",
            time: "Time",
            dateTime: "Date time",
            duration: "Duration",
            moreFormats: "More formats..."
        },
        formatDecreaseDecimal: "Decrease decimal",
        formatIncreaseDecimal: "Increase decimal",
        freeze: "Freeze panes",
        freezeButtons: {
            freezePanes: "Freeze panes",
            freezeRows: "Freeze rows",
            freezeColumns: "Freeze columns",
            unfreeze: "Unfreeze panes"
        },
        italic: "Italic",
        merge: "Merge cells",
        mergeButtons: {
            mergeCells: "Merge all",
            mergeHorizontally: "Merge horizontally",
            mergeVertically: "Merge vertically",
            unmerge: "Unmerge"
        },
        paste: "Paste",
        quickAccess: {
            redo: "Redo",
            undo: "Undo"
        },
        sortAsc: "Sort ascending",
        sortDesc: "Sort descending",
        sortButtons: {
            sortSheetAsc: "Sort sheet A to Z",
            sortSheetDesc: "Sort sheet Z to A",
            sortRangeAsc: "Sort range A to Z",
            sortRangeDesc: "Sort range Z to A"
        },
        textColor: "Text Color",
        textWrap: "Wrap text",
        underline: "Underline",
        validation: "Data validation..."
    };

    var defaultTools = {
        home: [
            "excelExport",
            [ "cut", "copy", "paste" ],
            [ "bold", "italic", "underline" ],
            "backgroundColor", "textColor",
            "borders",
            "fontSize", "fontFamily",
            "alignment",
            "textWrap",
            [ "formatDecreaseDecimal", "formatIncreaseDecimal" ],
            "format",
            "merge",
            "freeze",
            "filter"
        ],
        insert: [
            [ "addColumnLeft", "addColumnRight", "addRowBelow", "addRowAbove" ],
            [ "deleteColumn", "deleteRow" ]
        ],
        data: [
            "sort",
            "filter",
            "validation"
        ]
    };

    var toolDefaults = {
        //home tab
        excelExport:           { type: "dialog", dialogName: "excelExport",        overflow: "never",         text: "",        iconClass: "xlsa" },
        bold:                  { type: "button", command: "PropertyChangeCommand", property: "bold",          value: true,     iconClass: "bold", togglable: true },
        italic:                { type: "button", command: "PropertyChangeCommand", property: "italic",        value: true,     iconClass: "italic", togglable: true },
        underline:             { type: "button", command: "PropertyChangeCommand", property: "underline",     value: true,     iconClass: "underline", togglable: true },
        formatDecreaseDecimal: { type: "button", command: "AdjustDecimalsCommand",                            value: -1,       iconClass: "decrease-decimal" },
        formatIncreaseDecimal: { type: "button", command: "AdjustDecimalsCommand",                            value: +1,       iconClass: "increase-decimal" },
        textWrap:              { type: "button", command: "TextWrapCommand",       property: "wrap",          value: true,     iconClass: "text-wrap", togglable: true },
        cut:                   { type: "button", command: "ToolbarCutCommand",                                                 iconClass: "cut" },
        copy:                  { type: "button", command: "ToolbarCopyCommand",                                                iconClass: "copy" },
        paste:                 { type: "button", command: "ToolbarPasteCommand",                                               iconClass: "paste" },
        separator:             { type: "separator" },
        alignment:             { type: "alignment",                           iconClass: "justify-left" },
        backgroundColor:       { type: "colorPicker", property: "background", iconClass: "background" },
        textColor:             { type: "colorPicker", property: "color",      iconClass: "text" },
        fontFamily:            { type: "fontFamily",  property: "fontFamily", iconClass: "text" },
        fontSize:              { type: "fontSize",    property: "fontSize",   iconClass: "font-size" },
        format:                { type: "format",      property: "format",     iconClass: "format-number" },
        filter:                { type: "filter",      property: "hasFilter",  iconClass: "filter" },
        merge:                 { type: "merge",                               iconClass: "merge-cells" },
        freeze:                { type: "freeze",                              iconClass: "freeze-panes" },
        borders:               { type: "borders",                             iconClass: "all-borders" },
        formatCells:           { type: "dialog", dialogName: "formatCells", overflow: "never" },

        //insert tab
        addColumnLeft:         { type: "button", command: "AddColumnCommand",    value: "left",  iconClass: "add-column-left"  },
        addColumnRight:        { type: "button", command: "AddColumnCommand",    value: "right", iconClass: "add-column-right" },
        addRowBelow:           { type: "button", command: "AddRowCommand",       value: "below", iconClass: "add-row-below"    },
        addRowAbove:           { type: "button", command: "AddRowCommand",       value: "above", iconClass: "add-row-above"    },
        deleteColumn:          { type: "button", command: "DeleteColumnCommand",                 iconClass: "delete-column"    },
        deleteRow:             { type: "button", command: "DeleteRowCommand",                    iconClass: "delete-row"       },

        //data tab
        sort:                  { type: "sort", iconClass: "sort-desc" },
        validation:            { type: "dialog", dialogName: "validation", iconClass: "exception", overflow: "never" }
    };

    var SpreadsheetToolBar = ToolBar.extend({
        init: function(element, options) {
            options.items = this._expandTools(options.tools || SpreadsheetToolBar.prototype.options.tools[options.toolbarName]);

            ToolBar.fn.init.call(this, element, options);
            var handleClick = this._click.bind(this);

            this.element.addClass("k-spreadsheet-toolbar");

            this._addSeparators(this.element);

            this.bind({
                click: handleClick,
                toggle: handleClick
            });
        },
        _addSeparators: function(element) {
            var groups = element.children(".k-widget, .k-button, .k-button-group");

            groups.slice(2).before("<span class='k-separator' />");
        },
        _expandTools: function(tools) {
            function expandTool(toolName) {
                // expand string to object, add missing tool properties
                var options = $.isPlainObject(toolName) ? toolName : toolDefaults[toolName] || {};
                var spriteCssClass = "k-icon k-font-icon k-i-" + options.iconClass;
                var type = options.type;
                var typeDefaults = {
                    splitButton: { spriteCssClass: spriteCssClass },
                    button: {
                        showText: "overflow"
                    },
                    colorPicker: {
                        toolIcon: spriteCssClass
                    }
                };

                var tool = $.extend({
                    name: options.name || toolName,
                    text: MESSAGES[options.name || toolName],
                    spriteCssClass: spriteCssClass,
                    attributes: { title: MESSAGES[options.name || toolName] }
                }, typeDefaults[type], options);

                if (type == "splitButton") {
                    tool.menuButtons = tool.menuButtons.map(expandTool);
                }

                tool.attributes["data-tool"] = toolName;

                if (options.property) {
                    tool.attributes["data-property"] = options.property;
                }

                return tool;
            }

            return tools.reduce(function(tools, tool) {
                if ($.isArray(tool)) {
                    tools.push({ type: "buttonGroup", buttons: tool.map(expandTool) });
                } else {
                    tools.push(expandTool.call(this, tool));
                }

                return tools;
            }, []);
        },
        _click: function(e) {
            var toolName = e.target.attr("data-tool");
            var tool = toolDefaults[toolName] || {};
            var commandType = tool.command;

            if (!commandType) {
                return;
            }

            var args = {
                command: commandType,
                options: {
                    property: tool.property || null,
                    value: tool.value || null
                }
            };

            if (typeof args.options.value === "boolean") {
                args.options.value = e.checked ? true : null;
            }

            this.action(args);
        },
        events: [
            "click",
            "toggle",
            "open",
            "close",
            "overflowOpen",
            "overflowClose",

            "action",
            "dialog"
        ],
        options: {
            name: "SpreadsheetToolBar",
            resizable: true,
            tools: defaultTools
        },
        action: function(args) {
            this.trigger("action", args);
        },
        dialog: function(args) {
            this.trigger("dialog", args);
        },
        refresh: function(activeCell) {
            var range = activeCell;
            var tools = this._tools();

            function setToggle(tool, value) {
                var toolbar = tool.toolbar;
                var overflow = tool.overflow;
                var togglable = (toolbar && toolbar.options.togglable) ||
                                 (overflow && overflow.options.togglable);

                if (!togglable) {
                    return;
                }

                var toggle = false;

                if (typeof value === "boolean") {
                    toggle = value;
                } else if (typeof value === "string") {
                    toggle = toolbar.options.value === value;
                }

                toolbar.toggle(toggle);

                if (overflow) {
                    overflow.toggle(toggle);
                }
            }

            function update(tool, value) {
                var toolbar = tool.toolbar;
                var overflow = tool.overflow;

                if (toolbar && toolbar.update) {
                    toolbar.update(value);
                }

                if (overflow && overflow.update) {
                    overflow.update(value);
                }
            }

            for (var i = 0; i < tools.length; i++) {
                var property = tools[i].property;
                var tool = tools[i].tool;
                var value = kendo.isFunction(range[property]) ? range[property]() : range;

                if (tool.type === "button") {
                    setToggle(tool, value);
                } else {
                    update(tool, value);
                }
            }
        },
        _tools: function() {
            return this.element.find("[data-property]").toArray().reduce(function(tools, element) {
                element = $(element);
                var property = element.attr("data-property");

                tools.push({
                    property: property,
                    tool: this._getItem(element)
                });

                return tools;
            }.bind(this), []);
        },
        destroy: function() {
            // TODO: move to ToolBar.destroy to take care of these
            this.element.find("[data-command],.k-button").each(function() {
                var element = $(this);
                var instance = element.data("instance");
                if (instance && instance.destroy) {
                    instance.destroy();
                }
            });

            ToolBar.fn.destroy.call(this);
        }
    });

    var DropDownTool = kendo.toolbar.Item.extend({
        init: function(options, toolbar) {
            var dropDownList = $("<select />").kendoDropDownList({
                height: "auto"
            }).data("kendoDropDownList");

            this.dropDownList = dropDownList;
            this.element = dropDownList.wrapper;
            this.options = options;
            this.toolbar = toolbar;

            this.attributes();
            this.addUidAttr();
            this.addOverflowAttr();

            dropDownList.bind("open", this._open.bind(this));
            dropDownList.bind("change", this._change.bind(this));

            this.element.width(options.width).attr({
                "data-command": "PropertyChangeCommand",
                "data-property": options.property
            });
        },
        _open: function() {
            var ddl = this.dropDownList;
            var list = ddl.list;
            var listWidth;

            list.css({
                whiteSpace: "nowrap",
                width: "auto"
            });

            listWidth = list.width();

            if (listWidth) {
                listWidth += 20;
            } else {
                listWidth = ddl._listWidth;
            }

            list.css("width", listWidth + kendo.support.scrollbar());

            ddl._listWidth = listWidth;
        },
        _change: function(e) {
            var instance = e.sender;
            var value = instance.value();
            var dataItem = instance.dataItem();
            var popupName = dataItem ? dataItem.popup : undefined;

            if (popupName) {
                this.toolbar.dialog({ name: popupName });
            } else {
                this.toolbar.action({
                    command: "PropertyChangeCommand",
                    options: {
                        property: this.options.property,
                        value: value == "null" ? null : value
                    }
                });
            }
        },
        value: function(value) {
            if (value !== undefined) {
                this.dropDownList.value(value);
            } else {
                return this.dropDownList.value();
            }
        }
    });

    var PopupTool = kendo.toolbar.Item.extend({
        init: function(options, toolbar) {
            this.element = $("<a href='#' class='k-button k-button-icon'>" +
                                "<span class='" + options.spriteCssClass + "'>" +
                                "</span><span class='k-icon k-i-arrow-s'></span>" +
                            "</a>");

            this.element
                .on("click", this.open.bind(this))
                .attr("data-command", options.command);

            this.options = options;
            this.toolbar = toolbar;

            this.attributes();
            this.addUidAttr();
            this.addOverflowAttr();

            this._popup();
        },
        destroy: function() {
            this.popup.destroy();
        },
        open: function() {
            this.popup.toggle();
        },
        _popup: function() {
            var element = this.element;

            this.popup = $("<div class='k-spreadsheet-popup' />").appendTo(element).kendoPopup({
                anchor: element
            }).data("kendoPopup");
        }
    });

    kendo.toolbar.registerComponent("dialog", kendo.toolbar.ToolBarButton.extend({
        init: function(options, toolbar) {
            kendo.toolbar.ToolBarButton.fn.init.call(this, options, toolbar);

            this._dialogName = options.dialogName;

            this.element.bind("click", this.open.bind(this))
                        .data("instance", this);
        },
        open: function() {
            this.toolbar.dialog({ name: this._dialogName });
        }
    }));

    var OverflowDialogButton = kendo.toolbar.OverflowButton.extend({
        init: function(options, toolbar) {
            kendo.toolbar.OverflowButton.fn.init.call(this, options, toolbar);

            this.element.on("click", this._click.bind(this));

            this.message = this.options.text;

            var instance = this.element.data("button");
            this.element.data(this.options.type, instance);
        },
        _click: $.noop
    });

    var ColorPicker = PopupTool.extend({
        init: function(options, toolbar) {
            PopupTool.fn.init.call(this, options, toolbar);
            this._colorPalette();

            this.element.attr({
                "data-property": options.property
            });

            this.element.data({
                type: "colorPicker",
                colorPicker: this,
                instance: this
            });
        },
        destroy: function() {
            this.colorPalette.destroy();
            PopupTool.fn.destroy.call(this);
        },
        update: function(value) {
            this.value(value);
        },
        value: function(value) {
            if (value !== undefined) {
                this.colorPalette.value(value);
            } else {
                return this.colorPalette.value();
            }
        },
        _colorPalette: function() {
            var element = $("<div />").appendTo(this.popup.element);
            this.colorPalette = element.kendoColorPalette({
                palette: [ //metro palette
                    "#ffffff", "#000000", "#d6ecff", "#4e5b6f", "#7fd13b", "#ea157a", "#feb80a", "#00addc", "#738ac8", "#1ab39f",
                    "#f2f2f2", "#7f7f7f", "#a7d6ff", "#d9dde4", "#e5f5d7", "#fad0e4", "#fef0cd", "#c5f2ff", "#e2e7f4", "#c9f7f1",
                    "#d8d8d8", "#595959", "#60b5ff", "#b3bcca", "#cbecb0", "#f6a1c9", "#fee29c", "#8be6ff", "#c7d0e9", "#94efe3",
                    "#bfbfbf", "#3f3f3f", "#007dea", "#8d9baf", "#b2e389", "#f272af", "#fed46b", "#51d9ff", "#aab8de", "#5fe7d5",
                    "#a5a5a5", "#262626", "#003e75", "#3a4453", "#5ea226", "#af0f5b", "#c58c00", "#0081a5", "#425ea9", "#138677",
                    "#7f7f7f", "#0c0c0c", "#00192e", "#272d37", "#3f6c19", "#750a3d", "#835d00", "#00566e", "#2c3f71", "#0c594f"
                ],
                change: this._colorChange.bind(this)
            }).data("kendoColorPalette");
        },
        _colorChange: function(e) {
            this.toolbar.action({
                command: "PropertyChangeCommand",
                options: {
                    property: this.options.property,
                    value: e.sender.value()
                }
            });
            this.popup.close();
        }
    });

    var ColorPickerButton = OverflowDialogButton.extend({
        init: function(options, toolbar) {
            options.iconName = "text";
            OverflowDialogButton.fn.init.call(this, options, toolbar);
        },
        _click: function() {
            this.toolbar.dialog({
                name: "colorPicker",
                options: {
                    title: this.options.property, property: this.options.property
                }
            });
        }
    });

    kendo.toolbar.registerComponent("colorPicker", ColorPicker, ColorPickerButton);

    var FONT_SIZES = [8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72];
    var DEFAULT_FONT_SIZE = 12;

    var FontSize = kendo.toolbar.Item.extend({
        init: function(options, toolbar) {
            var comboBox = $("<input />").kendoComboBox({
                change: this._valueChange.bind(this),
                dataSource: options.fontSizes || FONT_SIZES,
                value: DEFAULT_FONT_SIZE
            }).data("kendoComboBox");

            this.comboBox = comboBox;
            this.element = comboBox.wrapper;
            this.options = options;
            this.toolbar = toolbar;

            this.attributes();
            this.addUidAttr();
            this.addOverflowAttr();

            this.element.width(options.width).attr({
                "data-command": "PropertyChangeCommand",
                "data-property": options.property
            });

            this.element.data({
                type: "fontSize",
                fontSize: this
            });
        },

        _valueChange: function(e) {
            this.toolbar.action({
                command: "PropertyChangeCommand",
                options: {
                    property: this.options.property,
                    value: kendo.parseInt(e.sender.value())
                }
            });
        },

        update: function(value) {
            this.value(kendo.parseInt(value) || DEFAULT_FONT_SIZE);
        },

        value: function(value) {
            if (value !== undefined) {
                this.comboBox.value(value);
            } else {
                return this.comboBox.value();
            }
        }
    });

    var FontSizeButton = OverflowDialogButton.extend({
        _click: function() {
            this.toolbar.dialog({
                name: "fontSize",
                options: {
                    sizes: FONT_SIZES,
                    defaultSize: DEFAULT_FONT_SIZE
                }
            });
        },
        update: function(value) {
            this._value = value || DEFAULT_FONT_SIZE;
            this.element.find(".k-text").text(this.message + " (" + this._value + ") ...");
        }
    });

    kendo.toolbar.registerComponent("fontSize", FontSize, FontSizeButton);

    var FONT_FAMILIES = ["Arial", "Courier New", "Georgia", "Times New Roman", "Trebuchet MS", "Verdana"];
    var DEFAULT_FONT_FAMILY = "Arial";

    var FontFamily = DropDownTool.extend({
        init: function(options, toolbar) {
            DropDownTool.fn.init.call(this, options, toolbar);

            var ddl = this.dropDownList;
            ddl.setDataSource(options.fontFamilies || FONT_FAMILIES);
            ddl.value(DEFAULT_FONT_FAMILY);

            this.element.data({
                type: "fontFamily",
                fontFamily: this
            });
        },
        update: function(value) {
            this.value(value || DEFAULT_FONT_FAMILY);
        }
    });

    var FontFamilyButton = OverflowDialogButton.extend({
        _click: function() {
            this.toolbar.dialog({
                name: "fontFamily",
                options: {
                    fonts: FONT_FAMILIES,
                    defaultFont: DEFAULT_FONT_FAMILY
                }
            });
        },
        update: function(value) {
            this._value = value || DEFAULT_FONT_FAMILY;
            this.element.find(".k-text").text(this.message + " (" + this._value + ") ...");
        }
    });

    kendo.toolbar.registerComponent("fontFamily", FontFamily, FontFamilyButton);

    var defaultFormats = kendo.spreadsheet.formats = {
        automatic: null,
        number: "#,0.00",
        percent: "0.00%",
        financial: '_("$"* #,##0.00_);_("$"* (#,##0.00);_("$"* "-"??_);_(@_)',
        currency: "$#,##0.00;[Red]$#,##0.00",
        date: "m/d/yyyy",
        time: "h:mm:ss AM/PM",
        dateTime: "m/d/yyyy h:mm",
        duration: "[h]:mm:ss"
    };

    var Format = DropDownTool.extend({
        _revertTitle: function(e) {
            e.sender.value("");
            e.sender.wrapper.width("auto");
        },
        init: function(options, toolbar) {
            DropDownTool.fn.init.call(this, options, toolbar);

            var ddl = this.dropDownList;
            ddl.bind("change", this._revertTitle.bind(this));
            ddl.bind("dataBound", this._revertTitle.bind(this));
            ddl.setOptions({
                dataValueField: "format",
                dataValuePrimitive: true,
                valueTemplate: "<span class='k-icon k-font-icon k-i-" + options.iconClass + "' style='line-height: 1em; width: 1.35em;'></span>",
                template:
                    "# if (data.sample) { #" +
                        "<span class='k-spreadsheet-sample'>#: data.sample #</span>" +
                    "# } #" +
                    "#: data.name #"
            });
            ddl.setDataSource([
                { format: defaultFormats.automatic, name: MESSAGES.formatTypes.automatic },
                { format: defaultFormats.number, name: MESSAGES.formatTypes.number , sample: "1,499.99" },
                { format: defaultFormats.percent, name: MESSAGES.formatTypes.percent , sample: "14.50%" },
                { format: defaultFormats.financial, name: MESSAGES.formatTypes.financial , sample: "(1,000.12)" },
                { format: defaultFormats.currency, name: MESSAGES.formatTypes.currency , sample: "$1,499.99" },
                { format: defaultFormats.date, name: MESSAGES.formatTypes.date , sample: "4/21/2012" },
                { format: defaultFormats.time, name: MESSAGES.formatTypes.time , sample: "5:49:00 PM" },
                { format: defaultFormats.dateTime, name: MESSAGES.formatTypes.dateTime , sample: "4/21/2012 5:49:00" },
                { format: defaultFormats.duration, name: MESSAGES.formatTypes.duration , sample: "168:05:00" },
                { popup: "formatCells", name: MESSAGES.formatTypes.moreFormats }
            ]);

            this.element.data({
                type: "format",
                format: this
            });
        }
    });

    var FormatButton = OverflowDialogButton.extend({
        _click: function() {
            this.toolbar.dialog({ name: "formatCells" });
        }
    });

    kendo.toolbar.registerComponent("format", Format, FormatButton);

    var BorderChangeTool = PopupTool.extend({
        init: function(options, toolbar) {
            PopupTool.fn.init.call(this, options, toolbar);
            this._borderPalette();

            this.element.data({
                type: "borders",
                instance: this
            });
        },
        destroy: function() {
            this.borderPalette.destroy();
            PopupTool.fn.destroy.call(this);
        },
        _borderPalette: function() {
            var element = $("<div />").appendTo(this.popup.element);
            this.borderPalette = new kendo.spreadsheet.BorderPalette(element, {
                change: this._action.bind(this)
            });
        },
        _action: function(e) {
            this.toolbar.action({
                command: "BorderChangeCommand",
                options: {
                    border: e.type,
                    style: { size: 1, color: e.color }
                }
            });
        }
    });

    var BorderChangeButton = OverflowDialogButton.extend({
        _click: function() {
            this.toolbar.dialog({ name: "borders" });
        }
    });

    kendo.toolbar.registerComponent("borders", BorderChangeTool, BorderChangeButton);

    var AlignmentTool = PopupTool.extend({
        init: function(options, toolbar) {
            PopupTool.fn.init.call(this, options, toolbar);

            this.element.attr({ "data-property": "alignment" });

            this._commandPalette();
            this.popup.element.on("click", ".k-button", function(e) {
                this._action($(e.currentTarget));
            }.bind(this));

            this.element.data({
                type: "alignment",
                alignment: this,
                instance: this
            });
        },
        buttons: [
            { property: "textAlign",     value: "left",    iconClass: "justify-left",   text: MESSAGES.alignmentButtons.justtifyLeft },
            { property: "textAlign",     value: "center",  iconClass: "justify-center", text: MESSAGES.alignmentButtons.justifyCenter },
            { property: "textAlign",     value: "right",   iconClass: "justify-right",  text: MESSAGES.alignmentButtons.justifyRight },
            { property: "textAlign",     value: "justify", iconClass: "justify-full",   text: MESSAGES.alignmentButtons.justifyFull },
            { property: "verticalAlign", value: "top",     iconClass: "align-top",      text: MESSAGES.alignmentButtons.alignTop },
            { property: "verticalAlign", value: "middle",  iconClass: "align-middle",   text: MESSAGES.alignmentButtons.alignMiddle },
            { property: "verticalAlign", value: "bottom",  iconClass: "align-bottom",   text: MESSAGES.alignmentButtons.alignBottom }
        ],
        destroy: function() {
            this.popup.element.off();
            PopupTool.fn.destroy.call(this);
        },
        update: function(range) {
            var textAlign = range.textAlign();
            var verticalAlign = range.verticalAlign();

            this.popup.element.find(".k-button").removeClass("k-state-active");

            if (textAlign) {
                this.popup.element.find(".k-button[data-value=" + textAlign + "]").addClass("k-state-active");
            }

            if (verticalAlign) {
                this.popup.element.find(".k-button[data-value=" + verticalAlign + "]").addClass("k-state-active");
            }
        },
        _commandPalette: function() {
            var buttons = this.buttons;
            var element = $("<div />").appendTo(this.popup.element);
            buttons.forEach(function(options, index) {
                var button = "<a title='" + options.text + "' data-property='" + options.property + "' data-value='" + options.value + "' class='k-button k-button-icon'>" +
                                "<span class='k-icon k-font-icon k-i-" + options.iconClass + "'></span>" +
                             "</a>";
                if (index !== 0 && buttons[index - 1].property !== options.property) {
                    element.append($("<span class='k-separator' />"));
                }
                element.append(button);
            });
        },
        _action: function(button) {
            var property = button.attr("data-property");
            var value = button.attr("data-value");

            this.toolbar.action({
                command: "PropertyChangeCommand",
                options: {
                    property: property,
                    value: value
                }
            });
        }
    });

    var AlignmentButton = OverflowDialogButton.extend({
        _click: function() {
            this.toolbar.dialog({ name: "alignment" });
        }
    });

    kendo.toolbar.registerComponent("alignment", AlignmentTool, AlignmentButton);

    var MergeTool = PopupTool.extend({
        init: function(options, toolbar) {
            PopupTool.fn.init.call(this, options, toolbar);

            this._commandPalette();
            this.popup.element.on("click", ".k-button", function(e) {
                this._action($(e.currentTarget));
            }.bind(this));

            this.element.data({
                type: "merge",
                merge: this,
                instance: this
            });
        },
        buttons: [
            { value: "cells",        iconClass: "merge-cells",        text: MESSAGES.mergeButtons.mergeCells },
            { value: "horizontally", iconClass: "merge-horizontally", text: MESSAGES.mergeButtons.mergeHorizontally },
            { value: "vertically",   iconClass: "merge-vertically",   text: MESSAGES.mergeButtons.mergeVertically },
            { value: "unmerge",      iconClass: "normal-layout",      text: MESSAGES.mergeButtons.unmerge }
        ],
        destroy: function() {
            this.popup.element.off();
            PopupTool.fn.destroy.call(this);
        },
        _commandPalette: function() {
            var element = $("<div />").appendTo(this.popup.element);
            this.buttons.forEach(function(options) {
                var button = "<a title='" + options.text + "' data-value='" + options.value + "' class='k-button k-button-icontext'>" +
                                "<span class='k-icon k-font-icon k-i-" + options.iconClass + "'></span>" + options.text +
                             "</a>";
                element.append(button);
            });
        },
        _action: function(button) {
            var value = button.attr("data-value");

            this.toolbar.action({
                command: "MergeCellCommand",
                options: {
                    value: value
                }
            });
        }
    });

    var MergeButton = OverflowDialogButton.extend({
        _click: function() {
            this.toolbar.dialog({ name: "merge" });
        }
    });

    kendo.toolbar.registerComponent("merge", MergeTool, MergeButton);

    var FreezeTool = PopupTool.extend({
        init: function(options, toolbar) {
            PopupTool.fn.init.call(this, options, toolbar);

            this._commandPalette();
            this.popup.element.on("click", ".k-button", function(e) {
                this._action($(e.currentTarget));
            }.bind(this));

            this.element.data({
                type: "freeze",
                freeze: this,
                instance: this
            });
        },
        buttons: [
            { value: "panes",    iconClass: "freeze-panes",  text: MESSAGES.freezeButtons.freezePanes },
            { value: "rows",     iconClass: "freeze-row",    text: MESSAGES.freezeButtons.freezeRows },
            { value: "columns",  iconClass: "freeze-col",    text: MESSAGES.freezeButtons.freezeColumns },
            { value: "unfreeze", iconClass: "normal-layout", text: MESSAGES.freezeButtons.unfreeze }
        ],
        destroy: function() {
            this.popup.element.off();
            PopupTool.fn.destroy.call(this);
        },
        _commandPalette: function() {
            var element = $("<div />").appendTo(this.popup.element);
            this.buttons.forEach(function(options) {
                var button = "<a title='" + options.text + "' data-value='" + options.value + "' class='k-button k-button-icontext'>" +
                                "<span class='k-icon k-font-icon k-i-" + options.iconClass + "'></span>" + options.text +
                             "</a>";
                element.append(button);
            });
        },
        _action: function(button) {
            var value = button.attr("data-value");

            this.toolbar.action({
                command: "FreezePanesCommand",
                options: {
                    value: value
                }
            });
        }
    });

    var FreezeButton = OverflowDialogButton.extend({
        _click: function() {
            this.toolbar.dialog({ name: "freeze" });
        }
    });

    kendo.toolbar.registerComponent("freeze", FreezeTool, FreezeButton);

    var Sort = DropDownTool.extend({
        _revertTitle: function(e) {
            e.sender.value("");
            e.sender.wrapper.width("auto");
        },
        init: function(options, toolbar) {
            DropDownTool.fn.init.call(this, options, toolbar);

            var ddl = this.dropDownList;
            ddl.bind("change", this._revertTitle.bind(this));
            ddl.bind("dataBound", this._revertTitle.bind(this));
            ddl.setOptions({
                valueTemplate: "<span class='k-icon k-font-icon k-i-" + options.iconClass + "' style='line-height: 1em; width: 1.35em;'></span>",
                template: "<span class='k-icon k-font-icon k-i-#= iconClass #' style='line-height: 1em; width: 1.35em;'></span>#=text#",
                dataValueField: "value"
            });
            ddl.setDataSource([
                // { value: 1, sheet: true, asc: true,  text: MESSAGES.sortButtons.sortSheetAsc,  iconClass: "sort-asc" },
                // { value: 2, sheet: true, asc: false, text: MESSAGES.sortButtons.sortSheetDesc,  , iconClass: "sort-desc" },
                { value: 1, sheet: false, asc: true,  text: MESSAGES.sortButtons.sortRangeAsc, iconClass: "sort-asc" },
                { value: 2, sheet: false, asc: false, text: MESSAGES.sortButtons.sortRangeDesc, iconClass: "sort-desc" }
            ]);

            this.element.data({
                type: "sort",
                sort: this
            });
        },
        _change: function(e) {
            var instance = e.sender;
            var dataItem = instance.dataItem();

            if (dataItem) {
                this.toolbar.action({
                    command: "SortCommand",
                    options: {
                        asc: dataItem.asc,
                        sheet: dataItem.sheet
                    }
                });
            }
        },
        value: $.noop
    });

    var SortButton = OverflowDialogButton.extend({
        _click: function() {
            this.toolbar.dialog({ name: "sort" });
        }
    });

    kendo.toolbar.registerComponent("sort", Sort, SortButton);

    var Filter = kendo.toolbar.ToolBarButton.extend({
        init: function(options, toolbar) {
            options.showText = "overflow";
            kendo.toolbar.ToolBarButton.fn.init.call(this, options, toolbar);

            this.element.on("click", this._click.bind(this));

            this.element.data({
                type: "filter",
                filter: this
            });
        },
        _click: function() {
            this.toolbar.action({ command: "FilterCommand" });
        },
        update: function(value) {
            this.toggle(value);
        }
    });

    var FilterButton = OverflowDialogButton.extend({
        init: function(options, toolbar) {
            OverflowDialogButton.fn.init.call(this, options, toolbar);

            this.element.data({
                type: "filter",
                filter: this
            });
        },
        _click: function() {
            this.toolbar.action({ command: "FilterCommand" });
        },
        update: function(value) {
            this.toggle(value);
        }
    });

    kendo.toolbar.registerComponent("filter", Filter, FilterButton);

    kendo.spreadsheet.ToolBar = SpreadsheetToolBar;

    kendo.spreadsheet.TabStrip = kendo.ui.TabStrip.extend({
        init: function(element, options) {
            kendo.ui.TabStrip.fn.init.call(this, element, options);
            element.addClass("k-spreadsheet-tabstrip");
            this._quickAccessButtons();

            this.quickAccessToolBar.on("click", ".k-button", function(e) {
                var action = $(e.currentTarget).attr("title").toLowerCase();

                this.trigger("action", { action: action });
            }.bind(this));

            this.toolbars = {};

            var tabs = options.dataSource;

            this.contentElements.each(function(idx, element) {
                this._toolbar($(element), tabs[idx].id, options.toolbarOptions[tabs[idx].id]);
            }.bind(this));

            this.one("activate", function() { //force resize of the tabstrip after TabStrip tab is opened
                this.toolbars[this.options.dataSource[0].id].resize();
            });
        },

        events: kendo.ui.TabStrip.fn.events.concat([ "action", "dialog" ]),

        destroy: function() {
            this.quickAccessToolBar.off("click");
            kendo.ui.TabStrip.fn.destroy.call(this);
            for (var name in this.toolbars) {
                this.toolbars[name].destroy();
            }
        },

        action: function(args) {
            this.trigger("action", args);
        },

        dialog: function(args) {
            this.trigger("dialog", args);
        },

        refreshTools: function(range) {
            var toolbars = this.toolbars;
            for (var name in toolbars) {
                if (toolbars.hasOwnProperty(name)) {
                    toolbars[name].refresh(range);
                }
            }
        },

        _quickAccessButtons: function() {
            var buttons = [
                { title: MESSAGES.quickAccess.undo, iconClass: "undo-large", action: "undo" },
                { title: MESSAGES.quickAccess.redo, iconClass: "redo-large", action: "redo" }
            ];
            var buttonTemplate = kendo.template("<a href='\\#' title='#= title #' data-action='#= action #' class='k-button k-button-icon'><span class='k-icon k-font-icon k-i-#=iconClass#'></span></a>");

            this.quickAccessToolBar = $("<div />", {
                "class": "k-spreadsheet-quick-access-toolbar",
                "html": kendo.render(buttonTemplate, buttons)
            }).insertBefore(this.wrapper);

            this.quickAccessToolBar.on("click", ".k-button", function(e) {
                var action = $(e.currentTarget).attr("title").toLowerCase();
                this.action({ action: action });
            }.bind(this));

            this.quickAccessAdjust();
        },

        quickAccessAdjust: function() {
            this.tabGroup.css("padding-left", this.quickAccessToolBar.outerWidth());
        },

        _toolbar: function(container, name, tools) {
            var element;
            var options;

            if (this.toolbars[name]) {
                this.toolbars[name].destroy();
                container.children(".k-toolbar").remove();
            }

            if (tools) {
                element = container.html("<div />").children("div");

                options = {
                    tools: typeof tools === "boolean" ? undefined : tools,
                    toolbarName: name,
                    action: this.action.bind(this),
                    dialog: this.dialog.bind(this)
                };

                this.toolbars[name] = new kendo.spreadsheet.ToolBar(element, options);
            }
        }

    });

})(window.kendo);

})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var $ = kendo.jQuery;
    var ObservableObject = kendo.data.ObservableObject;

    var MESSAGES = kendo.spreadsheet.messages.dialogs = {
        apply: "Apply",
        save: "Save",
        cancel: "Cancel",
        remove: "Remove",
        okText: "OK",
        formatCellsDialog: {
            title: "Format",
            categories: {
                number: "Number",
                currency: "Currency",
                date: "Date"
            }
        },
        fontFamilyDialog: {
            title: "Font"
        },
        fontSizeDialog: {
            title: "Font size"
        },
        bordersDialog: {
            title: "Borders"
        },
        alignmentDialog: {
            title: "Alignment",
            buttons: {
                justtifyLeft: "Align left",
                justifyCenter: "Center",
                justifyRight: "Align right",
                justifyFull: "Justify",
                alignTop: "Align top",
                alignMiddle: "Align middle",
                alignBottom: "Align bottom"
            }
        },
        mergeDialog: {
            title: "Merge cells",
            buttons: {
                mergeCells: "Merge all",
                mergeHorizontally: "Merge horizontally",
                mergeVertically: "Merge vertically",
                unmerge: "Unmerge"
            }
        },
        freezeDialog: {
            title: "Freeze panes",
            buttons: {
                freezePanes: "Freeze panes",
                freezeRows: "Freeze rows",
                freezeColumns: "Freeze columns",
                unfreeze: "Unfreeze panes"
            }
        },
        validationDialog: {
            title: "Data Validation",
            hintMessage: "Please enter a valid {0} value {1}.",
            hintTitle: "Validation {0}",
            criteria: {
                any: "Any value",
                number: "Number",
                text: "Text",
                date: "Date",
                custom: "Custom Formula",
                list: "List"
            },
            comparers: {
                greaterThan: "greater than",
                lessThan: "less than",
                between: "between",
                notBetween: "not between",
                equalTo: "equal to",
                notEqualTo: "not equal to",
                greaterThanOrEqualTo: "greater than or equal to",
                lessThanOrEqualTo: "less than or equal to"
            },
            comparerMessages: {
                greaterThan: "greater than {0}",
                lessThan: "less than {0}",
                between: "between {0} and {1}",
                notBetween: "not between {0} and {1}",
                equalTo: "equal to {0}",
                notEqualTo: "not equal to {0}",
                greaterThanOrEqualTo: "greater than or equal to {0}",
                lessThanOrEqualTo: "less than or equal to {0}",
                custom: "that satisfies the formula: {0}"
            },
            labels: {
                criteria: "Criteria",
                comparer: "Comparer",
                min: "Min",
                max: "Max",
                value: "Value",
                start: "Start",
                end: "End",
                onInvalidData: "On invalid data",
                rejectInput: "Reject input",
                showWarning: "Show warning",
                showHint: "Show hint",
                hintTitle: "Hint title",
                hintMessage: "Hint message"
            },
            placeholders: {
                typeTitle: "Type title",
                typeMessage: "Type message"
            }
        },
        saveAsDialog: {
            title: "Save As...",
            labels: {
                fileName: "File name",
                saveAsType: "Save as type"
            }
        },
        excelExportDialog: {
            title: "Export to Excel..."
        },
        modifyMergedDialog: {
            errorMessage: "Cannot change part of a merged cell."
        },
        useKeyboardDialog: {
            title: "Copying and pasting",
            errorMessage: "These actions cannot be invoked through the menu. Please use the keyboard shortcuts instead:",
            labels: {
                forCopy: "for copy",
                forCut: "for cut",
                forPaste: "for paste"
            }
        },
        unsupportedSelectionDialog: {
            errorMessage: "That action cannot be performed on multiple selection."
        }
    };

    var registry = {};
    kendo.spreadsheet.dialogs = {
        register: function(name, dialogClass) {
            registry[name] = dialogClass;
        },
        create: function(name, options) {
            var dialogClass = registry[name];

            if (dialogClass) {
                return new dialogClass(options);
            }
        }
    };

    var SpreadsheetDialog = kendo.spreadsheet.SpreadsheetDialog = kendo.Observable.extend({
        init: function(options) {
            kendo.Observable.fn.init.call(this, options);

            this.options = $.extend(true, {}, this.options, options);
        },
        dialog: function() {
            if (!this._dialog) {
                this._dialog = $("<div class='k-spreadsheet-window k-action-window' />")
                    .addClass(this.options.className || "")
                    .append(this.options.template)
                    .appendTo(document.body)
                    .kendoWindow({
                        scrollable: false,
                        resizable: false,
                        maximizable: false,
                        modal: true,
                        visible: false,
                        width: this.options.width || 320,
                        title: this.options.title,
                        open: function() {
                            this.center();
                        },
                        deactivate: function() {
                            this._dialog.destroy();
                            this._dialog = null;
                        }.bind(this)
                    })
                    .data("kendoWindow");
            }

            return this._dialog;
        },
        destroy: function() {
            if (this._dialog) {
                this._dialog.destroy();
                this._dialog = null;
            }
        },
        open: function() {
            this.dialog().open();
        },
        apply: function() {
            this.close();
        },
        close: function() {
            this.dialog().close();
        }
    });

    function formattedValue(value, format) {
        var dom = kendo.spreadsheet.formatting.format(value, format);
        return dom.children[0].nodeValue;
    }

    var FormatCellsViewModel = kendo.spreadsheet.FormatCellsViewModel = ObservableObject.extend({
        init: function(options) {
            ObservableObject.fn.init.call(this, options);

            this.useCategory(this.category);
        },
        useCategory: function(category) {
            var type = category && category.type || "number";
            var formatCurrency = type == "currency";

            this.category = category;

            this.set("showCurrencyFilter", formatCurrency && this.currencies.length > 1);

            if (!formatCurrency) {
                this.set("formats", this.allFormats[type + "Formats"]);
            } else {
                this.currency(this.currencies[0]);
            }

            this.useFirstFormat();
        },
        useFirstFormat: function() {
            if (this.formats.length) {
                this.set("format", this.formats[0].value);
            }
        },
        currency: function(currency) {
            if (currency !== undefined) {
                this._currency = currency;

                var info = currency.value;
                var formats = [
                    { currency: info, decimals: true },
                    { currency: info, decimals: true, iso: true },
                    { currency: info, decimals: false }
                ];

                formats = formats.map(function(format) {
                    format = FormatCellsViewModel.convert.currency(format);

                    return { value: format, name: formattedValue(1000, format) };
                });

                this.set("formats", formats);

                this.useFirstFormat();
            }

            return this._currency || this.currencies[0];
        },
        categoryFilter: function(category) {
            if (category !== undefined) {
                this.useCategory(category);
            }

            return this.category;
        },
        preview: function() {
            var format = this.get("format");
            var value = this.value || 0;

            if (format && format.length) {
                return formattedValue(value, format);
            } else {
                return value;
            }
        }
    });

    FormatCellsViewModel.convert = {
        currency: function(options) {
            function repeat(token, n) {
                return new Array(n+1).join(token);
            }

            // convert culture info to spreadsheet format
            var info = options.currency;
            var format = info.pattern[1];

            if (options.decimals) {
                format = format.replace(/n/g, "n" + info["."] + repeat("0", info.decimals));
            }

            if (options.iso) {
                format = '"' + info.abbr + '" ' + format.replace(/\s*\$\s*/g, "");
            } else {
                format = format.replace(/\$/g, info.symbol);
            }

            format = format.replace(/n/g, "?");

            return format;
        },
        date: function(format) {
            if ((/T|Z/).test(format)) {
                return "";
            }

            return format.toLowerCase().replace(/tt/g, "AM/PM").replace(/'/g, '"');
        }
    };

    function uniqueBy(field, array) {
        var result = [];
        var values = [];

        for (var i = 0; i < array.length; i++) {
            if ($.inArray(array[i][field], values) == -1) {
                result.push(array[i]);
                values.push(array[i][field]);
            }
        }

        return result;
    }

    var FormatCellsDialog = SpreadsheetDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);

            this._generateFormats();
        },
        options: {
            title: MESSAGES.formatCellsDialog.title,
            className: "k-spreadsheet-format-cells",
            categories: [
                { type: "number", name: MESSAGES.formatCellsDialog.categories.number },
                { type: "currency", name: MESSAGES.formatCellsDialog.categories.currency },
                { type: "date", name: MESSAGES.formatCellsDialog.categories.date }
            ],
            template:
                "<div class='k-root-tabs' data-role='tabstrip' " +
                     "data-text-field='name' " +
                     "data-bind='source: categories, value: categoryFilter' " +
                     "data-animation='false' />" +

                "<div class='k-spreadsheet-preview' data-bind='text: preview' />" +

                "<script type='text/x-kendo-template' id='format-item-template'>" +
                    "#: data.name #" +
                "</script>" +

                "<select data-role='dropdownlist' class='k-format-filter' " +
                    "data-text-field='description' " +
                    "data-value-field='value.name' " +
                    "data-bind='visible: showCurrencyFilter, value: currency, source: currencies' />" +

                "<ul data-role='staticlist' tabindex='0' " +
                    "class='k-list k-reset' " +
                    "data-template='format-item-template' " +
                    "data-value-primitive='true' " +
                    "data-value-field='value' " +
                    "data-bind='source: formats, value: format' />" +

                "<div class='k-action-buttons'>" +
                    "<button class='k-button k-primary' data-bind='click: apply'>" + MESSAGES.apply + "</button>" +
                    "<button class='k-button' data-bind='click: close'>" + MESSAGES.cancel + "</button>" +
                "</div>"
        },
        _generateFormats: function() {
            var options = this.options;

            if (!options.currencies) {
                options.currencies = FormatCellsDialog.currenciesFrom(kendo.cultures);
            }

            if (!options.numberFormats) {
                options.numberFormats = [
                    { value: "#.00%", name: "100.00%" },
                    { value: "#%", name: "100%" },
                    { value: "#.00", name: "1024.00" },
                    { value: "#,###.00", name: "1,024.00" }
                ];
            }

            if (!options.dateFormats) {
                var calendarPatterns = kendo.cultures.current.calendars.standard.patterns;

                options.dateFormats = uniqueBy("value", $.map(calendarPatterns, function(format) {
                    format = FormatCellsViewModel.convert.date(format);

                    if (!format) {
                        return;
                    }

                    return { value: format, name: formattedValue(34567.7678, format) };
                }));
            }
        },
        open: function(range) {
            var options = this.options;
            var value = range.value();
            var categories = options.categories.slice(0);
            var element;

            this.viewModel = new FormatCellsViewModel({
                currencies: options.currencies.slice(0),
                allFormats: {
                    numberFormats: options.numberFormats.slice(0),
                    dateFormats: options.dateFormats.slice(0)
                },
                categories: categories,
                format: range.format(),
                category: value instanceof Date ? categories[2] : categories[0],
                apply: this.apply.bind(this),
                close: this.close.bind(this),
                value: value
            });

            SpreadsheetDialog.fn.open.call(this);

            element = this.dialog().element;

            kendo.bind(element, this.viewModel);

            var currencyFilter = element.find("select.k-format-filter").data("kendoDropDownList");

            if (options.currencies.length > 10) {
                currencyFilter.setOptions({ filter: "contains" });
            }

            element.find(kendo.roleSelector("staticlist")).parent().addClass("k-list-wrapper");
        },
        apply: function() {
            var format = this.viewModel.format;

            SpreadsheetDialog.fn.apply.call(this);

            this.trigger("action", {
                command: "PropertyChangeCommand",
                options: {
                    property: "format",
                    value: format
                }
            });
        }
    });

    FormatCellsDialog.currenciesFrom = function (cultures) {
        return uniqueBy("description", $.map(cultures, function(culture, name) {
            if (!(/-/).test(name)) {
                return;
            }

            var currency = culture.numberFormat.currency;
            var description = kendo.format(
                "{0} ({1}, {2})",
                currency.name,
                currency.abbr,
                currency.symbol
            );

            return { description: description, value: currency };
        }));
    };

    kendo.spreadsheet.dialogs.register("formatCells", FormatCellsDialog);

    kendo.spreadsheet.dialogs.FormatCellsDialog = FormatCellsDialog;

    var MessageDialog = SpreadsheetDialog.extend({
        options: {
            className: "k-spreadsheet-message",
            title: "",
            text: "",
            template:
                "<div class='k-spreadsheet-message-content' data-bind='text: text' />" +
                "<div class='k-action-buttons'>" +
                    "<button class='k-button k-primary' data-bind='click: close, text: okText' />" +
                "</div>"
        },
        open: function() {
            SpreadsheetDialog.fn.open.call(this);

            kendo.bind(this.dialog().element, {
                text: this.options.text,
                okText: MESSAGES.okText,
                close: this.close.bind(this)
            });
        }
    });

    kendo.spreadsheet.dialogs.register("message", MessageDialog);

    var FontFamilyDialog = SpreadsheetDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);

            this._list();
        },
        options: {
            title: MESSAGES.fontFamilyDialog.title,
            template: "<ul class='k-list k-reset'></ul>"
        },
        _list: function() {
            var ul = this.dialog().element.find("ul");
            var fonts = this.options.fonts;
            var defaultFont = this.options.defaultFont;

            this.list = new kendo.ui.StaticList(ul, {
                dataSource: new kendo.data.DataSource({ data: fonts }),
                template: "#:data#",
                value: defaultFont,
                change: this.apply.bind(this)
            });

            this.list.dataSource.fetch();
        },
        apply: function(e) {
            SpreadsheetDialog.fn.apply.call(this);

            this.trigger("action", {
                command: "PropertyChangeCommand",
                options: {
                    property: "fontFamily",
                    value: e.sender.value()[0]
                }
            });
        }
    });

    kendo.spreadsheet.dialogs.register("fontFamily", FontFamilyDialog);

    var FontSizeDialog = SpreadsheetDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);

            this._list();
        },
        options: {
            title: MESSAGES.fontSizeDialog.title,
            template: "<ul class='k-list k-reset'></ul>"
        },
        _list: function() {
            var ul = this.dialog().element.find("ul");
            var sizes = this.options.sizes;
            var defaultSize = this.options.defaultSize;

            this.list = new kendo.ui.StaticList(ul, {
                dataSource: new kendo.data.DataSource({ data: sizes }),
                template: "#:data#",
                value: defaultSize,
                change: this.apply.bind(this)
            });

            this.list.dataSource.fetch();
        },
        apply: function(e) {
            SpreadsheetDialog.fn.apply.call(this);

            this.trigger("action", {
                command: "PropertyChangeCommand",
                options: {
                    property: "fontSize",
                    value: kendo.parseInt(e.sender.value()[0])
                }
            });
        }
    });

    kendo.spreadsheet.dialogs.register("fontSize", FontSizeDialog);

    var BordersDialog = SpreadsheetDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);

            this.element = this.dialog().element;
            this._borderPalette();

            this.viewModel = kendo.observable({
                apply: this.apply.bind(this),
                close: this.close.bind(this)
            });

            kendo.bind(this.element.find(".k-action-buttons"), this.viewModel);
        },
        options: {
            title: MESSAGES.bordersDialog.title,
            width: 177,
            template:   "<div></div>" +
                        "<div class='k-action-buttons'>" +
                            "<button class='k-button k-primary' data-bind='click: apply'>" + MESSAGES.apply + "</button>" +
                            "<button class='k-button' data-bind='click: close'>" + MESSAGES.cancel + "</button>" +
                        "</div>"
        },
        apply: function() {
            SpreadsheetDialog.fn.apply.call(this);

            var state = this.value();

            this.trigger("action", {
                command: "BorderChangeCommand",
                options: {
                    border: state.type,
                    style: { size: 1, color: state.color }
                }
            });
        },
        _borderPalette: function() {
            var element = this.dialog().element.find("div:first");

            this.borderPalette = new kendo.spreadsheet.BorderPalette(element, {
                change: this.value.bind(this)
            });
        },
        value: function(state) {
            if (state === undefined) {
                return this._state;
            } else {
                this._state = state;
            }
        }
    });

    kendo.spreadsheet.dialogs.register("borders", BordersDialog);

    var ColorPickerDialog = SpreadsheetDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);

            this.element = this.dialog().element;
            this._colorPalette();
            this.property = options.property;
            this.options.title = options.title;

            this.viewModel = kendo.observable({
                apply: this.apply.bind(this),
                close: this.close.bind(this)
            });

            kendo.bind(this.element.find(".k-action-buttons"), this.viewModel);
        },
        options: {
            width: 177,
            template:   "<div></div>" +
                        "<div class='k-action-buttons'>" +
                            "<button class='k-button k-primary' data-bind='click: apply'>" + MESSAGES.apply + "</button>" +
                            "<button class='k-button' data-bind='click: close'>" + MESSAGES.cancel + "</button>" +
                        "</div>"
        },
        apply: function() {
            SpreadsheetDialog.fn.apply.call(this);

            this.trigger("action", {
                command: "PropertyChangeCommand",
                options: {
                    property: this.property,
                    value: this.value()
                }
            });
        },
        _colorPalette: function() {
            var element = this.dialog().element.find("div:first");
            this.colorPalette = element.kendoColorPalette({
                palette: [ //metro palette
                    "#ffffff", "#000000", "#d6ecff", "#4e5b6f", "#7fd13b", "#ea157a", "#feb80a", "#00addc", "#738ac8", "#1ab39f",
                    "#f2f2f2", "#7f7f7f", "#a7d6ff", "#d9dde4", "#e5f5d7", "#fad0e4", "#fef0cd", "#c5f2ff", "#e2e7f4", "#c9f7f1",
                    "#d8d8d8", "#595959", "#60b5ff", "#b3bcca", "#cbecb0", "#f6a1c9", "#fee29c", "#8be6ff", "#c7d0e9", "#94efe3",
                    "#bfbfbf", "#3f3f3f", "#007dea", "#8d9baf", "#b2e389", "#f272af", "#fed46b", "#51d9ff", "#aab8de", "#5fe7d5",
                    "#a5a5a5", "#262626", "#003e75", "#3a4453", "#5ea226", "#af0f5b", "#c58c00", "#0081a5", "#425ea9", "#138677",
                    "#7f7f7f", "#0c0c0c", "#00192e", "#272d37", "#3f6c19", "#750a3d", "#835d00", "#00566e", "#2c3f71", "#0c594f"
                ],
                change: this.value.bind(this)
            }).data("kendoColorPalette");
        },
        value: function(e) {
            if (e === undefined) {
                return this._value;
            } else {
                this._value = e.value;
            }
        }
    });

    kendo.spreadsheet.dialogs.register("colorPicker", ColorPickerDialog);

    var AlignmentDialog = SpreadsheetDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);

            this._list();
        },
        options: {
            title: "Alignment",
            template: "<ul class='k-list k-reset'></ul>",
            buttons: [
                { property: "textAlign",     value: "left",    iconClass: "justify-left",   text: MESSAGES.alignmentDialog.buttons.justtifyLeft },
                { property: "textAlign",     value: "center",  iconClass: "justify-center", text: MESSAGES.alignmentDialog.buttons.justifyCenter },
                { property: "textAlign",     value: "right",   iconClass: "justify-right",  text: MESSAGES.alignmentDialog.buttons.justifyRight },
                { property: "textAlign",     value: "justify", iconClass: "justify-full",   text: MESSAGES.alignmentDialog.buttons.justifyFull },
                { property: "verticalAlign", value: "top",     iconClass: "align-top",      text: MESSAGES.alignmentDialog.buttons.alignTop },
                { property: "verticalAlign", value: "middle",  iconClass: "align-middle",   text: MESSAGES.alignmentDialog.buttons.alignMiddle },
                { property: "verticalAlign", value: "bottom",  iconClass: "align-bottom",   text: MESSAGES.alignmentDialog.buttons.alignBottom }
            ]
        },
        _list: function() {
            var ul = this.dialog().element.find("ul");

            this.list = new kendo.ui.StaticList(ul, {
                dataSource: new kendo.data.DataSource({ data: this.options.buttons }),
                template: "<a title='#=text#' data-property='#=property#' data-value='#=value#'>" +
                                "<span class='k-icon k-font-icon k-i-#=iconClass#'></span>" +
                                "#=text#" +
                           "</a>",
                change: this.apply.bind(this)
            });

            this.list.dataSource.fetch();
        },
        apply: function(e) {
            var dataItem = e.sender.value()[0];
            SpreadsheetDialog.fn.apply.call(this);

            this.trigger("action", {
                command: "PropertyChangeCommand",
                options: {
                    property: dataItem.property,
                    value: dataItem.value
                }
            });
        }
    });

    kendo.spreadsheet.dialogs.register("alignment", AlignmentDialog);

    var MergeDialog = SpreadsheetDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);

            this._list();
        },
        options: {
            title: MESSAGES.mergeDialog.title,
            template: "<ul class='k-list k-reset'></ul>",
            buttons: [
                { value: "cells",        iconClass: "merge-cells",        text: MESSAGES.mergeDialog.buttons.mergeCells },
                { value: "horizontally", iconClass: "merge-horizontally", text: MESSAGES.mergeDialog.buttons.mergeHorizontally },
                { value: "vertically",   iconClass: "merge-vertically",   text: MESSAGES.mergeDialog.buttons.mergeVertically },
                { value: "unmerge",      iconClass: "normal-layout",      text: MESSAGES.mergeDialog.buttons.unmerge }
            ]
        },
        _list: function() {
            var ul = this.dialog().element.find("ul");

            this.list = new kendo.ui.StaticList(ul, {
                dataSource: new kendo.data.DataSource({ data: this.options.buttons }),
                template: "<a title='#=text#' data-value='#=value#'>" +
                            "<span class='k-icon k-font-icon k-i-#=iconClass#'></span>#=text#" +
                          "</a>",
                change: this.apply.bind(this)
            });

            this.list.dataSource.fetch();
        },
        apply: function(e) {
            var dataItem = e.sender.value()[0];
            SpreadsheetDialog.fn.apply.call(this);

            this.trigger("action", {
                command: "MergeCellCommand",
                options: {
                    value: dataItem.value
                }
            });
        }
    });

    kendo.spreadsheet.dialogs.register("merge", MergeDialog);

    var FreezeDialog = SpreadsheetDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);

            this._list();
        },
        options: {
            title: MESSAGES.freezeDialog.title,
            template: "<ul class='k-list k-reset'></ul>",
            buttons: [
                { value: "panes",    iconClass: "freeze-panes",  text: MESSAGES.freezeDialog.buttons.freezePanes },
                { value: "rows",     iconClass: "freeze-row",    text: MESSAGES.freezeDialog.buttons.freezeRows },
                { value: "columns",  iconClass: "freeze-col",    text: MESSAGES.freezeDialog.buttons.freezeColumns },
                { value: "unfreeze", iconClass: "normal-layout", text: MESSAGES.freezeDialog.buttons.unfreeze }
            ]
        },
        _list: function() {
            var ul = this.dialog().element.find("ul");

            this.list = new kendo.ui.StaticList(ul, {
                dataSource: new kendo.data.DataSource({ data: this.options.buttons }),
                template: "<a title='#=text#' data-value='#=value#'>" +
                            "<span class='k-icon k-font-icon k-i-#=iconClass#'></span>#=text#" +
                          "</a>",
                change: this.apply.bind(this)
            });

            this.list.dataSource.fetch();
        },
        apply: function(e) {
            var dataItem = e.sender.value()[0];
            SpreadsheetDialog.fn.apply.call(this);

            this.trigger("action", {
                command: "FreezePanesCommand",
                options: {
                    value: dataItem.value
                }
            });
        }
    });

    kendo.spreadsheet.dialogs.register("freeze", FreezeDialog);

    var ValidationViewModel = kendo.spreadsheet.ValidationCellsViewModel = ObservableObject.extend({
        init: function(options) {
            ObservableObject.fn.init.call(this, options);

            this.bind("change", (function(e) {

                if (e.field === "criterion") {
                    this.reset();

                    if (this.criterion === "custom" || this.criterion === "list") {
                        this.setHintMessageTemplate();
                    }
                }

                if (e.field === "comparer") {
                    this.setHintMessageTemplate();
                }

                if ((e.field == "hintMessage" || e.field == "hintTitle") && !this._mute) {
                    this.shouldBuild = false;
                }

                if ((e.field == "from" || e.field == "to" || e.field == "hintMessageTemplate" || e.field == "type") && this.shouldBuild) {
                    this.buildMessages();
                }
            }).bind(this));

            this.reset();
        },
        buildMessages: function() {
            this._mute = true;
            this.set("hintTitle", this.hintTitleTemplate ? kendo.format(this.hintTitleTemplate, this.type) : "");
            this.set("hintMessage", this.hintMessageTemplate ? kendo.format(this.hintMessageTemplate, this.from, this.to) : "");
            this._mute = false;
        },
        reset: function() {
            this.setComparers();
            this.set("comparer", this.comparers[0].type);
            this.set("from", null);
            this.set("to", null);

            this.set("useCustomMessages", false);

            this.shouldBuild = true;

            this.hintTitleTemplate = this.defaultHintTitle;
            this.buildMessages();
        },
        //TODO: refactor
        setComparers: function() {
            var all = this.defaultComparers;
            var comparers = [];

            if (this.criterion === "text") {
                var text_comparers = ["equalTo", "notEqualTo"];
                for (var idx = 0; idx < all.length; idx++) {
                    if (text_comparers[0] == all[idx].type) {
                        comparers.push(all[idx]);
                        text_comparers.shift();
                    }
                }
            } else {
                comparers = all.slice();
            }

            this.set("comparers", comparers);
        },
        setHintMessageTemplate: function() {
           if (this.criterion !== "custom" && this.criterion !== "list") {
               this.set("hintMessageTemplate", kendo.format(this.defaultHintMessage, this.criterion, this.comparerMessages[this.comparer]));
           } else {
               this.set("hintMessageTemplate", "");
               this.set("hintMessage", "");
           }
        },
        isAny: function() {
            return this.get("criterion") === "any";
        },
        isNumber: function() {
            return this.get("criterion") === "number";
        },
        isText: function() {
            return this.get("criterion") === "text";
        },
        isDate: function() {
            return this.get("criterion") === "date";
        },
        isList: function() {
            return this.get("criterion") === "list";
        },
        isCustom: function() {
            return this.get("criterion") === "custom";
        },
        showRemove: function() {
            return this.get("hasValidation");
        },
        showTo: function() {
            return this.get("comparer") == "between" || this.get("comparer") == "notBetween";
        },
        update: function(validation) {
            this.set("hasValidation", !!validation);

            if (validation) {
                this.fromValidationObject(validation);
            }
        },
        fromValidationObject: function(validation) {
            this.set("criterion", validation.dataType);
            this.set("comparer", validation.comparerType);
            this.set("from", validation.from);
            this.set("to", validation.to);
            this.set("type", validation.type);

            if (validation.messageTemplate || validation.titleTemplate) {
                this.hintMessageTemplate = validation.messageTemplate;
                this.hintMessage = validation.messageTemplate;
                this.hintTitle = validation.titleTemplate;
                this.useCustomMessages = true;
                this.buildMessages();
            } else {
                this.useCustomMessages = false;
            }
        },
        toValidationObject: function() {
            if (this.criterion === "any") {
                return null;
            }

            var options = {
                type: this.type,
                dataType: this.criterion,
                comparerType: this.comparer,
                from: this.from,
                to: this.to
            };

            if (this.useCustomMessages) {
                options.messageTemplate = this.shouldBuild ? this.hintMessageTemplate : this.hintMessage;
                options.titleTemplate = this.hintTitle;
            }

            return options;
        }
    });

    var ValidationDialog = SpreadsheetDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);
        },
        options: {
            width: 420,
            title: MESSAGES.validationDialog.title,
            criterion: "any",
            type: "reject",
            hintMessage: MESSAGES.validationDialog.hintMessage,
            hintTitle: MESSAGES.validationDialog.hintTitle,
            useCustomMessages: false,
            criteria: [
                { type: "any", name: "Any value" },
                { type: "number", name: "Number" },
                { type: "text", name: "Text" },
                { type: "date", name: "Date" },
                { type: "custom", name: "Custom Formula" },
                { type: "list", name: "List" }
            ],
            comparers: [
                { type: "greaterThan", name: MESSAGES.validationDialog.comparers.greaterThan },
                { type: "lessThan",    name: MESSAGES.validationDialog.comparers.lessThan },
                { type: "between",     name: MESSAGES.validationDialog.comparers.between },
                { type: "notBetween",  name: MESSAGES.validationDialog.comparers.notBetween },
                { type: "equalTo",     name: MESSAGES.validationDialog.comparers.equalTo },
                { type: "notEqualTo",  name: MESSAGES.validationDialog.comparers.notEqualTo },
                { type: "greaterThanOrEqualTo", name: MESSAGES.validationDialog.comparers.greaterThanOrEqualTo },
                { type: "lessThanOrEqualTo",    name: MESSAGES.validationDialog.comparers.lessThanOrEqualTo }
            ],
            comparerMessages: MESSAGES.validationDialog.comparerMessages,
            template:
                '<div class="k-edit-form-container">' +
                    '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.criteria + ':</label></div>' +
                    '<div class="k-edit-field">' +
                        '<select data-role="dropdownlist" ' +
                            'data-text-field="name" ' +
                            'data-value-field="type" ' +
                            'data-bind="value: criterion, source: criteria" />' +
                    '</div>' +

                    '<div data-bind="visible: isNumber">' +
                        '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.comparer + ':</label></div>' +
                        '<div class="k-edit-field">' +
                            '<select data-role="dropdownlist" ' +
                                'data-text-field="name" ' +
                                'data-value-field="type" ' +
                                'data-bind="value: comparer, source: comparers" />' +
                        '</div>' +
                        '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.min + ':</label></div>' +
                        '<div class="k-edit-field">' +
                            '<input placeholder="e.g. 10" class="k-textbox" data-bind="value: from" />' +
                        '</div>' +
                        '<div data-bind="visible: showTo">' +
                            '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.max + ':</label></div>' +
                            '<div class="k-edit-field">' +
                                '<input placeholder="e.g. 100" class="k-textbox" data-bind="value: to" />' +
                            '</div>' +
                        '</div>' +
                    '</div>' +

                    '<div data-bind="visible: isText">' +
                        '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.comparer + ':</label></div>' +
                        '<div class="k-edit-field">' +
                            '<select data-role="dropdownlist" ' +
                                'data-text-field="name" ' +
                                'data-value-field="type" ' +
                                'data-bind="value: comparer, source: comparers" />' +
                        '</div>' +
                        '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.value + ':</label></div>' +
                        '<div class="k-edit-field">' +
                            '<input class="k-textbox" data-bind="value: from" />' +
                        '</div>' +
                    '</div>' +

                    '<div data-bind="visible: isDate">' +
                        '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.comparer + ':</label></div>' +
                        '<div class="k-edit-field">' +
                            '<select data-role="dropdownlist" ' +
                                'data-text-field="name" ' +
                                'data-value-field="type" ' +
                                'data-bind="value: comparer, source: comparers" />' +
                        '</div>' +
                        '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.start + ':</label></div>' +
                        '<div class="k-edit-field">' +
                            '<input class="k-textbox" data-bind="value: from" />' +
                        '</div>' +
                        '<div data-bind="visible: showTo">' +
                            '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.end + ':</label></div>' +
                            '<div class="k-edit-field">' +
                                '<input class="k-textbox" data-bind="value: to" />' +
                            '</div>' +
                        '</div>' +
                    '</div>' +

                    '<div data-bind="visible: isCustom">' +
                        '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.value + ':</label></div>' +
                        '<div class="k-edit-field">' +
                            '<input class="k-textbox" data-bind="value: from" />' +
                        '</div>' +
                    '</div>' +

                    '<div data-bind="visible: isList">' +
                        '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.value + ':</label></div>' +
                        '<div class="k-edit-field">' +
                            '<input class="k-textbox" data-bind="value: from" />' +
                        '</div>' +
                    '</div>' +

                    '<div data-bind="invisible: isAny">' +
                        '<div class="k-action-buttons"></div>' +
                        '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.onInvalidData + ':</label></div>' +
                        '<div class="k-edit-field">' +
                            '<input type="radio" id="validationTypeReject" name="validationType" value="reject" data-bind="checked: type" class="k-radio" />' +
                            '<label for="validationTypeReject" class="k-radio-label">' +
                                 MESSAGES.validationDialog.labels.rejectInput +
                            '</label> ' +
                            '<input type="radio" id="validationTypeWarning" name="validationType" value="warning" data-bind="checked: type" class="k-radio" />' +
                            '<label for="validationTypeWarning" class="k-radio-label">' +
                                 MESSAGES.validationDialog.labels.showWarning +
                            '</label>' +
                        '</div>' +
                    '</div>' +

                    '<div data-bind="invisible: isAny">' +
                        '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.showHint + ':</label></div>' +
                        '<div class="k-edit-field">' +
                            '<input type="checkbox" name="useCustomMessages" id="useCustomMessages" class="k-checkbox" data-bind="checked: useCustomMessages" />' +
                            '<label class="k-checkbox-label" for="useCustomMessages"></label>' +
                        '</div>' +

                        '<div data-bind="visible: useCustomMessages">' +
                            '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.hintTitle + ':</label></div>' +
                            '<div class="k-edit-field">' +
                                '<input class="k-textbox" placeholder="' + MESSAGES.validationDialog.placeholders.typeTitle + '" data-bind="value: hintTitle" />' +
                            '</div>' +
                            '<div class="k-edit-label"><label>' + MESSAGES.validationDialog.labels.hintMessage + ':</label></div>' +
                            '<div class="k-edit-field">' +
                                '<input class="k-textbox" placeholder="' + MESSAGES.validationDialog.placeholders.typeMessage + '" data-bind="value: hintMessage" />' +
                            '</div>' +
                        '</div>' +
                    '</div>' +

                    '<div class="k-action-buttons">' +
                        '<button class="k-button" data-bind="visible: showRemove, click: remove">' + MESSAGES.remove + '</button>' +
                        '<button class="k-button k-primary" data-bind="click: apply">' + MESSAGES.apply + '</button>' +
                        '<button class="k-button" data-bind="click: close">' + MESSAGES.cancel + '</button>' +
                    "</div>" +
                "</div>"
        },
        open: function(range) {
            var options = this.options;
            var element;

            this.viewModel = new ValidationViewModel({
                type: options.type,
                defaultHintMessage: options.hintMessage,
                defaultHintTitle: options.hintTitle,
                defaultComparers: options.comparers.slice(0),
                comparerMessages: options.comparerMessages,
                criteria: options.criteria.slice(0),
                criterion: options.criterion,
                apply: this.apply.bind(this),
                close: this.close.bind(this),
                remove: this.remove.bind(this)
            });

            this.viewModel.update(range.validation());

            SpreadsheetDialog.fn.open.call(this);

            element = this.dialog().element;

            kendo.bind(element, this.viewModel);
        },
        apply: function() {
            SpreadsheetDialog.fn.apply.call(this);

            this.trigger("action", {
                command: "EditValidationCommand",
                options: {
                    value: this.viewModel.toValidationObject()
                }
            });
        },
        remove: function() {
            this.viewModel.set("criterion", "any");
            this.apply();
        }
    });

    kendo.spreadsheet.dialogs.register("validation", ValidationDialog);
    kendo.spreadsheet.dialogs.ValidationDialog = ValidationDialog;

    var SaveAsDialog = SpreadsheetDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);

            this.viewModel = kendo.observable({
                name: this.options.name,
                extension: this.options.extension,
                fileFormats: this.options.fileFormats,
                fileName: function() {
                    return this.name + this.extension;
                },

                apply: this.apply.bind(this),
                close: this.close.bind(this)
            });

            kendo.bind(this.dialog().element, this.viewModel);
        },
        options: {
            title: MESSAGES.saveAsDialog.title,
            name: "Workbook",
            extension: ".xlsx",
            editExtension: true,
            fileFormats: [{
                description: "Excel Workbook (.xlsx)",
                extension: ".xlsx"
            }],
            width: 350,
            template:
                "<div class='k-edit-label'><label>" + MESSAGES.saveAsDialog.labels.fileName + ":</label></div>" +
                    "<div class='k-edit-field'>" +
                    "<input class='k-textbox' data-bind='value: name' />" +
                "</div>" +
                "<div data-bind='visible: editExtension'>" +
                    "<div class='k-edit-label'><label>" + MESSAGES.saveAsDialog.labels.saveAsType + ":</label></div>" +
                        "<div class='k-edit-field'>" +
                        "<select data-role='dropdownlist' class='k-file-format' " +
                            "data-text-field='description' " +
                            "data-value-field='extension' " +
                            "data-bind='value: extension, source: fileFormats' />" +
                    "</div>" +
                "</div>" +

                "<div class='k-action-buttons'>" +
                    "<button class='k-button k-primary' data-bind='click: apply'>" + MESSAGES.save + "</button>" +
                    "<button class='k-button' data-bind='click: close'>" + MESSAGES.cancel + "</button>" +
                "</div>"
        },
        apply: function() {
            SpreadsheetDialog.fn.apply.call(this);

            this.trigger("action", {
                command: "SaveAsCommand",
                options: {
                    fileName: this.viewModel.fileName()
                }
            });
        }
    });
    kendo.spreadsheet.dialogs.register("saveAs", SaveAsDialog);

    var ExcelExportDialog = SaveAsDialog.extend({
        options: {
            title: MESSAGES.excelExportDialog.title,
            editExtension: false
        }
    });
    kendo.spreadsheet.dialogs.register("excelExport", ExcelExportDialog);

    var ModifyMergedDialog = MessageDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);
        },
        options: {
            template: MESSAGES.modifyMergedDialog.errorMessage +
                '<div class="k-action-buttons">' +
                    "<button class='k-button k-primary' data-bind='click: close, text: okText' />" +
                "</div>"
        }
    });

    kendo.spreadsheet.dialogs.register("modifyMerged", ModifyMergedDialog);

    var UseKeyboardDialog = MessageDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);
        },
        options: {
            title: MESSAGES.useKeyboardDialog.title,
            template: MESSAGES.useKeyboardDialog.errorMessage +
                "<div>Ctrl+C " + MESSAGES.useKeyboardDialog.labels.forCopy + "</div>" +
                "<div>Ctrl+X " + MESSAGES.useKeyboardDialog.labels.forCut + "</div>" +
                "<div>Ctrl+V " + MESSAGES.useKeyboardDialog.labels.forPaste + "</div>" +
                '<div class="k-action-buttons">' +
                    "<button class='k-button k-primary' data-bind='click: close, text: okText' />" +
                "</div>"
        }
    });

    kendo.spreadsheet.dialogs.register("useKeyboard", UseKeyboardDialog);

    var UnsupportedSelectionDialog = MessageDialog.extend({
        init: function(options) {
            SpreadsheetDialog.fn.init.call(this, options);
        },
        options: {
            template: MESSAGES.unsupportedSelectionDialog.errorMessage +
                '<div class="k-action-buttons">' +
                    "<button class='k-button k-primary' data-bind='click: close, text: okText' />" +
                "</div>"
        }
    });

    kendo.spreadsheet.dialogs.register("unsupportedSelection", UnsupportedSelectionDialog);

})(window.kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var SheetDataSourceBinder = kendo.Class.extend({
        init: function(options) {

            this.options = $.extend({}, this.options, options);

            this.columns = this._normalizeColumns(this.options.columns);

            this._sheet();
            this._dataSource();

            this._header();

            this.dataSource.fetch();
        },

        _sheet: function() {
            this.sheet = this.options.sheet;

            this._sheetChangeHandler = this._sheetChange.bind(this);
            this._sheetDeleteRowHandler = this._sheetDeleteRow.bind(this);
            this._sheetInsertRowHandler = this._sheetInsertRow.bind(this);

            this.sheet.bind("change", this._sheetChangeHandler)
                .bind("deleteRow", this._sheetDeleteRowHandler)
                .bind("insertRow", this._sheetInsertRowHandler);
        },

        _sheetInsertRow: function(e) {
            if (e.index !== undefined) {
                this.dataSource.insert(Math.max(e.index - 1, 0), {});
            }
        },

        _sheetDeleteRow: function(e) {
            if (e.index !== undefined) {
                var dataSource = this.dataSource;
                var model = dataSource.view()[e.index - 1];

                if (model) {
                    dataSource.remove(model);
                }
            }
        },

        _header: function() {
            this.sheet.batch(function() {
                this.columns.forEach(function(column, index) {
                    this.sheet.range(0,index).value(column.title);
                }.bind(this));
            }.bind(this));
        },

        _sheetChange: function(e) {
            if (e.recalc && e.ref) {
                var dataSource = this.dataSource;
                var data = dataSource.view();
                var columns = this.columns;

                if (!columns.length && data.length) {
                    columns = Object.keys(data[0].toJSON());
                }

                this._skipRebind = true;

                e.ref.forEach(function(ref) {
                    ref = ref.toRangeRef();
                    for (var ri = ref.topLeft.row; ri <= ref.bottomRight.row; ri++) {
                        var record = data[ri - 1]; // skip header row

                        if (!record) {
                            record = dataSource.insert(ri - 1, {});
                            data = dataSource.view();
                        }

                        for (var ci = ref.topLeft.col; ci <= ref.bottomRight.col && ci < columns.length; ci++) {
                            record.set(columns[ci].field, e.value);
                        }
                    }
                });

                this._skipRebind = false;
            }
        },

        _normalizeColumns: function(columns) {
            return columns.map(function(column) {
                var field = column.field || column;
                return {
                    field: field,
                    title: column.title || field
                };
            });
        },

        _dataSource: function() {
            var options = this.options;
            var dataSource = options.dataSource;

            dataSource = Array.isArray(dataSource) ? { data: dataSource } : dataSource;

            if (this.dataSource && this._changeHandler) {
                this.dataSource.unbind("change", this._changeHandler);
            } else {
                this._changeHandler = this._change.bind(this);
            }

            this.dataSource = kendo.data.DataSource.create(dataSource)
                .bind("change", this._changeHandler);
        },

        _change: function() {
            if (this._skipRebind) {
                return;
            }

            var data = this.dataSource.view();
            var columns = this.columns;

            if (!columns.length && data.length) {
                this.columns = columns = this._normalizeColumns(Object.keys(data[0].toJSON()));
                this._header();
            }

            var getters = columns.map(function(column) {
                return kendo.getter(column.field);
            });

            this.sheet.batch(function() {
                for (var idx = 0, length = data.length; idx < length; idx++) {
                    for (var getterIdx = 0; getterIdx < getters.length; getterIdx++) {
                        //skip header row
                        this.sheet.range(idx + 1,getterIdx).value(getters[getterIdx](data[idx]));
                    }
                }
            }.bind(this));
        },

        destroy: function() {
            this.dataSource.unbind("change", this._changeHandler);

            this.sheet.unbind("change", this._sheetChangeHandler)
                .unbind("deleteRow", this._sheetDeleteRowHandler)
                .unbind("insertRow", this._sheetInsertRowHandler);
        },

        options: {
            columns: []
        }
    });

    kendo.spreadsheet.SheetDataSourceBinder = SheetDataSourceBinder;
})(kendo);
})();

(function(){

    (function(kendo) {
        if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
            return;
        }

        var $ = kendo.jQuery;
        var Widget = kendo.ui.Widget;
        var classNames = {
            details: "k-details",
            button: "k-button",
            detailsSummary: "k-details-summary",
            detailsContent: "k-details-content",
            icon: "k-icon k-font-icon",
            iconCollapse: "k-i-collapse-se",
            iconExpand: "k-i-expand-e",
            iconSearch: "k-i-search",
            textbox: "k-textbox",
            wrapper: "k-spreadsheet-filter-menu",
            filterByCondition: "k-spreadsheet-condition-filter",
            filterByValue: "k-spreadsheet-value-filter",
            valuesTreeViewWrapper: "k-spreadsheet-value-treeview-wrapper",
            actionButtons: "k-action-buttons"
        };

        var Details = Widget.extend({
            init: function(element, options) {
                Widget.fn.init.call(this, element, options);

                this.element.addClass(FilterMenu.classNames.details);

                this._summary = this.element.find("." + FilterMenu.classNames.detailsSummary)
                    .on("click", this._toggle.bind(this));

                var iconClass = options.expanded ? FilterMenu.classNames.iconCollapse : FilterMenu.classNames.iconExpand;
                this._icon = $("<span />", { "class": FilterMenu.classNames.icon + " " + iconClass })
                    .prependTo(this._summary);

                this._container = kendo.wrap(this._summary.next(), true);

                if (!options.expanded) {
                    this._container.hide();
                }
            },
            options: {
                name: "Details"
            },
            events: [ "toggle" ],
            visible: function() {
                return this.options.expanded;
            },
            toggle: function(show) {
                var animation = kendo.fx(this._container).expand("vertical");

                animation.stop()[show ? "reverse" : "play"]();

                this._icon.toggleClass(FilterMenu.classNames.iconExpand, show)
                          .toggleClass(FilterMenu.classNames.iconCollapse, !show);

                this.options.expanded = !show;
            },
            _toggle: function() {
                var show = this.visible();
                this.toggle(show);
                this.trigger("toggle", { show: show });
            }
        });

        var FILTERMENU_MESSAGES = kendo.spreadsheet.messages.filterMenu = {
            sortAscending: "Sort range A to Z",
            sortDescending: "Sort range Z to A",
            filterByValue: "Filter by value",
            filterByCondition: "Filter by condition",
            apply: "Apply",
            search: "Search",
            clear: "Clear",
            blanks: "(Blanks)",
            operatorNone: "None",
            and: "AND",
            or: "OR",
            operators: {
                string: {
                    contains: "Text contains",
                    doesnotcontain: "Text does not contain",
                    startswith: "Text starts with",
                    endswith: "Text ends with"
                },
                date: {
                    eq:  "Date is",
                    neq: "Date is not",
                    lt:  "Date is before",
                    gt:  "Date is after"
                },
                number: {
                    eq: "Is equal to",
                    neq: "Is not equal to",
                    gte: "Is greater than or equal to",
                    gt: "Is greater than",
                    lte: "Is less than or equal to",
                    lt: "Is less than"
                }
            }
        };

        var templates = {
            filterByValue:
                "<div class='" + classNames.detailsSummary + "'>#= messages.filterByValue #</div>" +
                "<div class='" + classNames.detailsContent + "'>" +
                    //"<div class='k-textbox k-space-right'>" +
                        //"<input placeholder='#= messages.search #' />" +
                        //"<span class='k-icon k-font-icon k-i-search' />" +
                    //"</div>" +
                    "<div class='" + classNames.valuesTreeViewWrapper + "'>" +
                        "<div data-#=ns#role='treeview' " +
                            "data-#=ns#checkboxes='{ checkChildren: true }' "+
                            "data-#=ns#bind='events: { check: valuesChange, select: valueSelect }' "+
                            "/>" +
                    "</div>" +
                "</div>",
            filterByCondition:
                "<div class='" + classNames.detailsSummary + "'>#= messages.filterByCondition #</div>" +
                "<div class='" + classNames.detailsContent + "'>" +
                    '<select ' +
                        'data-#=ns#role="dropdownlist"' +
                        'data-#=ns#bind="value: customFilter.criteria[0].operator, source: operators"' +
                        'data-value-primitive="false"' +
                        'data-option-label="#=messages.operatorNone#"' +
                        'data-height="auto"' +
                        'data-text-field="text"' +
                        'data-value-field="unique">'+
                    '</select>'+
                    '<input data-#=ns#bind="value: customFilter.criteria[0].value" class="k-textbox" />'+
                "</div>",
            menuItem:
                "<li data-command='#=command#' data-dir='#=dir#'>" +
                    "<span class='k-icon k-font-icon k-i-#=iconClass#'></span>#=text#" +
                "</li>",
            actionButtons:
                "<button data-#=ns#bind='click: apply' class='k-button k-primary'>#=messages.apply#</button>" +
                "<button data-#=ns#bind='click: clear' class='k-button'>#=messages.clear#</button>"
        };

        function distinctValues(values) {
            var hash = {};
            var result = [];

            for (var i = 0; i < values.length; i++) {
                if (!hash[values[i].value]) {
                    hash[values[i].value] = true;
                    result.push(values[i]);
                }
            }

            return result;
        }

        var FilterMenuViewModel = kendo.spreadsheet.FilterMenuViewModel = kendo.data.ObservableObject.extend({
            valuesChange: function(e) {
                var checked = function(item) { return item.checked && item.value; };
                var value = function(item) {
                    return item.dataType === "date" ? kendo.spreadsheet.dateToNumber(item.value) : item.value;
                };
                var data = e.sender.dataSource.data();
                var values = data[0].children.data().toJSON();
                var blanks = values.filter(function(item) {
                    return item.dataType === "blank";
                });

                blanks = blanks.length ? blanks[0].checked : false;
                values = values.filter(checked).map(value);

                this.set("valueFilter", {
                    values: values,
                    blanks: blanks
                });
            },
            valueSelect: function(e) {
                e.preventDefault();

                var node = e.sender.dataItem(e.node);
                node.set("checked", !node.checked);
            },
            validateCriteria: function(criteria) {
                return criteria.filter(function(item) {
                    var type = item.operator.type;
                    var value = item.value;

                    if (value && type === "number") {
                        return !!kendo.parseFloat(item.value);
                    } else if (value && type === "date") {
                        return !!kendo.parseDate(item.value);
                    } else if (value && type === "string") {
                        return !!item.value.toString();
                    } else {
                        return false;
                    }
                });
            },
            normalizeCriteria: function(criteria) {
                return criteria.map(function(item) {
                    item.type = item.operator.type;
                    item.operator = item.operator.value;

                    if (item.type === "number") {
                        item.value = kendo.parseFloat(item.value);
                    } else if (item.type === "date") {
                        item.value = kendo.parseDate(item.value);
                    } else {
                        item.value = item.value.toString();
                    }

                    return item;
                });
            },
            buildCustomFilter: function() {
                var customFilter = this.customFilter.toJSON();

                customFilter.criteria = this.validateCriteria(customFilter.criteria);
                customFilter.criteria = this.normalizeCriteria(customFilter.criteria);

                return customFilter;
            },
            reset: function() {
                this.set("customFilter", { logic: "and", criteria: [ { operator: null, value: null } ] });
                this.set("valueFilter", { values: [] });
            }
        });

        function flattern(operators) {
            var messages = FILTERMENU_MESSAGES.operators;
            var result = [];
            for (var type in operators) {
                for (var operator in operators[type]) {
                    result.push({
                        text: messages[type][operator],
                        value: operator,
                        unique: type + "_" + operator,
                        type: type
                    });
                }
            }
            return result;
        }

        var FilterMenu = Widget.extend({
            init: function(options) {
                var element = $("<div />", { "class": FilterMenu.classNames.wrapper }).appendTo(document.body);
                Widget.call(this, element, options);

                this.viewModel = new FilterMenuViewModel({
                    active: "value",
                    operators: flattern(this.options.operators),
                    clear: this.clear.bind(this),
                    apply: this.apply.bind(this)
                });

                this._setFilter();
                this._popup();
                this._sort();
                this._filterByCondition();
                this._filterByValue();
                this._actionButtons();
            },

            options: {
                name: "FilterMenu",
                column: 0,
                range: null,
                operators: {
                    string: {
                        contains: "Text contains",
                        doesnotcontain: "Text does not contain",
                        startswith: "Text starts with",
                        endswith: "Text ends with"
                    },
                    date: {
                        eq:  "Date is",
                        neq: "Date is not",
                        lt:  "Date is before",
                        gt:  "Date is after"
                    },
                    number: {
                        eq: "Is equal to",
                        neq: "Is not equal to",
                        gte: "Is greater than or equal to",
                        gt: "Is greater than",
                        lte: "Is less than or equal to",
                        lt: "Is less than"
                    }
                }
            },

            events: [
                "action"
            ],

            destroy: function() {
                Widget.fn.destroy.call(this);

                this.menu.destroy();
                this.valuesTreeView.destroy();
                this.popup.destroy();
            },

            openFor: function(anchor) {
                this.popup.setOptions({ anchor: anchor });
                this.popup.open();
            },

            close: function() {
                this.popup.close();
            },

            clear: function() {
                this.action({
                    command: "ClearFilterCommand",
                    options: {
                        column: this.options.column
                    }
                });
                this.viewModel.reset();
                this.close();
            },

            apply: function() {
                this._active();

                var options = {
                    operatingRange: this.options.range,
                    column: this.options.column
                };

                var valueFilter;
                var customFilter;

                if (this.viewModel.active === "value") {
                    this.viewModel.valuesChange({ sender: this.valuesTreeView });
                    valueFilter = this.viewModel.valueFilter.toJSON();

                    if (valueFilter.values && valueFilter.values.length) {
                        options.valueFilter = valueFilter;
                    }
                } else if (this.viewModel.active === "custom") {
                    customFilter = this.viewModel.buildCustomFilter();

                    if (customFilter.criteria.length) {
                        options.customFilter = customFilter;
                    }
                }

                if (options.valueFilter || options.customFilter) {
                    this.action({ command: "ApplyFilterCommand", options: options });
                }
            },

            action: function(options) {
                this.trigger("action", $.extend({ }, options));
            },

            getValues: function() {
                var values = [];
                var messages = FILTERMENU_MESSAGES;
                var column = this.options.column;
                var columnRange = this.options.range.resize({ top: 1 }).column(column);
                var sheet = this.options.range.sheet();

                columnRange.forEachCell(function(row, col, cell) {
                    var formatter;

                    if (cell.value === undefined) {
                        cell.dataType = "blank";
                    } else if (cell.format) {
                        cell.dataType = kendo.spreadsheet.formatting.type(cell.value, cell.format);
                    } else {
                        cell.dataType = typeof cell.value;
                    }

                    if (cell.value !== null && cell.format) {
                        formatter = kendo.spreadsheet.formatting.compile(cell.format);
                        cell.text = formatter(cell.value).text();
                    } else {
                        cell.text = cell.value ? cell.value : messages.blanks;
                    }

                    if (cell.dataType === "date") {
                        cell.value = kendo.spreadsheet.numberToDate(cell.value);
                    }

                    cell.checked = !sheet.isHiddenRow(row);

                    values.push(cell);
                });

                values = distinctValues(values);

                values.sort(function(a, b) {
                    if (a.dataType === b.dataType) {
                        return 0;
                    }

                    if (a.dataType === "blank" || b.dataType === "blank") {
                        return a.dataType === "blank" ? -1 : 1;
                    }

                    if (a.dataType === "number" || b.dataType === "number") {
                        return a.dataType === "number" ? -1 : 1;
                    }

                    if (a.dataType === "date" || b.dataType === "date") {
                        return a.dataType === "date" ? -1 : 1;
                    }

                    return 0;
                });

                return [{
                    text: "All",
                    expanded: true,
                    checked: true,
                    items: values
                }];
            },

            _setFilter: function() {
                var column = this.options.column;
                var sheet = this.options.range.sheet();
                var filterObject = sheet.filter();
                var serializedFilter;
                var criterion;
                var type;
                var operator;

                if (filterObject) {
                    filterObject = filterObject.columns.filter(function(item) {
                        return item.index === column;
                    })[0];
                }

                if (filterObject) {
                    serializedFilter = filterObject.filter.toJSON();

                    if (serializedFilter.filter === "custom") {
                        criterion = serializedFilter.criteria.pop();

                        if (typeof criterion.operator === "string") {
                            type = criterion.value instanceof Date ? "date" : typeof criterion.value;
                            operator = criterion.operator;
                            serializedFilter.criteria.push({
                                operator: {
                                    text: this.options.operators[type][operator],
                                    type: type,
                                    value: operator,
                                    unique: type + "_" + operator
                                },
                                value: criterion.value
                            });
                        } else {
                            serializedFilter.criteria.push({
                                operator: {
                                    text: this.options.operators[criterion.type][criterion.operator],
                                    type: criterion.type,
                                    value: criterion.operator,
                                    unique: criterion.type + "_" + criterion.operator
                                },
                                value: criterion.value
                            });
                        }
                    }

                    this.viewModel.set("active", serializedFilter.filter);
                    this.viewModel.set(serializedFilter.filter + "Filter", serializedFilter);
                } else {
                    this.viewModel.reset();
                }
            },

            _popup: function() {
                this.popup = this.element.kendoPopup({
                    copyAnchorStyles: false
                }).data("kendoPopup");
            },

            _sort: function() {
                var template = kendo.template(FilterMenu.templates.menuItem);
                var messages = FILTERMENU_MESSAGES;
                var items = [
                    { command: "sort", dir: "asc", text: messages.sortAscending, iconClass: "sort-asc" },
                    { command: "sort", dir: "desc", text: messages.sortDescending, iconClass: "sort-desc" }
                ];

                var ul = $("<ul />", {
                    "html": kendo.render(template, items)
                }).appendTo(this.element);

                this.menu = ul.kendoMenu({
                    orientation: "vertical",
                    select: function(e) {
                        var dir = $(e.item).data("dir") === "asc";
                        var range = this.options.range.resize({ top: 1 });
                        var options = {
                            asc: dir,
                            sheet: false,
                            operatingRange: range,
                            column: this.options.column
                        };

                        if (range.isSortable()) {
                            this.action({ command: "SortCommand", options: options });
                        } else {
                            this.close();
                        }
                    }.bind(this)
                }).data("kendoMenu");
            },

            _appendTemplate: function(template, className, details, expanded) {
                var compiledTemplate = kendo.template(template);
                var wrapper = $("<div class='" + className + "'/>").html(compiledTemplate({
                    messages: FILTERMENU_MESSAGES,
                    ns: kendo.ns
                }));

                this.element.append(wrapper);

                if (details) {
                    details = new Details(wrapper, { expanded: expanded, toggle: this._detailToggle.bind(this) }); // jshint ignore:line
                }

                kendo.bind(wrapper, this.viewModel);

                return wrapper;
            },

            _detailToggle: function(e) {
                this.element
                    .find("[data-role=details]")
                    .not(e.sender.element)
                    .data("kendoDetails")
                    .toggle(!e.show);
            },

            _filterByCondition: function() {
                var isExpanded = this.viewModel.active === "custom";
                this._appendTemplate(FilterMenu.templates.filterByCondition, FilterMenu.classNames.filterByCondition, true, isExpanded);
            },

            _filterByValue: function() {
                var isExpanded = this.viewModel.active === "value";
                var wrapper = this._appendTemplate(FilterMenu.templates.filterByValue, FilterMenu.classNames.filterByValue, true, isExpanded);

                this.valuesTreeView = wrapper.find("[data-role=treeview]").data("kendoTreeView");

                this.valuesTreeView.setDataSource(this.getValues());
            },

            _actionButtons: function() {
                this._appendTemplate(FilterMenu.templates.actionButtons, FilterMenu.classNames.actionButtons, false);
            },

            _active: function() {
                var activeContainer = this.element.find("[data-role=details]").filter(function(index, element) {
                    return $(element).data("kendoDetails").visible();
                });

                if (activeContainer.hasClass(FilterMenu.classNames.filterByValue)) {
                    this.viewModel.set("active", "value");
                } else if (activeContainer.hasClass(FilterMenu.classNames.filterByCondition)) {
                    this.viewModel.set("active", "custom");
                }
            }
        });

        kendo.spreadsheet.FilterMenu = FilterMenu;
        $.extend(true, FilterMenu, { classNames: classNames, templates: templates });

    })(window.kendo);
})();

(function(){

(function(kendo) {
    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var SheetEditor = kendo.Observable.extend({
        init: function(view) {
            kendo.Observable.fn.init.call(this);

            this.view = view;
            this.formulaBar = view.formulaBar;

            this.barInput = view.formulaBar.formulaInput;
            this.cellInput = view.formulaInput;

            this.barInput.syncWith(this.cellInput);
            this.cellInput.syncWith(this.barInput);

            this.barInput.bind("keyup", this._triggerUpdate.bind(this));
            this.cellInput.bind("keyup", this._triggerUpdate.bind(this));
        },

        events: [
            "activate",
            "deactivate",
            "change",
            "update"
        ],

        _triggerUpdate: function() {
            this.trigger("update", { value: this.value() });
        },

        activeEditor: function() {
            var editor = null;
            var activeElement = kendo._activeElement();

            if (this.barElement()[0] === activeElement) {
                editor = this.barInput;
            } else if (this.cellElement()[0] === activeElement) {
                editor = this.cellInput;
            }

            return editor;
        },

        activate: function(options) {
            this._active = true;
            this._rect = options.rect;

            this.cellInput.position(options.rect);
            this.cellInput.resize(options.rect);
            this.cellInput.tooltip(options.tooltip);

            this.trigger("activate");

            return this;
        },

        deactivate: function() {
            var cellInput = this.cellInput;

            if (!this._active) {
                return;
            }

            this._active = false;
            this._rect = null;

            cellInput.hide();

            if (cellInput.value() != this._value) {
                this.trigger("change", { value: cellInput.value() });
            }

            this.trigger("deactivate");
        },

        barElement: function() {
            return this.barInput.element;
        },

        cellElement: function() {
            return this.cellInput.element;
        },

        focus: function(inputType) {
            inputType = inputType || "cell";

            if (inputType === "cell") {
                this.cellInput.element.focus();
                this.cellInput.end();
            } else {
                this.barInput.element.focus();
            }
        },

        isActive: function() {
            return this._active;
        },

        isFiltered: function() {
            return this.barInput.popup.visible() || this.cellInput.popup.visible();
        },

        canInsertRef: function(isKeyboardAction) {
            var editor = this.activeEditor();
            return editor && editor.canInsertRef(isKeyboardAction);
        },

        highlightedRefs: function() {
            var editor = this.activeEditor();
            var refs = [];

            if (editor) {
                refs = editor.highlightedRefs();
            }

            return refs;
        },

        scale: function() {
            this.cellInput.scale();
        },

        toggleTooltip: function(rect) {
            this.cellInput.toggleTooltip(notEqual(this._rect, rect));
        },

        value: function(value) {
            if (value === undefined) {
                return this.barInput.value();
            }

            if (value === null) {
                value = "";
            }

            this._value = value;

            this.barInput.value(value);
            this.cellInput.value(value);
        }
    });

    function notEqual(oldRect, newRect) {
        return oldRect && (oldRect.top !== newRect.top || oldRect.left !== newRect.left);
    }

    kendo.spreadsheet.SheetEditor = SheetEditor;
})(kendo);
})();

(function(){
    "use strict";

    // jshint eqnull:true

    if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
        return;
    }

    var spreadsheet = kendo.spreadsheet;
    var Range = spreadsheet.Range;
    var runtime = spreadsheet.calc.runtime;
    var Formula = runtime.Formula;

    var MSG_INCOMPATIBLE = "Incompatible ranges in fillFrom";
    var MSG_NO_DIRECTION = "Cannot determine fill direction";

    // `srcRange`: the range containing data that we wish to fill.  `direction`: 0↓, 1→, 2↑, 3←.  So
    // when bit 0 is set we're doing horizontal filling, and when bit 1 is set we're doing it in
    // reverse order.
    Range.prototype._previewFillFrom = function(srcRange, direction) {
        var destRange = this, sheet = destRange._sheet;
        if (typeof srcRange == "string") {
            srcRange = sheet.range(srcRange);
        }
        var src = srcRange._ref.toRangeRef();
        var dest = destRange._ref.toRangeRef();

        if (src.intersects(dest)) {
            // the UI will send e.g. C2:C8.fillFrom(C7:D8) (intersecting ranges).  this figures out
            // the actual destination range.
            if (src.eq(dest)) {
                return null; // nothing to do
            }
            dest = dest.clone();
            if (src.topLeft.eq(dest.topLeft)) {
                if (src.width() == dest.width()) {
                    dest.topLeft.row += src.height();
                    direction = 0;
                } else if (src.height() == dest.height()) {
                    dest.topLeft.col += src.width();
                    direction = 1;
                } else {
                    throw new Error(MSG_INCOMPATIBLE);
                }
            } else if (src.bottomRight.eq(dest.bottomRight)) {
                if (src.width() == dest.width()) {
                    dest.bottomRight.row -= src.height();
                    direction = 2;
                } else if (src.height() == dest.height()) {
                    dest.bottomRight.col -= src.width();
                    direction = 3;
                } else {
                    throw new Error(MSG_INCOMPATIBLE);
                }
            } else {
                throw new Error(MSG_INCOMPATIBLE);
            }
            return sheet.range(dest)._previewFillFrom(srcRange, direction);
        }

        if (direction == null) {
            // try to determine based on ranges location/geometry
            if (src.topLeft.col == dest.topLeft.col) {
                // assume vertical filling
                direction = src.topLeft.row < dest.topLeft.row ? 0 : 2;
            } else if (src.topLeft.row == dest.topLeft.row) {
                direction = src.topLeft.col < dest.topLeft.col ? 1 : 3;
            } else {
                throw new Error(MSG_NO_DIRECTION);
            }
        }
        var horizontal = direction & 1;
        var descending = direction & 2;
        if ((horizontal && src.height() != dest.height()) ||
            (!horizontal && src.width() != dest.width())) {
            throw new Error(MSG_INCOMPATIBLE);
        }
        var data = srcRange._properties(), n;
        if (!horizontal) {
            data = transpose(data);
            n = dest.height();
        } else {
            n = dest.width();
        }
        var fill = new Array(data.length);
        for (var i = 0; i < data.length; ++i) {
            var s = data[i];
            var f = findSeries(s);
            var a = fill[i] = new Array(n);
            for (var j = 0; j < n; ++j) {
                var idx = descending ? -j - 1 : s.length + j;
                var srcIdx = descending ? s.length - (j % s.length) - 1 : (j % s.length);
                a[descending ? n - j - 1 : j] = f(idx, srcIdx);
            }
        }
        if (!horizontal) {
            fill = transpose(fill);
        }
        return { props: fill, direction: direction, dest: destRange };
    };

    Range.prototype.fillFrom = function(srcRange, direction) {
        var x = this._previewFillFrom(srcRange, direction);
        x.dest._properties(x.props);
        return x.dest;
    };

    // This is essentially the FORECAST function, see ./runtime.functions.2.js.
    // It receives an array of values, and returns a function that "predicts"
    // the value in cell N.
    function linearRegression(data) {
        var N = data.length;
        var mx = (N + 1) / 2, my = data.reduce(function(a, b){
            return a + b;
        }, 0) / N;
        var s1 = 0, s2 = 0;
        for (var i = 0; i < N; i++) {
            var t1 = (i + 1) - mx, t2 = data[i] - my;
            s1 += t1 * t2;
            s2 += t1 * t1;
        }
        if (!s2) {
            return function(N){
                return data[N % data.length];
            };
        }
        var b = s1 / s2, a = my - b * mx;
        return function(N) {
            return a + b * (N + 1);
        };
    }

    function findSeries(properties) {
        function findStep(a) {
            var diff = a[1] - a[0];
            for (var i = 2; i < a.length; ++i) {
                if (a[i] - a[i-1] != diff) {
                    return null;
                }
            }
            return diff;
        }
        function getData(a) {
            return a.map(function(v){
                return v.number;
            });
        }
        var series = [];
        var data = properties.map(function(x){
            return x.formula || x.value;
        });
        forEachSeries(data, function(begin, end, type, a){
            var f, values;
            if (type == "number") {
                values = getData(a);
                if (values.length == 1 && (begin > 0 || end < data.length || properties[begin].format)) {
                    values.push(values[0] + 1);
                }
                f = linearRegression(values);
            } else if (type == "string" || type == "formula") {
                // formulas are simply copied over; the sheet will internally clone the objects
                f = function(N, i) {
                    return data[i];
                };
            } else if (Array.isArray(type)) {
                if (a.length == 1) {
                    f = function(N) {
                        return type[(a[0].number + N) % type.length];
                    };
                } else {
                    // figure out the step
                    var diff = findStep(getData(a));
                    if (diff == null) {
                        // seemingly no pattern, just repeat those strings
                        f = function(N) {
                            return a[(N) % a.length].value;
                        };
                    } else {
                        f = function(N) {
                            var idx = a[0].number + diff * N;
                            return type[idx % type.length];
                        };
                    }
                }
            } else if (type != "null") {
                values = getData(a);
                if (values.length == 1) {
                    values.push(values[0] + 1);
                }
                values = linearRegression(values);
                f = function(N, i) {
                    return data[i].replace(/^(.*\D)\d+/, "$1" + values(N, i));
                };
            } else {
                f = function() { return null; };
            }
            var s = { f: f, begin: begin, end: end, len: end - begin };
            for (var i = begin; i < end; ++i) {
                series[i] = s;
            }
        });
        return function(N, i) {
            var s = series[i];
            var q = N / data.length | 0;
            var r = N % data.length;
            var n = q * s.len + r - s.begin;
            var value = s.f(n, i);
            var props = clone(properties[i]);
            if (value instanceof Formula) {
                props.formula = value;
            } else {
                props.value = value;
            }
            return props;
        };
    }

    function clone(obj) {
        var copy = {};
        Object.keys(obj || {}).forEach(function(key){
            copy[key] = obj[key];
        });
        return copy;
    }

    function forEachSeries(data, f) {
        var prev = null, start = 0, a = [], type;
        for (var i = 0; i < data.length; ++i) {
            type = getType(data[i]);
            a.push(type);
            if (prev != null && type.type !== prev.type) {
                f(start, i, prev.type, a.slice(start, i));
                start = i;
            }
            prev = type;
        }
        f(start, i, prev.type, a.slice(start, i));
    }

    function getType(el) {
        if (typeof el == "number") {
            return { type: "number", number: el };
        }
        if (typeof el == "string") {
            var lst = findStringList(el);
            if (lst) {
                return lst;
            }
            var m = /^(.*\D)(\d+)/.exec(el);
            if (m) {
                el = el.replace(/^(.*\D)\d+/, "$1-######");
                return { type: el, match: m, number: parseFloat(m[2]) };
            }
            return { type: "string" };
        }
        if (typeof el == "boolean") {
            return { type: "boolean" };
        }
        if (el == null) {
            return { type: "null" };
        }
        if (el instanceof Formula) {
            return { type: "formula" };
        }
        window.console.error(el);
        throw new Error("Cannot fill data");
    }

    function stringLists() {
        var culture = kendo.culture();
        return [
            culture.calendars.standard.days.namesAbbr,
            culture.calendars.standard.days.names,
            culture.calendars.standard.months.namesAbbr,
            culture.calendars.standard.months.names
        ];
    }

    function findStringList(str) {
        var strl = str.toLowerCase();
        var lists = stringLists();
        for (var i = 0; i < lists.length; ++i) {
            var a = lists[i];
            for (var j = a.length; --j >= 0;) {
                var el = a[j].toLowerCase();
                if (el == strl) {
                    return { type: a, number: j, value: str };
                }
            }
        }
    }

    function transpose(a) {
        var height = a.length, width = a[0].length;
        var t = [];
        for (var i = 0; i < width; ++i) {
            t[i] = [];
            for (var j = 0; j < height; ++j) {
                t[i][j] = a[j][i];
            }
        }
        return t;
    }

})();

(function(){
    

    (function(kendo, undefined) {
        if (kendo.support.browser.msie && kendo.support.browser.version < 9) {
            return;
        }

        var $ = kendo.jQuery;

        var Widget = kendo.ui.Widget;
        var Workbook = kendo.spreadsheet.Workbook;
        var Controller = kendo.spreadsheet.Controller;
        var View = kendo.spreadsheet.View;
        var NS = ".kendoSpreadsheet";

        var ALL_REASONS = {
            recalc: true,
            selection: true,
            activeCell: true,
            layout: true,
            sheetSelection: true,
            resize: true
        };

        var classNames = {
            wrapper: "k-widget k-spreadsheet"
        };

        var Spreadsheet = kendo.ui.Widget.extend({
            init: function(element, options) {
                Widget.fn.init.call(this, element, options);

                this.element.addClass(Spreadsheet.classNames.wrapper);

                this._view = new View(this.element, {
                    toolbar: this.options.toolbar,
                    sheetsbar:this.options.sheetsbar
                });

                this._workbook = new Workbook(this.options, this._view);

                this._controller = new Controller(this._view, this._workbook);

                this._autoRefresh = true;

                this._bindWorkbookEvents();

                this._view.workbook(this._workbook);

                this.refresh();

                this._resizeHandler = function() { this.resize(); }.bind(this);
                $(window).on("resize" + NS, this._resizeHandler);
            },

            _resize: function() {
                this.refresh({ layout: true });
            },

            _workbookChange: function(e) {
                if (this._autoRefresh) {
                    this.refresh(e);
                }
            },

            activeSheet: function(sheet) {
                return this._workbook.activeSheet(sheet);
            },

            moveSheetToIndex: function (sheet, index) {
                return this._workbook.moveSheetToIndex(sheet, index);
            },

            insertSheet: function(options) {
                return this._workbook.insertSheet(options);
            },

            sheets: function() {
                return this._workbook.sheets();
            },

            removeSheet: function(sheet) {
                return this._workbook.removeSheet(sheet);
            },

            sheetByName: function(sheetName) {
                return this._workbook.sheetByName(sheetName);
            },

            sheetIndex: function(sheet) {
                return this._workbook.sheetIndex(sheet);
            },

            sheetByIndex: function(index) {
                return this._workbook.sheetByIndex(index);
            },

            renameSheet: function(sheet, newSheetName) {
                return this._workbook.renameSheet(sheet, newSheetName);
            },

            refresh: function(reason) {
                if (!reason) {
                    reason = ALL_REASONS;
                }

                this._view.sheet(this._workbook.activeSheet());
                this._controller.sheet(this._workbook.activeSheet());

                this._workbook.refresh(reason);
                this._view.refresh(reason);
                this._controller.refresh();
                this._view.render();

                this.trigger("render");
                return this;
            },

            openDialog: function(name, options) {
                return this._view.openDialog(name, options);
            },

            autoRefresh: function(value) {
                if (value !== undefined) {
                    this._autoRefresh = value;

                    if (value === true) {
                        this.refresh();
                    }

                    return this;
                }

                return this._autoRefresh;
            },

            toJSON: function() {
                return this._workbook.toJSON();
            },

            fromJSON: function(json) {
                if (json.sheets) {
                    this._workbook.destroy();

                    this._workbook = new Workbook($.extend({}, this.options, json));

                    this._bindWorkbookEvents();

                    this._view.workbook(this._workbook);
                    this._controller.workbook(this._workbook);

                    this.activeSheet(this.activeSheet());
                } else {
                    this.refresh();
                }
            },

            saveAsExcel: function(options) {
                this._workbook.saveAsExcel(options);
            },

            _workbookExcelExport: function(e) {
                if (this.trigger("excelExport", e)) {
                    e.preventDefault();
                }
            },

            _bindWorkbookEvents: function() {
                this._workbook.bind("change", this._workbookChange.bind(this));
                this._workbook.bind("excelExport", this._workbookExcelExport.bind(this));
            },

            destroy: function() {
                kendo.ui.Widget.fn.destroy.call(this);

                this._workbook.destroy();
                this._controller.destroy();
                this._view.destroy();

                if (this._resizeHandler) {
                    $(window).off("resize" + NS, this._resizeHandler);
                }
            },

            options: {
                name: "Spreadsheet",
                toolbar: true,
                sheetsbar: true,
                rows: 200,
                columns: 50,
                rowHeight: 20,
                columnWidth: 64,
                headerHeight: 20,
                headerWidth: 32,
                excel: {
                    proxyURL: "",
                    fileName: "Workbook.xlsx"
                }
            },

            events: [
                "excelExport",
                "render"
            ]
        });

        kendo.spreadsheet.ALL_REASONS = ALL_REASONS;
        kendo.ui.plugin(Spreadsheet);
        $.extend(true, Spreadsheet, { classNames: classNames });
    })(window.kendo);

})();

return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });