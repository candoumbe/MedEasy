﻿namespace MedEasy.CQRS.Core.UnitTests.Events
{
    using FluentAssertions;

    using Measures.CQRS.Events;

    using MediatR;

    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Categories;

    [UnitTest]
    public class PatientCreatedTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public PatientCreatedTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void Is_Notification() => typeof(PatientCreated).Should()
                .BeAssignableTo<INotification>();
    }
}
