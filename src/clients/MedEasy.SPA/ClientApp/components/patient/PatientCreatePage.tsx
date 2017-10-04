import * as React from "react";
import { FormField } from "./../../restObjects/FormField";
import { FormComponent } from "./../FormComponent"
import { Form } from "./../../restObjects/Form"
import { Endpoint } from "./../../restObjects/Endpoint"
import { LoadingComponent } from "./../LoadingComponent";
import * as LinQ from "linq";


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


    private handleChangeEvent(event: React.FormEvent<HTMLInputElement>, propertyName : string){
        let target = event.target;
        
    }

    private loadFormContents(): void {
        fetch(this.props.endpoint)
            .then((response) => response.json() as Promise<Array<Endpoint>>)
            .then((endpoints) => {

                let patientsEndpoint: Endpoint | undefined = LinQ.from(endpoints)
                    .singleOrDefault((x) => x.name.toLowerCase() === "patients");

                if (patientsEndpoint) {
                    let createForm: Form | undefined = LinQ.from(patientsEndpoint.forms)
                        .singleOrDefault((x) => x.meta && x.meta.relation === "create-form");

                    if (createForm) {
                        this.setState({ form: createForm, loading: false })
                    }
                }
            })
    }


    public render(): JSX.Element | null {
        let content = this.state.form
            ? <FormComponent form={this.state.form} />
            : <LoadingComponent />;
        return (
            <form>
                <h1>Nouveau</h1>
                <div className="form-group">
                    <label htmlFor="firstname" className="control-label"></label>
                    <input className="form-control col-md-6 col-sd-12"
                        onChange={(event) => {
                            this.setState((prevState, props) => {

                                let { formState } = prevState;

                                return {
                                    form: prevState.form,
                                    formState: prevState.formState,
                                    loading: prevState.loading
                                }
                            }); 
                    }} />
                </div>
            </form>
        );
    }
}