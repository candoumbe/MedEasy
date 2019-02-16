import * as React from "react";
import { FormField } from "./../../restObjects/FormField";
import { FormComponent } from "./../FormComponent"
import { Form } from "./../../restObjects/Form"
import { Endpoint } from "./../../restObjects/Endpoint"
import { LoadingComponent } from "./../LoadingComponent";
import * as LinQ from "linq";
import { Container } from "react-bootstrap/lib/Tab";
import { Nav } from "react-bootstrap";


/** State of the component */
interface PatientCreatePageState {
    /** The current form displayed */
    form?: Form,
    /** Should a loading component be displayed ? */
    loading: boolean,

    formState?: {
        [key: string]: boolean | number | string | Date | undefined
    }

}

interface PatientCreateComponentProps {
    /** endpoint where to get forms descriptions from */
    endpoint: string,

}

export class PatientCreatePage extends React.Component<PatientCreateComponentProps, PatientCreatePageState> {

    /**
     * Builds a new PatientCreatePage component
     * @param {PatientCreateComponentProps} props
     */
    public constructor(props: PatientCreateComponentProps) {
        super(props);
        this.state = { loading: true };
        this.loadFormContents();
    }



    private async loadFormContents(): Promise<void> {
        let response: Response = await fetch(this.props.endpoint);
        let endpoints: Array<Endpoint> = await (response.json() as Promise<Array<Endpoint>>);

        let patientsEndpoint: Endpoint | undefined = LinQ.from(endpoints)
            .singleOrDefault((x) => x.name.toLowerCase() === "patients");

        if (patientsEndpoint) {
            let form: Form | undefined = LinQ.from(patientsEndpoint.forms)
                .singleOrDefault((x) => x.meta && x.meta.relation === "create-form");

            if (form) {
                this.setState({ form: form, loading: false })
            }
        }

    }


    public render(): JSX.Element | null {

        let submit: React.EventHandler<React.FormEvent<HTMLFormElement>> = async (event) => {
            event.preventDefault();

            let response: Response = await fetch(
                this.props.endpoint,
                {
                    headers: { "content-type": "application/json" },
                    method: "POST",
                    body: JSON.stringify(this.state.form)
                });
            if (!response.ok) {
                let errors = await (response.json() as Promise<Array<MedEasy.DTO.ErrorInfo>>)
                this.setState((prevState, props) => {
                    let newState = Object.assign({}, prevState, { ongoing: false, errors: errors });

                    return newState;
                });
            } else {
                let token: string = await (response.json() as Promise<string>);
                console.log(`received token : '${token}'`);
            }
        };
        let onChange: (name: string, value: any) => void = (name, val) => {
            this.setState((prevState, props) => {
                let newState = prevState;
                newState.formState[name] = val;
                return newState;
            });
        }
        let content = this.state.form
            ? <FormComponent form={this.state.form} handleSubmit={submit} onChange={onChange}>
            </FormComponent>
            : <LoadingComponent />;

        return content;
    }
}