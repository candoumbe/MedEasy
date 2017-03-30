"use strict";
var HttpMethod_1 = require("./HttpMethod");
/**
 * Defines a form and it's content
 */
var Form = (function () {
    /**
     * Builds a new Form
     * @param {string} action url where the form data will be sent.
     * @param {HttpMethod} method method to used to send data.
     *
     * @throws Error if action is empty or whitespace.
     */
    function Form(action, method) {
        this._fields = [];
        if (this._action.trim().length === 0) {
            throw new Error("action cannot be empty or whitespace");
        }
        this._action = action.trim();
        this._method = method;
    }
    Object.defineProperty(Form.prototype, "fields", {
        /**
         * Gets the fields for the current form
         */
        get: function () {
            return this._fields;
        },
        enumerable: true,
        configurable: true
    });
    /**
     * Adds a field to the form
     * @param {FormField} field the field to add
     *
     * @throws Error if a field with the same id already exists
     */
    Form.prototype.addField = function (field) {
        this._fields.push(field);
    };
    /**
     * Builds the jquery representation of the form.
     */
    Form.prototype.create = function () {
        var action = this._action || HttpMethod_1.HttpMethod.POST;
        var form = $("<form role='form' action='" + action + "'>");
        switch (this._method) {
            case HttpMethod_1.HttpMethod.POST:
                form.attr("method", "POST");
                break;
            case HttpMethod_1.HttpMethod.PATCH:
                form.attr("method", "PATCH");
                break;
            case HttpMethod_1.HttpMethod.PUT:
                form.attr("method", "PUT");
                break;
            case HttpMethod_1.HttpMethod.DELETE:
                form.attr("method", "DELETE");
                break;
            default:
                throw new Error("Unknown http method");
        }
        this.fields.forEach(function (ff) {
            form.append(ff.create())
                .append($("<button role='submit' class='btn btn-primary'>Submit</button>"))
                .append($("<button role='button' class='btn btn-default'>Cancel</button>"));
        });
        return form;
    };
    return Form;
}());
exports.Form = Form;
//# sourceMappingURL=Form.js.map