import { Browsable } from "./../../restObjects/Browsable"
import { EndpointList, EndpointListProps } from "./../../components/EndpointList";

interface BodyWeightsProps extends EndpointListProps<Browsable<MedEasy.DTO.BodyWeight>> {
    
}

interface BodyWeightsState {

}

/**
 * Display most recents BodyWeight measures.
 */
export class BodyWeights extends EndpointList<Browsable<MedEasy.DTO.BodyWeight>>{

    public constructor(props : BodyWeightsProps) {
        super(props);
    }
}