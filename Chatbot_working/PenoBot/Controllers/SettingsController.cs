using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Recognizers.Text.DateTime.German;
using Newtonsoft.Json.Linq;

namespace PenoBot.Controllers
{
    [Route("api/chocolade")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly IActionContextAccessor _accessor;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private readonly string connectionString = "Data Source=clusterbot.database.windows.net;Initial Catalog=Cluster;" +
            "Persist Security Info=True;User ID=Martijn;Password=sY6WRDL2pY7qmsY3";
        private string ip;

        public SettingsController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, 
            ConversationReference> conversationReferences, IActionContextAccessor accessor)
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            ISet<string> allowedIPs = await GetAllowedIPs(this.connectionString);
            if (allowedIPs.Contains(this.ip))
                return ReturnSettingsForm();
            else
                return ReturnNotImplemented();
        }

        [HttpGet]
        [Route("save")]
        public async Task<IActionResult> Post(int timeout)
        {
            ISet<string> allowedIPs = await GetAllowedIPs(this.connectionString);
            if (allowedIPs.Contains(this.ip))
            {
                //string timeoutString = collection["timeout"];
                //float timeout = float.Parse(timeoutString);
                UpdateTimeout(timeout, this.connectionString);
                return ReturnSettingsForm();
            }
            return ReturnNotImplemented();
        }

        private ContentResult ReturnNotImplemented()
        {
            return new ContentResult()
            {
                Content = "",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.NotImplemented,
            };
        }

        private ContentResult ReturnSettingsForm()
        {
            var timeout = GetTimeout(this.connectionString).Result;
            var content = $"<html><body><form method=\"get\" action=\"/api/chocolade/save\"><label for=\"timeout\">Timeout: </label><input id=\"timeout\"" +
                $" name=\"timeout\" type=\"number\" value=\"{timeout}\" />" +
                "<input type=\"submit\" value=\"Save\" /></form></body></html>";
            return new ContentResult()
            {
                Content = content,
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.Accepted,
            };
        }

        private ContentResult ReturnAccepted()
        {
            return new ContentResult()
            {
                Content = "{}",
                ContentType = "application/json",
                StatusCode = (int)HttpStatusCode.Accepted,
            };
        }

        private async void UpdateTimeout(float timeout, string connectionString)
        {
            Globals.timeout = timeout;
            string commandText = "UPDATE dbo.ChatbotSettings SET timeout = @timeout;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(commandText, connection);
                command.Parameters.Add("@timeout", SqlDbType.Float);
                command.Parameters["@timeout"].Value = timeout;

                // SQL Server will implicitly convert strings into XML.

                try
                {
                    await connection.OpenAsync();
                    Int32 rowsAffected = await command.ExecuteNonQueryAsync();
                    Console.WriteLine("RowsAffected: {0}", rowsAffected);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async Task<float> GetTimeout(string connectionString)
        {
            string commandText = "SELECT timeout FROM dbo.ChatbotSettings LIMIT 1;";

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
                    reader.Read();
                    return (float)reader.GetValue(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return Globals.timeout;
            }
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
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                            allowedIPs.Add((string)reader.GetValue(i));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            return allowedIPs;
        }

    }

    public class Settings
    {
        private int _timeout = 0;
        public int timeout { get => _timeout; set => _timeout = value; }
    }
}