using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using StoreApi.Controllers;
using StoreApi.DAL;
using StoreApi.Models;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class GivenAStoreController_WhenGetCategoriesIsCalled
    {       
        private IStoreController sut;
        private List<Category> expectedCategories = new List<Category> { 
            new Category { Name = "Cat1" },
            new Category { Name = "Cat2" },
            new Category { Name = "Cat3" },
        };
        private Mock<IDataService> iDataServiceMock = new Mock<IDataService>();
        private Mock<ILogger<StoreController>> iLoggerMock = new Mock<ILogger<StoreController>>();
        private string calledExMessage;
        private const string ExpectedGetCategoryExceptionMessage = "An error occurred retrieving the category data";

        [SetUp]
        public void SetUp()
        {           
            iDataServiceMock.Setup(d => d.GetCategoriesAsync()).ReturnsAsync(expectedCategories);
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
            iDataServiceMock.Setup(d => d.GetCategoriesAsync()).ThrowsAsync(new DataServiceException(ExpectedGetCategoryExceptionMessage));
            sut.GetCategoriesAsync();

            iLoggerMock.Verify(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()));

            calledExMessage.Should().Be(ExpectedGetCategoryExceptionMessage);
        }

        [Test]
        public async Task AndTheDataServiceThrowsAnException_ThenAnInternalServerErrorIsReturned()
        {
            iDataServiceMock.Setup(d => d.GetCategoriesAsync()).ThrowsAsync(new DataServiceException("An error occurred"));
            var statusResult = await sut.GetCategoriesAsync() as StatusCodeResult;

            statusResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }

        [Test]
        public async Task AndTheDataServiceReturnsCategories_TheExpectedCategriesAreReturned()
        {
            var objectResult = await sut.GetCategoriesAsync() as JsonResult;

            var cats = objectResult.Value as List<Category>;

            cats.Should().BeEquivalentTo(expectedCategories);
        }        
    }
}
