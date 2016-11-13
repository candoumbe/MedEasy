using AutoMapper;
using FluentAssertions;
using MedEasy.API.Controllers;
using MedEasy.API.Stores;
using MedEasy.DAL.Interfaces;
using MedEasy.DTO;
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
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;
using static Newtonsoft.Json.JsonConvert;

namespace MedEasy.API.Tests.Controllers
{
    public class PrescriptionsControllerTests : IDisposable
    {
        private IActionContextAccessor _actionContextAccessor;
        private Mock<IOptions<MedEasyApiOptions>> _apiOptionsMock;
        private PrescriptionsController _controller;
        private IUnitOfWorkFactory _factory;
        private Mock<ILogger<PrescriptionsController>> _loggerMock;
        private IMapper _mapper;
        private ITestOutputHelper _outputHelper;
        private Mock<IPrescriptionService> _prescriptionServiceMock;
        private Mock<IUrlHelperFactory> _urlHelperFactoryMock;

        public PrescriptionsControllerTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _prescriptionServiceMock = new Mock<IPrescriptionService>(Strict);

            _loggerMock = new Mock<ILogger<PrescriptionsController>>(Strict);
            _urlHelperFactoryMock = new Mock<IUrlHelperFactory>(Strict);
            _urlHelperFactoryMock.Setup(mock => mock.GetUrlHelper(It.IsAny<ActionContext>()).Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext urlContext) => $"api/{urlContext.Controller}/{urlContext.Action}?{(urlContext.Values == null ? string.Empty : $"{urlContext.Values?.ToQueryString()}")}");

            _actionContextAccessor = new ActionContextAccessor()
            {
                ActionContext = new ActionContext()
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            DbContextOptionsBuilder<MedEasyContext> dbOptions = new DbContextOptionsBuilder<MedEasyContext>();
            dbOptions.UseInMemoryDatabase($"InMemoryMedEasyDb_{Guid.NewGuid()}");
            _factory = new EFUnitOfWorkFactory(dbOptions.Options);
            _mapper = AutoMapperConfig.Build().CreateMapper();

            _apiOptionsMock = new Mock<IOptions<MedEasyApiOptions>>(Strict);


            _controller = new PrescriptionsController(_loggerMock.Object, _apiOptionsMock.Object, _urlHelperFactoryMock.Object, _actionContextAccessor, _prescriptionServiceMock.Object);
        }

