using System;
using Xunit.Abstractions;
using Moq;
using Xunit;
using FluentAssertions;
using MedEasy.Commands.Specialty;
using System.Threading.Tasks;
using MedEasy.Handlers.Specialty.Commands;
using MedEasy.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using MedEasy.API.Stores;

namespace MedEasy.Handlers.Tests.Handlers.Commands.Specialty
{
    public class RunDeleteSpecialtyByIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private RunDeleteSpecialtyByIdCommand _runner;
        
        public RunDeleteSpecialtyByIdCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MedEasyContext> dbOptionsBuiler = new DbContextOptionsBuilder<MedEasyContext>();
            dbOptionsBuiler.UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(dbOptionsBuiler.Options);
            
            _runner = new RunDeleteSpecialtyByIdCommand(_unitOfWorkFactory);
        }


        
        [Fact]
        public void ConstructorThrowsArgumentNullExceptionWhenParameterIsNull()
        {
            Action action = () => new RunDeleteSpecialtyByIdCommand(null);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task ValidCommandShouldDeleteTheResource()
        {
            //Arrange
            
            Guid idToDelete = Guid.NewGuid();
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<Objects.Specialty>().Create(new Objects.Specialty { Name = "SPEC1", UUID = idToDelete });

                await uow.SaveChangesAsync();
            }

            //Act
            DeleteSpecialtyByIdCommand command = new DeleteSpecialtyByIdCommand(idToDelete);
            _outputHelper.WriteLine($"Command : {command}");

            await _runner.RunAsync(command);


            // Assert

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                (await uow.Repository<Objects.Specialty>().AnyAsync(x => x.UUID == idToDelete )).Should()
                    .BeFalse();
                
            }
        }

        

        [Fact]
        public void ShouldThrowArgumentNullException()
        {
            Func<Task> action = async () =>
            {
                RunDeleteSpecialtyByIdCommand handler = new RunDeleteSpecialtyByIdCommand(Mock.Of<IUnitOfWorkFactory>());

                await handler.RunAsync(null);
            };

            action.ShouldThrow<ArgumentNullException>();
        }



        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactory = null;
            _runner = null;
        }
    }
}
