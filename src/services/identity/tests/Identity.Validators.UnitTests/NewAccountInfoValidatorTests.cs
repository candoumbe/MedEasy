using Bogus;

using FluentAssertions;

using FluentValidation;

using Identity.DataStores;
using Identity.DTO;
using Identity.Objects;

using MedEasy.DAL.EFStore;
using MedEasy.DAL.Interfaces;
using MedEasy.IntegrationTests.Core;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using NodaTime;
using NodaTime.Testing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Identity.Validators.UnitTests
{
    [UnitTest]
    [Feature("Accounts")]
    public class NewAccountInfoValidatorTests : IAsyncLifetime, IClassFixture<SqliteDatabaseFixture>
    {
        private ITestOutputHelper _outputHelper;
        private IUnitOfWorkFactory _uowFactory;
        private Mock<ILogger<NewAccountInfoValidator>> _loggerMock;
        private NewAccountInfoValidator _sut;

        public NewAccountInfoValidatorTests(ITestOutputHelper outputHelper, SqliteDatabaseFixture databaseFixture)
        {
            _outputHelper = outputHelper;
            DbContextOptionsBuilder<IdentityContext> dbContextOptionsBuilder = new();
            dbContextOptionsBuilder.UseInMemoryDatabase($"{Guid.NewGuid()}");
            _uowFactory = new EFUnitOfWorkFactory<IdentityContext>(dbContextOptionsBuilder.Options, (options) =>
            {
                IdentityContext context = new IdentityContext(options, new FakeClock(new Instant()));
                context.Database.EnsureCreated();
                return context;
            });
            _loggerMock = new Mock<ILogger<NewAccountInfoValidator>>();

            _sut = new NewAccountInfoValidator(_uowFactory, _loggerMock.Object);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            using IUnitOfWork uow = _uowFactory.NewUnitOfWork();
            uow.Repository<Account>().Clear();
            await uow.SaveChangesAsync()
                     .ConfigureAwait(false);
        }

        [Fact]
        public void IsNewAccountValidator() => typeof(NewAccountInfoValidator).Should()
            .Implement<IValidator<NewAccountInfo>>();

        public static IEnumerable<object[]> ValidationCases
        {
            get
            {
                Faker faker = new();
                yield return new object[]
                {
                    Enumerable.Empty<Account>(),
                    new NewAccountInfo(),
                    (Expression<Func<ValidationResult, bool>>)(vr =>
                        vr.Errors.Count == 4
                        && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.Email))
                        && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.Password))
                        && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.ConfirmPassword))
                        && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.Username))

                    ),
                    "No property set"
                };

                {
                    Account account = new Account(id: Guid.NewGuid(),
                                                  username: faker.Person.UserName,
                                                  passwordHash: faker.Lorem.Word(),
                                                  salt: faker.Lorem.Word(),
                                                  email: faker.Internet.Email("joker"));
                    yield return new object[]
                    {
                        new[] {account},
                        new NewAccountInfo
                        {
                            Name = faker.Person.Company.Name,
                            Email = "joker@card-city.com",
                            Password = "smile",
                            ConfirmPassword = "smile",
                            Username = account.Username
                        },
                        (Expression<Func<ValidationResult, bool>>)(vr =>
                            vr.Errors.Count == 1
                            && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.Username))

                        ),
                        "An account with the same username already exists"
                    };
                }

                {
                    Account account = new Account
                    (
                        username: "joker",
                        passwordHash: faker.Lorem.Word(),
                        salt: faker.Lorem.Word(),
                        email: faker.Internet.Email("joker"),
                        id: Guid.NewGuid()
                    );
                    yield return new object[]
                    {
                        new[] {account},
                        new NewAccountInfo
                        {
                            Name = faker.Person.Company.Name,
                            Email = "joker@card-city.com",
                            Password = "smile",
                            ConfirmPassword = "smiles",
                            Username = faker.Person.UserName
                        },
                        (Expression<Func<ValidationResult, bool>>)(vr =>
                            vr.Errors.Count == 1
                            && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.ConfirmPassword))

                        ),
                        $"{nameof(NewAccountInfo.Password)} && {nameof(NewAccountInfo.ConfirmPassword)} don't match"
                    };
                }

                {
                    Account account = new Account
                    (
                        username: "joker",
                        passwordHash: faker.Lorem.Word(),
                        salt: faker.Lorem.Word(),
                        email: "joker@card-city.com",
                        id: Guid.NewGuid()
                    );

                    yield return new object[]
                    {
                        new[] {account},
                        new NewAccountInfo
                        {
                            Name = faker.Person.Company.Name,
                            Email = account.Email,
                            Password = "smile",
                            ConfirmPassword = "smile",
                            Username = faker.Person.UserName
                        },
                        (Expression<Func<ValidationResult, bool>>)(vr =>
                            vr.Errors.Count == 1
                            && vr.Errors.Once(err => err.PropertyName == nameof(NewAccountInfo.Email))

                        ),
                        $"An account with the same {nameof(NewAccountInfo.Email)} already exists"
                    };


                    yield return new object[]
                    {
                        Enumerable.Empty<Account>(),
                        new NewAccountInfo
                        {
                            Name = "The dark knight",
                            Email = "batman@gotham.fr",
                            Password = "smile",
                            ConfirmPassword = "smile",
                            Username = "capedcrusader"
                        },
                        (Expression<Func<ValidationResult, bool>>)(vr => vr.Errors.Count == 0),
                        "Informations are ok"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(ValidationCases))]
        public async Task ValidateTests(IEnumerable<Account> accounts, NewAccountInfo newAccountInfo, Expression<Func<ValidationResult, bool>> validationResultExpectation, string reason)
        {
            // Arrange
            using (IUnitOfWork uow = _uowFactory.NewUnitOfWork())
            {
                uow.Repository<Account>().Create(accounts);
                await uow.SaveChangesAsync()
                    .ConfigureAwait(false);
            }

            _outputHelper.WriteLine($"Accounts in datastore : {accounts.Jsonify()}");
            _outputHelper.WriteLine($"NewAccount : {newAccountInfo.Jsonify()}");

            // Act
            ValidationResult vr = await _sut.ValidateAsync(newAccountInfo, default)
                .ConfigureAwait(false);

            // Assert
            _outputHelper.WriteLine($"Validation results : {vr.Jsonify()}");
            vr.Should()
                .Match(validationResultExpectation, reason);

        }
    }
}
