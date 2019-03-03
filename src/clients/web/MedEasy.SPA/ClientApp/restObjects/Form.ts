import { FormField } from "./FormField";
import { IonResource } from "./IonResource";

/**
 * A group of fields that can be submitted together
 */
export class Form extends IonResource {
    /** Fields of the form */
    public items : Array<FormField>
    
    /**
     * Builds a new Form instance
     */
    public constructor() {
        super();
        this.items = [];
    }    
}