import { BasePageElementMap } from "./BasePageElementMap";
export declare abstract class AbstractPage<T extends BasePageElementMap> {
    private _map;
    protected constructor(map: T);
    readonly map: T;
}
