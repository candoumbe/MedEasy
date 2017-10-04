import { BrowsableResource } from "./../../restObjects/BrowsableResource"
import { EndpointList, EndpointListProps } from "./../../components/EndpointList";

interface BodyWeightsProps extends EndpointListProps<BrowsableResource<MedEasy.DTO.BodyWeight>> {
    
}

interface BodyWeightsState {

}

/**
 * Display most recents BodyWeight measures.
 */
export class BodyWeights extends EndpointList<BrowsableResource<MedEasy.DTO.BodyWeight>>{

    public constructor(props : BodyWeightsProps) {
        super(props);
    }
}