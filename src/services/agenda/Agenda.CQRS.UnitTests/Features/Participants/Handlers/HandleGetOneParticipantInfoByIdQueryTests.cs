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
                uow.Repository<Attendee>().Clear();
                
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
            GetOneAttendeeInfoByIdQuery request = new GetOneAttendeeInfoByIdQuery(Guid.NewGuid());

            // Act
            Option<AttendeeInfo> optionalParticipant = await _sut.Handle(request, cancellationToken: default)
                .ConfigureAwait(false);

            // Assert
            optionalParticipant.HasValue.Should().BeFalse();
        }

        [Fact]
        public async Task GivenRecordExistsInDataStore_Get_Returns_Some()
        {
            // Arrange
            Guid attendeeId = Guid.NewGuid();
            Attendee attendee = new Attendee(id: attendeeId, name: "Bruce Wayne");

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Create(attendee);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            GetOneAttendeeInfoByIdQuery request = new GetOneAttendeeInfoByIdQuery(attendeeId);

            // Act
            Option<AttendeeInfo> optionalAttendee = await _sut.Handle(request, default)
                .ConfigureAwait(false);

            // Assert
            optionalAttendee.HasValue.Should()
                .BeTrue($"the record <{attendeeId}> exists in the datastore");
            optionalAttendee.MatchSome((attendeeInfo) =>
            {
                attendeeInfo.Id.Should().Be(attendee.Id);
                attendeeInfo.Name.Should().Be(attendee.Name);
                attendeeInfo.PhoneNumber.Should().Be(attendee.PhoneNumber);
                attendeeInfo.Email.Should().Be(attendee.Email);
            });
        }
    }
}
