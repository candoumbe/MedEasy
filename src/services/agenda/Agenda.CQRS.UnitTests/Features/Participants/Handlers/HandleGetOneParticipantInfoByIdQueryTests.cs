using Agenda.CQRS.Features.Participants.Handlers;
using Agenda.CQRS.Features.Participants.Queries;
using Agenda.DataStores;
using Agenda.DTO;
using Agenda.Mapping;
using Agenda.Objects;
using AutoMapper;
using FluentAssertions;
using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;
using Microsoft.EntityFrameworkCore;
using Optional;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Agenda.CQRS.UnitTests.Features.Participants.Handlers
{
    [Feature("Agenda")]
    [UnitTest]
    public class HandleGetOneParticipantInfoByIdQueryTests : IDisposable, IClassFixture<SqliteDatabaseFixture>
    {
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;
        private HandleGetOneParticipantInfoByIdQuery _sut;

        public HandleGetOneParticipantInfoByIdQueryTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture database)
        {
            DbContextOptionsBuilder<AgendaContext> optionsBuilder = new DbContextOptionsBuilder<AgendaContext>();
            optionsBuilder.UseSqlite(database.Connection);

            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(optionsBuilder.Options, (options) =>
            {
                AgendaContext context = new AgendaContext(options);
                context.Database.EnsureCreated();
                return context;
            });
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _sut = new HandleGetOneParticipantInfoByIdQuery(_uowFactory, _mapper);
        }

        public async void Dispose()
        {
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Participant>().Clear();
                
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _uowFactory = null;
            _mapper = null;
            _sut = null;
        }

        [Fact]
        public async Task GivenEmptyDataStore_Handle_Returns_None()
        {
            // Arrange
            GetOneParticipantInfoByIdQuery request = new GetOneParticipantInfoByIdQuery(Guid.NewGuid());

            // Act
            Option<ParticipantInfo> optionalParticipant = await _sut.Handle(request, cancellationToken: default)
                .ConfigureAwait(false);

            // Assert
            optionalParticipant.HasValue.Should().BeFalse();
        }

        [Fact]
        public async Task GivenRecordExistsInDataStore_Get_Returns_Some()
        {
            // Arrange
            Guid participantId = Guid.NewGuid();
            Participant participant = new Participant
            {
                UUID = participantId,
                Name = "Bruce Wayne"
            };

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Participant>().Create(participant);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            GetOneParticipantInfoByIdQuery request = new GetOneParticipantInfoByIdQuery(participantId);

            // Act
            Option<ParticipantInfo> optionalParticipant = await _sut.Handle(request, default)
                .ConfigureAwait(false);

            // Assert
            optionalParticipant.HasValue.Should().BeTrue($"the record <{participantId}> exists in the datastore");
            optionalParticipant.MatchSome((participantInfo) =>
            {
                participantInfo.Id.Should().Be(participant.UUID);
                participantInfo.Name.Should().Be(participant.Name);
                participantInfo.PhoneNumber.Should().Be(participant.PhoneNumber);
                participantInfo.Email.Should().Be(participant.Email);
            });
        }
    }
}
