
export class Link implements MedEasy.RestObjects.ILink {

    public relation: string;
    public href: string;
    public method?: string;
    public title?: string;


    public static create(href: string, rel: string): Link {
        let link = new Link();

        link.href = href;
        link.relation = rel;

        return link;
    }
}