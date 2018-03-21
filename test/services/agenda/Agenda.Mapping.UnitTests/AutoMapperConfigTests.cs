using System;
using Xunit;

namespace Agenda.Mapping.UnitTests
{
    public class AutoMapperConfigTests
    {

        [Fact]
        public void Mapping_Is_Valid() => AutoMapperConfig.Build().AssertConfigurationIsValid();
    }
}
