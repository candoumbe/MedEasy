export class Option<T> {
    private element : T | undefined;

    /**
     * Builds a new Option wrapper
     * @param element
     */
    private constructor(element: T | undefined) {
        this.element = element;
    }


    public static some<TElement>(element: TElement): Option<TElement> {
        return new Option<TElement>(element);
    }

    public static none<TElement>(): Option<TElement> {
        return new Option<TElement>(null);
    }

    public static someWhen<T>(predicate: (element?: T) => boolean, element?) {
        return predicate(element)
            ? Option.some(element)
            : Option.none();
    }

    public static someWhenNotNull<T>(element? : T) {
        return Option.someWhen(() => !!element, element);
    }

    public static noneWhen<T>(predicate: (element?: T) => boolean, element?) {
        return predicate(element)
            ? Option.none()
            : Option.some(element);
    }

    /**
     * 
     * @param some callback function that will be called if the
     * @param none
     */
    public match(some: (element: T) => void, none: () => void): void {
        if (this.element) {
            some(this.element);
        }
        else
        {
            none();
        }
    }
}