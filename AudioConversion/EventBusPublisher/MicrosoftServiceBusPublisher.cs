using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using AudioConversions;


// https://damienbod.com/2019/04/23/using-azure-service-bus-queues-with-asp-net-core-services/

namespace AudioConversion.EventBusPublishers
{
    public interface IEventBusPublisher
    {
         Task PublishAsync(string MimeType, byte[] Body);
    }

    public class MicrosoftServiceBusTopicPublisher : IEventBusPublisher
    {
        private readonly ILogger<Program> _logger = null;
        private readonly TopicClient _topicClient = null;


        public MicrosoftServiceBusTopicPublisher(ILogger<Program> logger, string ConnectionString)
        {
            // Check for bad parameters.
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentNullException(nameof(ConnectionString));
            }

            _logger = logger;

            // Create the Azure Bus topic client. Don't create if this is being created from the Unit Testing where configuration is empty.
            _topicClient = new TopicClient(new ServiceBusConnectionStringBuilder(ConnectionString));
        }

        /// <summary>
        /// Send a message to the topic, this will in turn copy the message to all subscribers of the topic
        /// </summary>
        /// <param name="MimeType">The content mime type</param>
        /// <param name="Body">The message body</param>
        virtual public async Task PublishAsync(string MimeType, byte[] Body)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;

            // Check for bad parameters.
            if (string.IsNullOrEmpty(MimeType))
            {
                throw new ArgumentNullException(nameof(MimeType));
            }
            if (Body == null)
            {
                throw new ArgumentNullException(nameof(Body));
            }

            // Make sure the client was created.
            if (_topicClient == null)
            {
                throw new Exception("client was not created, unexpected");
            }

            // Build the service bus message. 
            Message message = new Message(Body);
            message.ContentType = MimeType;

            // Publish to the event bus.
            await _topicClient.SendAsync(message);

            // All done.
            _logger.LogInformation("Published event to topic bus (" + (DateTime.UtcNow - BeginTimeUTC).TotalMilliseconds.ToString("###,##0") + " ms). Content-type [" + message.ContentType + "] and payload "+message.Body.Length.ToString("###,###,###,##0")+" bytes");
            return;
        }
    }
}