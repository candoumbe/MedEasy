namespace Identity.CQRS.UnitTests.Queries.Accounts;

using FluentAssertions;

using Identity.CQRS.Queries.Accounts;
using Identity.DTO;

using MedEasy.CQRS.Core.Queries;

using Optional;

using System;

using Xunit;
using Xunit.Categories;

[UnitTest]
public class GetOneAccountInfoByUsernameQueryTests
{
    [Fact]
    public void Should_be_command() => typeof(GetOneAccountInfoByUsernameQuery).Should()
                                                                               .BeDerivedFrom<QueryBase<Guid, string, Option<AccountInfo>>>().And
                                                                               .NotBeAbstract().And
                                                                               .NotHaveDefaultConstructor();
}
