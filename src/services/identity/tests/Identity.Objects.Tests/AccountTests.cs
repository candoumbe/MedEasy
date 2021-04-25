using Bogus;

using FakeItEasy;

using FluentAssertions;
using FluentAssertions.Extensions;

using Identity.Ids;

using MedEasy.Ids;

using NodaTime;
using NodaTime.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using Xunit;
using Xunit.Categories;

namespace Identity.Objects.Tests
{
    [UnitTest]
    [Feature("Identity")]
    [Feature("Domains")]
    [Feature("Accounts")]
    public class AccountTests
    {
        private readonly Faker<Account> _accountFaker;

        public AccountTests()
        {
            _accountFaker = new Faker<Account>()
                    .CustomInstantiator((faker) => new Account(id: AccountId.New(),
                                                               username: faker.Internet.UserName(),
                                                               email: faker.Internet.Email(),
                                                               passwordHash: faker.Internet.Password(),
                                                               locked: faker.PickRandom(new[] { true, false }),
                                                               isActive: faker.PickRandom(new[] { true, false }),
                                                               salt: faker.Lorem.Word(),
                                                               tenantId: faker.PickRandom(new[] { TenantId.New(), default }),
                                                               refreshToken: faker.Lorem.Word()));
        }

        public static IEnumerable<object[]> AddOrUpdateClaimCases
        {
            get
            {
                Faker<Account> accountFaker = new Faker<Account>()
                    .CustomInstantiator((faker) => new Account(id: AccountId.New(),
                                                               username: faker.Internet.UserName(),
                                                               email: faker.Internet.Email(),
                                                               passwordHash: faker.Internet.Password(),
                                                               locked: faker.PickRandom(new[] { true, false }),
                                                               isActive: faker.PickRandom(new[] { true, false }),
                                                               salt: faker.Lorem.Word(),
                                                               tenantId: faker.PickRandom(new[] { TenantId.New(), default }),
                                                               refreshToken: faker.Lorem.Word()));
                {
                    Instant utcNow = 12.December(2010).AsUtc().ToInstant();
                    yield return new object[]
                    {
                        accountFaker.Generate(),
                        (type : "create", value : "1",  start : utcNow, end :  (Instant?)null),
                        (Expression<Func<Account, bool>>)(account => account.Claims.Exactly(1)
                                                                     && account.Claims.Once(ac => ac.Claim.Type == "create"
                                                                                                  && ac.Claim.Value == "1"
                                                                                                  && ac.Start == utcNow
                                                                                                  && ac.End == default
                                                                     )
                        )
                    };
                }

                {
                    Instant utcNow = 12.December(2010).AsUtc().ToInstant();
                    Account account = accountFaker.Generate();

                    account.AddOrUpdateClaim(type: "create", value: "1", start: utcNow);
                    yield return new object[]
                    {
                        account,
                        (type : "create", value : "0",  start : utcNow, end :  (Instant?)null),
                        (Expression<Func<Account, bool>>)(acc => acc.Claims.Once()
                                                                 && acc.Claims.Once(ac => ac.Claim.Value == "0"
                                                                                          && ac.Start == utcNow
                                                                                          && ac.End == default
                                                                 )
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(AddOrUpdateClaimCases))]
        public void AddOrUpdate(Account account,
                                (string type, string value, Instant start, Instant? end) claim,
                                Expression<Func<Account, bool>> accountExpectation)
        {
            // Act
            account.AddOrUpdateClaim(type: claim.type, value: claim.value, claim.start, claim.end);

            // Assert
            account.Should()
                .Match(accountExpectation);
        }

        public static IEnumerable<object[]> RemoveClaimCases
        {
            get
            {
                Faker<Account> accountFaker = new Faker<Account>()
                    .CustomInstantiator((faker) => new Account(id: AccountId.New(),
                                                               username: faker.Internet.UserName(),
                                                               email: faker.Internet.Email(),
                                                               passwordHash: faker.Internet.Password(),
                                                               locked: faker.PickRandom(new[] { true, false }),
                                                               isActive: faker.PickRandom(new[] { true, false }),
                                                               salt: faker.Lorem.Word(),
                                                               tenantId: faker.PickRandom(new[] { TenantId.New(), default }),
                                                               refreshToken: faker.Lorem.Word()));
                {
                    Instant utcNow = 12.December(2010).AsUtc().ToInstant();

                    yield return new object[]
                    {
                        accountFaker.Generate(),
                        "create",
                        (Expression<Func<Account, bool>>)(account => !account.Claims.Any()),
                        "Account has no claim"
                    };
                }

                {
                    Instant utcNow = 12.December(2010).AsUtc().ToInstant();
                    Account account = accountFaker.Generate();
                    account.AddOrUpdateClaim(type: "create", value: "1", start: utcNow);
                    yield return new object[]
                    {
                        account,
                        "create",
                        (Expression<Func<Account, bool>>)(acc => !acc.Claims.Any()),
                        "The corresponding claim must no longer exists"
                    };
                }

                {
                    Instant utcNow = 12.December(2010).AsUtc().ToInstant();
                    Account account = accountFaker;
                    account.AddOrUpdateClaim(type: "create", value: "1", start: utcNow);
                    account.AddOrUpdateClaim(type: "delete", value: "1", start: utcNow);
                    yield return new object[]
                    {
                        account,
                        "create",
                        (Expression<Func<Account, bool>>)(acc =>
                            acc.Claims.Exactly(1)
                            && acc.Claims.Exactly(ac => ac.Claim.Type == "create", 0)),
                        "The corresponding claim must no longer exists"
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(RemoveClaimCases))]
        public void RemoveClaim(Account account, string type, Expression<Func<Account, bool>> accountExpectation, string reason)
        {
            // Act
            account.RemoveClaim(type);

            // Assert
            account.Should()
                .Match(accountExpectation, reason);
        }

        [Fact]
        public void GivenNullParameter_RemoveClaim_Throws_ArgumentNullException()
        {
            // Arrange
            Account account = _accountFaker.Generate();
            account.AddOrUpdateClaim(type: "create", value: "1", start: 12.July(2003).AsUtc().ToInstant());

            // Act
            Action action = () => account.RemoveClaim(null);

            // Assert
            action.Should()
                .Throw<ArgumentNullException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GivenNullParameter_AddOrUpdateClaim_Throws_ArgumentNullException()
        {
            // Arrange
            Account account = _accountFaker.Generate();

            // Act
            Action action = () => account.AddOrUpdateClaim(type: null, value: "0", start: 12.April(1998).AsUtc().ToInstant(), end: 15.May(2008).AsUtc().ToInstant());

            // Assert
            action.Should()
                .Throw<ArgumentNullException>($"<{nameof(AccountClaim)}.{nameof(AccountClaim.Claim.Type)}> cannot be null").Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void AddOrUpdateClaim_Throws_ArgumentOutOfRangeException_when_end_gt_start()
        {
            // Arrange
            Account account = _accountFaker;

            // Act
            Action action = () => account.AddOrUpdateClaim(type: "claimType", value: "0", start: 2.January(2002).AsUtc().ToInstant(), end: 1.January(2002).AsUtc().ToInstant());

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Changing_TenantId_to_empty_throws_ArgumentNullException()
        {
            // Arrange
            Account account = _accountFaker;

            // Act
            Action action = () => account.OwnsBy(TenantId.Empty);

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void Add_role_enrolls_account_if_not_already_in_role()
        {
            // Arrange
            Account account = _accountFaker;

            Role adminRole = new(RoleId.New(), "admin");

            // Act
            account.AddRole(adminRole);

            // Assert
            account.Roles.Should()
                    .ContainSingle(ar => ar.RoleId == adminRole.Id);
        }
    }
}
