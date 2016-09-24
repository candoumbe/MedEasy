
/**
 * An instance of this class represents a Guid identifier.
 */
export class Guid
{
    private _guid: string;

    constructor(guid: string)
    {
        this._guid = guid;
    }

    /**
        * Converts the Guid to string
        */
    public toString(): string
    {
        return this._guid;
    }

    /**
    * Create a new Guid instance
    */
    static new(): Guid
    {
        var result: string;
        var i: string;
        var j: number;

        result = "";
        for (j = 0; j < 32; j++)
        {
            if (j == 8 || j == 12 || j == 16 || j == 20)
                result = result + '-';
            i = Math.floor(Math.random() * 16).toString(16).toUpperCase();
            result = result + i;
        }
        return new Guid(result);
    }
}