using Microsoft.AspNetCore.Mvc;
using MyTested.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MedEasy.Tests.Controllers
{
    public class PatientControllerTests
    {
        [Fact]
        public void ShouldReturnTheView()
        {
            MyMvc
                .Controller<PatientController>()
                .Calling(x => x.Index())
                .ShouldHave()
                .ActionAttributes(x => x.ContainingAttributeOfType<HttpGetAttribute>())

        }
    }
}
