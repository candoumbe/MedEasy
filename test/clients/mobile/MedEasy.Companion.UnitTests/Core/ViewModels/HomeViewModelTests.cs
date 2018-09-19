using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MedEasy.Mobile.Core.ViewModels;
using MedEasy.Mobile.Core.ViewModels.Base;
using Xunit;
using Xunit.Categories;

namespace MedEasy.Mobile.UnitTests.Core.ViewModels
{
    [UnitTest]
    [Feature("Mobile")]
    public class HomeViewModelTests
    {


        [Fact]
        public void IsViewModel() => typeof(ViewModelBase).IsAssignableFrom(typeof(HomeViewModel))
            .Should().BeTrue();
    }
}
