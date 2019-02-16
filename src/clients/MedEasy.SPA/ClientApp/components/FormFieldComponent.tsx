import * as React from "react";
import { FormField } from "./../restObjects/FormField";
import { ErrorComponent } from "./ErrorComponent";
import { FormGroup } from "react-bootstrap";



interface FormFieldComponentProps {
    field: FormField;
    /**
     * 
     */
    onChange?: (value: any) => void;
    onBlur?: (value: any) => void;
    /**
     *
     */
    onChanging?: (oldValue: any, newValue: any) => boolean;
    errors? : Array<string>
}

interface FormFieldComponentState {
    value: any,
    errors? : Array<string>
}

export class FormFieldComponent extends React.Component<FormFieldComponentProps, FormFieldComponentState>{

    private readonly handleChange: React.EventHandler<React.FormEvent<HTMLInputElement>>;
    private readonly handleBlur?: React.EventHandler<React.FormEvent<HTMLInputElement>>;


    /**
     * Builds a new FormFieldComponent instance
     * @param {FormFieldComponentProps} props
     */
    public constructor(props: FormFieldComponentProps) {
        super(props);
        this.state = {value : props.field.value};
        this.handleChange = (event) => {
            if(this.props.onChanging)
            {
                let newValue: any = event.currentTarget.value;
                if (this.props.onChanging(this.state ? this.state.value : undefined, newValue )) {
                  this.setState({ value: newValue });
                }
            } else {
                  this.setState({ value: event.currentTarget.value });
                  if (this.props.onChange) {
                      this.props.onChange(event.currentTarget.value);
                  }
            }
        };
        
    }


    private mapFieldTypeToInputType: (f: FormField) => string = (f) => {
        let inputType: string = "text";

        if (f.type) {
            if (f.secret) {
                inputType = "password";
            }
            else {
                switch (f.type.toLowerCase()) {
                    case "string":
                        inputType = "text";
                        break;
                    case "email":
                        inputType = "email";
                        break;
                    case "date":
                        inputType = "date";
                        break;
                    case "boolean":
                        inputType = "checkbox";
                        break;
                }
            }
        }

        return inputType;

    };

    
    public render() {
        let f = this.props.field;
        let attributes : React.InputHTMLAttributes<HTMLInputElement> = {
            type: this.mapFieldTypeToInputType(f) || "text",
            className: "form-control",
            id: f.name,
            name: f.name,
            minLength: f.minLength,
            maxLength: f.maxLength,
            
            title: f.description,
            placeholder: f.placeholder,
            value: this.state && this.state.value
                ? this.state.value
                : "",
            required: f.required,
            pattern: f.pattern,
            onChange : this.handleChange
        };

        let errorMessage : string |undefined = null;
        if (this.state.errors) {
            this.state.errors.forEach(error => errorMessage ? error : `${errorMessage} <br />${error}`);
        }
        let errorComponent = errorMessage
            ? <ErrorComponent text={errorMessage} />
            : null;

        let input = f.type !== "Boolean"
            ? <FormGroup>
                <label htmlFor={f.name}>{f.label}{f.required ? <span className="text-danger">&nbsp;*&nbsp;</span> : null}</label>
                <input ref={f.name} {...attributes} />
                {errorComponent}
            </FormGroup>
            : <FormGroup>
                <input ref={f.name} {...attributes}  />
                <label htmlFor={f.name}>{f.label}</label>
                {errorComponent}
            </FormGroup>;

        
        return input;
    }
}