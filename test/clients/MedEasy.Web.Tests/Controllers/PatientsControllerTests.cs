using FluentAssertions;
using MedEasy.DTO;
using MedEasy.RestObjects;
using MedEasy.Web.Api;
using MedEasy.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;

namespace MedEasy.Web.Tests.Controllers
{
    public class PatientsControllerTests : IDisposable
    {
        private ITestOutputHelper _outputHelper;
        private Mock<IPatientInfoCrudApi> _patientsApiMock;
        private Mock<IModelMetadataProvider> _modelMetadataProviderMock;
        private PatientsController _controller;

        public PatientsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _patientsApiMock = new Mock<IPatientInfoCrudApi>(Strict);
            _modelMetadataProviderMock = new Mock<IModelMetadataProvider>(Strict);
            _controller = new PatientsController(_patientsApiMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _patientsApiMock = null;
            _controller = null;
            _modelMetadataProviderMock = null;
        }

        [Fact]
        public void Index()
        {
            // Act
            IActionResult actionResult = _controller.Index();

            // Assert
            actionResult.Should()
                .BeOfType<ViewResult>();

        }

        [Fact]
        public async Task List()
        {
            // Arrange
            _patientsApiMock.Setup(mock => mock.GetMany(It.IsAny<PaginationConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GenericPagedGetResponse<BrowsableResource<PatientInfo>>(Enumerable.Empty<BrowsableResource<PatientInfo>>()));
            DefaultModelBindingContext modelBindingContext = new DefaultModelBindingContext
            {
                
            };

            //_controller.ViewDataDictionary = new 
            
            // Act
            IActionResult actionResult = await _controller.List(1, 10);

            // Assert
            actionResult.Should()
                .BeOfType<ViewResult>().Which
                .Model.Should()
                .BeOfType<GenericPagedGetResponse<BrowsableResource<PatientInfo>>>();

        }



    }
}
