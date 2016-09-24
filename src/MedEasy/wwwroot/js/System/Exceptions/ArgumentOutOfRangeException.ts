import { Exception } from "./Exception";
    /**
     * Root class for all exceptions
     */
export class ArgumentOutOfRangeException extends Exception
{
    private _paramName: string;

    public get paramName()
    {
        return this._paramName;
    }


    constructor(paramName: string = null, message : string)
    {
        super("ArgumentOutOfRangeException", message);
        this._paramName = paramName;
    }
}

