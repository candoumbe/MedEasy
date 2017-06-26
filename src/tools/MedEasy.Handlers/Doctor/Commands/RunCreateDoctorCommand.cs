using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using System.Diagnostics;
using AutoMapper;
using MedEasy.Commands.Doctor;
using MedEasy.Handlers.Core.Doctor.Commands;
using MedEasy.Handlers.Core.Exceptions;
using System.Threading;
using Microsoft.CodeAnalysis;
using Optional;

namespace MedEasy.Handlers.Doctor.Commands
{


    /// <summary>
    /// An instance of this class process process <see cref="IRunCreateDoctorCommand"/>
    /// </summary>
    public class RunCreateDoctorCommand : IRunCreateDoctorCommand
    {
        private readonly IMapper _mapper;

        public RunCreateDoctorCommand(IUnitOfWorkFactory factory, IMapper mapper)
        {
            UowFactory = factory;
            _mapper = mapper;
        }

        private IUnitOfWorkFactory UowFactory { get; }

        public async Task<Option<DoctorInfo, CommandException>> RunAsync(ICreateDoctorCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            Option<DoctorInfo, CommandException> result = default(Option<DoctorInfo, CommandException>);
            CreateDoctorInfo info = command.Data;
            Debug.Assert(!string.IsNullOrWhiteSpace(info.Lastname));

            using (IUnitOfWork uow = UowFactory.New())
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;

                int? specialtyId = null;

                if (info.SpecialtyId.HasValue)
                {
                    var specialty = await uow.Repository<Objects.Specialty>()
                        .SingleOrDefaultAsync(x => new { x.Id }, x => x.UUID == info.SpecialtyId);

                    specialty.MatchSome(x => specialtyId = x.Id);

                    if (specialtyId.HasValue)
                    {
                        Objects.Doctor itemToCreate = new Objects.Doctor()
                        {
                            Firstname = info.Firstname?.Trim()?.ToTitleCase(),
                            Lastname = info.Lastname?.ToUpper(),
                            SpecialtyId = specialtyId,
                            UpdatedDate = now,
                            CreatedDate = now
                        };

                        uow.Repository<Objects.Doctor>().Create(itemToCreate);
                        await uow.SaveChangesAsync(cancellationToken);

                        result = _mapper.Map<DoctorInfo>(itemToCreate).Some<DoctorInfo, CommandException>();
                    }
                    else
                    {
                        result = Option.None<DoctorInfo, CommandException>(new CommandEntityNotFoundException($"Specialty <{info.SpecialtyId}> not found"));
                    }
                }

                return result;

            }
        }
    }
}
