import { PageRestResponseLink } from "./PagedRestResponseLink";
export class GenericGetPagedResponse<T> implements MedEasy.RestObjects.IGenericGetPagedResponse<T> {


        public constructor(public readonly items: Array<T>, public readonly count: number, public readonly links : PageRestResponseLink) {

        }
    }
