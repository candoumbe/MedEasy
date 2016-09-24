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
var ArgumentException = (function (_super) {
    __extends(ArgumentException, _super);
    function ArgumentException(paramName, message) {
        if (paramName === void 0) { paramName = null; }
        _super.call(this, "ArgumentException", message);
        this.paramName = paramName;
    }
    return ArgumentException;
}(Exception_1.Exception));
exports.ArgumentException = ArgumentException;
//# sourceMappingURL=ArgumentException.js.map