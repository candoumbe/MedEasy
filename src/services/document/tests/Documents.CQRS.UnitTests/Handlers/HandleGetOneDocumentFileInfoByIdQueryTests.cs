using AutoMapper;
using Documents.CQRS.Handlers;
using Documents.CQRS.Queries;
using Documents.DataStore.SqlServer;
using Documents.DTO.v1;
using Documents.Objects;
using FluentAssertions;
using FluentAssertions.Extensions;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
using Optional;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Documents.CQRS.UnitTests.Features.Documents.Handlers
{
    [Feature("Documents")]
    [UnitTest]
    public class HandleGetOneDocumentFileInfoByIdQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private IUnitOfWorkFactory _uowFactory;
        private HandleGetOneDocumentInfoByIdQuery _sut;

        public HandleGetOneDocumentFileInfoByIdQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            DbContextOptionsBuilder<DocumentsStore> optionsBuilder = new DbContextOptionsBuilder<DocumentsStore>();
            optionsBuilder.UseSqlite(database.Connection);

            _uowFactory = new EFUnitOfWorkFactory<DocumentsStore>(optionsBuilder.Options, (options) =>
            {
                DocumentsStore context = new DocumentsStore(options);
                context.Database.EnsureCreated();
                return context;
            });

            _sut = new HandleGetOneDocumentInfoByIdQuery(_uowFactory);
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

        [Fact]
        public async Task GivenEmptyDataStore_Handle_Returns_None()
        {
            // Arrange
            GetOneDocumentFileInfoByIdQuery request = new GetOneDocumentFileInfoByIdQuery(Guid.NewGuid());

            // Act
            Option<(DocumentInfo, byte[])> optionalDocument = await _sut.Handle(request, cancellationToken: default)
                .ConfigureAwait(false);

            // Assert
            optionalDocument.HasValue.Should().BeFalse("The store is empty");
        }

        [Fact]
        public async Task GivenRecordExistsInDataStore_Get_Returns_Some()
        {
            // Arrange
            Guid documentId = Guid.NewGuid();
            Document document = new Document
            (
                id: documentId,
                name : "Wayne Tower",
                mimeType: "image/jpeg"
            )
            .SetFile(new byte[] { 123 });


            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Document>().Create(document);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }
            
            GetOneDocumentInfoByIdQuery request = new GetOneDocumentInfoByIdQuery(documentId);

            // Act
            Option<DocumentInfo> optionalDocument = await _sut.Handle(request, default)
                .ConfigureAwait(false);

            // Assert
            optionalDocument.HasValue.Should().BeTrue($"the record <{documentId}> exists in the datastore");
            optionalDocument.MatchSome((documentInfo) =>
            {
                documentInfo.Id.Should().Be(document.Id);
                documentInfo.Name.Should().Be(document.Name);
                documentInfo.MimeType.Should().Be(document.MimeType);
                documentInfo.Hash.Should().Be(document.Hash);
            });
        }
    }
}
