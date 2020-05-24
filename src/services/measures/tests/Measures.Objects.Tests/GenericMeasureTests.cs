using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

using FluentAssertions;

using Measures.Objects;

using Xunit;

namespace Measures.Objects.UnitTests
{
    public class GenericMeasureTests
    {
        [Fact]
        public void Should_be_a_valid_measure()
        {
            // Act
            Type genericMeasureType = typeof(GenericMeasure);

            // Assert
            genericMeasureType.Should()
                              .NotBeAbstract().And
                              .NotHaveDefaultConstructor().And
                              .BeDerivedFrom<PhysiologicalMeasurement>().And
                              .HaveConstructor(new[] { typeof(Guid), typeof(Guid), typeof(DateTime), typeof(JsonDocument) }).And
                              .HaveProperty<JsonDocument>("Data").And
                              .HaveProperty<MeasureForm>("Form");
        }
    }
}
