import { PageRestResponseLink } from "./PagedRestResponseLink";

/**
 * A page of result
 */
export class GenericGetPagedResponse<T> implements MedEasy.RestObjects.IGenericGetPagedResponse<T> {

        /**
         * Builds a new GenericGetPagedResponse<T> instance
         * @param {Array<T>} items elements of the page
         * @param {number} count total number of elements in the resuultset.
         * @param {PageRestResponseLink} links links to navigate through pages. 
         */
        public constructor(public readonly items: Array<T>, public readonly count: number, public readonly links : PageRestResponseLink) {

        }
    }
