/// <reference types="jquery" />
/// <reference types="bootstrap" />
import { FormFieldType } from "../RestObjects/FormFieldType";
/**
 * Defines a form field and its content
 */
export declare class FormField {
    private _enabled;
    private _name;
    private _placeholder;
    private _type;
    private _required;
    private _secret;
    private _minLength;
    private _maxLength;
    private _label;
    private _pattern;
    /**
     * indicates whether or not the field value may be modified or submitted to a linked resource location.
     *
     **/
    readonly enabled: boolean | null;
    /**
     * Type of the field
    **/
    readonly type: FormFieldType | null;
    /**
     * Label that can be associated with the field
     **/
    readonly label: string | null;
    /**
     *  Name of the field that should be submitted
    **/
    readonly name: string | null;
    /**
     *  Regular expression that the field should be validated against.
    **/
    readonly pattern: string | null;
    /**
     *  Short hint that described the expected value of the field.
    **/
    readonly placeholder: string | null;
    /**
     *  Indicates if the field must be submitted
    **/
    readonly required: boolean | null;
    /**
     *  Indicates the maximum length of the value
    **/
    readonly maxLength: number | null;
    /**
     *  Indicates the minimum length of the input
     **/
    readonly minLength: number | null;
    /**
     *  Indicates whether or not the field value is considered sensitive information
     *  and should be kept secret.
    **/
    /**
     *  Defines whether or not the field value is considered sensitive information
     *  and should be kept secret.
     * @param {boolean | null} secret the new value
     **/
    secret: boolean | null;
    /**
     * Buids a JQuery
     * @param {FormField} this. field of the form to build bootstrap element for.
     *
     * @returns the JQuery representation of the current element that can be append to a <form>
     */
    create(): JQuery;
}
