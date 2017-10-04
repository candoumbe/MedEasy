import { FormField } from "./FormField";
import { IonResource } from "./IonResource";

export class Form extends IonResource {
    public items : Array<FormField>
    
    /**
     * Builds a new Form instance
     */
    public constructor() {
        super();
        this.items = [];
    }

    
}