﻿import * as React from "react";
import { PatientList } from "./PatientList";
import { Link, NavLink } from "react-router-dom";
import { BrowsableResource } from "./../../restObjects/BrowsableResource";
import { RouteComponentProps } from 'react-router';


interface PatientMainPageProps {
    endpoint : string
}

export class PatientMainPage extends React.Component<PatientMainPageProps, {}> {
    public constructor(props : PatientMainPageProps) {
        super(props);
    }

    /**
     * Renders the component
     */
    public render(): JSX.Element | null {

        //let dataSource = new  RemoteDataSource<BrowsableResource<MedEasy.DTO.Patient>>(
        //    {
        //        create: {
        //            url: this.props.endpoint,
        //            type: HttpVerb.GET,
        //            contentType : "application/json"
        //        }
        //    },
        //    {
        //        model: {
        //            id: (p: BrowsableResource<MedEasy.DTO.Patient>) => p.resource.id,
        //            fields: [
        //                { from: (p: BrowsableResource<MedEasy.DTO.Patient>) => p.resource.id },
        //                { from: (p: BrowsableResource<MedEasy.DTO.Patient>) => p.resource.firstname },
        //                { from: (p: BrowsableResource<MedEasy.DTO.Patient>) => p.resource.lastname },
        //            ]
        //        }
        //    }
            

        //);

        return (
            <PatientList

                endpoint={`${this.props.endpoint}/patients`} 
                canCreate={true}
                id={(item) => item.resource.id}
                columns={new Map<string,(item : BrowsableResource<MedEasy.DTO.Patient>) => any>([
                    ["Fullname", (item) => <Link to={`/patients/details/${item.resource.id}`} rel="details">{item.resource.fullname}</Link>],
                    ["Birth Place", (item) => item.resource.birthPlace],
                    ["Birth Date", (item) => item.resource.birthDate ? item.resource.birthDate : ""],
                    ["", (item) => (
                        <div>
                            <button className='btn btn-danger' >
                                <span className="glyphicon glyphicon-trash"></span>
                            </button>
                            <button className='btn btn-success'>
                                <span className="glyphicon glyphicon-pencil"></span>
                            </button>
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