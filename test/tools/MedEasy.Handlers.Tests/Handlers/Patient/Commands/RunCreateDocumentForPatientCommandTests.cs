using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using Xunit;
using AutoMapper;
using FluentAssertions;
using MedEasy.Validators;
using MedEasy.Commands.Patient;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.DAL.Interfaces;
using MedEasy.Mapping;
using System.Threading.Tasks;
using MedEasy.RestObjects;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MedEasy.API.Stores;
using MedEasy.Handlers.Core.Exceptions;
using Optional;

namespace MedEasy.Handlers.Tests.Commands.Patient
{
    public class RunCreateDocumentForPatientCommandTests : IDisposable
    {
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private RunCreateDocumentForPatientCommand _handler;
        private IMapper _mapper;
        private ITestOutputHelper _outputHelper;


        public RunCreateDocumentForPatientCommandTests(ITestOutputHelper output)
        {
            DbContextOptionsBuilder<MedEasyContext> builder = new DbContextOptionsBuilder<MedEasyContext>()
                .UseInMemoryDatabase($"InMemory_{Guid.NewGuid()}");
            _unitOfWorkFactory = new EFUnitOfWorkFactory(builder.Options);
            
            
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _outputHelper = output;

            _handler = new RunCreateDocumentForPatientCommand(_unitOfWorkFactory, _mapper);
        }

        public void Dispose()
        {
            _unitOfWorkFactory = null;
            _outputHelper = null;
            _handler = null;

        }


        public static IEnumerable<object[]> ConstructorCases
        {
            get
            {
                return new[] {null, Mock.Of<IUnitOfWorkFactory>() }.CrossJoin(new object[]
                {
                    null,
                    Mock.Of<IMapper>()
                })
                .Where(item => item.Item1 == null || item.Item2 == null)
                .Select(item => new[] { item.Item1, item.Item2 });
            }
        }

        [Theory]
        [MemberData(nameof(ConstructorCases))]
        public void ConstructorWithInvalidArgumentsThrowsArgumentNullException(IUnitOfWorkFactory factory, IMapper mapper)
        {

            _outputHelper.WriteLine($"{nameof(factory)} == null : {factory == null}");
            _outputHelper.WriteLine($"{nameof(mapper)} == null : {mapper == null}");

            Action action = () => new RunCreateDocumentForPatientCommand(factory, mapper);

            action.ShouldThrow<ArgumentNullException>().And
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void Throws_ArgumentNullException_When_Command_To_Run_Is_Null()
        {
            // Act
            Func<Task> action = async () => await _handler.RunAsync(null);

            // Assert
            action.ShouldThrow<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }

}
