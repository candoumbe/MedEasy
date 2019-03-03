export class Option<TElement, TException> {
    
    /**
     * Builds a new Option wrapper
     * @param {TElement} element
     * @param {TException} exception
     * 
     */
    private constructor(readonly element?: TElement | null | undefined, readonly exception?: TException | null | undefined) {
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

    /**
     * Performs the specified callback if the current instance holds a value
     * @param {(element: TElement) => TResult} callback function that will be called if the
     */
    public matchSome<TResult>(callback: (element: TElement) => TResult): void {
        if (this.element) {
            callback(this.element);
        }
    }

    /**
     * Performs the specified callback if the current instance doesn't hold a value
     * @param {(exception: TException) => void} callback
     */
    public matchNone<TResult>(callback: (exception: TException) => void): void {
        if (!!!this.element) {
            callback(this.exception);
        }
    }
}