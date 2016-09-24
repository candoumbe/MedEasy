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
var ArgumentNullException = (function (_super) {
    __extends(ArgumentNullException, _super);
    /**
        * Creates an ArgumentNullException instance
        * @param {string} paramName name of the parameter that is null
        * @param {string} message additional information message
        */
    function ArgumentNullException(paramName, message) {
        if (message === void 0) { message = null; }
        _super.call(this, "ArgumentNullException", message);
        this._paramName = paramName;
    }
    Object.defineProperty(ArgumentNullException.prototype, "paramName", {
        get: function () {
            return this._paramName;
        },
        enumerable: true,
        configurable: true
    });
    return ArgumentNullException;
}(Exception_1.Exception));
exports.ArgumentNullException = ArgumentNullException;
//# sourceMappingURL=ArgumentNullException.js.map