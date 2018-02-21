using FluentAssertions;
using FluentAssertions.Extensions;
using Measures.CQRS.Events;
using MediatR;
using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Categories;

namespace MedEasy.CQRS.Core.UnitTests.Events
{
    [UnitTest]
    public class PatientCreatedTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public PatientCreatedTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }
       
        [Fact]
        public void Is_Notification()
        {
            PatientCreated @event = new PatientCreated(patientId: Guid.NewGuid(), firstname: "Bruce", lastname: "Wayne", dateOfBirth: 3.July(1967));

            @event.Should()
                .BeAssignableTo<INotification>();
        }
    }
}
