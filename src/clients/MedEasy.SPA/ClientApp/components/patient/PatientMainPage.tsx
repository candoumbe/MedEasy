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
    restClient: RestClient<string, Browsable<MedEasy.DTO.Patient>>;
}

export class PatientMainPage extends React.Component<PatientMainPageProps, {}> {
    public constructor(props : PatientMainPageProps) {
        super(props);
    }

    /**
     * Renders the component
     */
    public render(): JSX.Element | null {

        //let dataSource = new RemoteDataSource<Browsable<MedEasy.DTO.Patient>>(
        //    {
        //        create: {
        //            url: this.props.endpoint,
        //            type: HttpVerb.GET,
        //            contentType: "application/json"
        //        }
        //    },
        //    {
        //        model: {
        //            id: (p: Browsable<MedEasy.DTO.Patient>) => p.resource.id,
        //            fields: [
        //                { from: (p: Browsable<MedEasy.DTO.Patient>) => p.resource.id },
        //                { from: (p: Browsable<MedEasy.DTO.Patient>) => p.resource.firstname },
        //                { from: (p: Browsable<MedEasy.DTO.Patient>) => p.resource.lastname },
        //            ]
        //        }
        //    }
            

        //);

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
                            <Button bsStyle="danger" >
                                <span className="glyphicon glyphicon-trash"></span>
                            </Button>
                            <Button bsStyle="success">
                                <span className="glyphicon glyphicon-pencil"></span>
                            </Button>
                            <Link to={`/patients/details/${item.resource.id}`} className='btn btn-default'>
                                <span className="glyphicon glyphicon-eye-open"></span>
                            </Link>
                        </div>
                    )]
                ])}
                resourceName={{ plural: "patients", singular: "patient" }}
                pageSize={30} />
            
        );
    }
}