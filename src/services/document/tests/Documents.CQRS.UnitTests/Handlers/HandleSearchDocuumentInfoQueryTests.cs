using AutoMapper.QueryableExtensions;
using Bogus;
using Documents.CQRS.Handlers;
using Documents.CQRS.Queries;
using Documents.DataStore;
using Documents.DTO.v1;
using Documents.Mapping;
using Documents.Objects;
using FluentAssertions;
using MedEasy.CQRS.Core.Handlers;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Documents.CQRS.UnitTests.Handlers
{
    [Feature(nameof(Documents))]
    [UnitTest]
    [Feature("Search")]
    public class HandleSearchDocumentInfoQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IHandleSearchQuery _searchQueryHandler;
        private HandleSearchDocumentInfoQuery _sut;
        private IUnitOfWorkFactory _uowFactory;
        private IExpressionBuilder _expressionBuilder;
        private static readonly Faker<Document> documentFaker = new Faker<Document>()
                        .CustomInstantiator(faker => new Document(
                            id: Guid.NewGuid(),
                            name: faker.PickRandom("pdf", "txt", "odt"),
                            mimeType: faker.System.MimeType())
                        );

        public HandleSearchDocumentInfoQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            DbContextOptionsBuilder<DocumentsStore> optionsBuilder = new DbContextOptionsBuilder<DocumentsStore>();
            optionsBuilder.UseSqlite(database.Connection)
                .EnableSensitiveDataLogging();

            _uowFactory = new EFUnitOfWorkFactory<DocumentsStore>(optionsBuilder.Options, (options) =>
            {
                DocumentsStore context = new DocumentsStore(options);
                context.Database.EnsureCreated();
                return context;
            });

            _expressionBuilder = AutoMapperConfig.Build().ExpressionBuilder;
            _outputHelper = outputHelper;
            _searchQueryHandler = new HandleSearchQuery(_uowFactory, _expressionBuilder);
            _sut = new HandleSearchDocumentInfoQuery(_searchQueryHandler);
        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Document>().Clear();

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            _uowFactory = null;
            _expressionBuilder = null;
            _searchQueryHandler = null;
            _sut = null;
        }

        public static IEnumerable<object[]> HandleCases
        {
            get
            {
                {
                    SearchDocumentInfo searchDocumentInfo = new SearchDocumentInfo
                    {
                        Page = 1,
                        PageSize = 10
                    };
                    yield return new object[]
                    {
                        Enumerable.Empty<Document>(),
                        searchDocumentInfo,
                        (
                            expectedPageCount : 1,
                            expectedPageSize : searchDocumentInfo.PageSize,
                            expetedTotal : 0,
                            itemsExpectation : (Expression<Func<IEnumerable<DocumentInfo>, bool>>)(items => items != null && !items.Any())
                        )
                    };
                }

                {
                    IEnumerable<Document> documents = documentFaker.Generate(10);
                    yield return new object[]
                    {
                        documents,
                        new SearchDocumentInfo
                        {
                            Page = 1,
                            PageSize = 10
                        },
                        (
                            expectedPageCount : 1,
                            expectedPageSize : 10,
                            expetedTotal : 10,
                            itemsExpectation : (Expression<Func<IEnumerable<DocumentInfo>, bool>>)(items => items.Select(x => x.Id).Distinct().Count() == 10)
                        )
                    };
                }

                {
                    
                    IEnumerable<Document> documents = documentFaker.Generate(100);
                    yield return new object[]
                    {
                        documents,
                        new SearchDocumentInfo
                        {
                            Page = 1,
                            PageSize = 10,
                            Name = "*.xml"
                        },
                        (
                            expectedPageCount : 1,
                            expectedPageSize : 10,
                            expetedTotal : 0,
                            itemsExpectation : (Expression<Func<IEnumerable<DocumentInfo>, bool>>)(items => !items.Any())
                        )
                    };
                }

                {
                    IEnumerable<Document> documents = documentFaker.Generate(100)
                                                                   .Select(doc => {
                                                                       doc.ChangeNameTo($"{doc.Name}.pdf");

                                                                       return doc;
                                                                    });
                    yield return new object[]
                    {
                        documents,
                        new SearchDocumentInfo
                        {
                            Page = 1,
                            PageSize = 10,
                            Name = "*.pdf"
                        },
                        (
                            expectedPageCount : 10,
                            expectedPageSize : 10,
                            expetedTotal : 100,
                            itemsExpectation : (Expression<Func<IEnumerable<DocumentInfo>, bool>>)
                            (
                                items => items.All(doc => doc.Name.EndsWith(".pdf"))
                            )
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(HandleCases))]
        public async Task GivenDataStoreHasRecords_Handle_Returns_Data(IEnumerable<Document> documents, SearchDocumentInfo searchCriteria,
            (int expectedPageCount, int expectedPageSize, int expectedTotal, Expression<Func<IEnumerable<DocumentInfo>, bool>> itemsExpectation) expectations)
        {
            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Document>().Create(documents);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                _outputHelper.WriteLine($"Datastore : {documents.Jsonify()}");
                _outputHelper.WriteLine($"Search criteria : {searchCriteria.Jsonify()}");
            }

            SearchDocumentInfoQuery request = new SearchDocumentInfoQuery(searchCriteria);

            // Act
            Page<DocumentInfo> page = await _sut.Handle(request, default)
                .ConfigureAwait(false);

            // Assert

            page.Should()
                .NotBeNull();
            page.Count.Should()
                .Be(expectations.expectedPageCount);
            page.Total.Should()
                .Be(expectations.expectedTotal);
            page.Size.Should()
                .Be(expectations.expectedPageSize);
            page.Entries.Should()
                .Match(expectations.itemsExpectation);
        }
    }
}
