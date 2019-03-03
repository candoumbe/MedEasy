import * as React from "react";
import { Form } from "./../restObjects/Form";
import { FormField } from "./../restObjects/FormField";
import { FormFieldType } from "./../restObjects/FormFieldType";
import { FormFieldComponent } from "./FormFieldComponent";
import * as ReactRouter from "react-router";
import * as LinQ from "linq";
import * as Enumerable from "linq";
import { Alert, Button } from "react-bootstrap";


interface FormComponentProps {
    form: Form;
    handleSubmit: React.EventHandler<React.FormEvent<HTMLFormElement>>;
    onChange?: (name: string, value: any) => void | undefined;
    onBlur?: (name: string, value: any) => void | undefined;
    errors?: { [name: string]: string }
}

/**
 * The current state of the form.
 *
 */
interface FormComponentState {
    /** Data currently set on the form */
    data: { [name: string]: any },
    ongoing?: boolean;
    /** Id of the resource created by the submission of the form */
    resource?: any,
    /** Errors associated with the form */
    errors?: {
        [name: string]: string
    },

    isValid: boolean;
}


export class FormComponent extends React.Component<FormComponentProps, FormComponentState>{

    public constructor(props: FormComponentProps) {
        super(props);
        this.state = { ongoing: false, data: {}, errors: {}, isValid: false };
    }

    public render(): JSX.Element {
        let form: Readonly<Form> = Object.freeze(this.props.form);
        console.trace("Form to display", form);
        let errors = this.props.errors || [];
        let fieldNames = form.items.map(field => field.name);
        let errorsEnumerable = Enumerable.from(errors);
        let errorContainer = errorsEnumerable.any(err => err.key === "")
            ? <Alert bsStyle="alert">
                <ul>
                    {
                        errorsEnumerable.where(e => e.key === "")
                            .toArray()
                            .map((item, index) => <li key={index}>{item.value}</li>)
                    }
                </ul>
                <Button className="close" data-dismiss="alert" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </Button>
            </Alert>
            : null;
        return <form role="form" onSubmit={this.props.handleSubmit} action={form.meta.href} method={form.meta.method}>
            {errorContainer}
            
            {
                // Renders the form fields
                form.items.map((f) => <FormFieldComponent
                    key={f.name}
                    field={f}
                    errors={
                        errorsEnumerable
                            .where(e => e.key == f.name)
                            .select(e => `${e.value}`)
                            .distinct()
                            .toArray()
                    }
                    onChange={(value) => {
                        this.setState((prevState, props) => {
                            let newState = Object.assign({}, prevState);
                            newState[f.name] = value;
                            if (this.props.onChange) {
                                this.props.onChange(f.name, value);
                            }
                            return newState;
                        })
                    }}
                     />
        )

    }

            {this.props.children}
        </form>;

    }
}