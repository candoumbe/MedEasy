
enum FieldType {
    Date,
    String,
    Boolean
}

export class Field<T> {

    public defaultValue?: any | undefined;

    public type?: FieldType;

    public nullable?: boolean | undefined;

    /** Can the field be modified */
    public editable?: boolean | undefined;

    /** Name of the field */
    public name?: string;

    /** Where to get the field value from*/
    public from?: string | ((element: T) => any) | undefined


}
