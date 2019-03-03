declare namespace MedEasy.DTO {
    export interface Doctor extends Resource<string> {
        /** Firstname */
        firstname: string,
        /** Lastname */
        lastname: string
    }
}