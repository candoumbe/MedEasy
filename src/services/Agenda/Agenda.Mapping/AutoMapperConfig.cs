using Agenda.DTO;
using Agenda.Objects;
using AutoMapper;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.RestObjects;
using System;

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

            cfg.CreateMap<AppointmentParticipant, ParticipantInfo>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.Participant.UUID))
                .ForMember(dto => dto.Name, opt => opt.MapFrom(entity => entity.Participant.Name))
                .ForMember(dto => dto.PhoneNumber, opt => opt.MapFrom(entity => entity.Participant.PhoneNumber))
                .ForMember(dto => dto.Email, opt => opt.MapFrom(entity => entity.Participant.Email))
                .ForMember(dto => dto.UpdatedDate, opt => opt.MapFrom(entity => entity.Participant.UpdatedDate))
                .ForMember(dto => dto.CreatedDate, opt => opt.MapFrom(entity => entity.Participant.CreatedDate))
                ;
                

            cfg.CreateMap<Appointment, AppointmentInfo>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.UUID))
                .ForMember(dto => dto.Participants, opt => opt.MapFrom(entity => entity.Participants));

            cfg.CreateMap<Participant, ParticipantInfo>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => entity.UUID))
                .ReverseMap()
                .ForMember(entity => entity.Id, opt => opt.Ignore())
                ;
        });
        
    }
}
