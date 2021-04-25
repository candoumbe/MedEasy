﻿using AutoMapper.QueryableExtensions;

using FluentAssertions;

using Measures.Context;
using Measures.CQRS.Handlers.Patients;
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

namespace Measures.CQRS.UnitTests.Handlers.Patients
{
    [UnitTest]
    public class HandleGetPageOfBloodPressureInfoByPatientIdQueryTests : IClassFixture<SqliteEfCoreDatabaseFixture<MeasuresContext>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly HandleGetPageOfBloodPressureInfoByPatientIdQuery _sut;

        public HandleGetPageOfBloodPressureInfoByPatientIdQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<MeasuresContext> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(database.OptionsBuilder.Options, (options) =>
            {
                MeasuresContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });

            _sut = new HandleGetPageOfBloodPressureInfoByPatientIdQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder);
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
            Action action = () => new HandleGetPageOfBloodPressureInfoByPatientIdQuery(unitOfWorkFactory, expressionBuilder);
#pragma warning restore IDE0039 // Utiliser une fonction locale

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
            GetPageOfBloodPressureInfoByPatientIdQuery query = new(PatientId.New(), new PaginationConfiguration());

            // Act
            Option<Page<BloodPressureInfo>> result = await _sut.Handle(query, default)
                .ConfigureAwait(false);

            // Assert
            result.HasValue.Should()
                .BeFalse("patient does not exist");
        }
    }
}
