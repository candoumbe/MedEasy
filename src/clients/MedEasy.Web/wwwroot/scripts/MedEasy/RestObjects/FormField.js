"use strict";
var FormFieldType_1 = require("../RestObjects/FormFieldType");
/**
 * Defines a form field and its content
 */
var FormField = (function () {
    function FormField() {
    }
    Object.defineProperty(FormField.prototype, "enabled", {
        /**
         * indicates whether or not the field value may be modified or submitted to a linked resource location.
         *
         **/
        get: function () {
            return this._enabled;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormField.prototype, "type", {
        /**
         * Type of the field
        **/
        get: function () {
            return this._type;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormField.prototype, "label", {
        /**
         * Label that can be associated with the field
         **/
        get: function () {
            return this._label;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormField.prototype, "name", {
        /**
         *  Name of the field that should be submitted
        **/
        get: function () {
            return this._name;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormField.prototype, "pattern", {
        /**
         *  Regular expression that the field should be validated against.
        **/
        get: function () {
            return this._pattern;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormField.prototype, "placeholder", {
        /**
         *  Short hint that described the expected value of the field.
        **/
        get: function () {
            return this._placeholder;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormField.prototype, "required", {
        /**
         *  Indicates if the field must be submitted
        **/
        get: function () {
            return this._required;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormField.prototype, "maxLength", {
        /**
         *  Indicates the maximum length of the value
        **/
        get: function () {
            return this._maxLength;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormField.prototype, "minLength", {
        /**
         *  Indicates the minimum length of the input
         **/
        get: function () {
            return this._minLength;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(FormField.prototype, "secret", {
        /**
         *  Indicates whether or not the field value is considered sensitive information
         *  and should be kept secret.
        **/
        get: function () {
            return this._secret;
        },
        /**
         *  Defines whether or not the field value is considered sensitive information
         *  and should be kept secret.
         * @param {boolean | null} secret the new value
         **/
        set: function (secret) {
            this._secret = secret;
        },
        enumerable: true,
        configurable: true
    });
    /**
     * Buids a JQuery
     * @param {FormField} this. field of the form to build bootstrap element for.
     *
     * @returns the JQuery representation of the current element that can be append to a <form>
     */
    FormField.prototype.create = function () {
        var divField = $("<div class='form-group'>");
        var input = $("<input name='" + this.name + "' id='" + this.name + "'>");
        switch (this.type) {
            case FormFieldType_1.FormFieldType.Boolean:
                input.attr("type", "checkbox");
                break;
            case FormFieldType_1.FormFieldType.Date:
                input.attr("type", "date");
                break;
            case FormFieldType_1.FormFieldType.DateTime:
                input.attr("type", "datetime-local");
                break;
            case FormFieldType_1.FormFieldType.File:
                input.attr("type", "file");
                break;
            default:
                input.attr("type", "text");
                break;
        }
        if (!Boolean(this.enabled)) {
            input.prop("disabled", "disabled");
        }
        if (this.minLength) {
            input.attr("minlength", this.minLength);
        }
        if (this.maxLength) {
            input.attr("maxlength", this.maxLength);
        }
        var label = null;
        if (this.label) {
            label = $("<label for='" + this.name + "'>");
        }
        if (this.type === FormFieldType_1.FormFieldType.Boolean) {
            divField.append(input);
            if (label) {
                divField.append(label);
            }
        }
        else {
            if (label) {
                divField.append(label);
            }
            divField.append(input);
        }
        return divField;
    };
    ;
    return FormField;
}());
exports.FormField = FormField;
//# sourceMappingURL=FormField.js.map