import { Exception } from "./Exception";

export default class ArgumentNullException extends Exception {

    /**
     * Builds a new instance
     * @param {string} paramName name of the parameter that causes the exception to be thrown
     */
    public constructor(public readonly paramName: string) {
        super(`'${paramName}' cannot be null`);
    }

}