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
import { NotFoundComponent } from "./../NotFoundComponent";
import { Guid } from "./../../System/Guid";
import { RestClient } from "./../../System/RestClient";

interface PatientDetailsComponentProps {
    /** endpoint where to get patient details from */
    endpoint: string,
    id : string | Guid
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

    private static measures = [
        { relation: "blood-pressures", resource : "bloodPressures" },
        { relation: "body-weights", resource : "bodyWeights" },
        { relation: "temperatures", resource: "temperatures" },
        { relation: "heartbeats", resource: "heartbeats" },
    ];

    private _httpClient: RestClient<Guid, BrowsableResource<MedEasy.DTO.Patient>>

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
                this.setState({ loading: false });
            }
        }
    }


    public render(): JSX.Element | null {
        
        let component: JSX.Element | null = null;

        if (this.state.loading) {
            component = <LoadingComponent />
        } else {
            let browsablePatient = this.state.patient;
            let measures = PatientDetails.measures.filter(measure => browsablePatient.links.some((link) => measure.relation == link.relation));
            let measuresComponents: Array<JSX.Element> = [];
            this.state.patient
                ?
                <div>
                    <div className="page-header">
                        <h1>{browsablePatient.resource.fullname} <small>{browsablePatient.resource.birthDate ? "né(e) le " + browsablePatient.resource.birthDate : ""}</small></h1>
                    </div>
                    {
                        
                        measures.forEach((measure) => {
                            switch (measures) {

                                default:
                                    break;
                            }

                            measuresComponents.push(<MeasuresRecap endpoint={this.props.endpoint} resourceName={"bloodPressures"} />);
                        })
                    }

                    {measuresComponents}
                </div>
                : <NotFoundComponent />;
        }
        return component;
    }
}