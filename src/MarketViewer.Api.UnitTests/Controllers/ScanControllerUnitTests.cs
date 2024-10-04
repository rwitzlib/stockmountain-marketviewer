using AutoFixture;
using MarketViewer.Api.Controllers;
using MarketViewer.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using MarketViewer.Contracts.Models;
using Xunit;
using FluentAssertions;
using MarketViewer.Contracts.Requests.Scan;

namespace MarketViewer.Api.UnitTests.Controllers
{
    public class ScanControllerUnitTests
    {
        #region Private Fields
        private ScanController _classUnderTest;
        private Fixture _autoFixture;
        private Mock<IMediator> _mediator;
        private Mock<ILogger<ScanController>> _logger;
        #endregion

        #region Constructor
        public ScanControllerUnitTests()
        {
            _autoFixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<ScanController>>();

            _classUnderTest = new ScanController(_logger.Object, _mediator.Object);
        }
        #endregion

        [Fact]
        public async Task HandleScanRequest_Returns_OK_Response()
        {
            // Arrange
            var request = _autoFixture.Create<ScanRequest>();

            _mediator.Setup(q => q.Send(It.IsAny<ScanRequest>(), default))
                .ReturnsAsync(new OperationResult<ScanResponse>
                {
                    Status = HttpStatusCode.OK,
                    Data = _autoFixture.Create<ScanResponse>()
                });

            // Act
            var response = await _classUnderTest.HandleScanRequest(request);

            // Assert
            var result = Assert.IsType<OkObjectResult>(response);
            result.Value.Should().BeOfType<ScanResponse>();
        }

        [Fact]
        public async Task HandleScanRequest_Returns_BadRequest_Response()
        {
            // Arrange
            var request = _autoFixture.Create<ScanRequest>();

            _mediator.Setup(q => q.Send(It.IsAny<ScanRequest>(), default))
                .ReturnsAsync(new OperationResult<ScanResponse>
                {
                    Status = HttpStatusCode.BadRequest,
                    ErrorMessages = _autoFixture.Create<List<string>>()
                });

            // Act
            var response = await _classUnderTest.HandleScanRequest(request);

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(response);
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            result.Value.Should().BeOfType<List<string>>().Which.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task HandleScanRequest_Returns_InternalServerError_Response()
        {
            // Arrange
            var request = _autoFixture.Create<ScanRequest>();

            _mediator.Setup(q => q.Send(It.IsAny<ScanRequest>(), default))
                .ReturnsAsync(new OperationResult<ScanResponse>
                {
                    Status = HttpStatusCode.InternalServerError,
                    ErrorMessages = _autoFixture.Create<List<string>>()
                });

            // Act
            var response = await _classUnderTest.HandleScanRequest(request);

            // Assert
            var result = Assert.IsType<ObjectResult>(response);
            result.StatusCode.Value.Should().Be((int)HttpStatusCode.InternalServerError);
            result.Value.Should().BeOfType<List<string>>().Which.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task HandleScanRequest_Returns_Unrecognized_Response()
        {
            // Arrange
            var request = _autoFixture.Create<ScanRequest>();

            _mediator.Setup(q => q.Send(It.IsAny<ScanRequest>(), default))
                .ReturnsAsync(new OperationResult<ScanResponse>
                {
                    Status = HttpStatusCode.BadGateway,
                    ErrorMessages = _autoFixture.Create<List<string>>()
                });

            // Act
            var response = await _classUnderTest.HandleScanRequest(request);

            // Assert
            var result = Assert.IsType<ObjectResult>(response);
            result.StatusCode.Value.Should().Be((int)HttpStatusCode.InternalServerError);
            result.Value.Should().BeOfType<List<string>>().Which.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task HandleScanRequest_Returns_InternalServerError()
        {
            // Arrange
            var request = _autoFixture.Create<ScanRequest>();

            _mediator.Setup(q => q.Send(It.IsAny<ScanRequest>(), default))
                .ThrowsAsync(new Exception());

            // Act
            var response = await _classUnderTest.HandleScanRequest(request);

            // Assert
            var result = Assert.IsType<ObjectResult>(response);
            result.StatusCode.Value.Should().Be((int)HttpStatusCode.InternalServerError);
            result.Value.Should().BeOfType<List<string>>().Which.Should().NotBeNullOrEmpty();
        }
    }
}
