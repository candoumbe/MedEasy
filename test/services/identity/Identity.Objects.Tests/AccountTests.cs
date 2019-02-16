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
        public static IEnumerable<object[]> AddOrUpdateClaimCases
        {
            get
            {
                {
                    DateTimeOffset utcNow = 12.December(2010);
                    yield return new object[]
                    {
                        new Account(),
                        (type : "create", value : "1",  start : utcNow, end :  (DateTimeOffset?)null),
                        ((Expression<Func<Account, bool>>)(account => account.Claims.Once()
                            && account.Claims.Once(userClaim => userClaim.Claim.Type == "create" && userClaim.Value == "1"
                                && userClaim.Start == 12.December(2010)
                                && userClaim.End == default
                            )

                        ))
                    };
                }

                {
                    DateTimeOffset utcNow = 12.December(2010);
                    Account account = new Account();
                    account.AddOrUpdateClaim(type: "create", value: "1", start: utcNow);
                    yield return new object[]
                    {
                        account,
                        (type : "create", value : "0",  start : utcNow, end :  (DateTimeOffset?)null),
                        ((Expression<Func<Account, bool>>)(acc => acc.Claims.Once()
                            && acc.Claims.Once(userClaim => userClaim.Value == "0"
                                && userClaim.Start == 12.December(2010)
                                && userClaim.End == default
                            )

                        ))
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
                {
                    DateTimeOffset utcNow = 12.December(2010);
                    yield return new object[]
                    {
                        new Account(),
                        "create",
                        ((Expression<Func<Account, bool>>)(account => !account.Claims.Any())),
                        "Account has no claim"
                    };
                }

                {
                    DateTimeOffset utcNow = 12.December(2010);
                    Account account = new Account();
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
                    Account account = new Account();
                    account.AddOrUpdateClaim(type: "create", value: "1", start: utcNow);
                    account.AddOrUpdateClaim(type: "delete", value: "1", start: utcNow);
                    yield return new object[]
                    {
                        account,
                        "create",
                        ((Expression<Func<Account, bool>>)(acc =>
                            acc.Claims.Count() == 1
                            && !acc.Claims.Any(uc => uc.Claim.Type == "create"))),
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
            Account account = new Account();
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
            Account account = new Account();

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
            Account account = new Account();

            // Act
            Action action = () => account.AddOrUpdateClaim(type: "claimType", value: "0", start: 2.January(2002), end: 1.January(2002));

            // Assert
            action.Should()
                .Throw<ArgumentOutOfRangeException>();
        }
    }
}
