/// <reference types="es6-promise" />
/// <reference types="jquery" />
export declare class Ajax {
    /**
     * Loads data from the server using GET verb.
     * @param {string} url endpoint where to get data from.
     * @param {any} data data to send along the request.
     *
     * @returns {Promise} that contains
     */
    static get<T>(url: string, data?: any): Promise<T>;
    /**
     * Load data from the server using POST verb
     * @param {string} url endpoint's url
     * @param {any} data data to send to the endpoint.
     * @type T type of data to load from the server
     * @returns {Promise} that resolves with the created resource
     */
    static post<T>(url: string, data?: any): Promise<T>;
    /**
     * Load data from the server using PUT verb
     * @param {string} url endpoint's url
     * @param {any} data data to send to the endpoint.
     * @type T type of data to load from the server
     */
    static put<T>(url: string, data?: any): Promise<T>;
    /**
     * Delete the resource with the specified url
     * @param {string} url url of the resource to delete
     * @returns Promise
     */
    static delete(url: string): Promise<{}>;
    /**
     * Loads data from the server using the specified options
     * @param {JQueryAjaxSettings} options options used to perform the ajax request
     * @returns {Promise<T>}
     */
    static ajax<T>(options: JQueryAjaxSettings): Promise<T>;
}
