import { Exception } from "./Exception";


    /**
     * Root class for all exceptions
     */
export class ArgumentNullException extends Exception
{
    private _paramName: string;

    public get paramName()
    {
        return this._paramName;
    }

    /**
        * Creates an ArgumentNullException instance
        * @param {string} paramName name of the parameter that is null
        * @param {string} message additional information message
        */
    constructor(paramName: string, message: string = null)
    {
        super("ArgumentNullException", message);
        this._paramName = paramName;
    }
}
