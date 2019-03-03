import { Browsable } from "./../../restObjects/Browsable"
import { EndpointList, EndpointListProps } from "./../../components/EndpointList";

interface BloodPressuresProps extends EndpointListProps<Browsable<MedEasy.DTO.BloodPressure>> {
}

interface BloodPressuresState {
    
}


export class BloodPressures extends EndpointList<Browsable<MedEasy.DTO.BloodPressure>>{

    public constructor(props : BloodPressuresProps) {
        super(props);
    }
}