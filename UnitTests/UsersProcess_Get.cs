//using Microsoft.Extensions.Logging;
//using Moq;
//using AudioConversion.Services;
//using System.Linq;
//using Xunit;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using AudioConversion.RESTApi.AudioConversion;
//using AudioConversion.EventBusPublishers;
//using AudioConversion.BackgroundServices;

//namespace UnitTests
//{
//    public partial class UnitTests
//    {
//        /// <summary>
//        /// Get a list of users without any filtering.
//        /// </summary>
//        [Fact]
//        public async Task Users_Get_All()
//        {
//            // Create mock for ILogger.
//            var loggerMock = new Mock<ILogger<AudioConversion.Program>>();

//            // Use the standard testing data. NOTE: Use a unique database name or tests will share read/write database with other tests and fail.
//            var databaseMock = MockRepositoryService.CreateStandard();

//            // Create mock for IPaymentGateway.
//            var paymentGatewayMock = new Mock<IPaymentGatewayService>();

//            // Create mock for IEventBusPublisher.
//            var eventbuspublisherMock = new Mock<IEventBusPublisher>();

//            // Create mock for IEventBusPublisher.
//            var eventpublisherbackgroundservice = new Mock<EventPublisherBackgroundService>(loggerMock.Object, databaseMock.Object, eventbuspublisherMock.Object);


//            // Create mock for IProcess.
//            var processMock = new Mock<CallsService>(loggerMock.Object, databaseMock.Object, paymentGatewayMock.Object, eventbuspublisherMock.Object, eventpublisherbackgroundservice.Object);


//            // Create mock for users controller.
//            var usersControllerMock = new CallsController(loggerMock.Object, processMock.Object);


//            // Execute the request.
//            // Get all of the entries which we know has three entries.
//            var response = await usersControllerMock.Get();


//            // Make sure the result was 200(OK).
//            OkObjectResult okresult = Assert.IsType<OkObjectResult>(response.Result);

//            // Make sure the result contains a list of user(s).
//            List<CallModel> models = Assert.IsType<List<CallModel>>(okresult.Value);

//            // Make sure there are three users in the result set.
//            Assert.Equal(3, models.Count());

//            // Make sure one of the users is 'Vo'.
//            Assert.Single(from r in models where r.callername == "Vo" select r);

//            // Make sure one of the users is 'Paul'.
//            Assert.Single(from r in models where r.callername == "Paul" select r);

//            // Make sure they don't have the same id's.
//            Assert.NotEqual(models[0].id, models[1].id);
//        }

//        /// <summary>
//        /// Try to get a user that doesn't exist.
//        /// </summary>
//        [Fact]
//        public async Task Users_Get_Single_NotFound()
//        {
//            // Create mock for ILogger.
//            var loggerMock = new Mock<ILogger<AudioConversion.Program>>();

//            // Use the standard testing data. NOTE: Use a unique database name or tests will share read/write database with other tests and fail.
//            var databaseMock = MockRepositoryService.CreateStandard();

//            // Create mock for IPaymentGateway.
//            var paymentGatewayMock = new Mock<IPaymentGatewayService>();

//            // Create mock for IEventBusPublisher.
//            var eventbuspublisherMock = new Mock<IEventBusPublisher>();

//            // Create mock for IEventBusPublisher.
//            var eventpublisherbackgroundservice = new Mock<EventPublisherBackgroundService>(loggerMock.Object, databaseMock.Object, eventbuspublisherMock.Object);

//            // Create mock for IProcess.
//            var processMock = new Mock<CallsService>(loggerMock.Object, databaseMock.Object, paymentGatewayMock.Object, eventbuspublisherMock.Object, eventpublisherbackgroundservice.Object);


//            // Create mock for users controller.
//            var usersControllerMock = new CallsController(loggerMock.Object, processMock.Object);


//            // Execute the request.
//            // Try to get a user wo's id we know doesn't exist.
//            var response = await usersControllerMock.Get(1234567890);


//            // Make sure we received a 404(Not Found).
//            Assert.IsType<NotFoundResult>(response.Result);
//        }

//        /// <summary>
//        /// Try to get a user that we know exists.
//        /// </summary>
//        [Fact]
//        public async Task Users_Get_Single_Found()
//        {
//            // Create mock for ILogger.
//            var loggerMock = new Mock<ILogger<AudioConversion.Program>>();

//            // Use the standard testing data. NOTE: Use a unique database name or tests will share read/write database with other tests and fail.
//            var databaseMock = MockRepositoryService.CreateStandard();

//            // Create mock for IPaymentGateway.
//            var paymentGatewayMock = new Mock<IPaymentGatewayService>();

//            // Create mock for IEventBusPublisher.
//            var eventbuspublisherMock = new Mock<IEventBusPublisher>();

//            // Create mock for IEventBusPublisher.
//            var eventpublisherbackgroundservice = new Mock<EventPublisherBackgroundService>(loggerMock.Object, databaseMock.Object, eventbuspublisherMock.Object);

//            // Create mock for IProcess.
//            var processMock = new Mock<CallsService>(loggerMock.Object, databaseMock.Object, paymentGatewayMock.Object, eventbuspublisherMock.Object, eventpublisherbackgroundservice.Object);


//            // Create mock for users controller.
//            var usersControllerMock = new CallsController(loggerMock.Object, processMock.Object);


//            // Execute the request.
//            // Get the 'Paul' entry which we know has the id 2.
//            var response = await usersControllerMock.Get(2);


//            // Make sure the result was 200(OK).
//            OkObjectResult okresult = Assert.IsType<OkObjectResult>(response.Result);

//            // Make sure the result contains a single user.
//            CallModel model = Assert.IsType<CallModel>(okresult.Value);

//            // Make sure the users details are correct.
//            Assert.Equal("Paul", model.callername);
//            Assert.Equal("Melbourne", model.callername);
//        }

//    }

//}

