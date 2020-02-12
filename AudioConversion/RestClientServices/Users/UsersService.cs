using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Web;
using AudioConversions;

namespace AudioConversion.RESTApi.Client.Users
{
    public interface IReSTApiUsersService
    {
        Task<List<string>> GetPermissionPhoneNumberRestrictionsByUsernameAsync(int accountid, string username);

        Task<List<string>> GetPermissionPhoneNumberRestrictionsByUserIdAsync(int userid);
    }

    public class ReSTApiUsersService : IReSTApiUsersService
    {
        private readonly ILogger<Program> _logger = null;
        private readonly IHttpClientFactory _httpClientFactory = null;
        private readonly string _usersmicroservicesurl = string.Empty;

        public ReSTApiUsersService(ILogger<Program> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _usersmicroservicesurl = configuration?.GetValue<string>("MicroserviceEndpoints:users:url");
        }

        private class GetUsersListResult
        {
            public GetUserModel[] users = null;
        }
        private class GetUserModel
        {
            public int? id = null;
        }

        public async Task<List<string>> GetPermissionPhoneNumberRestrictionsByUsernameAsync(int accountid, string username)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;

            // Check for bad parameters.
            if (username == null)
                throw new ArgumentNullException("username");
            if (username.Length == 0)
                throw new ArgumentException("cannot be empty", "username");

            // Get the user id for the requested username.
            _logger.LogInformation($"Retrieving user details for username {username} within account id {accountid}");
            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.GetAsync($"{_usersmicroservicesurl}?accountid={accountid}&username={HttpUtility.UrlEncode(username)}&limit=1&fields=id");

            // Make sure we received the correct status code.
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    // Deserialize the body.
                    GetUsersListResult lResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<GetUsersListResult>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } });
                    if (lResponse == null)
                    {
                        throw new Exception($"get users result was unable to be deserialized");
                    }
                    else if (lResponse.users == null || lResponse.users.Count() == 0)
                    {
                        throw new Exception("unable to find username '{username}' within account id {accountid}");
                    }
                    else if (lResponse.users.Count() >= 2)
                    {
                        throw new Exception($"found {lResponse.users.Count().ToString("###,###,##0")} users with username {username} within account id {accountid} when a maximum of one was expected");
                    }
                    if (lResponse.users[0].id == null)
                    {
                        throw new Exception("returned record doesn't include the users id");
                    }

                    // Now that a single user id was identified, lookup the phone number restrictions.
                    int UserId = lResponse.users[0].id.Value;
                    _logger.LogInformation($"Found the user has id {UserId}");

                    // Return the restriction list for that user.
                    return await GetPermissionPhoneNumberRestrictionsByUserIdAsync(UserId);

                case HttpStatusCode.NotFound:
                    throw new Exception("unable to find username '{username}' within account id {accountid}");

                default:
                    throw new Exception($"unable to retrieve a user with username {username}, status code {response.StatusCode}");
            }
        }


        public async Task<List<string>> GetPermissionPhoneNumberRestrictionsByUserIdAsync(int userid)
        {
            DateTime BeginTimeUTC = DateTime.UtcNow;

            // Get the user id for the requested username.
            _logger.LogInformation($"Retrieving view permission restriction phone number list for user id {userid}");
            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.GetAsync($"{_usersmicroservicesurl}/{userid}/permission/calls/phonenumberrestrictions");

            // Make sure we received the correct status code.
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    // Deserialize the body.
                    String[] lPhoneNumbers = Newtonsoft.Json.JsonConvert.DeserializeObject<String[]>(await response.Content.ReadAsStringAsync(), new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } });

                    // Log what was found.
                    _logger.LogInformation($"Retrieved a list of {lPhoneNumbers.Count().ToString("###,###,##0")} call permission phone number restrictions");
                        return lPhoneNumbers.ToList();
     
                case HttpStatusCode.NotFound:
                    throw new Exception("unable to find user with id {userid}");

                default:
                    throw new Exception($"unexpected status code {response.StatusCode}");
            }
        }

    }
}
