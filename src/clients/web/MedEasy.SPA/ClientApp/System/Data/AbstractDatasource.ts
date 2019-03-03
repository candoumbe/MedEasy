/**
 * Base class for datasources
 */
export abstract class AbstractDataSource {

    private _page: number;
    private _pageSize: number;

    public get pageSize(): number {
        return this._pageSize;
    }


    public set pageSize(value: number) {
        if (value < 0) {
            throw new Error("value cannot be negative")
        }
        if (value >= 0) {
            this._pageSize = value;
        }
    }


    public get page(): number {
        return this._page;
    }


    public set page(value: number) {
        if (value < 0) {
            throw new Error("value cannot be negative")
        }
        if (value >= 0) {
            this._pageSize = value;
        }
    }


}