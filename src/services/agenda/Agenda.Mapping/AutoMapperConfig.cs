using Agenda.Models.v1.Appointments;
using Agenda.DTO;
using Agenda.Objects;
using AutoMapper;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.RestObjects;
using Agenda.API.Resources.v1.Appointments;
using Agenda.Models.v1.Attendees;
using Agenda.DTO.Resources.Search;
using Agenda.Ids;

namespace Agenda.Mapping
{
    public static class AutoMapperConfig
    {
        /// <summary>
        /// Builds mappings between entities and dtos
        /// </summary>
        /// <returns></returns>
        public static MapperConfiguration Build() => new((cfg) =>
        {
            cfg.CreateCoreMapping();
            cfg.CreateMap<NewAppointmentInfo, Appointment>()
                .IgnoreAllPropertiesWithAnInaccessibleSetter()
                .ConstructUsing((dto) => new Appointment(AppointmentId.New(),
                                                         dto.Subject,
                                                         dto.Location,
                                                         dto.StartDate.ToInstant(),
                                                         dto.EndDate.ToInstant()))
                .ForMember(entity => entity.CreatedDate, opt => opt.Ignore())
                .ForMember(entity => entity.CreatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedDate, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.Id, opt => opt.Ignore())
                .ReverseMap();

            cfg.CreateMap<NewAppointmentModel, NewAppointmentInfo>()
               .ReverseMap();

            cfg.CreateMap<AppointmentAttendee, AttendeeInfo>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Attendee.Id))
                .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Attendee.Name))
                .ForMember(dto => dto.PhoneNumber, opt => opt.MapFrom(entity => entity.Attendee.PhoneNumber))
                .ForMember(dto => dto.Email, opt => opt.MapFrom(entity => entity.Attendee.Email))
                .ForMember(dto => dto.UpdatedDate, opt => opt.MapFrom(entity => entity.Attendee.UpdatedDate))
                .ForMember(dto => dto.CreatedDate, opt => opt.MapFrom(entity => entity.Attendee.CreatedDate))
                ;

            cfg.CreateMap<Appointment, AppointmentInfo>()
                .ForMember(dto => dto.CreatedDate, opt => opt.MapFrom(entity => entity.CreatedDate))
                .ForMember(dto => dto.UpdatedDate, opt => opt.MapFrom(entity => entity.UpdatedDate))
                .IncludeBase<IEntity<AppointmentId>, Resource<AppointmentId>>()
                .ForMember(dto => dto.Attendees, opt => opt.MapFrom(entity => entity.Attendees));

            cfg.CreateMap<AppointmentModel, AppointmentInfo>()
                .ForMember(dto => dto.StartDate, opt => opt.MapFrom(model => model.StartDate.ToInstant()))
                .ForMember(dto => dto.EndDate, opt => opt.MapFrom(model => model.EndDate.ToInstant()))
                .ReverseMap()
                .ForMember(model => model.StartDate, opt => opt.MapFrom(dto => dto.StartDate.InUtc()))
                .ForMember(model => model.EndDate, opt => opt.MapFrom(dto => dto.EndDate.InUtc()))
                ;

            cfg.CreateMap<Attendee, AttendeeInfo>()
                .ForMember(dto => dto.CreatedDate, opt => opt.MapFrom(entity => entity.CreatedDate))
                .ForMember(dto => dto.UpdatedDate, opt => opt.MapFrom(entity => entity.UpdatedDate))
                .IncludeBase<IEntity<AttendeeId>, Resource<AttendeeId>>()
                .ReverseMap()
                .ConstructUsing(dto => new Attendee(dto.Id, dto.Name, dto.Email, dto.PhoneNumber))
                ;

            cfg.CreateMap<AttendeeModel, AttendeeInfo>()
                .ReverseMap();

            cfg.CreateMap<SearchAppointmentInfo, SearchAppointmentModel>()
                .ReverseMap();

            cfg.CreateMap<SearchAttendeeInfo, SearchAttendeeModel>()
                .ReverseMap();
        });
    }
}
