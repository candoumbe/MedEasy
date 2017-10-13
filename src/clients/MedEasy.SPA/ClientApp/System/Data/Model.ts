import { Field } from "./Field";

export class Model<T> {
    /** Name of the property that can uniquely identifies a item in a model */
    public id: string | number | ((element : T) => string | number);

    /** Fields of the model */
    public fields: Array<Field<T>>;

    
}
