import { Guid } from "./../../System/Guid"
export class NewAccountInfo {

    /**
     * Builds a new instance
     * @param name name of the account
     * @param username username associated with the account
     * @param email email associated with the account
     * @param password password of the account
     * @param confirmPassword confirmPassword
     */
    public constructor(
        readonly name: string, readonly username: string, readonly email: string, readonly password: string,
        readonly confirmPassword: string,
        readonly tenantId : Guid | string
    ) {

    }

}