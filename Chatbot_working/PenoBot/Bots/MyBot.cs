// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using PenoBot.Dialogs;
using PenoBot.CognitiveModels;


namespace PenoBot.Bots
{
    public class MyBot<T> : ActivityHandler where T: Dialog
    {
        
        protected ILogger Logger;
        protected readonly IBotServices _botServices;

        protected readonly Dialog Dialog;

        // State management.
        protected readonly BotState ConversationState;
        protected readonly BotState UserState;

        public MyBot(IBotServices botServices, ILogger<MyBot<T>> logger, T dialog,
            ConversationState conversationState, UserState userState)
        {
            Logger = logger;
            _botServices = botServices;

            Dialog = dialog;

            ConversationState = conversationState;
            UserState = userState;
        }

        // Method called on each turn. You can either dispatch from the
        // activity type here or use the OnActivityTypeAsync like below.
        public override async Task OnTurnAsync(ITurnContext turnContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }



        //When turn with message 
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            /**
            var replyText = $"Echo: {turnContext.Activity.Text}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
               **/

            Logger.LogInformation("Running dialog with Message Activity");


            // Run the last dialog in the stack.
            await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)),
                cancellationToken);
        }

        //When someone starts a new conversation with a bot 
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
