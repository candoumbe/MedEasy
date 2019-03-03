declare namespace MedEasy.RestObjects {

    export interface IPagedRestResponseLink {
        first?: ILink,
        previous?: ILink
        next?: ILink;
        last?: ILink;

    }
}