using FluentAssertions;
using FluentAssertions.Extensions;

using Measures.CQRS.Events.BloodPressures;
using Measures.DTO;

using NodaTime.Extensions;

using System.Collections.Generic;

using Xunit;
using Xunit.Categories;

namespace Measures.CQRS.UnitTests.Events
{
    [UnitTest]
    [Feature("Events")]
    public class BloodPressureCreatedTests
    {
        public BloodPressureCreatedTests()
        {

        }

        public static IEnumerable<object[]> EqualsCases
        {
            get
            {
                yield return new object[]
                {
                    new BloodPressureCreated(new BloodPressureInfo { DateOfMeasure = 1.January(2010).AsUtc().ToInstant(), SystolicPressure = 120, DiastolicPressure = 80 }),
                    null,
                    false,
                    "comparing to null always returns false"
                };

                {
                    BloodPressureCreated bloodPressureCreated = new(new BloodPressureInfo { DateOfMeasure = 1.January(2010).AsUtc().ToInstant(), SystolicPressure = 120, DiastolicPressure = 80 });
                    yield return new object[]
                    {
                        bloodPressureCreated,
                        bloodPressureCreated,
                        true,
                        "comparing instance to itself always returns true"
                    };
                }
                yield return new object[]
                {
                    new BloodPressureCreated(new BloodPressureInfo { DateOfMeasure = 1.January(2010).AsUtc().ToInstant(), SystolicPressure = 120, DiastolicPressure = 80 }),
                    new BloodPressureCreated(new BloodPressureInfo { DateOfMeasure = 1.January(2010).AsUtc().ToInstant(), SystolicPressure = 120, DiastolicPressure = 80 }),
                    true,
                    "comparing two instances with same data always returns true"
                };
            }
        }

        [Theory]
        [MemberData(nameof(EqualsCases))]
        public void TestEquals(BloodPressureCreated first, object second, bool expected, string reason)
        {
            // Act
            bool actual = first.Equals(second);

            // Assert
            actual.Should()
                .Be(expected, reason);
        }
    }
}
