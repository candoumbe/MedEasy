using Xunit;
using MyTested.AspNetCore.Mvc;
using MedEasy.Web.Controllers;

namespace MedEasy.Tests.Controllers
{
    public class HomeControllerTests
    {

        [Fact]
        public void ShouldReturnsView()
        {
            MyMvc
                .Controller<HomeController>()
                .Calling(c => c.About())
                .ShouldReturn()
                .View();
        }

    }
}
