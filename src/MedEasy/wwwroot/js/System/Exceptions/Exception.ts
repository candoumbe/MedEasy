export class Exception implements Error
{
    public constructor(
        /** Name of the exception */
        public name: string,

        /**Message of the exception */
        public message: string
    )
    {
    }
}