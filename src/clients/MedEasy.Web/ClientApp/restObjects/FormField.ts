import { FormFieldType } from "./FormFieldType";

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
    public placeholder?: string;
    public required?: true;
    public maxLength?: number;
    public minLength?: number;
    public secret?: boolean;
    public description?: string


}