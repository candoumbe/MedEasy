﻿using AutoMapper.QueryableExtensions;

using FluentAssertions;

using Measures.Context;
using Measures.CQRS.Handlers.BloodPressures;
using Measures.Mapping;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using Moq;

using NodaTime;
using NodaTime.Testing;

using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Measures.CQRS.UnitTests.Handlers.BloodPressures
{
    [UnitTest]
    public class HandleGetPageOfBloodPressureInfoQueryTests : IClassFixture<SqliteEfCoreDatabaseFixture<MeasuresContext>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly HandleGetPageOfBloodPressureInfoQuery _sut;

        public HandleGetPageOfBloodPressureInfoQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<MeasuresContext> database)
        {
            _outputHelper = outputHelper;
            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(database.OptionsBuilder.Options, (options) =>
            {
                MeasuresContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleGetPageOfBloodPressureInfoQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder);
        }

        public static IEnumerable<object[]> CtorThrowsArgumentNullExceptionCases
        {
            get
            {
                IUnitOfWorkFactory[] uowFactorieCases = { null, Mock.Of<IUnitOfWorkFactory>() };
                IExpressionBuilder[] expressionBuilderCases = { null, Mock.Of<IExpressionBuilder>() };

                IEnumerable<object[]> cases = uowFactorieCases
                    .CrossJoin(expressionBuilderCases, (uowFactory, expressionBuilder) => (uowFactory, expressionBuilder))
                    .Where(tuple => tuple.uowFactory == null || tuple.expressionBuilder == null)
                    .Select(tuple => new object[] { tuple.uowFactory, tuple.expressionBuilder });

                return cases;
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
            Action action = () => new HandleGetOneBloodPressureInfoByIdQuery(unitOfWorkFactory, expressionBuilder);
#pragma warning restore IDE0039 // Utiliser une fonction locale

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }
    }
}
