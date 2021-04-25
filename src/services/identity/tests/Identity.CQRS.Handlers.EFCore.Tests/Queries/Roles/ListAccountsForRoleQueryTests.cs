using FluentAssertions;

using Identity.CQRS.Queries.Roles;
using Identity.DTO;
using Identity.Ids;

using MedEasy.CQRS.Core.Queries;

using Optional;

using System;
using System.Collections.Generic;

using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace Identity.CQRS.Handlers.EFCore.Tests.Queries.Roles
{
     [UnitTest]
     [Feature("Identity")]
    public class ListAccountsForRoleQueryTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ListAccountsForRoleQueryTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void IsValid() => typeof(ListAccountsForRoleQuery).Should()
                                                                .NotHaveDefaultConstructor().And
                                                                .NotBeStatic().And
                                                                .HaveConstructor(new[] { typeof(RoleId) }).And
                                                                .BeDerivedFrom<QueryBase<Guid, RoleId, Option<IEnumerable<AccountInfo>>>>();
    }
}
