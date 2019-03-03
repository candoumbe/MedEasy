// A '.tsx' file enables JSX support in the TypeScript compiler, 
// for more information see the following page on the TypeScript wiki:
// https://github.com/Microsoft/TypeScript/wiki/JSX

import { PageRestResponseLink } from "./PagedRestResponseLink"

/**
 * Represents a subset of items
 */
export class PageOfResult<T> {

    
    public static readonly empty = new PageOfResult([], new PageRestResponseLink("#", "#"), 0);
   

    /**
     * Builds a new {PageOfResult} instance
     * @param {Array<T>} items items of the current page
     * @param {PageRestResponseLink} links navigation links
     * @param {number} count total number of elements
     */
    public constructor(
        public readonly items: Array<T>,
        public readonly links: PageRestResponseLink,
        public readonly count: number) {
        
    }
}