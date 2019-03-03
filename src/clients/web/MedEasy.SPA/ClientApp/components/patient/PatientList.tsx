import { EndpointPageOfData, EndpointPageOfDataProps } from "./../EndpointPageOfData"
import { Browsable } from "./../../restObjects/Browsable"
/**
 * Properties of the {PatientList} component.
 */

interface PatientListState {
    /** Patients  */
    patients: Array<Browsable<MedEasy.DTO.Patient>>;
    /** 1-based index of the page currently displayed */
    page: number;
    /** Indicates if the page is currently loading */
    loading: boolean;
}


/**
 * Displays a list of patients.
 * 
 */
export class PatientList extends EndpointPageOfData<Browsable<MedEasy.DTO.Patient>> {

    public constructor(props: EndpointPageOfDataProps<Browsable<MedEasy.DTO.Patient>>) {
        super(props);
    }
}