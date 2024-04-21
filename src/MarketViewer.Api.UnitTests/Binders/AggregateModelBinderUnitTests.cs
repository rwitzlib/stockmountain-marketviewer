using AutoFixture;
using FluentAssertions;
using MarketViewer.Api.Binders;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Requests;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MarketViewer.Api.UnitTests.Binders
{
    public class AggregateModelBinderUnitTests
    {
        private readonly IFixture _autoFixture;
        private readonly AutoMocker _autoMocker;
        private readonly AggregateModelBinder _classUnderTest;

        public AggregateModelBinderUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();

            _classUnderTest = new AggregateModelBinder();
        }

        [Fact]
        public async Task BindingContext_Is_Null_Throws_Exception()
        {
            // Arrange
            var context = (ModelBindingContext)null;

            // Act
            var action = async () => await _classUnderTest.BindModelAsync(context);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task BindingContext_ModelStateDictionary_Is_Null_Throws_Exception()
        {
            // Arrange
            var context = _autoMocker.GetMock<ModelBindingContext>();

            // Act
            var action = async () => await _classUnderTest.BindModelAsync(context.Object);

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task BindingContext_ValueProvider_Is_Null_Returns_Failed_Result()
        {
            // Arrange
            var context = _autoMocker.GetMock<ModelBindingContext>();
            context.SetupGet(q => q.ModelState)
                .Returns(_autoFixture.Create<ModelStateDictionary>());

            // Act
            await _classUnderTest.BindModelAsync(context.Object);

            // Assert
            context.Object.Result.ToString().Should().Contain("Failed");
            context.Object.ModelState.ErrorCount.Should().Be(1);
        }

        [Fact]
        public async Task Valid_Request_Returns_Success()
        {
            // Arrange
            var result = ModelBindingResult.Success(new StocksRequest());
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "minute" },
                { "from", "12-20-2020" },
                { "to", "12-21-2020" }
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Success");
            context.ModelState.ErrorCount.Should().Be(0);
            context.Result.Model.Should().BeOfType<StocksRequest>();
        }

        [Fact]
        public async Task Missing_Ticker_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "multiplier", "1" }
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("Must include ticker. Ex. \"TSLA\"");
        }

        [Fact]
        public async Task Invalid_Multiplier_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "asdf" }
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("Must include multiplier. Ex. \"1\"");
        }

        [Fact]
        public async Task Missing_Multiplier_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("Must include multiplier. Ex. \"1\"");
        }

        [Fact]
        public async Task Invalid_Timespan_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "asdf" }
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("Must include Timespan. Ex. \"minute\"");
        }

        [Fact]
        public async Task Missing_Timespan_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("Must include Timespan. Ex. \"minute\"");
        }

        [Fact]
        public async Task Invalid_From_Date_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "minute" },
                { "from", "asdf" }
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("Must include \"From\" date. Ex. \"12-24-2020\"");
        }

        [Fact]
        public async Task Missing_From_Date_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "minute" },
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("Must include \"From\" date. Ex. \"12-24-2020\"");
        }

        [Fact]
        public async Task Invalid_To_Date_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "minute" },
                { "from", "12-20-2020" },
                { "to", "asdf" }
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("Must include \"To\" date. Ex. \"12-25-2020\"");
        }

        [Fact]
        public async Task Missing_To_Date_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "minute" },
                { "from", "12-20-2020" },
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should().Be("Must include \"To\" date. Ex. \"12-25-2020\"");
        }

        [Fact]
        public async Task Valid_Request_With_Simple_Study_Returns_Success()
        {
            // Arrange
            var result = ModelBindingResult.Success(new StocksRequest());
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "minute" },
                { "from", "12-20-2020" },
                { "to", "12-21-2020" },
                { "_study", "vwap" },
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Success");
            context.ModelState.ErrorCount.Should().Be(0);
            context.Result.Model.Should().BeOfType<StocksRequest>();
        }

        [Fact]
        public async Task Valid_Request_With_Medium_Study_Returns_Success()
        {
            // Arrange
            var result = ModelBindingResult.Success(new StocksRequest());
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "minute" },
                { "from", "12-20-2020" },
                { "to", "12-21-2020" },
                { "_study", "ema:9" }
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Success");
            context.ModelState.ErrorCount.Should().Be(0);
            context.Result.Model.Should().BeOfType<StocksRequest>();
        }

        [Fact]
        public async Task Valid_Request_With_Complex_Study_Returns_Success()
        {
            // Arrange
            var result = ModelBindingResult.Success(new StocksRequest());
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "minute" },
                { "from", "12-20-2020" },
                { "to", "12-21-2020" },
                { "_study", "macd:12,26,9,ema" }
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Success");
            context.ModelState.ErrorCount.Should().Be(0);
            context.Result.Model.Should().BeOfType<StocksRequest>();
        }

        [Fact]
        public async Task Valid_Request_With_Invalid_Study_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "minute" },
                { "from", "12-20-2020" },
                { "to", "12-21-2020" },
                { "_study", "asdf" }
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should()
                .Be("Study should be formatted like: [name of study]:[comma separated list of parameters for study].  Ex. EMA:9");
        }

        [Fact]
        public async Task Valid_Request_With_Invalid_Study_Length_Returns_Failure()
        {
            // Arrange
            var result = ModelBindingResult.Failed();
            var valueDictionary = new RouteValueDictionary
            {
                { "ticker", "TSLA" },
                { "multiplier", "1" },
                { "timespan", "minute" },
                { "from", "12-20-2020" },
                { "to", "12-21-2020" },
                { "_study", "ema:9:error" }
            };

            var context = GivenValidRequestWithValueDictionary(valueDictionary, result);

            // Act
            await _classUnderTest.BindModelAsync(context);

            // Assert
            context.Result.ToString().Should().Contain("Failed");
            context.ModelState.ErrorCount.Should().Be(1);
            context.ModelState.Values.First().Errors.First().ErrorMessage.Should()
                .Be("Study should be formatted like: [name of study]:[comma separated list of parameters for study].  Ex. EMA:9");
        }

        private ModelBindingContext GivenValidRequestWithValueDictionary(RouteValueDictionary valueDictionary, ModelBindingResult result)
        {
            var context = _autoMocker.GetMock<ModelBindingContext>();

            context.SetupGet(q => q.ValueProvider)
                .Returns(new RouteValueProvider(new BindingSource("", "", false, false), valueDictionary));

            context.SetupGet(q => q.ModelState)
                .Returns(_autoFixture.Create<ModelStateDictionary>());

            context.SetupGet(q => q.Result)
                .Returns(result);

            return context.Object;
        }
    }
}
