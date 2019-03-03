declare namespace MedEasy.DTO {
    export interface Patient extends Resource<string> {

        /** Firstname */
        firstname: string,

        /** Lastname */
        lastname: string,

        /** Fullname */
        fullname : string,

        birthDate? : Date,

        birthPlace: string,

        mainDoctor : Doctor
    }
}