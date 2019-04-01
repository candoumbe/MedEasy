﻿import { Option } from "./Option_Maybe";
import { RestClientOptions } from "./RestClientOptions";


export class RestClient {

    /**
     * Builds a new RestClient instance
     * @param {RestClientOptions} options 
     */
    public constructor(public readonly options: RestClientOptions) {}


    private buildRequestUrl<TInput>(request: undefined | TInput | { page: number, pageSize: number }) {
        let obj = request;
        let url: string | undefined;
        if (request) {
            if (request.hasOwnProperty("page") || obj.hasOwnProperty("pageSize")) {
                url = `${this.options.baseUrl}/?page=${request['page']}&pageSize=${request['pageSize']}`
            } else {
                url = `${this.options.baseUrl}/${request}`;
            }
        } else {
            url = `${this.options.baseUrl}/`;
        }

        return url;
    }

    private buildRequestDefaultHeaders(): { [key: string]: string } | undefined {
        let defaultHeaders: { [key: string]: string } | undefined;
        if (this.options.defaultHeaders) {
            defaultHeaders = this.options.defaultHeaders();
        }
        return defaultHeaders;
    }

    /**
     * Gets an array of resources
     */
    public async get<TInput, TResult>(request: undefined | TInput | { page: number, pageSize?: number } = { page: 1, pageSize: 30 }): Promise<Option<Promise<TResult>>> {
        if (this.options.beforeRequestCallback) {
            await this.options.beforeRequestCallback();
        }

        let response = await fetch(this.buildRequestUrl(request), {
            method: 'GET',
            headers: this.options.defaultHeaders ? this.options.defaultHeaders() : null
        });

        let result: Option<Promise<TResult>> = response.ok
            ? Option.some(await response.json() as Promise<TResult>)
            : Option.none<Promise<TResult>>();

        return result;
    }

    /**
     * Gets a resource by its id
     * @param id {TKey} id of the resource to get
     */
    public async head<TInput>(request: TInput | { page: number, pageSize: number } = { page: 1, pageSize: 30 }): Promise<{ [key: string]: string | number | Date | boolean }> {

        if (this.options.beforeRequestCallback) {
            await this.options.beforeRequestCallback();
        }

        let response = await fetch(this.buildRequestUrl(request), Object.assign({ method: 'HEAD', headers: this.buildRequestDefaultHeaders() }));

        let responseHeaders: { [key: string]: string | number | Date | boolean } = {};
        response.headers.forEach((value, key) => responseHeaders[key] = value);

        return responseHeaders;
    }

    /**
     * Creates a resource
     * @param data
     */
    public async post<TInput, TResult>(data: TInput): Promise<TResult> {
        if (this.options.beforeRequestCallback) {
            await this.options.beforeRequestCallback();
        }

        let response = await fetch(this.buildRequestUrl(null), { method: 'POST', body: JSON.stringify(data) , headers: this.buildRequestDefaultHeaders() });
        let createdResource: TResult;
        if (response.ok) {
            createdResource = await response.json() as TResult;
        }

        return createdResource;


    }

    /**
     * Creates a resource
     * @param data
     */
    public async put<TInput, TResult>(data: TInput): Promise<TResult> {
        if (this.options.beforeRequestCallback) {
            await this.options.beforeRequestCallback();
        }

        let response = await fetch(this.buildRequestUrl(null), { method: 'PUT', body: JSON.stringify(data), headers: this.buildRequestDefaultHeaders() });
        let updated: TResult;
        if (response.ok) {
            updated = await response.json() as TResult;
        }

        return updated;


    }



}