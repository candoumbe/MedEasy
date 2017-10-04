import { BrowsableResource } from "./../../restObjects/BrowsableResource"
import { EndpointList, EndpointListProps } from "./../../components/EndpointList";

interface BloodPressuresProps extends EndpointListProps<BrowsableResource<MedEasy.DTO.BloodPressure>> {
}

interface BloodPressuresState {
    
}


export class BloodPressures extends EndpointList<BrowsableResource<MedEasy.DTO.BloodPressure>>{

    public constructor(props : BloodPressuresProps) {
        super(props);
    }
}