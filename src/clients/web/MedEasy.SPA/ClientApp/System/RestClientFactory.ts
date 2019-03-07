import { RestClientOptions } from "./RestClientOptions";
import { RestClient } from "./RestClient";

/**
 * Factory to build instances of REST clients
 * 
 * @see {RestClient}
 * */
export class RestClientFactory {

    

    /**
     * Builds a new instance
     * @param settings default settings for each instance of REST client to build
     */
    public constructor(public readonly settings: RestClientOptions) {
    }

    /**
     * 
     * 
     */
    public createClient(baseUrl: string): RestClient {
        let url = `${this.settings.baseUrl}${(this.settings.baseUrl.endsWith("/") ? "" : "/")}${baseUrl}`;

        return new RestClient({baseUrl :url, defaultHeaders: this.settings.defaultHeaders, beforeRequestCallback : this.settings.beforeRequestCallback});
    }

}