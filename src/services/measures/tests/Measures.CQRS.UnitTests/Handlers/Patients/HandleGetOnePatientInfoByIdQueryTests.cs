using AutoMapper.QueryableExtensions;

using FluentAssertions;

using Measures.DataStores;
using Measures.CQRS.Handlers.Patients;
using Measures.CQRS.Queries.Patients;
using Measures.DTO;
using Measures.Ids;
using Measures.Mapping;

using MedEasy.Abstractions.ValueConverters;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Moq;

using NodaTime;
using NodaTime.Testing;

using Optional;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Measures.CQRS.UnitTests.Handlers.Patients
{
    [UnitTest]
    [Feature("Handlers")]
    [Feature("Patients")]
    public class HandleGetOnePatientInfoByIdQueryTests : IClassFixture<SqliteEfCoreDatabaseFixture<MeasuresStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly HandleGetOnePatientInfoByIdQuery _sut;

        public HandleGetOnePatientInfoByIdQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<MeasuresStore> database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresStore> builder = new();
            builder.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>()
                .UseInMemoryDatabase($"{Guid.NewGuid()}");

            _uowFactory = new EFUnitOfWorkFactory<MeasuresStore>(builder.Options, (options) =>
            {
                MeasuresStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleGetOnePatientInfoByIdQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder);
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>() };
                IExpressionBuilder[] expressionBuilderCases = { null, Mock.Of<IExpressionBuilder>() };

                return uowFactorieCases
                    .CrossJoin(expressionBuilderCases, (uowFactory, expressionBuilder) => (uowFactory, expressionBuilder))
                    .Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder == null)
                    .Select(tuple => new object[] { tuple.uowFactory, tuple.expressionBuilder });
            }
        }

        [Theory]
        [MemberData(nameof(CtorThrowsArgumentNullExceptionCases))]
        public void Ctor_Throws_ArgumentNullException_When_Parameters_Is_Null(IUnitOfWorkFactory unitOfWorkFactory, IExpressionBuilder expressionBuilder)
        {
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {unitOfWorkFactory == null}");
            _outputHelper.WriteLine($"{nameof(expressionBuilder)} is null : {expressionBuilder == null}");

            // Act
#pragma warning disable IDE0039 // Utiliser une fonction locale
            Action action = () => new HandleGetOnePatientInfoByIdQuery(unitOfWorkFactory, expressionBuilder);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Get_Unknown_Id_Returns_None()
        {
            // Act
            Option<PatientInfo> optionalResource = await _sut.Handle(new GetPatientInfoByIdQuery(PatientId.New()), default)
                .ConfigureAwait(false);

            // Assert
            optionalResource.HasValue.Should()
                .BeFalse();
        }
    }
}
