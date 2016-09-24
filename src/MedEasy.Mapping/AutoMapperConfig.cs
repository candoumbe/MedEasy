using AutoMapper;
using MedEasy.DTO;
using MedEasy.DTO.Autocomplete;
using MedEasy.Objects;

namespace MedEasy.Mapping
{
    public class AutoMapperConfig
    {
        public static MapperConfiguration Build() => new MapperConfiguration(cfg =>
        {


            cfg.CreateMap<Patient, PatientInfo>();
            cfg.CreateMap<Patient, BrowsablePatientInfo>()
                 .ForMember(dest => dest.Location, opt => opt.Ignore());
            cfg.CreateMap<CreatePatientInfo, Patient>();


            cfg.CreateMap<Doctor, DoctorInfo>();
            cfg.CreateMap<Doctor, BrowsableDoctorInfo>()
                .ForMember(dest => dest.Location, opt => opt.Ignore());

            cfg.CreateMap<Specialty, SpecialtyInfo>();
            cfg.CreateMap<Specialty, BrowsableSpecialtyInfo>()
                 .ForMember(dest => dest.Location, opt => opt.Ignore());
            cfg.CreateMap<CreateSpecialtyInfo, Specialty>();

            #region Autocomplete

            cfg.CreateMap<Doctor, DoctorAutocompleteInfo>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(source => source.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(source => source.Firstname + " " + source.Lastname))
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(source => source.Specialty.Name));

           
            #endregion

        });
    }
}
