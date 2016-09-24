import { Exception } from "./Exception";
    /**
     * Root class for all exceptions
     */
export class ArgumentException extends Exception
{
    public paramName: string;


    constructor(paramName: string = null, message : string)
    {
        super("ArgumentException", message);
        this.paramName = paramName;
    }
}