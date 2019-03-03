import * as React from "react";
import { match } from 'react-router-dom';
import { FormField } from "./../../restObjects/FormField";
import { FormComponent } from "./../FormComponent"
import { Form } from "./../../restObjects/Form"
import { Endpoint } from "./../../restObjects/Endpoint"
import { LoadingComponent } from "./../LoadingComponent";
import * as LinQ from "linq";
import { Browsable } from "./../../restObjects/Browsable";
import { MeasuresRecap } from "./../../components/measures/MeasuresRecap";
import { NotFoundComponent } from "./../NotFoundComponent";
import { Guid } from "./../../System/Guid";
import { RestClient } from "./../../System/RestClient";
import { Option } from "./../../System/Option_Maybe";

interface PatientDetailsComponentProps {
    /** endpoint where to get patient details from */
    restClient: RestClient
    id: string | Guid
    measuresEndpoint : string
}

interface PatientDetailsComponentState {
    /** The patient currently displayed */
    patient: null | Browsable<MedEasy.DTO.Patient>,
    loading: boolean | undefined
}

/**
 * Displays a patient details
 * @see Patient
 */
export class PatientDetails extends React.Component<PatientDetailsComponentProps, PatientDetailsComponentState> {

    private static measures = [
        { relation: "blood-pressures", resource: "bloodPressures" },
        { relation: "body-weights", resource: "bodyWeights" },
        { relation: "temperatures", resource: "temperatures" },
        { relation: "heartbeats", resource: "heartbeats" },
    ];

    private readonly measuresRestClient: RestClient;


    public constructor(props: PatientDetailsComponentProps) {
        super(props);
        this.state = { loading: true, patient: null };

        this.loadContent()
            .then(() => console.trace("Details loaded"));
        this.measuresRestClient = new RestClient({
            host: `${this.props.measuresEndpoint}/patients/${this.props.id}`,
            beforeRequestCallback: this.props.restClient.options.beforeRequestCallback,
            defaultHeaders: this.props.restClient.options.defaultHeaders
        });
    }

    private async loadContent(): Promise<void> {

        let optionalPatient = await this.props.restClient.get<Guid | string, Browsable<MedEasy.DTO.Patient>>(this.props.id);
        optionalPatient.match(
            async patient => this.setState({ patient: await patient as Browsable<MedEasy.DTO.Patient>, loading: false }),
            () => this.setState({ loading: false })
        );
    }


    public render(): JSX.Element | null {

        let component: JSX.Element | null = null;

        if (this.state.loading) {
            component = <LoadingComponent />
        } else if (this.state.patient) {
            let browsablePatient = this.state.patient;
            let measures = PatientDetails.measures.filter(measure => browsablePatient.links.some((link) => measure.relation == link.relation));
            let measuresComponents: Array<JSX.Element> = [];

            component = <div>
                <div className="page-header">
                    <h1>{browsablePatient.resource.fullname} <small>{browsablePatient.resource.birthDate ? "né(e) le " + browsablePatient.resource.birthDate.toLocaleString() : ""}</small></h1>
                </div>
                <MeasuresRecap restClient={this.measuresRestClient} resourceName={"bloodPressures"} />
                {

                    measures.forEach((measure) => {
                        switch (measures) {

                            default:
                                break;
                        }

                       
                })
            }

                
            </div>
        } else {
            component = <NotFoundComponent />;
        }
        return component;
    }
}