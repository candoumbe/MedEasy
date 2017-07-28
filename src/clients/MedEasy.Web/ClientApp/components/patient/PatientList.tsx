import { EndpointPageOfData, EndpointPageOfDataProps } from "./../EndpointPageOfData"
import { BrowsableResource } from "./../../restObjects/BrowsableResource"
/**
 * Properties of the {PatientList} component.
 */

interface PatientListState {
    patients: Array<BrowsableResource<MedEasy.DTO.Patient>>;
    page: number;
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