using System;
using Microsoft.Azure.ServiceBus;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Memory;


namespace AudioConversion.HealthCheck
{
    /// <summary>
    /// Extension for microsofts HealthCheck Nuget package to check the status of an Azure ServiceBus Subscription
    /// </summary>
    public class MicrosoftServiceBusSubscriptionHealthCheck : IHealthCheck
    {
        private readonly string _topicconnectionstring = string.Empty;
        private readonly string _subscriptionname = string.Empty;
        private readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions {});

        /// <summary>
        /// Create the object
        /// </summary>
        /// <param name="TopicConnectionString">The topic connection string</param>
        /// <param name="SubscriptionName">The subscription name</param>
        public MicrosoftServiceBusSubscriptionHealthCheck(string TopicConnectionString, string SubscriptionName)
        {
            // Check for bad parameters.
            if (string.IsNullOrEmpty(TopicConnectionString))
            {
                throw new ArgumentNullException(nameof(TopicConnectionString));
            }
            if (string.IsNullOrEmpty(SubscriptionName))
            {
                throw new ArgumentNullException(nameof(SubscriptionName));
            }
            _topicconnectionstring = TopicConnectionString;
            _subscriptionname = SubscriptionName;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if there is a previous result in the cache that has not expired yet.
                if (_cache.TryGetValue("result", out HealthCheckResult result))
                {
                    return result;
                }

                // This is the only way I can see to check if the subscription queue is available.
                SubscriptionClient subscriptionClient = new SubscriptionClient(new ServiceBusConnectionStringBuilder(_topicconnectionstring), _subscriptionname, ReceiveMode.PeekLock, RetryPolicy.NoRetry);
                await subscriptionClient.GetRulesAsync();

                // Subscription is available.
                // Let's cache the result for 5 minutes, as it is very slow and once up, the state shouldn't change very much.
                result = HealthCheckResult.Healthy();
                _cache.Set("result", result, TimeSpan.FromMinutes(5));
                return result;
            }
            catch (Microsoft.Azure.ServiceBus.MessagingEntityNotFoundException)
            {
                // Let's cache the result for 1 minute, as it is very slow and we want to know fairly quickly once it is up.
                HealthCheckResult result = new HealthCheckResult(context.Registration.FailureStatus, "the subscription doesn't exist");
                _cache.Set("result", result, TimeSpan.FromMinutes(1));
                return result;
            }
            catch (Exception ex)
            {
                // Let's cache the result for 1 minute, as it is very slow and we want to know fairly quickly once it is up.
                HealthCheckResult result = new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
                _cache.Set("result", result, TimeSpan.FromMinutes(1));
                return result;
            }
        }
    }
}
