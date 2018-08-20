﻿using FluentAssertions;
using Identity.CQRS.Queries.Accounts;
using MedEasy.CQRS.Core.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Categories;

namespace Identity.CQRS.UnitTests.Queries.Accounts
{
    [UnitTest]
    [Feature("Query")]
    public class IsTenantQueryTests
    {
        [Fact]
        public void IsQuery() => typeof(IsTenantQuery)
            .Should()
            .Implement<IQuery<Guid, Guid, bool>>();
    }
}