using AudioConversion.BackgroundServices;
using AudioConversion.EventBusPublishers;
using AudioConversion.RESTApi;
using AudioConversion.RESTApi.AudioConversion;
using AudioConversion.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests;
using Xunit;

namespace UnitTests.Controllers
{
    public class CallsController_Get : ControllerTesting<CallsController>, IDisposable
    {

        public CallsController_Get()
        {
            // Don't want to save changes to the database and therefore will get instanced contexts rather than static contexts.
            //this.StaticContext = false;
        }

        protected override CallsController CreateController()
        {
            //Create mock for ILogger.
            var loggerMock = new Mock<ILogger<AudioConversion.Program>>();

            // Use the standard testing data. NOTE: Use a unique database name or tests will share read/write database with other tests and fail.
            var repositoryMock = MockRepositoryService.CreateStandard();

            // Create mock for IEventBusPublisher.
            var eventbuspublisherMock = new Mock<IEventBusPublisher>();

            // Create mock for IEventBusPublisher.
            var eventpublisherervice = new Mock<IEventBusPublisher>();

            var hostEnvironment = new Mock<IHostEnvironment>();
            hostEnvironment.SetupGet(x => x.EnvironmentName).Returns("Development");

            // Create mock for IConfiguration
            var configurationMock = new Mock<IConfiguration>();

            // Create mock for IProcess.
            var processMock = new Mock<CallsService>(loggerMock.Object, repositoryMock.Object, eventpublisherervice.Object, null);

            return new CallsController(loggerMock.Object, processMock.Object);

        }

        [Fact]
        public async Task Calls_Get_All_ReturnsOk()
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await callsController.Get();
            //assert
            Assert.IsType<OkObjectResult>(response.Result);
        }

        [Fact]
        public async Task Calls_Get_GetAll()
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await Controller.Get();
            var calls = GetCallModels(response);
            //assert
            Assert.Equal(3, AudioConversion.Count);
        }

        [Theory]
        [InlineData(1)]
        public async Task Calls_Get_ById_ReturnsOk(int callId)
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await callsController.Get(callId);
            //assert
            Assert.IsType<OkObjectResult>(response.Result);
        }

        [Theory]
        [InlineData(-1)]
        public async Task Calls_Get_ById_InvalidData(int callId)
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await callsController.Get(callId);
            //assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Theory]
        [InlineData(23456)]
        [InlineData(99)]
        public async Task Calls_Get_ById_DoesNotExist(int callId)
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await callsController.Get(callId);
            //assert
            Assert.IsType<NotFoundResult>(response.Result);
        }

        [Theory]
        [InlineData(1111)]
        public async Task Calls_Get_SearchAccountId_Matches(int AccountId)
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await Controller.Get(accountid: AccountId);
            var calls = GetCallModels(response);
            //assert
            Assert.Equal(2, AudioConversion.Count);
        }

        [Theory]
        [InlineData(99999)]
        public async Task Calls_Get_SearchAccountId_NoMatches(int AccountId)
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await Controller.Get(accountid: AccountId);
            var calls = GetCallModels(response);
            //assert
            Assert.Empty(calls);
        }

        [Theory]
        [InlineData("Vo")]
        public async Task Calls_Get_SearchText_Matches(string TextSearch)
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await Controller.Get(textsearch: TextSearch);
            var calls = GetCallModels(response);
            //assert
            Assert.Equal(3, AudioConversion.Count);
        }

        [Theory]
        [InlineData("No Match")]
        public async Task Calls_Get_SearchText_NoMatches(string TextSearch)
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await Controller.Get(textsearch: TextSearch);
            var calls = GetCallModels(response);
            //assert
            Assert.Empty(calls);
        }

        [Theory]
        [InlineData("CallerName")]
        public async Task Calls_Get_ReturnProperties_Matches(string Properties)
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await Controller.Get(properties: Properties);
            var calls = GetCallModelsCutDown(response, Properties);
            //assert
            Assert.True(IsPropertyExist(calls[0], Properties));
        }

        [Fact]
        public async Task Calls_Get_Sort_Ascending()
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await Controller.Get(sort: "CallerName:asc");
            var calls = GetCallModels(response);
            //assert
            Assert.Equal("Jo", calls[0].callername);
        }

        [Fact]
        public async Task Calls_Get_Sort_Descending()
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await Controller.Get(sort: "CallerName:desc");
            var calls = GetCallModels(response);
            //assert
            Assert.Equal("Vo", calls[0].callername);
        }

        [Fact]
        public async Task Calls_Get_InvalidSort()
        {
            //arange
            var callsController = this.Controller;
            //act
            //var response = await Controller.Get(sort: "CallerName:random");
            //var calls = GetCallModels(response);
            //assert
           await Assert.ThrowsAsync<Exception>(async () => await Controller.Get(sort: "CallerName:random"));

        }

        [Theory]
        [InlineData(1)]
        public async Task Calls_Delete_ValidId(int callId)
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await Controller.Delete(callId);

            //assert
            Assert.IsType<OkResult>(response);
            // Check if there are now 2 items
            var responseLeft = await Controller.Get();
            var calls = GetCallModels(responseLeft);
            //assert
            Assert.Equal(2, AudioConversion.Count);
        }

        [Theory]
        [InlineData(1111111)]
        public async Task Calls_Delete_InValidId(int callId)
        {
            //arange
            var callsController = this.Controller;
            //act
            var response = await Controller.Delete(callId);

            //assert
            Assert.IsType<NotFoundResult>(response);
        }

        internal List<CallModel> GetCallModels(ActionResult<List<CallModel>> Calls)
        {
            // Make sure the result was 200(OK).
            Assert.IsType<OkObjectResult>(AudioConversion.Result);

            var okresult = (OkObjectResult)AudioConversion.Result;

            // Make sure the result contains a list of user(s).
            Assert.IsType<List<CallModel>>(okresult.Value);

            var models = (List<CallModel>)okresult.Value;

            return models;
        }

        internal List<ExpandoObject> GetCallModelsCutDown(ActionResult<List<CallModel>> Calls, string Properties)
        {
            // Make sure the result was 200(OK).
            Assert.IsType<OkObjectResult>(AudioConversion.Result);

            var okresult = (OkObjectResult)AudioConversion.Result;

            var models = (List<ExpandoObject>)okresult.Value;

            return models;
        }

        public static bool IsPropertyExist(dynamic settings, string name)
        {
            if (settings is ExpandoObject)
                return ((IDictionary<string, object>)settings).ContainsKey(name);

            return settings.GetType().GetProperty(name) != null;
        }
    }      
}


