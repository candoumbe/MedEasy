"use strict";
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var AbstractPage_1 = require("../AbstractPage");
var IndexPageElementMap_1 = require("./IndexPageElementMap");
/**
 * Represents the index page for patient
 */
var IndexPage = (function (_super) {
    __extends(IndexPage, _super);
    /**
     * Builds a new IndexPage instance.
     */
    function IndexPage() {
        return _super.call(this, new IndexPageElementMap_1.IndexPageElementMap()) || this;
    }
    return IndexPage;
}(AbstractPage_1.AbstractPage));
exports.IndexPage = IndexPage;
//# sourceMappingURL=IndexPage.js.map