using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClusterClient.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace PenoBot.Controllers
{
    [Route("api/ClusterClient")]
    [ApiController]
    public class ClusterClientController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        private string _sorryMsg = "I hope I don't interrupt you.";
        private string _answerMsg = "I think I have found an answer to a previous question of yours.";
        private string _questionMsg = "I have a question for you:";
        private string _messageForUser = null;

        public ClusterClientController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"];

            // If the channel is the Emulator, and authentication is not in use,
            // the AppId will be null.  We generate a random AppId for this case only.
            // This is not required for production, since the AppId will have a value.
            if (string.IsNullOrEmpty(_appId))
            {
                _appId = Guid.NewGuid().ToString(); //if no AppId, use a random Guid
            }
        }

        [HttpPost]
        [Route(Actions.Default)]
        public async Task<IActionResult> Post(ServerMessage message)
        {
            switch(message.action)
            {
                case Actions.Answer:
                    return await Post((ServerAnswer)message);
                case Actions.Questions:
                    return await Post((ServerQuestionsMessage)message);
                default:
                    return ReturnNotImplemented();
            }
        }

        [HttpPost]
        [Route(Actions.Answer)]
        public async Task<IActionResult> Post(ServerAnswer answer)
        {
            if (answer.answer_id > 0 && answer.answer != null)
            {
                this._messageForUser = this._answerMsg;
                this._messageForUser += "\nYou asked: '" + answer.question + "'";
                this._messageForUser += " Well: " + answer.answer;
                await NotifyUser(answer.user_id);
                // Let the caller know proactive messages have been sent
                return ReturnAccepted();
            }
            // No reason to send the message now.
            return ReturnNotImplemented();
        }

        [HttpPost]
        [Route(Actions.Questions)]
        public async Task<IActionResult> Post(ServerQuestionsMessage questionsMessage)
        {
            this._messageForUser = this._questionMsg;
            this._messageForUser += " " + questionsMessage.answer_questions[0].question;
            await NotifyUser(questionsMessage.user_id);
            return ReturnAccepted();
        }

        private ContentResult ReturnNotImplemented()
        {
            return new ContentResult()
            {
                Content = "{}",
                ContentType = "application/json",
                StatusCode = (int)HttpStatusCode.NotImplemented,
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

        private async Task NotifyUser(string userID)
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                if(userID == conversationReference.User.Id ||
                    Globals.UserIdToActivityUserId.ContainsKey(userID) && conversationReference.User.Id == Globals.UserIdToActivityUserId[userID])
                {
                    await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
                }
            }
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl
            MicrosoftAppCredentials.TrustServiceUrl(turnContext.Activity.ServiceUrl);
            await turnContext.SendActivityAsync(_sorryMsg);
            await turnContext.SendActivityAsync(_messageForUser);
        }
    }
}