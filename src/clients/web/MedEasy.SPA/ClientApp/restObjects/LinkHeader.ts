import { Link } from "./Link";

export class LinkHeader {


    public static read(linkHeader: string): Array<Link> {
        let links: Array<Link> = linkHeader
            .split(",")
            .map((value) => LinkHeader.parse(value))
            .filter(link => link.href.length > 0);
        
        return links;
    }


    private static parse(aLink: string): Link {

        let linkParts = aLink.split(";");
        let link: Link = new Link;

        linkParts.forEach((value) => {
            value = value.trim();
            if (value.startsWith("<") && value.endsWith(">")) {
                link.href = `${value.substring(1, value.length - 1)}`;
            } else if (value.startsWith("rel=")) {
                link.relation = `${value.substring(3)}`;
            } else if (value.startsWith("title=")) {
                link.title = `${value.substring(7)}`;
            }
        });

        return link;
    }
}