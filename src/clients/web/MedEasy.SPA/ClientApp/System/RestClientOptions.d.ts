export interface RestClientOptions {
    /** url of the rest endpoint */
    readonly baseUrl: string,
    /**
     * Function to call when computing default headers that will be added to each request made
     */
    readonly defaultHeaders?: () => { [key: string]: string },
    /**
     * Method to call before performing any request
     */
    readonly beforeRequestCallback?: () => Promise<void>
}

