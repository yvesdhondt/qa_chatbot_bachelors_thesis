using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace PenoBot.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly IActionContextAccessor _accessor;
        private string ip;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly string connectionString = "Data Source=clusterbot.database.windows.net;Initial Catalog=Cluster;" +
            "Persist Security Info=True;User ID=Martijn;Password=sY6WRDL2pY7qmsY3";

        public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences, IActionContextAccessor accessor)
        {
            _accessor = accessor;
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"];
            ip = _accessor.ActionContext.HttpContext.Connection.RemoteIpAddress.ToString();

            // If the channel is the Emulator, and authentication is not in use,
            // the AppId will be null.  We generate a random AppId for this case only.
            // This is not required for production, since the AppId will have a value.
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }

        public async Task<IActionResult> Get()
        {
            var content = "<html><body><table><thead><th>ActivityId</th><th>Bot</th><th>ChannelId</th><th>Conversation</th>";
            content += "<th>ServiceUrl</th><th>User</th></thead><tbody>";

            ISet<string> allowedIPs = await GetAllowedIPs(this.connectionString);
            if (allowedIPs.Contains(ip))
            {
                foreach (var conversationReference in _conversationReferences.Values)
                {
                    content += "<tr>";
                    content += $"<td>{conversationReference.ActivityId}</td><td>Id: {conversationReference.Bot.Id}, name: {conversationReference.Bot.Name}, " +
                        $"role: {conversationReference.Bot.Role}</td>";
                    content += $"<td>{conversationReference.ChannelId}</td><td>{conversationReference.ServiceUrl}</td><td>Id: {conversationReference.User.Id}, " +
                        $"name: {conversationReference.User.Name}, AAD: {conversationReference.User.AadObjectId}, {conversationReference.User.Role}</td>";

                    content += "</tr>";
                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
                }
            } else
            {
                foreach (var conversationReference in _conversationReferences.Values)
                {
                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
                }
            }
            content += $"</tbody></table>Your IP: {ip}";
            content += "</body></html>";


            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = content,
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }

        private async Task<ISet<string>> GetAllowedIPs(string connectionString)
        {
            ISet<string> allowedIPs = new HashSet<string>();
            string commandText = "SELECT ip FROM dbo.ChatbotAllowedIPs;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(commandText, connection);

                try
                {
                    await connection.OpenAsync();
                    var result = command.BeginExecuteReader();
                    while (!result.IsCompleted)
                        await Task.Delay(5);
                    SqlDataReader reader = command.EndExecuteReader(result);
                    Console.WriteLine("Reading allowed ips.");
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            allowedIPs.Add((string)reader.GetValue(i));
                            Console.WriteLine("Ip found: " + reader.GetValue(i));
                        }
                    }
                    Console.WriteLine("Done");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return allowedIPs;
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            MicrosoftAppCredentials.TrustServiceUrl(turnContext.Activity.ServiceUrl);
            await turnContext.SendActivityAsync("proactive hello");
        }
    }
}