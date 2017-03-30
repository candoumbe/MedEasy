"use strict";
var AbstractPage = (function () {
    function AbstractPage(map) {
        this._map = map;
    }
    Object.defineProperty(AbstractPage.prototype, "map", {
        get: function () { return this._map; },
        enumerable: true,
        configurable: true
    });
    return AbstractPage;
}());
exports.AbstractPage = AbstractPage;
//# sourceMappingURL=AbstractPage.js.map