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
using System.Collections.Concurrent;
using System.Text;
//using PenoBot.Controllers.NotifyController;

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

        // Proactive code
        ConcurrentDictionary<string, ConversationReference> _conversationReferences = new ConcurrentDictionary<string, ConversationReference>();

        public MyBot(IBotServices botServices, ILogger<MyBot<T>> logger, T dialog,
            ConversationState conversationState, UserState userState, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            Logger = logger;
            _botServices = botServices;

            Dialog = dialog;

            ConversationState = conversationState;
            UserState = userState;

            _conversationReferences = conversationReferences;

            //conversationReferences = //TurnContext.getConversationReference(context.activity);
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


        // Generate a random string with a given size, with the purpose to create a userID  
        public string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
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
                userProfile.userID = RandomString(20, true);
                Globals.userID = userProfile.userID;

                // Acknowledge that we got their name.
                await turnContext.SendActivityAsync($"Nice to meet you {userProfile.userID}.");
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)),
            cancellationToken);

            } else {
                Globals.userID = userProfile.userID;
                // Run the last dialog in the stack.
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)),
                    cancellationToken);
            }
        }

        // CODE FOR PROACTIVE MESSAGES
        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }
        // END


        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            //var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    //await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);

                    var userStateAccessors = UserState.CreateProperty<UserProfile>(nameof(UserProfile));
                    var userProfile = await userStateAccessors.GetAsync(turnContext, () => new UserProfile());



                    if (string.IsNullOrEmpty(userProfile.Name))
                    {
                        // Prompt the user f or their name.
                        var welcomeText = "Hello and welcome! I'm here to help you with whatever you need.";
                        await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                        await turnContext.SendActivityAsync($"Could I please get your name?");
                    }
                    else
                    {
                        List<string> randomList = new List<string>(new String[] { $"Nice to see you again, {userProfile.Name}.",
                        $"It's good to have you back, {userProfile.Name}.", $"Hello {userProfile.Name}, glad I can be of service again!",
                        $"Welcome back {userProfile.Name}.", $"Hello {userProfile.Name}. How can I help you this time?",
                        $"Hello there {userProfile.Name}. I hope you're having a nice day."});
                        Random r = new Random();
                        var question = randomList[r.Next(randomList.Count)];
                        await turnContext.SendActivityAsync(question);
                        // Run the last dialog in the stack.
                        await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)),
                            cancellationToken);
                    }
                }
            }
        }
    }
}
