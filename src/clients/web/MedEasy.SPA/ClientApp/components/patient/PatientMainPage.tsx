import * as React from "react";
import { PatientList } from "./PatientList";
import { Link, NavLink, } from "react-router-dom";
import { Browsable } from "./../../restObjects/Browsable";
import { RemoteDataSource } from "./../../System/Data/RemoteDataSource";
import { HttpVerb } from "./../../System/Data/TransportOperation";
import { RouteComponentProps } from 'react-router';
import { Button } from "react-bootstrap";
import { RestClient } from "./../../System/RestClient";



interface PatientMainPageProps {
    restClient: RestClient;
}

export class PatientMainPage extends React.Component<PatientMainPageProps, {}> {
    public constructor(props : PatientMainPageProps) {
        super(props);
    }

    /**
     * Renders the component
     */
    public render(): JSX.Element | null {

        return (
            <PatientList
                restClient={this.props.restClient} 
                canCreate={true}
                id={(item) => item.resource.id}
                columns={new Map<string,(item : Browsable<MedEasy.DTO.Patient>) => any>([
                    ["Fullname", (item) => <NavLink to={`/patients/details/${item.resource.id}`}>{item.resource.fullname}</NavLink>],
                    ["Birth Place", (item) => item.resource.birthPlace],
                    ["Birth Date", (item) => item.resource.birthDate ? item.resource.birthDate : ""],
                    ["", (item) => (
                        <div>
                            <Button variant="danger" >
                                <span className="glyphicon glyphicon-trash"></span>
                            </Button>
                            <Button variant="success">
                                <Link to={`/patients/edit/${item.resource.id}`}>
                                    <span className="glyphicon glyphicon-pencil"></span>
                                </Link>
                            </Button>
                            <Button>
                                <Link to={`/patients/details/${item.resource.id}`}>
                                    <span className="glyphicon glyphicon-eye-open"></span>
                                </Link>
                            </Button>
                        </div>
                    )]
                ])}
                resourceName={{ plural: "patients", singular: "patient" }}
                pageSize={30} />
            
        );
    }
}