"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var Index_1 = require("../Exceptions/Index");
var KeyValuePair_1 = require("./KeyValuePair");
var KeyAlreadyPresentException = (function (_super) {
    __extends(KeyAlreadyPresentException, _super);
    function KeyAlreadyPresentException(keyName) {
        _super.call(this, "keyName", "The key already exists");
    }
    return KeyAlreadyPresentException;
}(Index_1.Exception));
exports.KeyAlreadyPresentException = KeyAlreadyPresentException;
/**
 * An associative key, value array that doesn't allow duplicate keys
 * @param {TKey} TKey type of the key
 * @param {TValue} TValue type of the value
 */
var DictionaryBase = (function () {
    /**
     * Creates a new Dictionary instance
     */
    function DictionaryBase() {
        this._count = 0;
        this._changedSinceLastComputation = false;
    }
    /**
     * Counts the number of entries of the dictionary
     */
    DictionaryBase.prototype.count = function () {
        return this._entries.length;
    };
    /**
     * Computes keys and values properties so they can be accessed separately
     */
    DictionaryBase.prototype.compute = function () {
        this._values = [];
        this._keys = [];
        for (var i = 0; i < this._entries.length; i++) {
            this._keys.push(this._entries[i].key);
            this._values.push(this._entries[i].value);
        }
        this._changedSinceLastComputation = false;
    };
    /**
     * Gets all elements
     */
    DictionaryBase.prototype.entries = function () {
        return this._entries;
    };
    /**
     * Gets all keys
     */
    DictionaryBase.prototype.keys = function () {
        if (this._changedSinceLastComputation) {
            this.compute();
        }
        return this._keys;
    };
    /**
     * Gets all values
     */
    DictionaryBase.prototype.values = function () {
        return this._values;
    };
    DictionaryBase.prototype.containsKey = function (key) {
        return this.keys().filter(function (value) { return value === key; }).length > 0;
    };
    DictionaryBase.prototype.contains = function (kv) {
        return this.entries().filter(function (entry) { return entry.key === kv.key && entry.value === kv.value; }).length > 0;
    };
    DictionaryBase.prototype.value = function (key) {
        var result;
        var found = false;
        var i = 0;
        while (!found && i < this._entries.length) {
            var currentEntry = this._entries[i];
            found = currentEntry.key === key;
            if (found) {
                result = currentEntry.value;
            }
            i++;
        }
        return result;
    };
    DictionaryBase.prototype.add = function (key, value) {
        if (!key) {
            throw new Index_1.ArgumentNullException("key", "key cannot be null");
        }
        if (this.containsKey(key)) {
            throw new KeyAlreadyPresentException(String(key));
        }
        this._changedSinceLastComputation = true;
        this._entries.push(new KeyValuePair_1.KeyValuePair(key, value));
    };
    DictionaryBase.prototype.remove = function (key) {
        if (key === null) {
            throw new Index_1.ArgumentNullException("key", "key cannot be null");
        }
        var found = false;
        var i = 0;
        while (!found && i < this._entries.length) {
            var currentEntry = this._entries[i];
            found = currentEntry.key === key;
            if (found) {
                delete this._entries[i];
                this._changedSinceLastComputation = true;
            }
            i++;
        }
    };
    DictionaryBase.prototype.clear = function () {
        this._entries = [];
        this._values = [];
        this._keys = [];
    };
    return DictionaryBase;
}());
exports.DictionaryBase = DictionaryBase;
//# sourceMappingURL=DictionaryBase.js.map