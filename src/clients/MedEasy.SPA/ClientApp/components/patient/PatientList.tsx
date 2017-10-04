import { EndpointPageOfData, EndpointPageOfDataProps } from "./../EndpointPageOfData"
import { BrowsableResource } from "./../../restObjects/BrowsableResource"
/**
 * Properties of the {PatientList} component.
 */

interface PatientListState {
    /** Patients  */
    patients: Array<BrowsableResource<MedEasy.DTO.Patient>>;
    /** 1-based index of the page currently displayed */
    page: number;
    /** Indicates if the page is currently loading */
    loading: boolean;
}


/**
 * Displays a list of patients.
 * 
 */
export class PatientList extends EndpointPageOfData<BrowsableResource<MedEasy.DTO.Patient>> {

    public constructor(props: EndpointPageOfDataProps<BrowsableResource<MedEasy.DTO.Patient>>) {
        super(props);
    }
}