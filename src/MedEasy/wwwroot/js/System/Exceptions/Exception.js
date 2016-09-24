"use strict";
var Exception = (function () {
    function Exception(
        /** Name of the exception */
        name, 
        /**Message of the exception */
        message) {
        this.name = name;
        this.message = message;
    }
    return Exception;
}());
exports.Exception = Exception;
//# sourceMappingURL=Exception.js.map