
import { BrowsableResource } from "./../../restObjects/BrowsableResource";
import * as React from "React";
import { BloodPressures } from "./../../components/measures/BloodPressures";
import { BodyWeights } from "./../../components/measures/BodyWeigths";

interface MeasuresRecapProps {
    /** endpoint where to get data from */
    endpoint: string,
    resourceName : string
}

interface MeasuresRecapState {


}

/**
 * Component that displays measures recap.
 */
export class MeasuresRecap extends React.Component<MeasuresRecapProps, MeasuresRecapState>{

    public constructor(props : MeasuresRecapProps) {
        super(props);
    }

    public render(): JSX.Element {
        console.log(this.props.endpoint);
        return (<div>
            <BloodPressures
                urls={
                    {
                        read: `${this.props.endpoint}/most-recent-bloodpressures`,
                        create: `${this.props.endpoint}/bloodPressures`,
                        delete: `${this.props.endpoint}/bloodPressures`
                    }
                }
                count={10}
                resourceName={{ singular: 'blood pressure', plural: 'blood pressures' }}
                columns={
                    new Map<string, (item: BrowsableResource<MedEasy.DTO.BloodPressure>) => any>([
                        ["Value", item => `${item.resource.systolic}/${item.resource.diastolic} mmHg`],
                        ["Date of measure", item => item.resource.dateOfMeasure.toString()],
                        ["", item => {
                            let buttons: Array<Element> = [];
                            return (<div>{buttons}</div>);
                        }]
                    ])
                }
            />

            <BodyWeights
                urls={
                    {
                        read: `${this.props.endpoint}/mostRecentBloodPressures`,
                        create: `${this.props.endpoint}/bloodPressures`,
                        delete: `${this.props.endpoint}/bloodPressures`
                    }
                }
                count={10}
                resourceName={{ singular: 'body weight', plural: 'body weights' }}
                columns={
                    new Map<string, (item: BrowsableResource<MedEasy.DTO.BodyWeight>) => any>([
                        ["Value", item => `${item.resource.value} kg`],
                        ["Date of measure", item => item.resource.dateOfMeasure],
                        ["", item => {
                            let buttons: Array<Element> = [];
                            return (<div>{buttons}</div>);
                        }]
                    ])
                }
            />

        </div>);
    }
}