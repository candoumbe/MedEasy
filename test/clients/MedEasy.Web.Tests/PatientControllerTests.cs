using FluentAssertions;
using MedEasy.Web.Controllers;
using MedEasy.Web.Models;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;

namespace MedEasy.Web.Tests
{
    public class PatientControllerTests : IDisposable
    {

        private PatientController _controller;
        private ITestOutputHelper _outputHelper;
        private Mock<IOptionsSnapshot<MedEasyAPIOptions>> _apiOptionsMock;

        /// <summary>
        /// Builds a new <see cref="PatientControllerTests"/>
        /// </summary>
        /// <param name="outputHelper"></param>
        public PatientControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _apiOptionsMock = new Mock<IOptionsSnapshot<MedEasyAPIOptions>>(Strict);
            _controller = new PatientController(_apiOptionsMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _controller = null;
        }


        [Fact]
        public void CtorShouldThrowArgumentNullException()
        {
            Action action = () => new PatientController(null);

            action.ShouldThrow<ArgumentNullException>()
                .Which.ParamName.Should()
                .NotBeNullOrWhiteSpace();
        }


        [Fact]
        public void Index()
        {

        }
    }
}