        public void Dispose()
        {
            _outputHelper = null;
            _prescriptionServiceMock = null;
            _loggerMock = null;
            _actionContextAccessor = null;
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
            //Arrange
            _urlHelperFactoryMock.Setup(mock => mock.GetUrlHelper(It.IsAny<ActionContext>()).Action(It.IsAny<UrlActionContext>()))
                .Returns((UrlActionContext urlContext) => $"api/{urlContext.Controller}/{urlContext.Action}?{(urlContext.Values == null ? string.Empty : $"{urlContext.Values?.ToQueryString()}")}");

            _prescriptionServiceMock.Setup(mock => mock.GetOnePrescriptionAsync(It.IsAny<int>()))
                .ReturnsAsync(new PrescriptionHeaderInfo { Id = 1, PatientId = 1, DeliveryDate = DateTimeOffset.UtcNow })
                .Verifiable();

            //Act
            IActionResult actionResult = await _controller.Get(1);

            //Assert
            IBrowsableResource<PrescriptionHeaderInfo> result = actionResult.Should()
                .NotBeNull().And
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                    .BeAssignableTo<IBrowsableResource<PrescriptionHeaderInfo>>().Which;

            IEnumerable<Link> links = result.Links;

            links.Should()
                .NotBeNull().And
                .Contain(x => x.Rel == "self");

            Link location = links.Single(x => x.Rel == "self");
            location.Href.Should()
                .NotBeNullOrWhiteSpace().And
                .BeEquivalentTo($"api/{PrescriptionsController.EndpointName}/{nameof(PrescriptionsController.Get)}?{nameof(PrescriptionHeaderInfo.Id)}=1");
            location.Rel.Should()
                .NotBeNullOrWhiteSpace()
                .And.BeEquivalentTo("self");

            PrescriptionHeaderInfo resource = result.Resource;
            resource.Should().NotBeNull();
            resource.Id.Should().Be(1);
            resource.PatientId.Should().Be(1);
            
            _prescriptionServiceMock.Verify();
            _urlHelperFactoryMock.Verify();

        }

        
        public static IEnumerable<object[]> DetailsCases
        {
            get
            {
                yield return new object[] {
                    Enumerable.Empty<Prescription>(),
                    1,
                    ((Expression<Func<IActionResult, bool>>)(x => x != null && x is NotFoundResult))
                };

                yield return new object[] {
                    new [] {
                        new Prescription
                        {
                            Id = 1,
                            PatientId = 1,
                            Items = new []
                            {
                                new PrescriptionItem { Id = 1, Code = "DRUG", Quantity = 1, Designation = "Doliprane" }
                            }
                        }
                    },
                    1,
                    ((Expression<Func<IActionResult, bool>>)(x => x != null && 
                        x is OkObjectResult && ((OkObjectResult) x).Value is IBrowsableResource<IEnumerable<PrescriptionItemInfo>> &&
                        ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Resource.Count() == 1 &&
                        ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Links != null &&
                        ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Links.Count(link => link.Rel == "self") == 1))
                };

                yield return new object[] {
                    new [] {
                        new Prescription
                        {
                            Id = 1,
                            PatientId = 1,
                            Items = Enumerable.Empty<PrescriptionItem>().ToList()
                        }
                    },
                    1,
                    ((Expression<Func<IActionResult, bool>>)(x => x != null &&
                        x is OkObjectResult && ((OkObjectResult) x).Value is IBrowsableResource<IEnumerable<PrescriptionItemInfo>> &&
                        ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Resource.Count() == 0 &&
                        ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Links != null &&
                        ((IBrowsableResource<IEnumerable<PrescriptionItemInfo>>)((OkObjectResult) x).Value).Links.Count(link => link.Rel == "self") == 1))
                };

                yield return new object[] {
                    new [] {
                        new Prescription
                        {
                            Id = 3,
                            PatientId = 1,
                            Items = new []
                            {
                                new PrescriptionItem { Id = 1, Code = "DRUG", Quantity = 1, Designation = "Doliprane" }
                            }
                        }
                    },
                    1,
                     ((Expression<Func<IActionResult, bool>>)(x => x != null && x is NotFoundResult))
                };
            }
        }

        /// <summary>
        /// Tests the <see cref="PrescriptionsController.Details(int)"/> method
        /// </summary>
        /// <param name="prescriptions">Current <see cref="Prescription"/> repository state</param>
        /// <param name="id">id of the prescription</param>
        /// <param name="resultExpectation">Expected result expression</param>
        /// <returns></returns>
        [Theory]
        [MemberData(nameof(DetailsCases))]
        public async Task GetDetails(IEnumerable<Prescription> prescriptions, int id, Expression<Func<IActionResult, bool>> resultExpectation)
        {



            // Arrange
            _prescriptionServiceMock.Setup(mock => mock.GetPrescriptionWithDetailsAsync(It.IsAny<int>()))
                .Returns((int prescriptionId) => Task.Run(() =>
                {
                    return prescriptions
                        .Select(_mapper.ConfigurationProvider.ExpressionBuilder.CreateMapExpression<Prescription, PrescriptionInfo>().Compile())
                        .SingleOrDefault(x => x.Id == prescriptionId);
                }));

            // Act
            _outputHelper.WriteLine($"Current store : {SerializeObject(prescriptions)}");
            _outputHelper.WriteLine($"ID : {id}");

            IActionResult actionResult = await _controller.Details(id);

            // Assert
            actionResult.Should().NotBeNull().And
                .Match(resultExpectation);

            
            _prescriptionServiceMock.Verify(mock => mock.GetPrescriptionWithDetailsAsync(id), Times.Once);
        }

    }
}
