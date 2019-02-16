import { Exception } from "./Exception";

export class SignUpFailedException extends Exception {

    public constructor(readonly errors: { [prop: string]: Array<string> }) {
        super("Sign up failed");
    }
}