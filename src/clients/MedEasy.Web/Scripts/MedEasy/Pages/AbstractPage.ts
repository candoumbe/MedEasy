import { BasePageElementMap } from "./BasePageElementMap";

export abstract class AbstractPage<T extends BasePageElementMap> {

    private _map: T;

    protected constructor(map: T) {
        this._map = map;
    }

    
    public get map(): T { return this._map; }

} 