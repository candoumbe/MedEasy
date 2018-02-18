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
    public class HandleDeleteBloodPressureInfoByIdCommandTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private EFUnitOfWorkFactory<MeasuresContext> _unitOfWorkFactory;
        private IExpressionBuilder _expressionBuilder;
        private HandleDeleteBloodPressureInfoByIdCommand _handler;

        public HandleDeleteBloodPressureInfoByIdCommandTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            DbContextOptionsBuilder<MeasuresContext> dbContextOptionsBuilder = new DbContextOptionsBuilder<MeasuresContext>();
            string dbName = $"InMemoryDb_{Guid.NewGuid()}";
            dbContextOptionsBuilder.UseInMemoryDatabase(dbName);
            
            _unitOfWorkFactory = new EFUnitOfWorkFactory<MeasuresContext>(dbContextOptionsBuilder.Options, (options) => new MeasuresContext(options));

            _expressionBuilder = AutoMapperConfig.Build().ExpressionBuilder;
            _handler = new HandleDeleteBloodPressureInfoByIdCommand(_unitOfWorkFactory, _expressionBuilder);
        }


        public void Dispose()
        {
            _outputHelper = null;
            _unitOfWorkFactory = null;
            _expressionBuilder = null;
        }

        [Fact]
        public async Task Delete()
        {
            // Arrange
            Guid measureId = Guid.NewGuid();
            BloodPressure measure = new BloodPressure
            {
                UUID = measureId,
                SystolicPressure = 120,
                DiastolicPressure = 80,
                DateOfMeasure = 30.September(2010).AddHours(14).AddMinutes(53),
                Patient = new Patient
                {
                    Firstname = "victor",
                    Lastname = "zsasz",
                    UUID = Guid.NewGuid()
                }
            };

            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                uow.Repository<BloodPressure>().Create(measure);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            // Act
            await _handler.Handle(new DeleteBloodPressureInfoByIdCommand(measureId), default)
                .ConfigureAwait(false);

            // Assert
            
            using (IUnitOfWork uow = _unitOfWorkFactory.New())
            {
                (await uow.Repository<BloodPressure>().AnyAsync(x => x.UUID == measureId)
                    .ConfigureAwait(false)).Should().BeFalse("Measure must be deleted after executing the command");
            }
        }
    }
}
