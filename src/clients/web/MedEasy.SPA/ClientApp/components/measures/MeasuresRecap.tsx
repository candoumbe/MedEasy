
import { Browsable } from "./../../restObjects/Browsable";
import * as React from "react";
import { BloodPressures } from "./../../components/measures/BloodPressures";
import { BodyWeights } from "./../../components/measures/BodyWeigths";
import { RestClient } from "./../../System/RestClient";

interface MeasuresRecapProps {
    /** endpoint where to get data from */
    restClient: RestClient,
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
        
        return (<div>
            <BloodPressures
                restClient={new RestClient({ baseUrl: `${this.props.restClient.options.baseUrl}/bloodpressures`, defaultHeaders: this.props.restClient.options.defaultHeaders, beforeRequestCallback: this.props.restClient.options.beforeRequestCallback })}
                capabilities={
                    {
                        create: true,
                        update: true,
                        delete: true
                    }  
                }
                count={10}
                resourceName={{ singular: 'blood pressure', plural: 'blood pressures' }}
                columns={
                    new Map<string, (item: Browsable<MedEasy.DTO.BloodPressure>) => any>([
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
                capabilities={
                    {
                        create: true,
                        update: true,
                        delete: true
                    }
                }
                restClient={new RestClient({ baseUrl: `${this.props.restClient.options.baseUrl}/body-weights`, defaultHeaders: this.props.restClient.options.defaultHeaders, beforeRequestCallback: this.props.restClient.options.beforeRequestCallback })}
                count={10}
                resourceName={{ singular: 'body weight', plural: 'body weights' }}
                columns={
                    new Map<string, (item: Browsable<MedEasy.DTO.BodyWeight>) => any>([
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