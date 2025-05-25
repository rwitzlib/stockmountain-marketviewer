using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using MarketViewer.Contracts.Models;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq.AutoMock;
using MarketViewer.Api.Controllers.Market;
using MarketViewer.Contracts.Requests.Market;
using MarketViewer.Contracts.Responses.Market;

namespace MarketViewer.Api.UnitTests.Controllers
{
    public class AggregateControllerUnitTests
    {
        #region Private Fields
        private StocksController _classUnderTest;
        private AutoMocker _autoMocker;
        private Fixture _autoFixture;
        private Mock<IMediator> _mediator;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<ILogger<StocksController>> _logger;
        #endregion

        #region Constructor
        public AggregateControllerUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<StocksController>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(q => q.HttpContext.Items["UserId"]).Returns(_autoFixture.Create<string>());

            _classUnderTest = new StocksController(_mockHttpContextAccessor.Object, _logger.Object, _mediator.Object);
        }
        #endregion

        [Fact]
        public async Task HandleAggregateRequest_Returns_OK_Response()
        {
            // Arrange
            var request = _autoFixture.Create<StocksRequest>();

            _mediator.Setup(q => q.Send(It.IsAny<StocksRequest>(), default))
                .ReturnsAsync(new OperationResult<StocksResponse>
                {
                    Status = HttpStatusCode.OK,
                    Data = _autoFixture.Create<StocksResponse>()
                });

            // Act
            var response = await _classUnderTest.HandleAggregateRequest(request);

            // Assert
            var result = Assert.IsType<OkObjectResult>(response);
            Assert.IsType<StocksResponse>(result.Value);
        }

        [Fact]
        public async Task HandleAggregateRequest_Returns_BadRequest_Response()
        {
            // Arrange
            var request = _autoFixture.Create<StocksRequest>();

            _mediator.Setup(q => q.Send(It.IsAny<StocksRequest>(), default))
                .ReturnsAsync(new OperationResult<StocksResponse>
                {
                    Status = HttpStatusCode.BadRequest,
                    ErrorMessages = _autoFixture.Create<List<string>>()
                });

            // Act
            var response = await _classUnderTest.HandleAggregateRequest(request);

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(response);
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().BeOfType<List<string>>().Which.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task HandleAggregateRequest_Returns_InternalServerError_Response()
        {
            // Arrange
            var request = _autoFixture.Create<StocksRequest>();

            _mediator.Setup(q => q.Send(It.IsAny<StocksRequest>(), default))
                .ReturnsAsync(new OperationResult<StocksResponse>
                {
                    Status = HttpStatusCode.InternalServerError,
                    ErrorMessages = _autoFixture.Create<List<string>>()
                });

            // Act
            var response = await _classUnderTest.HandleAggregateRequest(request);

            // Assert
            var result = Assert.IsType<ObjectResult>(response);
            result.StatusCode.Value.Should().Be((int)HttpStatusCode.InternalServerError);
            result.Value.Should().BeOfType<List<string>>().Which.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task HandleAggregateRequest_Returns_InternalServerError()
        {
            // Arrange
            var request = _autoFixture.Create<StocksRequest>();

            _mediator.Setup(q => q.Send(It.IsAny<StocksRequest>(), default))
                .ThrowsAsync(new Exception());

            // Act
            var response = await _classUnderTest.HandleAggregateRequest(request);

            // Assert
            var result = Assert.IsType<ObjectResult>(response);
            result.StatusCode.Value.Should().Be((int)HttpStatusCode.InternalServerError);
            result.Value.Should().BeOfType<List<string>>().Which.Should().NotBeNullOrEmpty();
        }
    }
}
