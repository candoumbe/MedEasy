using System;
using MedEasy.DTO;
using MedEasy.DAL.Interfaces;
using System.Threading.Tasks;
using System.Diagnostics;
using AutoMapper;
using MedEasy.Commands.Doctor;

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

        public async Task<DoctorInfo> RunAsync(ICreateDoctorCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            CreateDoctorInfo info = command.Data;
            Debug.Assert(!string.IsNullOrWhiteSpace(info.Lastname));

            using (var uow = UowFactory.New())
            {
                var now = DateTime.UtcNow;
                Objects.Doctor itemToCreate = new Objects.Doctor()
                {
                    Firstname = info.Firstname?.Trim()?.ToTitleCase(),
                    Lastname = info.Lastname?.ToUpper(),
                    SpecialtyId = info.SpecialtyId,
                    UpdatedDate = now,
                    CreatedDate = now,
#if DNX451
                    CreatedBy = Environment.UserName,
                    UpdatedBy = Environment.UserName
#endif

                };

                uow.Repository<Objects.Doctor>().Create(itemToCreate);
                await uow.SaveChangesAsync();

                return _mapper.Map<DoctorInfo>(itemToCreate); 

            }
        }
    }
}
