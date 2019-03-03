declare namespace MedEasy.DTO {

    export enum ErrorLevel {
        Warning,
        Error
    }


    export interface ErrorInfo {
        [key: string]: Array<string>
    }
}

