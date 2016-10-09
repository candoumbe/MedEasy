using AutoMapper;
using FluentAssertions;
using MedEasy.API.Controllers;
using MedEasy.API.Filters;
using MedEasy.API.Stores;
using MedEasy.DTO;
using MedEasy.Handlers.Patient.Commands;
using MedEasy.Handlers.Patient.Queries;
using MedEasy.Mapping;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using static Moq.MockBehavior;

namespace MedEasy.API.Tests.Filters
{
    public class EnveloppeFilterAttributeTests
    {
        private Mock<IUrlHelperFactory> _urlHelperFactoryMock;
        private Mock<ILogger<PatientsController>> _loggerMock;
        private PatientsController _controller;
        private ITestOutputHelper _outputHelper;
        private IActionContextAccessor _actionContextAccessor;
        private Mock<IHandleGetOnePatientInfoByIdQuery> _iHandleGetOnePatientInfoByIdQueryMock;
        private Mock<IHandleGetManyPatientInfosQuery> _iHandleGetManyPatientInfoQueryMock;
        private EFUnitOfWorkFactory _factory;
        private IMapper _mapper;
        private Mock<IRunCreatePatientCommand> _iRunCreatePatientInfoCommandMock;
        private Mock<IRunDeletePatientByIdCommand> _iRunDeletePatientInfoByIdCommandMock;
        private Mock<IOptions<MedEasyApiOptions>> _apiOptionsMock;
        private Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo, TemperatureInfo>> _iRunAddNewTemperatureCommandMock;
        private Mock<IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo>> _iHandleGetOnePatientTemperatureMock;
        private Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo, BloodPressureInfo>> _iRunAddNewBloodPressureCommandMock;
        private Mock<IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo>> _iHandleGetOnePatientBloodPressureMock;
        

        public EnveloppeFilterAttributeTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
            _loggerMock = new Mock<ILogger<PatientsController>>(Strict);

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

            _iHandleGetOnePatientInfoByIdQueryMock = new Mock<IHandleGetOnePatientInfoByIdQuery>(Strict);
            _iHandleGetManyPatientInfoQueryMock = new Mock<IHandleGetManyPatientInfosQuery>(Strict);
            _iRunCreatePatientInfoCommandMock = new Mock<IRunCreatePatientCommand>(Strict);
            _iRunDeletePatientInfoByIdCommandMock = new Mock<IRunDeletePatientByIdCommand>(Strict);
            _iRunAddNewTemperatureCommandMock = new Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateTemperatureInfo, TemperatureInfo>>(Strict);
            _iRunAddNewBloodPressureCommandMock = new Mock<IRunAddNewPhysiologicalMeasureCommand<Guid, CreateBloodPressureInfo, BloodPressureInfo>>(Strict);
            _iHandleGetOnePatientTemperatureMock = new Mock<IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo>>(Strict);
            _iHandleGetOnePatientBloodPressureMock = new Mock<IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo>>(Strict);

            _apiOptionsMock = new Mock<IOptions<MedEasyApiOptions>>(Strict);
            

           

        }

        [Fact]
        public void OnResultExecuting_ForActionThatReturnsOneResource()
        {
            // Arrange
            PatientInfo resource = new PatientInfo
            {
                Id = 1,
                Firstname = "Bruce",
                Lastname = "Wayne"
            };
            BrowsableResource<PatientInfo> browsableResource = new BrowsableResource<PatientInfo>
            {
                Resource = resource,
                Location = new Link { Href = "url/to/resource", Rel = "self" }
            };
            ResultExecutingContext resultExecutingContext = new ResultExecutingContext(
                new ActionContext()
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData() ,
                    ActionDescriptor = new ActionDescriptor()
                    {
                        DisplayName = "Get"
                    }
                },
                new List<IFilterMetadata>(), new OkObjectResult(browsableResource), _controller);

            // Act
            EnvelopeFilterAttribute filter = new EnvelopeFilterAttribute();
            filter.OnResultExecuting(resultExecutingContext);

            //Assert
            resultExecutingContext.Result.Should()
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .BeAssignableTo<PatientInfo>();

            IHeaderDictionary headers = resultExecutingContext.HttpContext.Response.Headers;

            headers.Should().ContainKey("Link");
            headers["Link"].Should().ContainSingle();
            headers["Link"][0].Should().Be($@"{browsableResource.Location.Href}; rel={browsableResource.Location.Rel}");
            
            

        }

        [Fact]
        public void OnResultExecuting_ForActionThatReturnsPageOfResource()
        {
            // Arrange
            GenericPagedGetResponse<PatientInfo> page = new GenericPagedGetResponse<PatientInfo>(Enumerable.Empty<PatientInfo>(),
                first: "url/patients?page=2",
                previous: "url/patients?page=1",
                next: "url/patients?page=3",
                last: "url/patients?page=20"
                );
            
            ResultExecutingContext resultExecutingContext = new ResultExecutingContext(
                new ActionContext()
                {
                    HttpContext = new DefaultHttpContext(),
                    RouteData = new RouteData(),
                    ActionDescriptor = new ActionDescriptor()
                    {
                        DisplayName = "Get"
                    }
                },
                new List<IFilterMetadata>(), new OkObjectResult(page), _controller);

            // Act
            EnvelopeFilterAttribute filter = new EnvelopeFilterAttribute();
            filter.OnResultExecuting(resultExecutingContext);

            //Assert

            _apiOptionsMock.Verify();


            resultExecutingContext.Result.Should()
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .BeAssignableTo<IEnumerable<PatientInfo>>();

            IHeaderDictionary headers = resultExecutingContext.HttpContext.Response.Headers;

            headers.Should().ContainKey("Link");
            headers["Link"].Count.Should().Be(1);
            headers["Link"][0].Should()
                .Match($@"*{page.Links.First.Href}; rel={page.Links.First.Rel}*").And
                .Match($@"*{page.Links.Previous.Href}; rel={page.Links.Previous.Rel}*").And
                .Match($@"*{page.Links.Last.Href}; rel={page.Links.Last.Rel}*").And
                .Match($@"*{page.Links.Next.Href}; rel={page.Links.Next.Rel}*");

            headers.Should().ContainKey("X-Total-Count");
            headers["X-Total-Count"].Count.Should().Be(1);
            headers["X-Total-Count"][0].Should().Be($"{page.Count}");

        }
    }
}
