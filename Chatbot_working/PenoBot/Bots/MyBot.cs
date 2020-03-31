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
using System;

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

            Logger.LogInformation("Running dialog with Message Activity");

            var userStateAccessors = UserState.CreateProperty<UserProfile>(nameof(UserProfile));
            var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());

            if (string.IsNullOrEmpty(userProfile.Name))
            {
                // Set the name to what the user provided.
                userProfile.Name = turnContext.Activity.Text?.Trim();

                // Acknowledge that we got their name.
                await turnContext.SendActivityAsync($"Nice to meet you {userProfile.Name}.");
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)),
            cancellationToken);

            } else { 
            
                // Run the last dialog in the stack.
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)),
                    cancellationToken);
            }
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

                    var userStateAccessors = UserState.CreateProperty<UserProfile>(nameof(UserProfile));
                    var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());



                    if (string.IsNullOrEmpty(userProfile.Name))
                    {
                        // Prompt the user f or their name.
                        await turnContext.SendActivityAsync($"What is your name?");
                    }
                    else
                    {
                        await turnContext.SendActivityAsync($"Nice to see you again, {userProfile.Name}"); 
                    }


                    // BEGIN
                    // Onderstaande code vraagt of user onbeantwoorde vragen wil beantwoorden of niet.
                    // Er wordt voorlopig nog niets met het antwoord gedaan, want geen idee hoe
                    /**
                    var reply = MessageFactory.Text("Would you like to answer some unaswered questions?");
                    reply.SuggestedActions = new SuggestedActions()
                    {
                        Actions = new List<CardAction>()
                {
                    new CardAction() { Title = "Yes, I would like to help!", Type = ActionTypes.ImBack, Value = "Yes" },
                    new CardAction() { Title = "No, I have my own questions.", Type = ActionTypes.ImBack, Value = "No" },
                },
                    };
                    await turnContext.SendActivityAsync(reply, cancellationToken);

    */
                    // EINDE
                }
            }
        }
    }
}
