declare namespace MedEasy.DTO {
    export interface Patient extends Resource<string> {

        firstname: string,

        lastname: string,

        birthDate? : Date,

        birthPlace: string,

        mainDoctor : Doctor
    }
}