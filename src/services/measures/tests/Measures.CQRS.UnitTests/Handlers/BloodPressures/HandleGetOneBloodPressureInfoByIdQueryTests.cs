using AutoMapper.QueryableExtensions;
using FluentAssertions;
using Measures.Context;
using Measures.CQRS.Handlers.BloodPressures;
using Measures.CQRS.Queries.BloodPressures;
using Measures.DTO;
using Measures.Mapping;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
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

namespace Measures.CQRS.UnitTests.Handlers.BloodPressures
{
    [UnitTest]
    public class HandleGetOneBloodPressureInfoByIdQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private HandleGetOneBloodPressureInfoByIdQuery _sut;

        public HandleGetOneBloodPressureInfoByIdQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<MeasuresContext> builder = new DbContextOptionsBuilder<MeasuresContext>();
            builder.UseInMemoryDatabase($"{Guid.NewGuid()}");

            _uowFactory = new EFUnitOfWorkFactory<MeasuresContext>(builder.Options, (options) => {
                MeasuresContext context = new MeasuresContext(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleGetOneBloodPressureInfoByIdQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder);
        }

        public void Dispose()
        {
            _uowFactory = null;
            _sut = null;
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
            _outputHelper.WriteLine($"{nameof(unitOfWorkFactory)} is null : {(unitOfWorkFactory == null)}");
            _outputHelper.WriteLine($"{nameof(expressionBuilder)} is null : {(expressionBuilder == null)}");

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

        [Fact]
        public async Task Get_Unknown_Id_Returns_None()
        {
            // Act
            Option<BloodPressureInfo> optionalResource = await _sut.Handle(new GetBloodPressureInfoByIdQuery(Guid.NewGuid()), default)
                .ConfigureAwait(false);

            // Assert
            optionalResource.HasValue.Should()
                .BeFalse();
        }
    }
}
