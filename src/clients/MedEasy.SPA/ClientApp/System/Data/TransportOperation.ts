
export enum HttpVerb {
    GET,
    POST,
    PUT,
    PATCH,
    DELETE,
    OPTIONS,
    HEAD
}
export class TransportOperation {
    /** endpoint's url where a datasource can get/send its data */
    public url: string;
    /** HTTP method (GET, POST, ...) of the request issued */
    public type?: HttpVerb;
    /** Content-Type of the request */
    public contentType: string | undefined;
    /** Additional data sent alongside the request */
    public data?: any | undefined;
    /** Indicates if a cache can be used */
    public cache?: boolean | undefined;

    /**
     * Builds a new instance.
     */
    public constructor() {
        this.type = HttpVerb.GET;
    }
}