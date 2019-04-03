using Agenda.Models.v1.Appointments;
using Agenda.DTO;
using Agenda.Objects;
using AutoMapper;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.RestObjects;
using System;
using Agenda.API.Resources.v1.Appointments;
using Agenda.Models.v1.Attendees;
using Agenda.DTO.Resources.Search;

namespace Agenda.Mapping
{
    public static class AutoMapperConfig
    {
        /// <summary>
        /// Builds mappings between entities and dtos
        /// </summary>
        /// <returns></returns>
        public static MapperConfiguration Build() => new MapperConfiguration((cfg) =>
        {
            cfg.CreateCoreMapping();
            cfg.CreateMap<NewAppointmentInfo, Appointment>()
                .ForMember(entity => entity.Id, opt => opt.Ignore())
                .ForMember(entity => entity.CreatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.CreatedDate, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedDate, opt => opt.Ignore())
                .ForMember(entity => entity.UpdatedBy, opt => opt.Ignore())
                .ForMember(entity => entity.UUID, opt => opt.Ignore())
                ;

            cfg.CreateMap<NewAppointmentModel, NewAppointmentInfo>()
               .ReverseMap();
            

            cfg.CreateMap<AppointmentAttendee, AttendeeInfo>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Attendee.UUID))
                .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Attendee.Name))
                .ForMember(dto => dto.PhoneNumber, opt => opt.MapFrom(entity => entity.Attendee.PhoneNumber))
                .ForMember(dto => dto.Email, opt => opt.MapFrom(entity => entity.Attendee.Email))
                .ForMember(dto => dto.UpdatedDate, opt => opt.MapFrom(entity => entity.Attendee.UpdatedDate))
                .ForMember(dto => dto.CreatedDate, opt => opt.MapFrom(entity => entity.Attendee.CreatedDate))
                ;


            cfg.CreateMap<Appointment, AppointmentInfo>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.UUID))
                .ForMember(dto => dto.Participants, opt => opt.MapFrom(entity => entity.Attendees));

            cfg.CreateMap<AppointmentModel, AppointmentInfo>()
                .ReverseMap();

            cfg.CreateMap<Attendee, AttendeeInfo>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.UUID))
                .ReverseMap()
                .ForMember(entity => entity.Id, opt => opt.Ignore())
                ;

            cfg.CreateMap<AttendeeModel, AttendeeInfo>()
                .ReverseMap();

            cfg.CreateMap<SearchAppointmentInfo, SearchAppointmentModel>()
                .ReverseMap();
        });
    }
}
