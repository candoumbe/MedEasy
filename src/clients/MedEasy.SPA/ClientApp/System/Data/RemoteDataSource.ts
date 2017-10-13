import { AbstractDataSource } from "./AbstractDataSource"
import { Schema } from "./Schema";
import { Transport } from "./Transport";

/**
 * DataSource that gets data from a remote endpoint
 */
export class RemoteDataSource<T> extends AbstractDataSource {

    /**
     * Builds a new instance
     * @param {Transport} transport transport of data configuration
     * @param {Schema} schema describe how data are structured
     */
    public constructor(public readonly transport: Transport, public readonly schema: Schema<T>) {
        super();
    }

}
