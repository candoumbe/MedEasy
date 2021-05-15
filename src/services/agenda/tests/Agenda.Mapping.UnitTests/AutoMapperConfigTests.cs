namespace Agenda.Mapping.UnitTests
{
    using Xunit;
    using Xunit.Categories;

    [UnitTest]
    [Feature("Agenda")]
    [Feature("Mapping")]
    public class AutoMapperConfigTests
    {
        [Fact]
        public void Mapping_Is_Valid() => AutoMapperConfig.Build().AssertConfigurationIsValid();
    }
}
