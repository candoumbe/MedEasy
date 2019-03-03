import { FormFieldType } from "./FormFieldType";
import { Guid } from "./../System/Guid";

/**
 * 
 */
export class FormField {


    /**
     * Builds a new Form field
     * @param {string} name name of the field
     * @param {string} type type of the field
     */
    public constructor(public readonly name: string, public readonly type: string) {

    }

    /** Defines/set wheter the user can interact with the field */
    public enabled?: boolean;
    public label?: string;
    public pattern?: string;
    /** 
     * Indicates whether or not the field value 
     * may be modified before it is submitted to the form’s linked resource location. 
     * 
     * 
     */
    public mutable?: boolean;
    public placeholder?: string;
    public required?: true;
    public maxLength?: number;
    public minLength?: number;
    public min?: number | Date;
    public max?: number | Date;
    public secret?: boolean;
    public description?: string
    public value? : string | number | boolean | Date | Guid


}