import * as React from "react";
import { Form } from "./../restObjects/Form";
import { FormField } from "./../restObjects/FormField";
import { FormFieldType } from "./../restObjects/FormFieldType";
import { FormFieldComponent } from "./FormFieldComponent";
import * as ReactRouter from "react-router";
import * as LinQ from "linq";

interface FormComponentProps {
    form: Form
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
    resource?: {
        id?: string;
        href?: string
    }
    /** Errors associated with the form */
    errors?: {
        [name: string]: string
    }
}


export class FormComponent extends React.Component<FormComponentProps, FormComponentState>{

    public constructor(props: FormComponentProps) {
        super(props);
        this.state = { ongoing: false, data: {}, errors: {} };
    }

    public handleChange: React.EventHandler<React.FormEvent<HTMLInputElement>> = (event) => {
        this.setState((prevState, props) => {
            let newState: FormComponentState = { ...prevState };
            newState.data = prevState.data || {};
            newState.data[event.currentTarget.name] = event.currentTarget.value;

            return newState;
        })
    }

    public handleSubmit: React.EventHandler<React.FormEvent<HTMLFormElement>> = (event) => {
        event.preventDefault();

        const form = this.props.form;

        let { data } = this.state;
        this.setState({ ongoing: true });
        let response = fetch(form.meta.href, { method: form.meta.method, body: JSON.stringify(data) })
            .then(async response => {
                let result;
                if (!response.ok) {
                    if (response.status === 400) {
                        let errors = await (response.json() as Promise<Array<MedEasy.DTO.ErrorInfo>>)
                        this.setState((prevState, props) => {
                            return {
                                ongoing: false,
                                errors: errors
                            }
                        });
                    }


                } else {
                    response.json() as Promise<MedEasy.DTO.Patient>
                }
                this.setState({ ongoing: false });

                return response.json() as Promise<MedEasy.DTO.Patient>
            })
            .then(patient => {
                this.setState({ resource: patient });
            })


    }


    private isValid(): boolean {
        return this.state.data && LinQ.from(this.props.form.items)
            .all(item => Boolean(this.state.data[item.name]));
    }

    public render() {
        let form: Readonly<Form> = Object.freeze(this.props.form);
        console.debug("Form to display", form);

        let component: JSX.Element;

        if (this.state.resource) {  
            component = <ReactRouter.Redirect to={`/patients/${this.state.resource.id}`} push={true} />
        } else {
            component = <form role="form" onSubmit={this.handleSubmit}>
                {
                    // Renders the form fields
                    form.items.map((f) => <FormFieldComponent key={f.name} field={f} />)
                }

                <nav>
                    <button type="submit" className="btn btn-primary btn-xs-12 btn-sm-6" disabled={Boolean(this.state.errors) || this.state.ongoing}>
                        <span className="glyphicon glyphicon-save"></span>&nbsp;Create
                    </button>
                    <button type="button" className="btn btn-default btn-xs-12 btn-sm-6">
                        <span className="glyphicon glyphicon-cancel"></span>&nbsp;Cancel
                    </button>
                </nav>
            </form>;

        }

        return component;

    }
}