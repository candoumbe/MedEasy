declare namespace MedEasy.DTO {
    export interface Resource<TKey> {
        /** Id*/
        id: TKey,
        /** Meta information on the resource*/
        meta: MedEasy.RestObjects.ILink
    }
}