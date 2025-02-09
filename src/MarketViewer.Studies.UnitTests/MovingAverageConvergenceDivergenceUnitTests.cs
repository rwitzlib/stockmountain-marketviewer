using AutoFixture;
using MarketViewer.Contracts.Models;
using Moq.AutoMock;
using Xunit;
using FluentAssertions;
using MarketDataProvider.Contracts.Models;
using Polygon.Client.Models;
using System.Text.Json;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Studies.UnitTests
{
    public class MovingAverageConvergenceDivergenceUnitTests
    {
        private readonly IFixture _autoFixture;
        private readonly AutoMocker _autoMocker;

        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        public MovingAverageConvergenceDivergenceUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();
        }

        [Fact]
        public void MACD_With_Invalid_Parameters_Returns_ErrorMessages()
        {
            // Arrange 
            string[] parameters = new[] { "12", "26", "9" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToList();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void MACD_With_Null_Parameters_Returns_ErrorMessages()
        {
            // Arrange 
            var candles = _autoFixture.CreateMany<Bar>(100).ToList();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, null);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void MACD_EMA_Returns_Valid_Response()
        {
            // Arrange 
            string[] parameters = new[] { "12", "26", "9", "EMA" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToList();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().NotBeNull();
            response.ErrorMessages.Should().BeNullOrEmpty();
        }

        [Fact]
        public void MACD_SMA_Returns_Valid_Response()
        {
            // Arrange 
            string[] parameters = new[] { "12", "26", "9", "SMA" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToList();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().NotBeNull();
            response.ErrorMessages.Should().BeNullOrEmpty();
        }

        [Fact]
        public void MACD_With_No_Candles_Returns_ErrorMessages()
        {
            // Arrange 
            string[] parameters = new[] { "12", "26", "9", "EMA" };
            var candles = new List<Bar> { };

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void MACD_With_Too_Few_Candles_Returns_ErrorMessages()
        {
            // Arrange 
            string[] parameters = new[] { "12", "26", "9", "EMA" };
            var candles = _autoFixture.CreateMany<Bar>(5).ToList();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.First().Should().BeEmpty();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("12", "26", "9", "WMA")]
        [InlineData("asdf", "26", "9", "EMA")]
        [InlineData("12", "asdf", "9", "EMA")]
        [InlineData("12", "26", "asdf", "EMA")]
        public void MACD_With_Invalid_Parameter_Types_Returns_ErrorMessages(string fast, string slow, string signal, string type)
        {
            // Arrange 
            string[] parameters = new[] { fast, slow, signal, type };
            var candles = _autoFixture.CreateMany<Bar>(100).ToList();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("12", "26", "9", "EMA", 2.08f)]
        [InlineData("12", "26", "9", "SMA", 3.61f)]
        //[InlineData("12", "26", "9", "Wilders", 3.61f)]
        public void MACD_Returns_Correct_Value(string fast, string slow, string signal, string type, float expectedValue)
        {
            // Arrange 
            var json = "{\r\n  \"ticker\": \"TSLA\",\r\n  \"queryCount\": 3641,\r\n  \"resultsCount\": 64,\r\n  \"adjusted\": true,\r\n  \"results\": [\r\n    {\r\n      \"v\": 803508,\r\n      \"vw\": 224.4514,\r\n      \"o\": 221.41,\r\n      \"c\": 225.18,\r\n      \"h\": 226.03,\r\n      \"l\": 220.12,\r\n      \"t\": 1725523200000,\r\n      \"n\": 17216\r\n    },\r\n    {\r\n      \"v\": 335248,\r\n      \"vw\": 224.6413,\r\n      \"o\": 225.2,\r\n      \"c\": 224.61,\r\n      \"h\": 225.2,\r\n      \"l\": 224.08,\r\n      \"t\": 1725526800000,\r\n      \"n\": 6365\r\n    },\r\n    {\r\n      \"v\": 180335,\r\n      \"vw\": 224.0597,\r\n      \"o\": 224.58,\r\n      \"c\": 224.01,\r\n      \"h\": 225,\r\n      \"l\": 223.5,\r\n      \"t\": 1725530400000,\r\n      \"n\": 4426\r\n    },\r\n    {\r\n      \"v\": 714295,\r\n      \"vw\": 225.3652,\r\n      \"o\": 224,\r\n      \"c\": 225.89,\r\n      \"h\": 226.2,\r\n      \"l\": 223.72,\r\n      \"t\": 1725534000000,\r\n      \"n\": 12803\r\n    },\r\n    {\r\n      \"v\": 1992080,\r\n      \"vw\": 224.009,\r\n      \"o\": 224.25,\r\n      \"c\": 224.94,\r\n      \"h\": 226.25,\r\n      \"l\": 218.85,\r\n      \"t\": 1725537600000,\r\n      \"n\": 26714\r\n    },\r\n    {\r\n      \"v\": 21907801,\r\n      \"vw\": 227.592,\r\n      \"o\": 224.99,\r\n      \"c\": 229.71,\r\n      \"h\": 230.55,\r\n      \"l\": 222.25,\r\n      \"t\": 1725541200000,\r\n      \"n\": 251551\r\n    },\r\n    {\r\n      \"v\": 25585983,\r\n      \"vw\": 232.7965,\r\n      \"o\": 229.76,\r\n      \"c\": 231.14,\r\n      \"h\": 235,\r\n      \"l\": 229.75,\r\n      \"t\": 1725544800000,\r\n      \"n\": 334295\r\n    },\r\n    {\r\n      \"v\": 17697171,\r\n      \"vw\": 228.3587,\r\n      \"o\": 231.12,\r\n      \"c\": 225.79,\r\n      \"h\": 231.59,\r\n      \"l\": 225.62,\r\n      \"t\": 1725548400000,\r\n      \"n\": 287352\r\n    },\r\n    {\r\n      \"v\": 15166294,\r\n      \"vw\": 228.8201,\r\n      \"o\": 225.79,\r\n      \"c\": 228.77,\r\n      \"h\": 230.59,\r\n      \"l\": 225.79,\r\n      \"t\": 1725552000000,\r\n      \"n\": 136048\r\n    },\r\n    {\r\n      \"v\": 10063130,\r\n      \"vw\": 229.2268,\r\n      \"o\": 228.81,\r\n      \"c\": 230.99,\r\n      \"h\": 231.2,\r\n      \"l\": 227.31,\r\n      \"t\": 1725555600000,\r\n      \"n\": 103117\r\n    },\r\n    {\r\n      \"v\": 8423414,\r\n      \"vw\": 230.4186,\r\n      \"o\": 231.0185,\r\n      \"c\": 230.27,\r\n      \"h\": 231.2599,\r\n      \"l\": 229.3,\r\n      \"t\": 1725559200000,\r\n      \"n\": 89816\r\n    },\r\n    {\r\n      \"v\": 11462078,\r\n      \"vw\": 229.2663,\r\n      \"o\": 230.26,\r\n      \"c\": 230.18,\r\n      \"h\": 230.295,\r\n      \"l\": 228.21,\r\n      \"t\": 1725562800000,\r\n      \"n\": 134370\r\n    },\r\n    {\r\n      \"v\": 902076,\r\n      \"vw\": 230.2833,\r\n      \"o\": 230.17,\r\n      \"c\": 229.81,\r\n      \"h\": 231.0889,\r\n      \"l\": 229.5564,\r\n      \"t\": 1725566400000,\r\n      \"n\": 10065\r\n    },\r\n    {\r\n      \"v\": 93781,\r\n      \"vw\": 229.7364,\r\n      \"o\": 229.85,\r\n      \"c\": 229.7,\r\n      \"h\": 230.17,\r\n      \"l\": 229.58,\r\n      \"t\": 1725570000000,\r\n      \"n\": 2636\r\n    },\r\n    {\r\n      \"v\": 101007,\r\n      \"vw\": 229.5216,\r\n      \"o\": 229.81,\r\n      \"c\": 229.5,\r\n      \"h\": 229.89,\r\n      \"l\": 229.27,\r\n      \"t\": 1725573600000,\r\n      \"n\": 2267\r\n    },\r\n    {\r\n      \"v\": 132885,\r\n      \"vw\": 229.5738,\r\n      \"o\": 229.45,\r\n      \"c\": 229.67,\r\n      \"h\": 229.85,\r\n      \"l\": 229.35,\r\n      \"t\": 1725577200000,\r\n      \"n\": 2420\r\n    },\r\n    {\r\n      \"v\": 352393,\r\n      \"vw\": 226.4165,\r\n      \"o\": 228.73,\r\n      \"c\": 225.7,\r\n      \"h\": 229.58,\r\n      \"l\": 225.2,\r\n      \"t\": 1725609600000,\r\n      \"n\": 8652\r\n    },\r\n    {\r\n      \"v\": 131436,\r\n      \"vw\": 226.6671,\r\n      \"o\": 225.84,\r\n      \"c\": 227.17,\r\n      \"h\": 227.2,\r\n      \"l\": 225.77,\r\n      \"t\": 1725613200000,\r\n      \"n\": 3561\r\n    },\r\n    {\r\n      \"v\": 86352,\r\n      \"vw\": 227.2296,\r\n      \"o\": 227.15,\r\n      \"c\": 227.7,\r\n      \"h\": 227.7,\r\n      \"l\": 226.81,\r\n      \"t\": 1725616800000,\r\n      \"n\": 2364\r\n    },\r\n    {\r\n      \"v\": 235884,\r\n      \"vw\": 228.0775,\r\n      \"o\": 227.74,\r\n      \"c\": 228.64,\r\n      \"h\": 228.97,\r\n      \"l\": 227.2,\r\n      \"t\": 1725620400000,\r\n      \"n\": 5654\r\n    },\r\n    {\r\n      \"v\": 2571705,\r\n      \"vw\": 232.1953,\r\n      \"o\": 227.7,\r\n      \"c\": 233.872,\r\n      \"h\": 234.68,\r\n      \"l\": 225.4802,\r\n      \"t\": 1725624000000,\r\n      \"n\": 40574\r\n    },\r\n    {\r\n      \"v\": 22448146,\r\n      \"vw\": 226.7751,\r\n      \"o\": 233.92,\r\n      \"c\": 224.43,\r\n      \"h\": 234.662,\r\n      \"l\": 222.9,\r\n      \"t\": 1725627600000,\r\n      \"n\": 251038\r\n    },\r\n    {\r\n      \"v\": 22986835,\r\n      \"vw\": 220.9177,\r\n      \"o\": 224.41,\r\n      \"c\": 218.4,\r\n      \"h\": 224.79,\r\n      \"l\": 217.51,\r\n      \"t\": 1725631200000,\r\n      \"n\": 295967\r\n    },\r\n    {\r\n      \"v\": 13677300,\r\n      \"vw\": 217.3743,\r\n      \"o\": 218.35,\r\n      \"c\": 216.74,\r\n      \"h\": 219.99,\r\n      \"l\": 215.58,\r\n      \"t\": 1725634800000,\r\n      \"n\": 273807\r\n    },\r\n    {\r\n      \"v\": 9672877,\r\n      \"vw\": 215.8303,\r\n      \"o\": 216.755,\r\n      \"c\": 214.97,\r\n      \"h\": 217.515,\r\n      \"l\": 214.31,\r\n      \"t\": 1725638400000,\r\n      \"n\": 117805\r\n    },\r\n    {\r\n      \"v\": 8860762,\r\n      \"vw\": 215.4678,\r\n      \"o\": 214.97,\r\n      \"c\": 214.17,\r\n      \"h\": 216.98,\r\n      \"l\": 214.12,\r\n      \"t\": 1725642000000,\r\n      \"n\": 136461\r\n    },\r\n    {\r\n      \"v\": 8299083,\r\n      \"vw\": 213.9543,\r\n      \"o\": 214.19,\r\n      \"c\": 214.26,\r\n      \"h\": 214.86,\r\n      \"l\": 213.13,\r\n      \"t\": 1725645600000,\r\n      \"n\": 102052\r\n    },\r\n    {\r\n      \"v\": 13490733,\r\n      \"vw\": 212.7813,\r\n      \"o\": 214.28,\r\n      \"c\": 210.71,\r\n      \"h\": 215.13,\r\n      \"l\": 210.51,\r\n      \"t\": 1725649200000,\r\n      \"n\": 167445\r\n    },\r\n    {\r\n      \"v\": 1817819,\r\n      \"vw\": 210.5041,\r\n      \"o\": 210.56,\r\n      \"c\": 210.05,\r\n      \"h\": 211.9,\r\n      \"l\": 209.73,\r\n      \"t\": 1725652800000,\r\n      \"n\": 19216\r\n    },\r\n    {\r\n      \"v\": 280594,\r\n      \"vw\": 210.2763,\r\n      \"o\": 210.05,\r\n      \"c\": 210.36,\r\n      \"h\": 210.73,\r\n      \"l\": 209.95,\r\n      \"t\": 1725656400000,\r\n      \"n\": 7269\r\n    },\r\n    {\r\n      \"v\": 354964,\r\n      \"vw\": 211.5273,\r\n      \"o\": 210.4,\r\n      \"c\": 212.13,\r\n      \"h\": 212.5,\r\n      \"l\": 210.4,\r\n      \"t\": 1725660000000,\r\n      \"n\": 5994\r\n    },\r\n    {\r\n      \"v\": 231724,\r\n      \"vw\": 211.4611,\r\n      \"o\": 212.14,\r\n      \"c\": 211.5,\r\n      \"h\": 212.17,\r\n      \"l\": 211,\r\n      \"t\": 1725663600000,\r\n      \"n\": 4165\r\n    },\r\n    {\r\n      \"v\": 367018,\r\n      \"vw\": 215.2979,\r\n      \"o\": 214.65,\r\n      \"c\": 215.1,\r\n      \"h\": 216.6,\r\n      \"l\": 213.54,\r\n      \"t\": 1725868800000,\r\n      \"n\": 9085\r\n    },\r\n    {\r\n      \"v\": 106744,\r\n      \"vw\": 215.0526,\r\n      \"o\": 215.1,\r\n      \"c\": 214.99,\r\n      \"h\": 215.67,\r\n      \"l\": 214.61,\r\n      \"t\": 1725872400000,\r\n      \"n\": 2849\r\n    },\r\n    {\r\n      \"v\": 105035,\r\n      \"vw\": 214.5394,\r\n      \"o\": 214.99,\r\n      \"c\": 214.6,\r\n      \"h\": 215.37,\r\n      \"l\": 213.8,\r\n      \"t\": 1725876000000,\r\n      \"n\": 3209\r\n    },\r\n    {\r\n      \"v\": 294969,\r\n      \"vw\": 214.6531,\r\n      \"o\": 214.63,\r\n      \"c\": 215,\r\n      \"h\": 215.21,\r\n      \"l\": 214.12,\r\n      \"t\": 1725879600000,\r\n      \"n\": 6407\r\n    },\r\n    {\r\n      \"v\": 856415,\r\n      \"vw\": 215.3779,\r\n      \"o\": 214.41,\r\n      \"c\": 216.47,\r\n      \"h\": 216.5,\r\n      \"l\": 213.69,\r\n      \"t\": 1725883200000,\r\n      \"n\": 15481\r\n    },\r\n    {\r\n      \"v\": 14534924,\r\n      \"vw\": 216.8759,\r\n      \"o\": 216.47,\r\n      \"c\": 218.38,\r\n      \"h\": 219.06,\r\n      \"l\": 213.67,\r\n      \"t\": 1725886800000,\r\n      \"n\": 180768\r\n    },\r\n    {\r\n      \"v\": 15585064,\r\n      \"vw\": 217.8889,\r\n      \"o\": 218.37,\r\n      \"c\": 215.0159,\r\n      \"h\": 219.87,\r\n      \"l\": 214.912,\r\n      \"t\": 1725890400000,\r\n      \"n\": 211832\r\n    },\r\n    {\r\n      \"v\": 7926459,\r\n      \"vw\": 215.3882,\r\n      \"o\": 215.01,\r\n      \"c\": 216.57,\r\n      \"h\": 216.57,\r\n      \"l\": 214.3,\r\n      \"t\": 1725894000000,\r\n      \"n\": 210550\r\n    },\r\n    {\r\n      \"v\": 5678028,\r\n      \"vw\": 217.4132,\r\n      \"o\": 216.53,\r\n      \"c\": 218.1746,\r\n      \"h\": 218.3765,\r\n      \"l\": 216.13,\r\n      \"t\": 1725897600000,\r\n      \"n\": 92883\r\n    },\r\n    {\r\n      \"v\": 5761230,\r\n      \"vw\": 217.6753,\r\n      \"o\": 218.19,\r\n      \"c\": 216.75,\r\n      \"h\": 218.84,\r\n      \"l\": 216.53,\r\n      \"t\": 1725901200000,\r\n      \"n\": 67658\r\n    },\r\n    {\r\n      \"v\": 4607637,\r\n      \"vw\": 216.8538,\r\n      \"o\": 216.6901,\r\n      \"c\": 216.3399,\r\n      \"h\": 217.68,\r\n      \"l\": 216.07,\r\n      \"t\": 1725904800000,\r\n      \"n\": 58971\r\n    },\r\n    {\r\n      \"v\": 6989395,\r\n      \"vw\": 216.413,\r\n      \"o\": 216.3,\r\n      \"c\": 216.49,\r\n      \"h\": 217.18,\r\n      \"l\": 215.38,\r\n      \"t\": 1725908400000,\r\n      \"n\": 93005\r\n    },\r\n    {\r\n      \"v\": 989163,\r\n      \"vw\": 216.4403,\r\n      \"o\": 216.27,\r\n      \"c\": 216.99,\r\n      \"h\": 217.4135,\r\n      \"l\": 216.2,\r\n      \"t\": 1725912000000,\r\n      \"n\": 4754\r\n    },\r\n    {\r\n      \"v\": 89045,\r\n      \"vw\": 216.9464,\r\n      \"o\": 216.99,\r\n      \"c\": 217,\r\n      \"h\": 217.148,\r\n      \"l\": 216.6,\r\n      \"t\": 1725915600000,\r\n      \"n\": 2046\r\n    },\r\n    {\r\n      \"v\": 41628,\r\n      \"vw\": 216.9559,\r\n      \"o\": 216.98,\r\n      \"c\": 217,\r\n      \"h\": 217.14,\r\n      \"l\": 216.69,\r\n      \"t\": 1725919200000,\r\n      \"n\": 1112\r\n    },\r\n    {\r\n      \"v\": 57056,\r\n      \"vw\": 217.0376,\r\n      \"o\": 216.96,\r\n      \"c\": 217.23,\r\n      \"h\": 217.23,\r\n      \"l\": 216.81,\r\n      \"t\": 1725922800000,\r\n      \"n\": 1147\r\n    },\r\n    {\r\n      \"v\": 99574,\r\n      \"vw\": 217.4526,\r\n      \"o\": 217,\r\n      \"c\": 217.71,\r\n      \"h\": 217.95,\r\n      \"l\": 217,\r\n      \"t\": 1725955200000,\r\n      \"n\": 2965\r\n    },\r\n    {\r\n      \"v\": 58610,\r\n      \"vw\": 217.0801,\r\n      \"o\": 217.57,\r\n      \"c\": 217.37,\r\n      \"h\": 217.6,\r\n      \"l\": 216.59,\r\n      \"t\": 1725958800000,\r\n      \"n\": 1762\r\n    },\r\n    {\r\n      \"v\": 111442,\r\n      \"vw\": 217.9176,\r\n      \"o\": 217.35,\r\n      \"c\": 217.77,\r\n      \"h\": 218.58,\r\n      \"l\": 217.27,\r\n      \"t\": 1725962400000,\r\n      \"n\": 2135\r\n    },\r\n    {\r\n      \"v\": 251651,\r\n      \"vw\": 218.744,\r\n      \"o\": 217.83,\r\n      \"c\": 219.15,\r\n      \"h\": 219.33,\r\n      \"l\": 217.71,\r\n      \"t\": 1725966000000,\r\n      \"n\": 5314\r\n    },\r\n    {\r\n      \"v\": 789857,\r\n      \"vw\": 219.3424,\r\n      \"o\": 218.74,\r\n      \"c\": 220.2,\r\n      \"h\": 220.3624,\r\n      \"l\": 214.98,\r\n      \"t\": 1725969600000,\r\n      \"n\": 14039\r\n    },\r\n    {\r\n      \"v\": 16605541,\r\n      \"vw\": 222.5911,\r\n      \"o\": 220.2,\r\n      \"c\": 225.1899,\r\n      \"h\": 225.3,\r\n      \"l\": 219.1,\r\n      \"t\": 1725973200000,\r\n      \"n\": 180930\r\n    },\r\n    {\r\n      \"v\": 16549682,\r\n      \"vw\": 223.7833,\r\n      \"o\": 225.18,\r\n      \"c\": 223.2485,\r\n      \"h\": 226.4,\r\n      \"l\": 221.37,\r\n      \"t\": 1725976800000,\r\n      \"n\": 232670\r\n    },\r\n    {\r\n      \"v\": 8496304,\r\n      \"vw\": 222.8354,\r\n      \"o\": 223.23,\r\n      \"c\": 220.88,\r\n      \"h\": 224.05,\r\n      \"l\": 220.71,\r\n      \"t\": 1725980400000,\r\n      \"n\": 181922\r\n    },\r\n    {\r\n      \"v\": 8879190,\r\n      \"vw\": 219.7542,\r\n      \"o\": 220.88,\r\n      \"c\": 219.29,\r\n      \"h\": 220.89,\r\n      \"l\": 218.6377,\r\n      \"t\": 1725984000000,\r\n      \"n\": 95240\r\n    },\r\n    {\r\n      \"v\": 6636913,\r\n      \"vw\": 221.3024,\r\n      \"o\": 219.291,\r\n      \"c\": 222.07,\r\n      \"h\": 222.45,\r\n      \"l\": 219.09,\r\n      \"t\": 1725987600000,\r\n      \"n\": 70311\r\n    },\r\n    {\r\n      \"v\": 5898226,\r\n      \"vw\": 223.1127,\r\n      \"o\": 222.09,\r\n      \"c\": 223.5549,\r\n      \"h\": 223.88,\r\n      \"l\": 222.09,\r\n      \"t\": 1725991200000,\r\n      \"n\": 68714\r\n    },\r\n    {\r\n      \"v\": 10156858,\r\n      \"vw\": 224.7968,\r\n      \"o\": 223.52,\r\n      \"c\": 226.13,\r\n      \"h\": 226.4,\r\n      \"l\": 222.93,\r\n      \"t\": 1725994800000,\r\n      \"n\": 122521\r\n    },\r\n    {\r\n      \"v\": 1157048,\r\n      \"vw\": 226.1052,\r\n      \"o\": 226.17,\r\n      \"c\": 226.55,\r\n      \"h\": 226.62,\r\n      \"l\": 225.65,\r\n      \"t\": 1725998400000,\r\n      \"n\": 7550\r\n    },\r\n    {\r\n      \"v\": 159346,\r\n      \"vw\": 226.0972,\r\n      \"o\": 226.57,\r\n      \"c\": 226.17,\r\n      \"h\": 226.6,\r\n      \"l\": 225.85,\r\n      \"t\": 1726002000000,\r\n      \"n\": 3547\r\n    },\r\n    {\r\n      \"v\": 87877,\r\n      \"vw\": 225.9882,\r\n      \"o\": 226.1049,\r\n      \"c\": 225.94,\r\n      \"h\": 226.19,\r\n      \"l\": 225.88,\r\n      \"t\": 1726005600000,\r\n      \"n\": 1953\r\n    },\r\n    {\r\n      \"v\": 171280,\r\n      \"vw\": 226.2688,\r\n      \"o\": 226,\r\n      \"c\": 226.5782,\r\n      \"h\": 226.69,\r\n      \"l\": 225.91,\r\n      \"t\": 1726009200000,\r\n      \"n\": 2857\r\n    }\r\n  ],\r\n  \"status\": \"OK\",\r\n  \"request_id\": \"de752dfda546fa8d2dad0b7292e1aa6b\",\r\n  \"count\": 64\r\n}";
            var stocksResponse = JsonSerializer.Deserialize<StocksResponse>(json, _options);

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(stocksResponse.Results, [fast, slow, signal, type]);

            // Assert
            response.Lines.Should().NotBeNullOrEmpty();

            var line = response.Lines.First();

            // TSLA 2024/09/10 6:00pm CST
            line.Last().Value.Should().BeApproximately(expectedValue, .01f);
        }
    }
}
