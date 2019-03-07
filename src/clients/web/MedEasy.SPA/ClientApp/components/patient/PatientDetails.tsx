import * as LinQ from "linq";
import * as React from "react";
import { Button, Col, FormGroup, Grid, Modal, ModalBody, ModalFooter, ModalHeader, ModalTitle, Row, Form, FormControl } from "react-bootstrap";
import { Browsable } from "./../../restObjects/Browsable";
import { Guid } from "./../../System/Guid";
import { RestClient } from "./../../System/RestClient";
import { LoadingComponent } from "./../LoadingComponent";
import { NotFoundComponent } from "./../NotFoundComponent";

interface PatientDetailsComponentProps {
    /** endpoint where to get patient details from */
    restClient: RestClient
    id: string | Guid
    measuresEndpoint: string
}

interface PatientDetailsComponentState {
    /** The patient currently displayed */
    patient: null | Browsable<MedEasy.DTO.Patient>,
    loading: boolean | undefined,
    creatingAppointment: boolean | undefined,
    newAppointmentFormState?: {
        startDate: Date,
        title: string,
        doctorId: Guid
    }
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
        this.state = {
            loading: true,
            patient: null,
            creatingAppointment: false
        };

        this.loadContent()
            .then(() => console.trace("Details loaded"));
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
        let now: Date = new Date();
        if (this.state.loading) {
            component = <LoadingComponent />
        } else if (this.state.patient) {
            let browsablePatient = this.state.patient;

            component = <div>
                <div className="page-header">
                    <h1>{browsablePatient.resource.fullname} <small>{browsablePatient.resource.birthDate ? "né(e) le " + browsablePatient.resource.birthDate.toLocaleString() : ""}</small></h1>
                </div>


                <Button onClick={() => this.setState({ creatingAppointment: true })}>
                    Nouveau rendez-vous
                </Button>
                <Modal onHide={() => this.setState({ creatingAppointment: false })}

                    animation
                    show={this.state.creatingAppointment} draggable>
                    <ModalHeader closeButton>
                        <ModalTitle>Nouveau rendez-vous</ModalTitle>
                    </ModalHeader>
                    <ModalBody>
                        <Form>

                            <FormGroup>
                                <label htmlFor={"dateDebut"}>Date</label>
                                <FormControl type="date" name="startDate" min={new Date().toString()} size={6} />

                                <select name={"startHour"} id={"startHour"} className="form-control" contentEditable>
                                    {
                                        LinQ.range(8, 10, 1).toArray()
                                            .map((hour) => <option key={hour} value={hour} selected={now.getHours() == hour}>{hour}</option>)
                                    }
                                </select>
                            </FormGroup>

                            <FormGroup>
                                <label htmlFor={"title"}>Objet</label>
                                <FormControl type="text" name="title" placeholder="Certificat médical, bilan, ..." />
                            </FormGroup>
                        </Form>
                        <ModalFooter>
                            <Button bsStyle="success" as="submit" onClick={() => this.setState({ creatingAppointment: false })}>
                                Enregistrer
                            </Button>
                            <Button bsStyle="danger" onClick={() => this.setState({ creatingAppointment: false })}>
                                Annuler
                            </Button>
                        </ModalFooter>
                    </ModalBody>
                </Modal>

            </div >
        } else {
            component = <NotFoundComponent />;
        }
        return component;
    }
}