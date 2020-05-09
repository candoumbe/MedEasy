using Bogus;
using Documents.CQRS.Handlers;
using Documents.Objects;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.DAL.Interfaces;
using MedEasy.DAL.Repositories;
using MedEasy.IntegrationTests.Core;
using MedEasy.RestObjects;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using MedEasy.DAL.EFStore;
using Documents.DTO.v1;
using Documents.CQRS.Queries;
using Documents.DataStore;

namespace Documents.CQRS.UnitTests.Handlers
{
    [Feature(nameof(Documents))]
    [UnitTest]
    public class HandleGetPageOfDocumentInfoQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private IUnitOfWorkFactory _uowFactory;
        private HandleGetPageOfDocumentInfoQuery _sut;
        private readonly ITestOutputHelper _outputHelper;
        private static readonly Faker<Document> documentFaker = new Faker<Document>()
                        .CustomInstantiator(faker => new Document(
                            id: Guid.NewGuid(),
                            name: faker.PickRandom("pdf", "txt", "odt"),
                            mimeType: faker.System.MimeType())
                        );

        public HandleGetPageOfDocumentInfoQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
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
            _sut = new HandleGetPageOfDocumentInfoQuery(_uowFactory);
            _outputHelper = outputHelper;
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
            _sut = null;
        }

        public static IEnumerable<object[]> HandleCases
        {
            get
            {
                yield return new object[]
                {
                    Enumerable.Empty<Document>(),
                    (1, 10),
                    (Expression<Func<Page<DocumentInfo>, bool>>)(page => page.Count == 1
                        && page.Total == 0
                        && page.Entries != null && page.Entries.Count() == 0
                    ),
                    "DataStore is empty"
                };

                {
                    IEnumerable<Document> items = documentFaker.Generate(50);
                    yield return new object[]
                    {
                        items,
                        (2, 10),
                        (Expression<Func<Page<DocumentInfo>, bool>>)(page => page.Count == 5
                            && page.Total == 50
                            && page.Entries != null && page.Entries.Count() == 10
                        ),
                        "DataStore contains elements"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(HandleCases))]
        public async Task TestHandle(IEnumerable<Document> documents, (int page, int pageSize) pagination, Expression<Func<Page<DocumentInfo>, bool>> pageExpectation, string reason)
        {
            // Arrange
            _outputHelper.WriteLine($"page : {pagination.page}");
            _outputHelper.WriteLine($"pageSize : {pagination.pageSize}");

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Document>().Create(documents);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);

                int appointmentsCount = await uow.Repository<Document>().CountAsync()
                    .ConfigureAwait(false);
                _outputHelper.WriteLine($"DataStore count : {appointmentsCount}");
            }
            GetPageOfDocumentInfoQuery request = new GetPageOfDocumentInfoQuery(new PaginationConfiguration { Page = pagination.page, PageSize = pagination.pageSize });

            // Act
            Page<DocumentInfo> page = await _sut.Handle(request, cancellationToken: default)
                .ConfigureAwait(false);

            // Assert
            page.Should()
                .Match(pageExpectation, reason);
        }
    }
}
