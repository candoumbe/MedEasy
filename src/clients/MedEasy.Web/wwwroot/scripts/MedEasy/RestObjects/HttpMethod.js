"use strict";
/**
 * Types of method that can be performed with a form
 **/
var HttpMethod;
(function (HttpMethod) {
    HttpMethod[HttpMethod["GET"] = 0] = "GET";
    HttpMethod[HttpMethod["POST"] = 1] = "POST";
    HttpMethod[HttpMethod["PUT"] = 2] = "PUT";
    HttpMethod[HttpMethod["DELETE"] = 3] = "DELETE";
    HttpMethod[HttpMethod["PATCH"] = 4] = "PATCH";
})(HttpMethod = exports.HttpMethod || (exports.HttpMethod = {}));
//# sourceMappingURL=HttpMethod.js.map