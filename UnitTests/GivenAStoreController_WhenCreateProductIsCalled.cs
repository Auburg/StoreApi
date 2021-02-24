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
using System.Threading.Tasks;

namespace UnitTests
{
    public class GivenAStoreController_WhenCreateProductIsCalled
    {
        private IStoreController sut;
        private Mock<IDataService> iDataServiceMock = new Mock<IDataService>();
        private Mock<ILogger<StoreController>> iLoggerMock = new Mock<ILogger<StoreController>>();
        private string calledExMessage;
        private const string ExpectedCreateProductExceptionMessage = "An error occurred creating the product";
        private const int ExpectedProductId = 12;

        [SetUp]
        public void Setup()
        {
            iDataServiceMock.Setup(d => d.AddProductAsync(It.IsAny<Product>())).ReturnsAsync(ExpectedProductId);
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
        public void AndTheDataServiceThrowsAnException_ThenTheExceptionIsLoggedWithTheExpectedMessage()
        {
            iDataServiceMock.Setup(d=>d.AddProductAsync(It.IsAny<Product>())).Throws(new DataServiceException(ExpectedCreateProductExceptionMessage));
            sut.AddProductAsync(new Product());

            iLoggerMock.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

            calledExMessage.Should().Be(ExpectedCreateProductExceptionMessage);
        }

        [Test]
        public async Task AndTheDataServiceThrowsAnException_ThenAnInternalServerErrorIsReturned()
        {
            iDataServiceMock.Setup(d => d.AddProductAsync(It.IsAny<Product>())).Throws(new DataServiceException(ExpectedCreateProductExceptionMessage));
            var statusResult = await sut.AddProductAsync(new Product()) as StatusCodeResult;
            statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Test]
        public async Task AndANullProductIsPassed_ThenABadRequestIsReturnedAsync()
        {
            var statusResult = await sut.AddProductAsync(null) as StatusCodeResult;
            statusResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Test]
        public async Task AndAndInvalidProductIsPassed_ThenABadRequestIsReturnedWithTheExpectedErrorMessage()
        {
            var sku = "987KL";
            iDataServiceMock.Setup(d => d.AddProductAsync(It.IsAny<Product>())).ReturnsAsync(-1);

            var statusResult = await sut.AddProductAsync(new Product { SKU = sku }) as ObjectResult;
            statusResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            statusResult.Value.Should().Be($"{sku} invalid Category");
        }

        [Test]
        public async Task AndAValidProductIsPassed_ThenTheProductIdIsReturned()
        {
            iDataServiceMock.Setup(d => d.AddProductAsync(It.IsAny<Product>())).ReturnsAsync(ExpectedProductId);

            var statusResult = await sut.AddProductAsync(new Product { SKU = "187KL" }) as JsonResult;

            statusResult.Value.Should().Be(ExpectedProductId);
        }
    }
}
