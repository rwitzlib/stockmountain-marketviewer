using AutoFixture;
using FluentAssertions;
using MarketViewer.Api.Binders;
using MarketViewer.Contracts.Requests;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MarketViewer.Api.UnitTests.Binders.Providers
{
    public class AggregateBinderProviderUnitTests
    {
        private readonly IFixture _autoFixture;
        private readonly AutoMocker _autoMocker;
        private readonly BinderProvider _classUnderTest;

        public AggregateBinderProviderUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();

            _classUnderTest = new BinderProvider();
        }

        [Fact]
        public void GetBinder_With_Null_Context_Returns_Null()
        {
            // Act
            var response =  _classUnderTest.GetBinder(null);

            // Assert
            response.Should().BeNull();
        }

        //[Fact]
        //public void GetBinder_With_AggregateRequest_Type_Returns_AggregateModelBinder()
        //{
        //    // Arrange
        //    var metadata = _autoMocker.GetMock<ModelMetadata>();
        //    metadata.SetupGet(q => q.ModelType)
        //        .Returns(typeof(AggregateRequest));

        //    var context = _autoMocker.GetMock<ModelBinderProviderContext>();
        //    context.SetupGet(q => q.Metadata)
        //        .Returns(metadata.Object);

        //    // Act
        //    var response = _classUnderTest.GetBinder(context.Object);

        //    // Assert
        //    response.Should().BeOfType<AggregateModelBinder>();
        //}

        //[Fact]
        //public void GetBinder_With_Other_Request_Type_Returns_Null()
        //{
        //    // Arrange
        //    var metadata = _autoMocker.GetMock<ModelMetadata>();
        //    metadata.SetupGet(q => q.ModelType)
        //        .Returns(typeof(ScanRequest));
        //    var context = _autoMocker.GetMock<ModelBinderProviderContext>();
        //    context.SetupGet(q => q.Metadata)
        //        .Returns(metadata.Object);

        //    // Act
        //    var response = _classUnderTest.GetBinder(context.Object);

        //    // Assert
        //    response.Should().BeNull();
        //}
    }
}
