import { Option } from "./Option";

export class RestClient<TKey, TElement> {

    public readonly baseUrl: string;

    /**
     * Builds a new RestClient instance
     * @param baseUrl url of the rest endpoint
     */
    public constructor(baseUrl: string) {
        this.baseUrl = baseUrl;
    }

    /**
     * Gets an array of resources
     */
    public async getMany(): Promise<Option<Promise<Array<TElement>>>> {
        let response = await fetch(`${this.baseUrl}`, { method: 'GET' });

        let result: Option<Promise<Array<TElement>>> = response.ok
            ? Option.Some(await response.json() as Promise<Array<TElement>>)
            : Option.None<Promise<Array<TElement>>>();

        return result;
    }

    /**
     * Gets a resource by its id
     * @param id {TKey} id of the resource to get
     */
    public async getOne(id : TKey): Promise<Option<Promise<TElement>>> {
        let response = await fetch(`${this.baseUrl}/${id}`, { method: 'GET' });

        let result: Option<Promise<TElement>> = response.ok
            ? Option.Some(await response.json() as Promise<TElement>)
            : Option.None<Promise<TElement>>();

        return result;
    }


}