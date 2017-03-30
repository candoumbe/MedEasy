using AutoMapper;
using FluentAssertions;
using MedEasy.API.Controllers;
using MedEasy.API.Filters;
using MedEasy.API.Stores;
using MedEasy.DTO;
using MedEasy.Handlers.Core.Patient.Commands;
using MedEasy.Handlers.Core.Patient.Queries;
using MedEasy.RestObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace MedEasy.API.Tests.Filters
{
    public class EnveloppeFilterAttributeTests
    {
        private PatientsController _controller;
        private ITestOutputHelper _outputHelper;
        private Mock<IHandleGetManyPatientInfosQuery> _iHandleGetManyPatientInfoQueryMock;
        private EFUnitOfWorkFactory _factory;
        private IMapper _mapper;
        private Mock<IRunCreatePatientCommand> _iRunCreatePatientInfoCommandMock;
        private Mock<IRunDeletePatientByIdCommand> _iRunDeletePatientInfoByIdCommandMock;
        private Mock<IOptions<MedEasyApiOptions>> _apiOptionsMock;
        private Mock<IHandleGetOnePhysiologicalMeasureQuery<TemperatureInfo>> _iHandleGetOnePatientTemperatureMock;
        private Mock<IHandleGetOnePhysiologicalMeasureQuery<BloodPressureInfo>> _iHandleGetOnePatientBloodPressureMock;
        

        public EnveloppeFilterAttributeTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void OnResultExecuting_ForActionThatReturnsOkObjectResult()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            PatientInfo resource = new PatientInfo
            {
                Id = patientId,
                Firstname = "Bruce",
                Lastname = "Wayne"
            };
            IBrowsableResource<PatientInfo> browsableResource = new BrowsableResource<PatientInfo>
            {
                Resource = resource,
                Links = new[] {
                    new Link { Href = "url/to/resource", Relation = "self"}
                }
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
            headers["Link"][0].Should().Be($@"<{browsableResource.Links.ElementAt(0).Href}>; rel=""{browsableResource.Links.ElementAt(0).Relation}""");
        }

        [Fact]
        public void OnResultExecuting_ForActionThatReturnsCreatedAtActionResult()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            PatientInfo resource = new PatientInfo
            {
                Id = patientId,
                Firstname = "Bruce",
                Lastname = "Wayne"
            };
            IBrowsableResource<PatientInfo> browsableResource = new BrowsableResource<PatientInfo>
            {
                Resource = resource,
                Links = new[] {
                    new Link { Href = "url/to/resource", Relation = "self" }
                }
            };
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
                new List<IFilterMetadata>(), new CreatedAtActionResult("Action", "Controller", new { }, browsableResource), _controller);

            // Act
            EnvelopeFilterAttribute filter = new EnvelopeFilterAttribute();
            filter.OnResultExecuting(resultExecutingContext);

            //Assert
            resultExecutingContext.Result.Should()
                .BeOfType<CreatedAtActionResult>().Which
                    .Value.Should()
                        .BeAssignableTo<PatientInfo>();

            IHeaderDictionary headers = resultExecutingContext.HttpContext.Response.Headers;

            headers.Should().ContainKey("Link");
            headers["Link"].Should().ContainSingle();
            headers["Link"][0].Should().Be($@"<{browsableResource.Links.ElementAt(0).Href}>; rel=""{browsableResource.Links.ElementAt(0).Relation}""");
        }

        [Fact]
        public void OnResultExecuting_ForActionThatReturnsCreatedAtActionResult_That_Contains_IBrowsableResource()
        {
            // Arrange
            Guid patientId = Guid.NewGuid();
            PatientInfo resource = new PatientInfo
            {
                Id = patientId,
                Firstname = "Bruce",
                Lastname = "Wayne"
            };
            IBrowsableResource<PatientInfo> browsableResource = new BrowsableResource<PatientInfo>
            {
                Resource = resource,
                Links = new[] {
                    new Link { Href = "url/to/resource", Relation = "self" }
                }
            };
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
                new List<IFilterMetadata>(), new CreatedAtActionResult("Action", "Controller", new { }, browsableResource), _controller);

            // Act
            EnvelopeFilterAttribute filter = new EnvelopeFilterAttribute();
            filter.OnResultExecuting(resultExecutingContext);

            //Assert
            resultExecutingContext.Result.Should()
                .BeOfType<CreatedAtActionResult>().Which
                    .Value.Should()
                        .BeAssignableTo<PatientInfo>();

            IHeaderDictionary headers = resultExecutingContext.HttpContext.Response.Headers;

            headers.Should().ContainKey("Link");
            headers["Link"].Should().ContainSingle();
            headers["Link"][0].Should().Be($@"<{browsableResource.Links.ElementAt(0).Href}>; rel=""{browsableResource.Links.ElementAt(0).Relation}""");
        }


        [Fact]
        public void OnResultExecuting_ForActionThatReturnsPageOfResource()
        {
            // Arrange
            IGenericPagedGetResponse<PatientInfo> page = new GenericPagedGetResponse<PatientInfo>(Enumerable.Empty<PatientInfo>(),
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
            resultExecutingContext.Result.Should()
                .BeOfType<OkObjectResult>().Which
                    .Value.Should()
                        .BeAssignableTo<IEnumerable<PatientInfo>>();

            IHeaderDictionary headers = resultExecutingContext.HttpContext.Response.Headers;

            headers.Should().ContainKey("Link");
            headers["Link"].Count.Should().Be(1);
            headers["Link"][0].Should()
                .Match($@"*<{page.Links.First.Href}>; rel=""{page.Links.First.Relation}""*").And
                .Match($@"*<{page.Links.Previous.Href}>; rel=""{page.Links.Previous.Relation}""*").And
                .Match($@"*<{page.Links.Last.Href}>; rel=""{page.Links.Last.Relation}""*").And
                .Match($@"*<{page.Links.Next.Href}>; rel=""{page.Links.Next.Relation}""*");

            headers.Should().ContainKey("X-Total-Count");
            headers["X-Total-Count"].Count.Should().Be(1);
            headers["X-Total-Count"][0].Should().Be($"{page.Count}");

        }
    }
}
