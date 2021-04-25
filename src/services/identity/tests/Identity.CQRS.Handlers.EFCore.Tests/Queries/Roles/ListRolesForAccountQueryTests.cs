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

namespace Identity.CQRS.Handlers.EFCore.Tests.Queries.Roles
{
    public class ListRolesForAccountQueryTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public ListRolesForAccountQueryTests(ITestOutputHelper outputHelper)
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
