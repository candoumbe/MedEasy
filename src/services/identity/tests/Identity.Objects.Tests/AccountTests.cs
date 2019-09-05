using Bogus;
using FluentAssertions;
using FluentAssertions.Extensions;
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
        private Faker<Account> _accountFaker;

        public AccountTests()
        {
            _accountFaker = new Faker<Account>()
                    .CustomInstantiator((faker) => new Account(
                        id: Guid.NewGuid(),
                        username: faker.Internet.UserName(),
                        email: faker.Internet.Email(),
                        passwordHash: faker.Internet.Password(),
                        locked: faker.PickRandom(new[] { true, false }),
                        isActive: faker.PickRandom(new[] { true, false }),
                        salt: faker.Lorem.Word(),
                        tenantId: faker.PickRandom(new[] { Guid.NewGuid(), default }),
                        refreshToken: faker.Lorem.Word()
                    ));
        }

        public static IEnumerable<object[]> AddOrUpdateClaimCases
        {
            get
            {
                Faker<Account> accountFaker = new Faker<Account>()
                    .CustomInstantiator((faker) => new Account(
                        id: Guid.NewGuid(),
                        username: faker.Internet.UserName(),
                        email: faker.Internet.Email(),
                        passwordHash: faker.Internet.Password(),
                        locked: faker.PickRandom(new[] {true, false}),
                        isActive: faker.PickRandom(new[] {true, false}),
                        salt: faker.Lorem.Word(),
                        tenantId: faker.PickRandom(new[] {Guid.NewGuid(), default}),
                        refreshToken: faker.Lorem.Word()
                    ));
                {
                    DateTimeOffset utcNow = 12.December(2010);
                    yield return new object[]
                    {
                        accountFaker.Generate(),
                        (type : "create", value : "1",  start : utcNow, end :  (DateTimeOffset?)null),
                        (Expression<Func<Account, bool>>)(account => account.Claims.Once()
                            && account.Claims.Once(userClaim => userClaim.Key == "create" && userClaim.Value.value == "1"
                                && userClaim.Value.start == 12.December(2010)
                                && userClaim.Value.end == default
                            )
                        )
                    };
                }

                {
                    DateTimeOffset utcNow = 12.December(2010);
                    Account account = accountFaker.Generate();

                    account.AddOrUpdateClaim(type: "create", value: "1", start: utcNow);
                    yield return new object[]
                    {
                        account,
                        (type : "create", value : "0",  start : utcNow, end :  (DateTimeOffset?)null),
                        (Expression<Func<Account, bool>>)(acc => acc.Claims.Once()
                            && acc.Claims.Once(userClaim => userClaim.Value.value == "0"
                                && userClaim.Value.start == 12.December(2010)
                                && userClaim.Value.end == default
                            )
                        )
                    };
                }
            }
        }

        [Theory]
        [MemberData(nameof(AddOrUpdateClaimCases))]
        public void AddOrUpdate(Account account, (string type, string value, DateTimeOffset start, DateTimeOffset? end) claim, Expression<Func<Account, bool>> accountExpectation)
        {
            // Act
            account.AddOrUpdateClaim(type : claim.type,  value : claim.value, claim.start, claim.end);

            // Assert
            account.Should()
                .Match(accountExpectation);
        }

        public static IEnumerable<object[]> RemoveClaimCases
        {
            get
            {
                Faker<Account> accountFaker = new Faker<Account>()
                    .CustomInstantiator((faker) => new Account(
                        id: Guid.NewGuid(),
                        username: faker.Internet.UserName(),
                        email: faker.Internet.Email(),
                        passwordHash: faker.Internet.Password(),
                        locked: faker.PickRandom(new[] { true, false }),
                        isActive: faker.PickRandom(new[] { true, false }),
                        salt: faker.Lorem.Word(),
                        tenantId: faker.PickRandom(new[] { Guid.NewGuid(), default }),
                        refreshToken: faker.Lorem.Word()
                    ));
                {
                    DateTimeOffset utcNow = 12.December(2010);

                    yield return new object[]
                    {
                        accountFaker.Generate(),
                        "create",
                        ((Expression<Func<Account, bool>>)(account => !account.Claims.Any())),
                        "Account has no claim"
                    };
                }

                {
                    DateTimeOffset utcNow = 12.December(2010);
                    Account account = accountFaker.Generate();
                    account.AddOrUpdateClaim(type: "create", value: "1", start: utcNow);
                    yield return new object[]
                    {
                        account,
                        "create",
                        ((Expression<Func<Account, bool>>)(acc => !acc.Claims.Any())),
                        "The corresponding claim must no longer exists"
                    };
                }

                {
                    DateTimeOffset utcNow = 12.December(2010);
                    Account account = accountFaker;
                    account.AddOrUpdateClaim(type: "create", value: "1", start: utcNow);
                    account.AddOrUpdateClaim(type: "delete", value: "1", start: utcNow);
                    yield return new object[]
                    {
                        account,
                        "create",
                        (Expression<Func<Account, bool>>)(acc =>
                            acc.Claims.Count() == 1
                            && !acc.Claims.Any(uc => uc.Key == "create")),
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
            account.AddOrUpdateClaim(type: "create", value: "1", start: 12.July(2003));

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
            Action action = () => account.AddOrUpdateClaim(type: null, value: "0", start: 12.April(1998), end: 15.May(2008));

            // Assert
            action.Should()
                .Throw<ArgumentNullException>($"<{nameof(AccountClaim)}.{nameof(AccountClaim.Claim)}.{nameof(Claim.Type)}> cannot be null").Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GivenEndDateBeforeStartDate_AddOrUpdateClaim_Throws_ArgumentOutOfRangeException()
        {
            // Arrange
            Account account = _accountFaker.Generate();

            // Act
            Action action = () => account.AddOrUpdateClaim(type: "claimType", value: "0", start: 2.January(2002), end: 1.January(2002));

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Changing_TenantId_To_Empty_Throws_ArgumentNullException()
        {
            // Arrange
            Account account = _accountFaker.Generate();

            // Act
            Action action = () => account.SetTenant(Guid.Empty);

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>().Which
                .ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }
    }
}
