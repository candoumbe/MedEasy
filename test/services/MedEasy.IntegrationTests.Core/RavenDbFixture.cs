using Raven.Client.Documents;
using Raven.TestDriver;

namespace MedEasy.IntegrationTests.Core
{
    public class RavenDbFixture : RavenTestDriver
    {
        public IDocumentStore CreateStore() => GetDocumentStore();
    }
}
