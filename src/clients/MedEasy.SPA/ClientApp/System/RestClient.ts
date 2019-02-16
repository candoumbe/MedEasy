import { Option } from "./Option_Maybe";
import { PageOfResult } from "./../restObjects/PageOfResult";

export class RestClient<TKey, TElement> {

    /**
     * Builds a new RestClient instance
     * @param baseUrl url of the rest endpoint
     */
    public constructor(private readonly baseUrl: string, private readonly defaultHeaders?: () => {[key:string] : string}) {
        this.baseUrl = baseUrl;
    }

    /**
     * Gets an array of resources
     */
    public async getMany(request: { page: number, pageSize: number } = { page: 1, pageSize: 30 }): Promise<Option<Promise<PageOfResult<TElement>>>> {
        let response = await fetch(`${this.baseUrl}/?page=${request.page}&pageSize=${request.pageSize}`, {
            method: 'GET',
            headers: this.defaultHeaders ? this.defaultHeaders() : null
        });

        let result: Option<Promise<PageOfResult<TElement>>> = response.ok
            ? Option.some(await response.json() as Promise<PageOfResult<TElement>>)
            : Option.none<Promise<PageOfResult<TElement>>>();

        return result;
    }

    /**
     * Gets a resource by its id
     * @param id {TKey} id of the resource to get
     */
    public async getOne(id : TKey): Promise<Option<Promise<TElement>>> {
        let response = await fetch(`${this.baseUrl}/${id}`, { method: 'GET' });

        let result: Option<Promise<TElement>> = response.ok
            ? Option.some(await response.json() as Promise<TElement>)
            : Option.none<Promise<TElement>>();

        return result;
    }


}