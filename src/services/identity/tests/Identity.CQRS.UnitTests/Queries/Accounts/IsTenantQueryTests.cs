namespace Identity.CQRS.UnitTests.Queries.Accounts
{
    using FluentAssertions;

    using Identity.CQRS.Queries.Accounts;

    using MedEasy.CQRS.Core.Queries;
    using MedEasy.Ids;

    using System;

    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Query")]
    public class IsTenantQueryTests
    {
        [Fact]
        public void IsQuery() => typeof(IsTenantQuery).Should()
                                                      .Implement<IQuery<Guid, TenantId, bool>>();
    }
}
