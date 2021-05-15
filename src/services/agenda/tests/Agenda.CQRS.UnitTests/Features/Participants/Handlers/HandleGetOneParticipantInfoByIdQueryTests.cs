namespace Agenda.CQRS.UnitTests.Features.Participants.Handlers
{
    using Agenda.CQRS.Features.Participants.Handlers;
    using Agenda.CQRS.Features.Participants.Queries;
    using Agenda.DataStores;
    using Agenda.DTO;
    using Agenda.Ids;
    using Agenda.Mapping;
    using Agenda.Objects;

    using AutoMapper;

    using FluentAssertions;

    using MedEasy.DAL.EFStore;
    using MedEasy.DAL.Interfaces;
    using MedEasy.IntegrationTests.Core;

    using NodaTime;
    using NodaTime.Testing;

    using Optional;

    using System.Threading.Tasks;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [Feature("Agenda")]
    [UnitTest]
    public class HandleGetOneParticipantInfoByIdQueryTests : IAsyncLifetime, IClassFixture<SqliteEfCoreDatabaseFixture<AgendaContext>>
    {
        private IUnitOfWorkFactory _uowFactory;
        private IMapper _mapper;
        private HandleGetOneParticipantInfoByIdQuery _sut;

        public HandleGetOneParticipantInfoByIdQueryTests(ITestOutputHelper outputHelper, SqliteEfCoreDatabaseFixture<AgendaContext> database)
        {
            _uowFactory = new EFUnitOfWorkFactory<AgendaContext>(database.OptionsBuilder.Options, (options) =>
            {
                AgendaContext context = new(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _sut = new HandleGetOneParticipantInfoByIdQuery(_uowFactory, _mapper);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
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
            GetOneAttendeeInfoByIdQuery request = new(AttendeeId.New());

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
            AttendeeId attendeeId = AttendeeId.New();
            Attendee attendee = new(id: attendeeId, name: "Bruce Wayne");

            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Attendee>().Create(attendee);

                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            GetOneAttendeeInfoByIdQuery request = new(attendeeId);

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
