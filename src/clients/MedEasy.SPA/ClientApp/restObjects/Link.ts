
export class Link implements MedEasy.RestObjects.ILink {

    public relation: string;
    public href: string;
    public method?: string;
    public title?: string;

    /**
     * Creates a new instance
     * @param {string} href URL
     * @param {string} rel relation of the link
     */
    public static create(href: string, rel: string): Link {
        let link = new Link();

        link.href = href;
        link.relation = rel;

        return link;
    }
}