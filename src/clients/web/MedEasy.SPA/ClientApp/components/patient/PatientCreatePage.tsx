import * as React from "react";
import { FormField } from "./../../restObjects/FormField";
import { FormComponent } from "./../FormComponent"
import { Form } from "./../../restObjects/Form"
import { Endpoint } from "./../../restObjects/Endpoint"
import { Browsable } from "./../../restObjects/Browsable"
import { RestClient } from "./../../System/RestClient";
import { Guid } from "./../../System/Guid";
import { LoadingComponent } from "./../LoadingComponent";
import * as LinQ from "linq";
import { Container } from "react-bootstrap/lib/Tab";
import { Nav, Button } from "react-bootstrap";
import { Redirect } from "react-router";


/** State of the component */
interface PatientCreatePageState {
    /** The current form displayed */
    form?: Form,
    /** Should a loading component be displayed ? */
    loading: boolean,

    formState: {
        [key: string]: boolean | number | string | Date | undefined
    }

    redirect?: string;

}

interface PatientCreateComponentProps {
    /** endpoint where to get forms descriptions from */
    endpoint: string,
    restClient: RestClient

}

export class PatientCreatePage extends React.Component<PatientCreateComponentProps, PatientCreatePageState> {

    /**
     * Builds a new PatientCreatePage component
     * @param {PatientCreateComponentProps} props
     */
    public constructor(props: PatientCreateComponentProps) {
        super(props);
        this.state = {
            loading: true,
            formState: {}
        };
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

            let browsableResource = await this.props.restClient.post<{ [key: string]: any }, Browsable<MedEasy.DTO.Patient>>(this.state.formState);

            this.setState({ redirect: `/patients/${browsableResource.resource.id}` })
        };
        let onChange: (name: string, value: any) => void = (name, val) => {
            this.setState((prevState, props) => {
                let newState = prevState;
                newState.formState[name] = val;
                return newState;
            });
        }
        let content: JSX.Element;
        if (this.state.form) {
            content = <FormComponent form={this.state.form} handleSubmit={submit} onChange={onChange}>
                <Button type='submit' bsStyle="success">Create</Button>
            </FormComponent>;
        } else if (this.state.loading) {
            content = <LoadingComponent />
        } else if (this.state.redirect) {
            content = <Redirect to={this.state.redirect} />
        }

        return content;
    }
}