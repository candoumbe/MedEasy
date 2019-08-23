using Documents.CQRS.Handlers;
using Documents.Objects;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.CQRS.Core.Commands.Results;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;
using Documents.DataStore.SqlServer;
using Documents.CQRS.Commands;

namespace Documents.CQRS.UnitTests.Handlers
{
    [Feature(nameof(Documents))]
    [UnitTest]
    public class HandleDeleteDocumentInfoByIdCommandTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private readonly ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private HandleDeleteDocumentInfoByIdCommand _sut;

        public HandleDeleteDocumentInfoByIdCommandTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            _outputHelper = outputHelper;
            DbContextOptionsBuilder<DocumentsStore> builder = new DbContextOptionsBuilder<DocumentsStore>();
            builder.UseSqlite(database.Connection)
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(warnings => warnings.Throw());

            _uowFactory = new EFUnitOfWorkFactory<DocumentsStore>(builder.Options, (options) =>
            {
                DocumentsStore context = new DocumentsStore(options);
                context.Database.EnsureCreated();
                return context;
            });
            _sut = new HandleDeleteDocumentInfoByIdCommand(_uowFactory);
        }

        public void Dispose()
        {
            _uowFactory = null;
            _sut = null;
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
            Guid documentid = Guid.NewGuid();
            Document document = new Document(id: documentid, name: "afile")
                .SetFile(file: new byte[] { 123 });


            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Document>().Create(document);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            DeleteDocumentInfoByIdCommand cmd = new DeleteDocumentInfoByIdCommand(documentid);
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
