using AutoMapper.QueryableExtensions;
using FluentAssertions;
using FluentAssertions.Extensions;
using Measures.Context;
using Measures.CQRS.Commands;
using Measures.CQRS.Handlers;
using Measures.DTO;
using Measures.Mapping;
using Measures.Objects;
using MedEasy.DAL.Context;
using MedEasy.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Measures.CQRS.UnitTests.Handlers
{
    [UnitTest]
    [Feature("Handlers")]
    [Feature("BloodPressures")]
    public class HandleCreateBloodPressureInfoCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private EFUnitOfWorkFactory<MeasuresContext> _unitOfWorkFactory;
        private IExpressionBuilder _expressionBuilder;
        private HandleCreateBloodPressureInfoCommand _handler;

        public HandleCreateBloodPressureInfoCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            DbContextOptionsBuilder<MeasuresContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MeasuresContext>();
            string dbName = $"InMemoryDb_{Guid.NewGuid()}";
            dbContextOptionsBuilder.UseInMemoryDatabase(dbName);
            
            _unitOfWorkFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbContextOptionsBuilder.Options, (options) => new MeasuresContext(options));

            _expressionBuilder = AutoMapperConfig.Build().ExpressionBuilder;
            _handler = new HandleCreateBloodPressureInfoCommand(_unitOfWorkFactory, _expressionBuilder);
        }


        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactory = null;
            _expressionBuilder = null;
        }

        [Fact]
        public async Task Create_A_New_Resource_With_Patient_Info()
        {
            // Arrange
            CreateBloodPressureInfo newResource = new CreateBloodPressureInfo
            {
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 30.September(2010).AddHours(14).AddMinutes(53),
                Patient = new PatientInfo
                {
                    Firstname = "victor",
                    Lastname = "zsasz"
                }
            };

            CreateBloodPressureInfoCommand command = new CreateBloodPressureInfoCommand(newResource);

            // Act
            BloodPressureInfo createdResource = await _handler.Handle(command, default)
                .ConfigureAwait(false);

            // Assert
            createdResource.Should()
                .NotBeNull();
            createdResource.Id.Should().NotBeEmpty();
            createdResource.PatientId.Should().NotBeEmpty();
            createdResource.DateOfMeasure.Should().Be(newResource.DateOfMeasure);
            createdResource.SystolicPressure.Should().Be(newResource.SystolicPressure);
            createdResource.DiastolicPressure.Should().Be(newResource.DiastolicPressure);

            
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                (await uow.Repository<Patient>().AnyAsync(x => x.UUID == createdResource.PatientId)
                    .ConfigureAwait(false)).Should().BeTrue("Creating a blood pressure with patient data should create the patient");
            }
        }
    }
}
