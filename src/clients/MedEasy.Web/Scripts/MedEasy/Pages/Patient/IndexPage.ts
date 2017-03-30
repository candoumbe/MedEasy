
import { AbstractPage } from "../AbstractPage";
import { IndexPageElementMap } from "./IndexPageElementMap";


/**
 * Represents the index page for patient
 */
export class IndexPage extends AbstractPage<IndexPageElementMap> {

    /**
     * Builds a new IndexPage instance.
     */
    public constructor() {
        super(new IndexPageElementMap());
    }

}
