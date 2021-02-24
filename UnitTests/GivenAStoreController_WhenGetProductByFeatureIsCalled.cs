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
    public class GivenAStoreController_WhenGetProductByFeatureIsCalled
    {
        private IStoreController sut;
        private Mock<IDataService> iDataServiceMock = new Mock<IDataService>();
        private Mock<ILogger<StoreController>> iLoggerMock = new Mock<ILogger<StoreController>>();
        private string calledExMessage;
        private const string ExpectedGetProductsByCategoryExceptionMessage = "An error occurred retrieving the products by category";
        private List<Product> ExpectedProducts = new List<Product>
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
        public async System.Threading.Tasks.Task AndTheDataServiceThrowsAnException_ThenTheExceptionMessageIsLogged()
        {
            iDataServiceMock.Setup(i => i.GetProductsByCategoryAsync(It.IsAny<string>())).Throws(new DataServiceException(ExpectedGetProductsByCategoryExceptionMessage));

            await sut.GetProductsByCategoryAsync("cat1");

            calledExMessage.Should().Be(ExpectedGetProductsByCategoryExceptionMessage);
        }

        [Test]
        public async System.Threading.Tasks.Task AndTheDataServiceThrowsAnException_ThenAnInternalServerErrorIsReturned()
        {
            iDataServiceMock.Setup(i => i.GetProductsByCategoryAsync(It.IsAny<string>())).Throws(new DataServiceException(ExpectedGetProductsByCategoryExceptionMessage));

            var statusResult = await sut.GetProductsByCategoryAsync("cat1") as StatusCodeResult;

            statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Test]
        public async System.Threading.Tasks.Task AndANullCategoryIsPassed_ThenABadRequestIsReturned()
        {
            var statusResult = await sut.GetProductsByCategoryAsync(null) as StatusCodeResult;

            statusResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        public async System.Threading.Tasks.Task AndAnInvalidCategoryIsPassed_ThenANotFoundIsReturned()
        {
            iDataServiceMock.Setup(i => i.GetProductsByCategoryAsync(It.IsAny<string>())).ReturnsAsync((false, null));

            var statusResult = await sut.GetProductsByCategoryAsync("cat1") as StatusCodeResult;

            statusResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Test]
        public async System.Threading.Tasks.Task AndAValidCategoryIsPassed_ThenTheExpectedProductsAreReturned()
        {
            iDataServiceMock.Setup(i => i.GetProductsByCategoryAsync(It.IsAny<string>())).ReturnsAsync((true, ExpectedProducts));

            var statusResult = await sut.GetProductsByCategoryAsync("cat2") as JsonResult;

            var products = statusResult.Value as List<Product>;

            products.Should().BeEquivalentTo(ExpectedProducts);
        }
    }
}
