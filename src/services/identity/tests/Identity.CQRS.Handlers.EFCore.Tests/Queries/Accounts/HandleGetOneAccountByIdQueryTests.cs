using AutoMapper.QueryableExtensions;
using FluentAssertions;
using Identity.CQRS.Handlers.Queries.Accounts;
using Identity.CQRS.Queries.Accounts;
using Identity.DataStores;
using Identity.DTO;
using Identity.Mapping;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Optional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Identity.CQRS.UnitTests.Handlers.Accounts
{
    [UnitTest]
    [Feature("Handlers")]
    [Feature("Accounts")]
    public class HandleGetOneAccountByIdQueryTests : IDisposable,IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private HandleGetOneAccountByIdQuery _sut;

        public HandleGetOneAccountByIdQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;

            DbContextOptionsBuilder<IdentityContext> builder = new DbContextOptionsBuilder<IdentityContext>();
            builder.UseSqlite(database.Connection);

            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(builder.Options, (options) => {
                IdentityContext context = new IdentityContext(options);
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleGetOneAccountByIdQuery(_uowFactory, AutoMapperConfig.Build().ExpressionBuilder);
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
            Action action = () => new HandleGetOneAccountByIdQuery(unitOfWorkFactory, expressionBuilder);
#pragma warning restore IDE0039 // Utiliser une fonction locale

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
            Option<AccountInfo> optionalResource = await _sut.Handle(new GetOneAccountByIdQuery(Guid.NewGuid()), default)
                .ConfigureAwait(false);

            // Assert
            optionalResource.HasValue.Should()
                .BeFalse();
        }
    }
}
