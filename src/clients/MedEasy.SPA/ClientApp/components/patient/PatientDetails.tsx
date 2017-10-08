import * as React from "react";
import {  match } from 'react-router-dom';
import { FormField } from "./../../restObjects/FormField";
import { FormComponent } from "./../FormComponent"
import { Form } from "./../../restObjects/Form"
import { Endpoint } from "./../../restObjects/Endpoint"
import { LoadingComponent } from "./../LoadingComponent";
import * as LinQ from "linq";
import { BrowsableResource } from "./../../restObjects/BrowsableResource";
import { MeasuresRecap } from "./../../components/measures/MeasuresRecap";

interface PatientDetailsComponentProps {
    /** endpoint where to get patient details from */
    endpoint: string
}

interface PatientDetailsComponentState {
    /** The patient currently displayed */
    patient: null | BrowsableResource<MedEasy.DTO.Patient>,
    loading : boolean | undefined
}

/**
 * Displays a patient details
 * @see Patient
 */
export class PatientDetails extends React.Component<PatientDetailsComponentProps, PatientDetailsComponentState> {

    public constructor(props: PatientDetailsComponentProps) {
        super(props);
        this.setState({ loading: true})
        this.loadContent()
            .then(() => console.trace("Details loaded"));
    }

    private async loadContent(): Promise<void> {
        let response = await fetch(this.props.endpoint);
        if (response.ok) {
            let item = await (response.json() as Promise<BrowsableResource<MedEasy.DTO.Patient>>);
            this.setState({  patient: item, loading : false });
        } else {
            if (response.status === 404) {

            }
        }
    }


    public render(): JSX.Element | null {
        
        let component: JSX.Element | null = null;

        if (this.state.loading) {
            component = <LoadingComponent />
        } else {
            let browsablePatient = this.state.patient;
            this.state.patient
                ?
                <div>
                    <div className="page-header">
                        <h1>{browsablePatient.resource.fullname} <small>{browsablePatient.resource.birthDate ? "né le " + browsablePatient.resource.birthDate : ""}</small></h1>
                    </div>

                    <MeasuresRecap endpoint={this.props.endpoint} />


                </div>
                : <div>>Resource not found</div>;
        }
        return component;
    }
}