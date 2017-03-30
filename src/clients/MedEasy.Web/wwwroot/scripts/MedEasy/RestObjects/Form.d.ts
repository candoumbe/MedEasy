/// <reference types="jquery" />
/// <reference types="bootstrap" />
import { FormField } from "./FormField";
import { HttpMethod } from "./HttpMethod";
/**
 * Defines a form and it's content
 */
export declare class Form {
    private _fields;
    private _method;
    private _action;
    /**
     * Builds a new Form
     * @param {string} action url where the form data will be sent.
     * @param {HttpMethod} method method to used to send data.
     *
     * @throws Error if action is empty or whitespace.
     */
    constructor(action: string, method?: HttpMethod);
    /**
     * Gets the fields for the current form
     */
    readonly fields: Array<FormField>;
    /**
     * Adds a field to the form
     * @param {FormField} field the field to add
     *
     * @throws Error if a field with the same id already exists
     */
    addField(field: FormField): void;
    /**
     * Builds the jquery representation of the form.
     */
    create(): JQuery;
}
