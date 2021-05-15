namespace MedEasy.IntegrationTests.Core
{
    using Raven.Client.Documents;
    using Raven.TestDriver;

    public class RavenDbFixture : RavenTestDriver
    {
        public IDocumentStore CreateStore() => GetDocumentStore();
    }
}
