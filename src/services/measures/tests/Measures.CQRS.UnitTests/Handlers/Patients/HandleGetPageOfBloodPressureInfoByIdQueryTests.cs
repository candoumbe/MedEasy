namespace Measures.CQRS.UnitTests.Handlers.Patients
{
    using AutoMapper.QueryableExtensions;

    using FluentAssertions;

    using Measures.DataStores;
    using Measures.CQRS.Handlers.Subjects;
    using Measures.CQRS.Queries.BloodPressures;
    using Measures.DTO;
    using Measures.Ids;
    using Measures.Mapping;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
    using MedEasy.IntegrationTests.Core;
    using MedEasy.RestObjects;

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

    [UnitTest]
    public class HandleGetPageOfBloodPressureInfoByPatientIdQueryTests : IClassFixture<SqliteEfCoreDatabaseFixture<MeasuresStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly HandleGetPageOfBloodPressureInfoBySubjectIdQuery _sut;

        public HandleGetPageOfBloodPressureInfoByPatientIdQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<MeasuresStore> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<MeasuresStore>(database.OptionsBuilder.Options, (options) =>
            {
                MeasuresStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _sut = new HandleGetPageOfBloodPressureInfoBySubjectIdQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder);
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
            Action action = () => new HandleGetPageOfBloodPressureInfoBySubjectIdQuery(unitOfWorkFactory, expressionBuilder);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task GivenNoData_Handle_Returns_None()
        {
            // Arrange
            GetPageOfBloodPressureInfoBySubjectIdQuery query = new(SubjectId.New(), new PaginationConfiguration());

            // Act
            Option<Page<BloodPressureInfo>> result = await _sut.Handle(query, default)
                .ConfigureAwait(false);

            // Assert
            result.HasValue.Should()
                .BeFalse("patient does not exist");
        }
    }
}
