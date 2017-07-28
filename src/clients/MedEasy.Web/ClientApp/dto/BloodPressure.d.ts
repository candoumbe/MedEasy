declare namespace MedEasy.DTO {
    export interface BloodPressure extends PhysiologicalMeasure {
        /** Diastolic blood pressure */
        diastolic: number,
        /** Systolic blood pressure */
        systolic: number
        
    }
}