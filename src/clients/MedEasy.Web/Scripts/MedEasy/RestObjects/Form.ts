import { FormField } from "./FormField";
import { HttpMethod } from "./HttpMethod";

/**
 * Defines a form and it's content
 */
export class Form {

    private _fields: Array<FormField>;

    private _method: HttpMethod | undefined;

    private _action: string;

    /**
     * Builds a new Form
     * @param {string} action url where the form data will be sent.
     * @param {HttpMethod} method method to used to send data.
     *
     * @throws Error if action is empty or whitespace.
     */
    public constructor(action: string, method?: HttpMethod) {
        this._fields = [];

        if (this._action.trim().length === 0) {
            throw new Error("action cannot be empty or whitespace");
        }

        this._action = action.trim();
        this._method = method;
    }

    /**
     * Gets the fields for the current form
     */
    public get fields(): Array<FormField> {
        return this._fields;
    }

    /**
     * Adds a field to the form
     * @param {FormField} field the field to add
     *
     * @throws Error if a field with the same id already exists
     */
    public addField(field: FormField): void {
        this._fields.push(field);
    }



    /**
     * Builds the jquery representation of the form.
     */
    public create(): JQuery {
        let action: HttpMethod | string = this._action || HttpMethod.POST;

        let form = $(`<form role='form' action='${action}'>`);
        switch (this._method) {
            case HttpMethod.POST:
                form.attr("method", "POST");
                break;
            case HttpMethod.PATCH:
                form.attr("method", "PATCH");
                break;
            case HttpMethod.PUT:
                form.attr("method", "PUT");
                break;
            case HttpMethod.DELETE:
                form.attr("method", "DELETE");
                break;
            default:
                throw new Error(`Unknown http method`)
        }

        this.fields.forEach((ff) => {
            form.append(ff.create())
                .append($("<button role='submit' class='btn btn-primary'>Submit</button>"))
                .append($("<button role='button' class='btn btn-default'>Cancel</button>"));
        })

        return form;
    }

}