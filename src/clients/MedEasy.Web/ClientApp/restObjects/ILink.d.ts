declare namespace MedEasy.RestObjects {
    export interface ILink {

        relation: string;
        href: string;
        method?: string;
        title?: string;
    }
}