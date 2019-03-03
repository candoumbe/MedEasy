import { Form } from "./Form";
import { IonResource } from "./IonResource";
import { Link } from "./Link";



export class Endpoint extends IonResource{

    public name: string;
    /** Forms */
    public forms: Array<Form>;
}