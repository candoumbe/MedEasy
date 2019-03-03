import ArgumentNullException from "./Exceptions/ArgumentNullException";

export class Option<T> {
    private readonly hasValue : boolean;

    /**
     * Builds a new Option wrapper
     * @param element
     */
    private constructor(private readonly element: T | undefined) {
        this.element = element;
        this.hasValue = !!element;
        
    }

    /**
     * Creates an option that holds a value
     * @param {TElement} element
     */
    public static some<TElement>(element: TElement): Option<TElement> {
        return new Option<TElement>(element);
    }
    /**
     * Creates an option that holds nothing 
     */
    public static none<TElement>(): Option<TElement> {
        return new Option<TElement>(null);
    }

    /**
     * Creates a new option that only holds a value if predicate is satisfied
     * 
     * @param {(element?: T) => boolean} predicate
     * @param {T} element
     */
    public static someWhen<T>(predicate: (element?: T) => boolean, element? : T): Option<T> {
        return predicate(element)
            ? Option.some(element)
            : Option.none();
    }

    public static someWhenNotNull<T>(element? : T) : Option<T> {
        return Option.someWhen((element) => Boolean(element), element);
    }

    
    public static noneWhen<T>(predicate: (element?: T) => boolean, element?) : Option<T> {
        return predicate(element)
            ? Option.none()
            : Option.some(element);
    }


    /**
     * Evaluates a specified action, based on whether a value is present or not.
     * @param {(element: T) => void} some callback function that will be called if the
     * @param {() => void} none function to call when this instance has no value
     * @throws {ArgumentNullException} either some or none callback are null.
     */
    public match<TResult>(some: (element: T) => TResult, none: () => TResult): TResult {
        let result : TResult
        if (!some) {
            throw new ArgumentNullException("some");
        }
        if (!none) {
            throw new ArgumentNullException("none");
        }
        result = this.hasValue
            ? some(this.element)
            : none();

        return result;
    }

    /**
     * Evaluates a specified action if a value is present.
     * @param {(element: T) => void} some callback function that will be called if the
     */
    public matchSome(some: (element: T) => void): void {
        if (!some) {
            throw new ArgumentNullException("some");
        }
        if (this.hasValue) {
            some(this.element);
        }
    }

    /**
     * Evaluates a specified action if a value is present.
     * @param {() => void} none callback function that will be called if no value is present
     */
    public matchNone(none: () => void): void {
        if (!none) {
            throw new ArgumentNullException("none");
        }
        if (!this.hasValue) {
            none();
        }
    }
}