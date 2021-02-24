using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using StoreApi.Controllers;
using StoreApi.DAL;
using StoreApi.Models;
using System;
using System.Collections.Generic;

namespace UnitTests
{    
    public class GivenAStoreController_WhenGetFeaturedProductsIsCalled
    {
        private IStoreController sut;        
        private Mock<IDataService> iDataServiceMock = new Mock<IDataService>();
        private Mock<ILogger<StoreController>> iLoggerMock = new Mock<ILogger<StoreController>>();
        private string calledExMessage;
        private const string ExpectedGetFeaturedProductsExceptionMessage = "An error occurred retrieving the featured products";
        private List<Product> ExpectedFeaturedProducts = new List<Product>
        {
            new Product{Name="Prod1"},
            new Product{Name="Prod2"},
            new Product{Name="Prod3"},
        };

        [SetUp]
        public void Setup()
        {
            sut = new StoreController(iLoggerMock.Object, iDataServiceMock.Object);

            iLoggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Callback(new InvocationAction(invocation =>
            {
                var logLevel = (LogLevel)invocation.Arguments[0]; // The first two will always be whatever is specified in the setup above
                var eventId = (EventId)invocation.Arguments[1];  // so I'm not sure you would ever want to actually use them
                var state = invocation.Arguments[2];
                var exception = (Exception?)invocation.Arguments[3];
                var formatter = invocation.Arguments[4];

                var invokeMethod = formatter.GetType().GetMethod("Invoke");
                calledExMessage = (string?)invokeMethod?.Invoke(formatter, new[] { state, exception });
            }));
        }

        [Test]
        public void AndTheDataServiceThrowsAnException_ThenTheExceptionMessageIsLogged()
        {
            iDataServiceMock.Setup(i => i.GetFeaturedProductsAsync()).Throws(new DataServiceException(ExpectedGetFeaturedProductsExceptionMessage));

            sut.GetFeaturedProductsAsync();

            calledExMessage.Should().Be(ExpectedGetFeaturedProductsExceptionMessage);
        }

        [Test]
        public async System.Threading.Tasks.Task AndTheDataServiceThrowsAnException_ThenAnInternalServerErrorIsReturned()
        {
            iDataServiceMock.Setup(i => i.GetFeaturedProductsAsync()).Throws(new DataServiceException(ExpectedGetFeaturedProductsExceptionMessage));

            var statusResult = await sut.GetFeaturedProductsAsync() as StatusCodeResult;

            statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Test]
        public async System.Threading.Tasks.Task AndTheDataServiceReturnsFeaturedProducts_ThenTheFeaturedProductsAreReturned()
        {
            iDataServiceMock.Setup(i => i.GetFeaturedProductsAsync()).ReturnsAsync(ExpectedFeaturedProducts);

            var statusResult = await sut.GetFeaturedProductsAsync() as JsonResult;

            var products = statusResult.Value as List<Product>;

            products.Should().BeEquivalentTo(ExpectedFeaturedProducts);
        }
    }
}
