import * as $ from "jquery";

export class Ajax {

    /**
     * Loads data from the server using GET verb.
     * @param {string} url endpoint where to get data from.  
     * @param {any} data data to send along the request.
     *
     * @returns {Promise} that contains 
     */
    public static async get<T>(url: string, data?: any): Promise<T> {
        return await this.ajax<T>({ url: url, data: data });
    }

    /**
     * Load data from the server using POST verb
     * @param {string} url endpoint's url
     * @param {any} data data to send to the endpoint.
     * @type T type of data to load from the server
     * @returns {Promise} that resolves with the created resource
     */
    public static async post<T>(url: string, data?: any): Promise<T> {
        return await this.ajax<T>({
            url: url,
            method: "POST",
            data: data
        });
    }

    /**
     * Load data from the server using PUT verb
     * @param {string} url endpoint's url
     * @param {any} data data to send to the endpoint.
     * @type T type of data to load from the server
     */
    public static async put<T>(url: string, data?: any): Promise<T> {
        return await this.ajax<T>({
            url: url,
            method: "PUT",
            data: data
        });
    }

    /**
     * Delete the resource with the specified url
     * @param {string} url url of the resource to delete
     * @returns Promise
     */
    public static async delete(url: string): Promise<{}> {
        return await this.ajax<{}>({ url: url, method: "DELETE" });
    }

    /**
     * Loads data from the server using the specified options
     * @param {JQueryAjaxSettings} options options used to perform the ajax request
     * @returns {Promise<T>}
     */
    public static async ajax<T>(options: JQueryAjaxSettings): Promise<T> {
        let promise: Promise<T> = await $.ajax(options).promise();
        return promise;
    }

}
