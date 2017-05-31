declare namespace MedEasy.RestObjects {

    export interface IPagedRestResponseLink {
        First?: ILink,
        Previous?: ILink
        Next?: ILink;
        Last?: ILink;

    }
}