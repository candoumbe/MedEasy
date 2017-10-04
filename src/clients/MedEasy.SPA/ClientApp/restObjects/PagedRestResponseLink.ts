import { Link } from "./Link";

export class PageRestResponseLink implements MedEasy.RestObjects.IPagedRestResponseLink {

    public readonly first: Link;
    public readonly last: Link;
    public readonly previous: Link | null;
    public readonly next: Link | null;


    /**
     * Builds a new {PageRestResponseLink}
     * 
     * @param first link to the first page of result
     * @param previous link to the previous page of result
     * @param next link to the next page of result
     * @param last link to the last page of result
     */
    public constructor(first: string | Link, last: string | Link, previous?: string | Link, next?: string | Link) {
        this.first = typeof first === "string"
            ? Link.create(first, "first")
            : first;

        this.last = typeof last === "string"
            ? Link.create(last, "last")
            : last;

        if (previous) {
            this.previous = typeof previous === "string"
                ? Link.create(previous, "previous")
                : previous;
        }

        if (next) {
            this.next = typeof next === "string"
                ? Link.create(next, "next")
                : next;
        }

    }
}