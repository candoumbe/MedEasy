export class LoginFailedException extends Error {

    public constructor(readonly errors: { [prop: string]: Array<string> }) {
        super("Login failed");
    }
}