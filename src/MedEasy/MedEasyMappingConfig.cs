using AutoMapper;
using MedEasy.DTO;
using MedEasy.ViewModels.Patient;

namespace MedEasy
{
    public class MedEasyMappingConfig
    {
        public static MapperConfiguration Build() => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<PatientCreateModel, CreatePatientInfo>();

            cfg.CreateMap<PatientInfo, PatientDetailModel>();
        });
    }
}
