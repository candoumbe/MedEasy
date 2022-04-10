namespace Identity.CQRS.UnitTests.Handlers.Accounts
{
    using AutoMapper.QueryableExtensions;

    using FluentAssertions;

    using Identity.CQRS.Handlers.Queries.Accounts;
    using Identity.CQRS.Queries.Accounts;
    using Identity.DataStores;
    using Identity.DTO;
    using Identity.Ids;
    using Identity.Mapping;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;

    using MediatR;

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
    [Feature("Handlers")]
    [Feature("Accounts")]
    public class HandleGetOneAccountByIdQueryTests : IClassFixture<SqliteEfCoreDatabaseFixture<IdentityDataStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly HandleGetOneAccountByIdQuery _sut;

        public HandleGetOneAccountByIdQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<IdentityDataStore> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<IdentityDataStore>(database.OptionsBuilder.Options, (options) =>
            {
                IdentityDataStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleGetOneAccountByIdQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder);
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
            Action action = () => new HandleGetOneAccountByIdQuery(unitOfWorkFactory, expressionBuilder);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                    .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void IsHandler() => typeof(HandleGetOneAccountByIdQuery)
            .Should().Implement<IRequestHandler<GetOneAccountByIdQuery, Option<AccountInfo>>>();

        [Fact]
        public async Task Get_Unknown_Id_Returns_None()
        {
            // Act
            Option<AccountInfo> optionalResource = await _sut.Handle(new GetOneAccountByIdQuery(AccountId.New()), default)
                .ConfigureAwait(false);

            // Assert
            optionalResource.HasValue.Should()
                .BeFalse();
        }
    }
}
