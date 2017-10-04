declare namespace MedEasy.DTO {

    export enum ErrorLevel {
        Warning,
        Error
    }


    export interface ErrorInfo {
        /** 
         * Key
        **/
        key : string,
        /** 
         * Description of the error
        **/
        description : string

        /** 
         * Severity
        **/
        severity : ErrorLevel,

     
}
}

