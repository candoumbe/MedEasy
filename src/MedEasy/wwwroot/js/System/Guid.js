"use strict";
/**
 * An instance of this class represents a Guid identifier.
 */
var Guid = (function () {
    function Guid(guid) {
        this._guid = guid;
    }
    /**
        * Converts the Guid to string
        */
    Guid.prototype.toString = function () {
        return this._guid;
    };
    /**
    * Create a new Guid instance
    */
    Guid.new = function () {
        var result;
        var i;
        var j;
        result = "";
        for (j = 0; j < 32; j++) {
            if (j == 8 || j == 12 || j == 16 || j == 20)
                result = result + '-';
            i = Math.floor(Math.random() * 16).toString(16).toUpperCase();
            result = result + i;
        }
        return new Guid(result);
    };
    return Guid;
}());
exports.Guid = Guid;
//# sourceMappingURL=Guid.js.map