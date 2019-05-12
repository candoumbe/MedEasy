using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.TestDriver;

namespace MedEasy.IntegrationTests.Core
{
    public class RavenDbFixture : RavenTestDriver
    {
        public IDocumentStore CreateStore() => GetDocumentStore();
    }
}
