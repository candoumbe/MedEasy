namespace Documents.CQRS.UnitTests.Handlers
{
    using Bogus;
    using Documents.CQRS.Handlers;
    using Documents.Objects;
    using FluentAssertions;
    using MedEasy.DAL.Interfaces;
    using MedEasy.DAL.Repositories;
    using MedEasy.IntegrationTests.Core;
    using MedEasy.RestObjects;
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
    using NodaTime.Testing;
    using NodaTime;
    using Documents.Ids;

    [Feature(nameof(Documents))]
    [UnitTest]
    public class HandleGetPageOfDocumentInfoQueryTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<DocumentsStore>>
    {
        private IUnitOfWorkFactory _uowFactory;
        private HandleGetPageOfDocumentInfoQuery _sut;
        private readonly ITestOutputHelper _outputHelper;
        private static readonly Faker<Document> documentFaker = new Faker<Document>()
                        .CustomInstantiator(faker => new Document(
                            id: DocumentId.New(),
                            name: faker.PickRandom("pdf", "txt", "odt"),
                            mimeType: faker.System.MimeType())
                        );

        public HandleGetPageOfDocumentInfoQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<DocumentsStore> database)
        {
            _uowFactory = new EFUnitOfWorkFactory<DocumentsStore>(database.OptionsBuilder.Options, (options) =>
            {
                DocumentsStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleGetPageOfDocumentInfoQuery(_uowFactory);
            _outputHelper = outputHelper;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
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
                        && page.Entries != null && page.Entries.Exactly(0)
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
            GetPageOfDocumentInfoQuery request = new(new PaginationConfiguration { Page = pagination.page, PageSize = pagination.pageSize });

            // Act
            Page<DocumentInfo> page = await _sut.Handle(request, cancellationToken: default)
                .ConfigureAwait(false);

            // Assert
            page.Should()
                .Match(pageExpectation, reason);
        }
    }
}
