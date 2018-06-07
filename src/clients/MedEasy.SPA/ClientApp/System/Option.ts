export class Option<T> {
    private element : T | undefined;

    /**
     * Builds a new Option wrapper
     * @param element
     */
    public constructor(element: T | undefined) {
        this.element = element;
    }


    public static Some<TElement>(element: TElement): Option<TElement> {
        return new Option<TElement>(element);
    }

    public static None<TElement>(): Option<TElement> {
        return new Option<TElement>(null);
    }

    /**
     * 
     * @param some callback function that will be called if the
     * @param none
     */
    public Match(some: (element: T) => void, none: () => void): void {
        if (this.element) {
            some(this.element);
        }
        else
        {
            none();
        }
    }
}