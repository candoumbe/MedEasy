import * as React from "react";
import { PatientList } from "./PatientList";

export class PatientMainPage extends React.Component<void, void> {
    constructor() {
        super();
    }


    

    public render() {
        return (
            <PatientList
                endpoint={"http://localhost:5000"}
                canCreate={true}
                columns={new Map<string,(item : MedEasy.DTO.Patient) => any>([
                    ["Id", (item) => item.id],
                    ["Firstname", (item) => item.firstname],
                    ["Lastname", (item) => item.lastname],
                    ["Birth Place", (item) => item.birthPlace],
                    ["Birth Date", (item) => item.birthDate],
                    ["", (item) => (
                        <div>
                            <button className='btn btn-danger'>
                                <span className="glyphicon glyphicon-trash"></span>
                            </button>
                            <button className='btn btn-success'>
                                <span className="glyphicon glyphicon-pencil"></span>
                            </button>
                        </div>
                    )]
                ])}
                resourceName={{ plural: "patients", singular: "patient" }}
                pageSize={30} />
        );
    }
}