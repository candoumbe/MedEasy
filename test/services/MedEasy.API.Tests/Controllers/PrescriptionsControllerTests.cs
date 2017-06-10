using AutoMapper;
using FluentAssertions;
using MedEasy.API.Controllers;
using MedEasy.API.Stores;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Exceptions;
using MedEasy.Mapping;
using MedEasy.Objects;
using MedEasy.RestObjects;
using MedEasy.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.API.Tests.Controllers
{
    public class PrescriptionsControllerTests : IDisposable
    {
        private Mock<IOptions<MedEasyApiOptions>> _apiOptionsMock;
        private PrescriptionsController _controller;
        private IUnitOfWorkFactory _factory;
        private Mock<ILogger<PrescriptionsController>> _loggerMock;
        private IMapper _mapper;
        private ITestOutputHelper _outputHelper;
        private Mock<IPrescriptionService> _prescriptionServiceMock;
        private Mock<IUrlHelper> _urlHelperMock;

        public PrescriptionsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _prescriptionServiceMock = new Mock<IPrescriptionService>(Strict);

            _loggerMock = new Mock<ILogger<PrescriptionsController>>(Strict);
            _urlHelperMock = new Mock<IUrlHelper>(Strict);
            _urlHelperMock.Setup(mock => mock.Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext urlContext) => $"api/{urlContext.Controller}/{urlContext.Action}?{(urlContext.Values == null ? string.Empty : $"{urlContext.Values?.ToQueryString()}")}");

            DbContextOptionsBuilder<MedEasyContext> dbOptions = new DbContextOptionsBuilder<MedEasyContext>();
            dbOptions.UseInMemoryDatabase($"InMemoryMedEasyDb_{Guid.NewGuid()}");
            _factory = new EFUnitOfWorkFactory(dbOptions.Options);
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _apiOptionsMock = new Mock<IOptions<MedEasyApiOptions>>(Strict);


            _controller = new PrescriptionsController(_loggerMock.Object, _apiOptionsMock.Object, _urlHelperMock.Object, _prescriptionServiceMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _prescriptionServiceMock = null;
            _loggerMock = null;
            _factory = null;
            _mapper = null;
            _apiOptionsMock = null;
            _controller = null;

        }


        [Fact]
        public void CheckEndpointName() => PrescriptionsController.EndpointName.Should().Be(nameof(PrescriptionsController).Replace("Controller", string.Empty));


        [Fact]
        public async Task Get()
        {
            Guid prescriptionId = Guid.NewGuid();
            Guid patientId = Guid.NewGuid();
            //Arrange
            _prescriptionServiceMock.Setup(mock => mock.GetOnePrescriptionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PrescriptionHeaderInfo { Id = prescriptionId, PatientId = patientId, DeliveryDate = DateTimeOffset.UtcNow })
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.Get(prescriptionId);

            //Assert
            IBrowsableResource<PrescriptionHeaderInfo> result = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeAssignableTo<IBrowsableResource<PrescriptionHeaderInfo>>().Which;

            PrescriptionHeaderInfo resource = result.Resource;
            IEnumerable<Link> links = result.Links;

            links.Should()
                .NotBeNull().And
                .Contain(x => x.Relation.Contains("self")).And
                .Contain(x => x.Relation.Contains(nameof(Prescription.Items)));

            Link location = links.Single(x => x.Relation.Contains("self"));
            location.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{PrescriptionsController.EndpointName}/{nameof(PrescriptionsController.Get)}?{nameof(resource.Id)}={resource.Id}");

            Link locationItems = links.Single(x => x.Relation.Contains(nameof(Prescription.Items)));
            locationItems.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{PrescriptionsController.EndpointName}/{nameof(PrescriptionsController.Details)}?{nameof(resource.Id)}={resource.Id}");


            resource.Should().NotBeNull();
            resource.Id.Should().Be(prescriptionId);
            resource.PatientId.Should().Be(patientId);

            _prescriptionServiceMock.Verify();
            _urlHelperMock.Verify();

        }


        public static IEnumerable<object[]> DetailsCases
        {
            get
            {
                {
                    Guid prescriptionId = Guid.NewGuid();
                    yield return new object[] {
                        new [] {
                            new Prescription
                            {
                                Id = 1,
                                PatientId = 1,
                                UUID = prescriptionId,
                                Items = new []
                                {
                                    new PrescriptionItem { Id = 1, Code = "DRUG", Quantity = 1, Designation = "Doliprane" }
                                }
                            }
                        },
                        prescriptionId,
                        ((Expression<Func<IActionResult, bool>>)(x => x != null &&
                            x is OkObjectResult && ((OkObjectResult) x).Value is IBrowsableResource<IEnumerable<PrescriptionItemInfo>> &&
                            ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Resource.Count() == 1 &&
                            ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Links != null &&
                            ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Links.Count(link => link.Relation.Contains("self")) == 1))
                    };
                }
                {
                    Guid prescriptionId = Guid.NewGuid();
                    yield return new object[] {
                        new [] {
                            new Prescription
                            {
                                Id = 1,
                                PatientId = 1,
                                UUID = prescriptionId,
                                Items = Enumerable.Empty<PrescriptionItem>().ToList()
                            }
                        },
                        prescriptionId,
                        ((Expression<Func<IActionResult, bool>>)(x => x != null &&
                            x is OkObjectResult && ((OkObjectResult) x).Value is IBrowsableResource<IEnumerable<PrescriptionItemInfo>> &&
                            ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Resource.Count() == 0 &&
                            ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Links != null &&
                            ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Links.Count(link => link.Relation.Contains("self")) == 1))
                    };  
                }
            }
        }

        /// <summary>
        /// Tests the <see cref="PrescriptionsController.Details(Guid)"/> method
        /// </summary>
        /// <param name="prescriptions">Current <see cref="Prescription"/> repository state</param>
        /// <param name="id">id of the prescription</param>
        /// <param name="resultExpectation">Expected result expression</param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(DetailsCases))]
        public async Task GetDetails(IEnumerable<Prescription> prescriptions, Guid id, Expression<Func<IActionResult, bool>> resultExpectation)
        {

            // Arrange
            _prescriptionServiceMock.Setup(mock => mock.GetItemsByPrescriptionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Returns((Guid prescriptionId, CancellationToken cancellationToken) => Task.Run(() =>
                {
                    IEnumerable<PrescriptionItem> items = prescriptions
                        .Single(x => x.UUID == prescriptionId).Items;

                    return _mapper.Map<IEnumerable<PrescriptionItem>, IEnumerable<PrescriptionItemInfo>>(items);

                }));

            // Act
            _outputHelper.WriteLine($"Current store : {SerializeObject(prescriptions)}");
            _outputHelper.WriteLine($"ID : {id}");

            IActionResult actionResult = await _controller.Details(id);

            // Assert
            actionResult.Should().NotBeNull().And
                .Match(resultExpectation);


            _prescriptionServiceMock.Verify(mock => mock.GetItemsByPrescriptionIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void GetDetailsShouldNotSwallowNotFoundException()
        {

            // Arrange
            Exception expectedException = new NotFoundException($"No prescription found");
            _prescriptionServiceMock.Setup(mock => mock.GetItemsByPrescriptionIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .Throws(expectedException);

            // Act
            Func<Task> action = async () => await _controller.Details(Guid.NewGuid());


            // Assert
            action.ShouldThrow<NotFoundException>()
                .Which.Message.Should()
                .Be(expectedException.Message);

        }

    }
}
