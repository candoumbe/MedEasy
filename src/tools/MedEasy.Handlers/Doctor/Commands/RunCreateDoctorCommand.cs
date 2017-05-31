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

        public async Task<DoctorInfo> RunAsync(ICreateDoctorCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            CreateDoctorInfo info = command.Data;
            Debug.Assert(!string.IsNullOrWhiteSpace(info.Lastname));

            using (IUnitOfWork uow = UowFactory.New())
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;

                int? specialtyId = null;
                if (info.SpecialtyId.HasValue)
                {
                    var specialty = await uow.Repository<Objects.Specialty>().SingleOrDefaultAsync(x => new { x.Id  } , x => x.UUID == info.SpecialtyId);
                    if (specialty == null)
                    {
                        throw new NotFoundException($"{nameof(Objects.Specialty)} <{info.SpecialtyId}> not found");
                    }
                    else
                    {
                        specialtyId = specialty.Id;
                    }

                }

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

                return _mapper.Map<DoctorInfo>(itemToCreate); 

            }
        }
    }
}
