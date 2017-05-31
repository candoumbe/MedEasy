import * as React from "react";
import { FormField } from "./../../restObjects/FormField";
import { FormComponent } from "./../FormComponent"
import { Form } from "./../../restObjects/Form"
import { Endpoint } from "./../../restObjects/Endpoint"
import { LoadingComponent } from "./../LoadingComponent";
import * as LinQ from "linq";

/** State of the component */
interface PatientDetailsState {
    patient?: MedEasy.DTO.Patient;
}

interface PatientDetailsComponentProps {
    /** endpoint where to get */
    endpoint: string,

}

export class PatientDetails extends React.Component<PatientDetailsComponentProps, PatientDetailsState> {

    public constructor(props: PatientDetailsComponentProps) {
        super(props);
        
        this.loadContent();
    }


    private loadContent(): void {
        fetch(this.props.endpoint)
            .then((response) => response.json() as Promise<MedEasy.DTO.Patient>)
            .then((patient) => {
                this.setState({ patient: patient });
            })
    }


    public render(): JSX.Element | null {
        let component: JSX.Element | null = this.state
            ? <div className="panel">
                <div className="panel-heading"></div>
                <div className="panel-body">
                    <span className="Firstname"></span>
                </div>
            </div>
            :

            <div className="panel">
                <div className="panel-body">
                    Aucune ligne sélectionné
                </div>
            </div>
            ;

        


        return component;
    }
}