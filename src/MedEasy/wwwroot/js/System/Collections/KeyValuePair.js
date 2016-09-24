"use strict";
/**
 * An instance of this class represents
 */
var KeyValuePair = (function () {
    /**
        * Creates an instance of a KeyValuePair
        * @param key the key
        * @param value the value
        */
    function KeyValuePair(key, value) {
        this._key = key;
        this._value = value;
    }
    Object.defineProperty(KeyValuePair.prototype, "key", {
        /**
         * Gets the key
         */
        get: function () {
            return this._key;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(KeyValuePair.prototype, "value", {
        /**
         * Gets the value
         */
        get: function () {
            return this._value;
        },
        enumerable: true,
        configurable: true
    });
    ;
    return KeyValuePair;
}());
exports.KeyValuePair = KeyValuePair;
//# sourceMappingURL=KeyValuePair.js.map