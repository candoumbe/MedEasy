import { TransportOperation } from "./TransportOperation";

export class Transport {

    public create?: TransportOperation;
    public read?: TransportOperation;
    public update?: TransportOperation;
    public delete?: TransportOperation;
}