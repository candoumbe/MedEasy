"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Exception_1 = require("./Exception");
/**
 * Root class for all exceptions
 */
var ArgumentOutOfRangeException = (function (_super) {
    __extends(ArgumentOutOfRangeException, _super);
    function ArgumentOutOfRangeException(paramName, message) {
        if (paramName === void 0) { paramName = null; }
        _super.call(this, "ArgumentOutOfRangeException", message);
        this._paramName = paramName;
    }
    Object.defineProperty(ArgumentOutOfRangeException.prototype, "paramName", {
        get: function () {
            return this._paramName;
        },
        enumerable: true,
        configurable: true
    });
    return ArgumentOutOfRangeException;
}(Exception_1.Exception));
exports.ArgumentOutOfRangeException = ArgumentOutOfRangeException;
//# sourceMappingURL=ArgumentOutOfRangeException.js.map