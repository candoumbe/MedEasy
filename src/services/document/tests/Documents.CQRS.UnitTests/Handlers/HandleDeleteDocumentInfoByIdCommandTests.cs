using Documents.CQRS.Handlers;
using Documents.Objects;
using FluentAssertions;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using Documents.CQRS.Commands;
using Documents.DataStore;
using NodaTime.Testing;
using NodaTime;
using Documents.Ids;

namespace Documents.CQRS.UnitTests.Handlers
{
    [Feature(nameof(Documents))]
    [UnitTest]
    public class HandleDeleteDocumentInfoByIdCommandTests : IClassFixture<SqliteEfCoreDatabaseFixture<DocumentsStore>>
    {
        private readonly ITestOutputHelper _outputHelper;
        private readonly IUnitOfWorkFactory _uowFactory;
        private readonly HandleDeleteDocumentInfoByIdCommand _sut;

        public HandleDeleteDocumentInfoByIdCommandTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<DocumentsStore> database)
        {
            _outputHelper = outputHelper;

            _uowFactory = new EFUnitOfWorkFactory<DocumentsStore>(database.OptionsBuilder.Options, (options) =>
            {
                DocumentsStore context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleDeleteDocumentInfoByIdCommand(_uowFactory);
        }

        [Fact]
        public void GivenNullParameter_Ctor_Throws_ArgumentException()
        {
            // Act
            Action action = () => new HandleDeleteDocumentInfoByIdCommand(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>();
        }

        [Fact]
        public async Task GivenRecordExists_Handle_Returns_DeleteOk()
        {
            // Arrange
            DocumentId documentid = DocumentId.New();
            Document document = new(id: documentid, name: "afile");

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Document>().Create(document);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            DeleteDocumentInfoByIdCommand cmd = new(documentid);
            // Act
            DeleteCommandResult cmdResult = await _sut.Handle(cmd, default)
                .ConfigureAwait(false);

            // Assert
            cmdResult.Should()
                .Be(DeleteCommandResult.Done);

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                bool deleteOk = !await uow.Repository<Document>().AnyAsync(x => x.Id == documentid)
                    .ConfigureAwait(false);

                deleteOk.Should()
                        .BeTrue("deleted resource must not be prensent in the datastore");
            }
        }
    }
}
