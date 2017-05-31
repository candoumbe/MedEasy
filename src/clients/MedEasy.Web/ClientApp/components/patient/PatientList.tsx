import { EndpointList, EndpointListProps } from "./../EndpointList"

/**
 * Properties of the {PatientList} component.
 */

interface PatientListState {
    patients: Array<MedEasy.DTO.Patient>;
    page: number;
    loading: boolean;
}


/**
 * Displays a list of patients.
 * 
 */
export class PatientList extends EndpointList<MedEasy.DTO.Patient> {

    public constructor(props: EndpointListProps<MedEasy.DTO.Patient>) {
        super(props);
    }
}