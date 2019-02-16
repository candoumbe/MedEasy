export class Option<TElement, TException> {
    
    /**
     * Builds a new Option wrapper
     * @param element
     * @param exception
     * 
     */
    private constructor(readonly element: TElement | undefined, readonly exception: TException) {
        if (element == null && exception == null) {
            throw new Error("element & exception cannot be both null")
        }
    }




    public static some<TElement, TException>(element: TElement): Option<TElement, TException> {
        return new Option<TElement, TException>(element, undefined);
    }

    public static none<TElement, TException>(exception : TException): Option<TElement, TException> {
        return new Option<TElement, TException>(null, exception);
    }

    /**
     * 
     * @param some callback function that will be called if the
     * @param none
     */
    public match<TResult>(some: (element: TElement) => TResult, none: (exception : TException) => TResult): TResult {
        return this.element
            ? some(this.element)
            : none(this.exception);
    }
}