import * as React from "react";
import { PatientList } from "./PatientList";
import { Link, NavLink } from "react-router-dom";
import { BrowsableResource } from "./../../restObjects/BrowsableResource";



interface PatientMainPageProps {
    endpoint : string
}

export class PatientMainPage extends React.Component<PatientMainPageProps, void> {
    constructor(props : PatientMainPageProps) {
        super(props);
    }

    /**
     * Renders the component
     */
    public render() {
        return (
            <PatientList

                endpoint={`${this.props.endpoint}/patients`} 
                canCreate={true}
                id={(item) => item.resource.id}
                columns={new Map<string,(item : BrowsableResource<MedEasy.DTO.Patient>) => any>([
                    ["Fullname", (item) => <Link to={"/patients/".concat(item.resource.id)} rel="details">{item.resource.fullname}</Link>],
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
                            <Link to={"/patients/" + item.resource.id} className='btn btn-default'>
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