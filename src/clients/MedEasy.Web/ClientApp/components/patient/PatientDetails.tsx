import * as React from "react";
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

/**
 * Displays a patient details
 * @see Patient
 */
export class PatientDetails extends React.Component<PatientDetailsComponentProps, null | BrowsableResource<MedEasy.DTO.Patient>> {

    public constructor(props: PatientDetailsComponentProps) {
        super(props);

        this.loadContent();
    }


    private loadContent(): void {
        fetch(this.props.endpoint)
            .then((response) => response.json() as Promise<BrowsableResource<MedEasy.DTO.Patient>>)
            .then((item) => {
                this.setState(() => item);
            })
    }


    public render(): JSX.Element | null {
        let browsablePatient : BrowsableResource<MedEasy.DTO.Patient> | null = this.state;
        let component: JSX.Element | null = browsablePatient
            ?
            <div>
                <div className="page-header">
                    <h1>{browsablePatient.resource.fullname} <small>{browsablePatient.resource.birthDate ? "né le " + browsablePatient.resource.birthDate : ""}</small></h1>
                </div>

                <MeasuresRecap endpoint={this.props.endpoint} />


            </div>
            : <LoadingComponent />;




        return component;
    }
}