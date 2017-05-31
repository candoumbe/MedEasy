declare namespace MedEasy.RestObjects {
    export interface IGenericGetPagedResponse<T> extends IGetResponse<T> {

        /**
         * Number of items in the result set that the current <see cref="Items"/> is just a portion.
         */
        count: number;

        /** <summary>
         * Items of the result
         */ 
        items: Array<T>;

        /// <summary>
        /// Link to navigate through the result set
        /// </summary>
        links: IPagedRestResponseLink;

    }
}