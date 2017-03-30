import { FormFieldType } from "../RestObjects/FormFieldType";
/**
 * Defines a form field and its content
 */
export class FormField {

    private _enabled: boolean | null;
    private _name: string | null;
    private _placeholder: string | null;
    private _type: FormFieldType;
    private _required: boolean | null;
    private _secret: boolean | null;
    private _minLength: number | null;
    private _maxLength: number | null;
    private _label: string | null;
    private _pattern: string | null;


    /** 
     * indicates whether or not the field value may be modified or submitted to a linked resource location. 
     * 
     **/
    public get enabled(): boolean | null {
        return this._enabled;
    }

    /** 
     * Type of the field 
    **/
    public get type(): FormFieldType | null {
        return this._type;
    }

    /** 
     * Label that can be associated with the field
     **/
    public get label(): string | null {
        return this._label;
    }

    /** 
     *  Name of the field that should be submitted
    **/
    public get name(): string | null {
        return this._name;
    }

    /** 
     *  Regular expression that the field should be validated against.
    **/
    public get pattern(): string | null {
        return this._pattern;
    }

    /** 
     *  Short hint that described the expected value of the field.
    **/
    public get placeholder(): string | null {
        return this._placeholder;
    }

    /** 
     *  Indicates if the field must be submitted
    **/
    public get required(): boolean | null {
        return this._required;
    }

    /** 
     *  Indicates the maximum length of the value
    **/
    public get maxLength(): number | null {
        return this._maxLength;
    }

    /** 
     *  Indicates the minimum length of the input
     **/
    public get minLength(): number | null {
        return this._minLength;
    }


    /** 
     *  Indicates whether or not the field value is considered sensitive information 
     *  and should be kept secret.
    **/
    public get secret(): boolean | null {
        return this._secret;
    }

    /** 
     *  Defines whether or not the field value is considered sensitive information 
     *  and should be kept secret.
     * @param {boolean | null} secret the new value
     **/
    public set secret(secret: boolean | null) {
        this._secret = secret;
    }


    /**
     * Buids a JQuery
     * @param {FormField} this. field of the form to build bootstrap element for.
     *
     * @returns the JQuery representation of the current element that can be append to a <form>
     */
    public create(): JQuery {
        let divField = $("<div class='form-group'>");

        let input: JQuery = $(`<input name='${this.name}' id='${this.name}'>`);
        switch (this.type) {
            case FormFieldType.Boolean:
                input.attr("type", "checkbox");
                break;
            case FormFieldType.Date:
                input.attr("type", "date")
                break;
            case FormFieldType.DateTime:
                input.attr("type", "datetime-local")
                break
            case FormFieldType.File:
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


        let label: JQuery | null = null;
        if (this.label) {
            label = $(`<label for='${this.name}'>`);
        }
        if (this.type === FormFieldType.Boolean) {
            divField.append(input);
            if (label) {
                divField.append(label);
            }
        } else {
            if (label) {
                divField.append(label);
            }
            divField.append(input)
        }

        return divField;
    };
}
