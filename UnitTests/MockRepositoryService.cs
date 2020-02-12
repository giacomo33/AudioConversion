using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using AudioConversion.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AudioConversion.Repository.SqlServer;

namespace UnitTests
{
    public class MockRepositoryService
    {
        /// <summary>
        /// Create a database containing no data
        /// </summary>
        public static Mock<RepositoryService> CreateEmpty()
        {
            // Create mock for ILogger.
            var loggerMock = new Mock<ILogger<AudioConversion.Program>>();

            // Create mock for IDatabase. Include some testing data. NOTE: Use a unique database name or tests will share and fail.
            var options = new DbContextOptionsBuilder<SQLDatabaseContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
            SQLDatabaseContext context = new SQLDatabaseContext(options);

            // Create the seed data.
            context.Database.EnsureCreated();

            // Return the mocked object.
            return new Mock<RepositoryService>(loggerMock.Object, context);
        }

        /// <summary>
        /// Create a database containing a static know set of testing data
        /// </summary>
        public static Mock<RepositoryService> CreateStandard()
        {
            // Create mock for ILogger.
            var loggerMock = new Mock<ILogger<AudioConversion.Program>>();

            // Create mock for IDatabase. Include some testing data. NOTE: Use a unique database name or tests will share and fail.
            var options = new DbContextOptionsBuilder<SQLDatabaseContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
            SQLDatabaseContext context = new SQLDatabaseContext(options);

            // Create the seed data.
            context.Database.EnsureCreated();
            context.AudioConversion.Add(new Call() { Id = 1, CallerName = "Vo", CallerPhone = "0393916603", DestinationName="Anne", DestinationPhone="0398673456", DirectionType=0, DurationSeconds=56 });
            context.AudioConversion.Add(new Call() { Id = 1, CallerName = "Jo", CallerPhone = "0393916603", DestinationName = "Vo", DestinationPhone = "0398673456", DirectionType = 0, DurationSeconds = 56 });
            context.AudioConversion.Add(new Call() { Id = 1, CallerName = "Paul", CallerPhone = "0393916603", DestinationName = "Vo", DestinationPhone = "040378956", DirectionType = 0, DurationSeconds = 56 });
            context.SaveChanges();

            var hostEnvironment = new Mock<IHostEnvironment>();
            hostEnvironment.SetupGet(x => x.EnvironmentName).Returns("Development");

            // context needs to be wrapped in an IServiceScopeTestory. Can't just put context straight into the constructor.
            return new Mock<RepositoryService>(loggerMock.Object, new MockServiceScopeFactory(context), hostEnvironment.Object);
        }

    }
    internal class MockServiceScopeFactory : IServiceScopeFactory
    {

        private SQLDatabaseContext _database;

        public MockServiceScopeFactory(SQLDatabaseContext Database)
        {
            _database = Database;
        }

        public IServiceScope CreateScope()
        {
            return new MockServiceScope(_database);
        }
    }

    internal class MockServiceScope : IServiceScope
    {
        public IServiceProvider ServiceProvider { get; }

        public MockServiceScope(SQLDatabaseContext Database)
        {
            ServiceProvider = new MockServiceProvider(Database);
        }

        public void Dispose()
        {

        }
    }

    internal class MockServiceProvider : IServiceProvider
    {

        private SQLDatabaseContext _database;

        public MockServiceProvider(SQLDatabaseContext Database)
        {
            _database = Database;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(SQLDatabaseContext))
                return _database;

            return null;
        }
    }
}
