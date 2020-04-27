using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using PenoBot.CognitiveModels;
using ClusterClient;
using ClusterClient.Models;
using System.Net.WebSockets;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace PenoBot.Dialogs
{
	public class MainDialog : ComponentDialog
	{
		protected readonly ILogger Logger;
		private readonly IBotServices _botServices;
		public static Connector conchatbot = Globals.connector;
		protected readonly BotState UserState;
		public static string userid = Globals.userID;
		public List<string> feedbackChoices = new List<string>();
		private int _answer_id = -1;
		private int _question_id = -1;
		private readonly Random random = new Random();


		public MainDialog(String id/**ContactRecognizer contactRecognizer**/ /**ILogger<LuisContactDialog> logger*/, IBotServices botServices) :
base(id)
		{
			_botServices = botServices; 
			feedbackChoices.Add("Yes");
			feedbackChoices.Add("No");

			
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new LuisContactDialog(nameof(LuisContactDialog)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				DispatchStepAsync,
				FeedbackStepAsync,
				FinalStepAsync
			}));

			InitialDialogId = nameof(WaterfallDialog);

		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext,
			System.Threading.CancellationToken cancellationToken)
		{
			if ((string)stepContext.Options == "firstTime")
			{
				List<string> randomList = new List<string>(new String[] { "What can I do for you?",
					"What question do you have for me?", "What can I help you with?",
					"How may I help you?", "How can I be of service to you?"});
				
				var question = randomList[random.Next(randomList.Count)];
				var questionMsg = MessageFactory.Text(question, question, InputHints.ExpectingInput);
				return await stepContext.PromptAsync(nameof(TextPrompt),
					new PromptOptions() { Prompt = questionMsg }, cancellationToken);
			}
			else
			{
				List<string> randomList = new List<string>(new String[] { "What else can I do for you?",
					"Is there anything else I can help with?", "Do you have another question?", "What else can I help you with?", 
					"Can I help you with something else?"});
				var question = randomList[random.Next(randomList.Count)];
				var questionMsg = MessageFactory.Text(question, question, InputHints.ExpectingInput);
				return await stepContext.PromptAsync(nameof(TextPrompt),
					new PromptOptions() { Prompt = questionMsg }, cancellationToken);
			}
		}

		private async Task<DialogTurnResult> DispatchStepAsync(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{

			// Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
			Debug.WriteLine(stepContext.Context);
			Debug.WriteLine(cancellationToken);
			var message = stepContext.Context;
			var qnaResult = await _botServices.QnAMakerService.GetAnswersAsync(message);

			var luisResult = await _botServices.LuisService.RecognizeAsync<LuisContactModel>(message, cancellationToken);

			var thresholdScore = 0.70;

			// Check if score is too low, then it is not understood.
			if ((luisResult.TopIntent().score < thresholdScore || (luisResult.TopIntent().score > thresholdScore && luisResult.TopIntent().intent == LuisContactModel.Intent.None)) &&
				(qnaResult.FirstOrDefault()?.Score*2 ?? 0) < thresholdScore)
			{

				var askAgain = "I can't seem to find my brain ... Could you please ask it again later?";

				// Responses when no answer available
				List<string> notUnderstoodResponses = new List<string>(new String[] { "I'll have to look that up. I'll let you know when I found something!",
					"I will ask someone. I'll let you know when I got an answer.", 
					"Hmmm. Good question. Give me some time and I will try to figure it out. I will keep you updated!"});
				var notUnderstood = notUnderstoodResponses[random.Next(notUnderstoodResponses.Count)];

				// Responses on timeout
				List<string> answerLateResponse = new List<string>(new String[] { "It's taking longer than I'd expect to find an answer. " +
					"I'm not that old though. I will notify you when I'm ready.", "I thought I was smart and quick, but right now I only seem to be smart. " +
					"Anyway, you'll hear from me when I got something!", "I'm sorry. I'm having trouble finding an answer right now. It seems like " +
					"I'm not perfect after all. However, I'll notify you when I found an answer."});
				var answerLate = answerLateResponse[random.Next(answerLateResponse.Count)];

				// Responses to nonsense input
				List<string> responsesToNonsense = new List<string>(new String[] { "Yeah, right.", "Are you sure?", 
					"I sometimes really don\'t get what you mean.", "You lost me there.", "I guess I\'m not supposed to understand that? Am I?" });
				var responseToNonsense = responsesToNonsense[random.Next(responsesToNonsense.Count)];

				// Responses to offensive input
				List<string> responsesToOffensive = new List<string>(new String[] { "I wouldn\'t say it like that.",
					"I personally don\'t speak that kind of language.", "Maybe you could rephrase that?", 
					"I would not say it is proper to say that.", "As a gentle bot I would not dare to talk like that. O my dear.",
					"O my goodness. Gentle bots would not dare to talk like that."});
				var responseToOffensive = responsesToOffensive[random.Next(responsesToOffensive.Count)];


				//sending to server
				ServerAnswer answer = null;
				try
				{
					answer = await Task.Run(() => conchatbot.SendQuestionAndWaitForAnswer(Globals.userID, message.Activity.Text, 10));
				} catch(Exception e) {
					Debug.WriteLine("Exception while requesting questions:\n" + e);
					// If you want to send the exception to the user.
					//await stepContext.Context.SendActivityAsync(MessageFactory.Text(e.ToString()), cancellationToken);
					await stepContext.Context.SendActivityAsync(MessageFactory.Text(askAgain), cancellationToken);
					return await stepContext.NextAsync(null, cancellationToken);
				}


				if (answer == null)
				{
					await stepContext.Context.SendActivityAsync(MessageFactory.Text(answerLate), cancellationToken);
				}
				else if (answer.status_code == (int)ServerStatusCode.Nonsense)
				{
					// nonsense
					await stepContext.Context.SendActivityAsync(MessageFactory.Text(responseToNonsense), cancellationToken);
				}
				else if (answer.status_code == (int)ServerStatusCode.Offensive)
				{
					// offensive
					await stepContext.Context.SendActivityAsync(MessageFactory.Text(responseToOffensive), cancellationToken);
				}
				else if (answer.answer_id < 0 || answer.answer == "" || answer == null)
				{
					await stepContext.Context.SendActivityAsync(MessageFactory.Text(notUnderstood), cancellationToken);
				}
				else
				{
					await stepContext.Context.SendActivityAsync(MessageFactory.Text(answer.answer), cancellationToken);
					this._answer_id = answer.answer_id;
					this._question_id = answer.question_id;
					//to ask the user if the answer was or was not a good answer to his/her question

					try
					{
						return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
						{

							Prompt = MessageFactory.Text("Was this a good answer? I would be grateful if you could press the YES button!"),
							RetryPrompt = MessageFactory.Text("Please press one of the following buttons."),
							Choices = ChoiceFactory.ToChoices(feedbackChoices),
							Style = ListStyle.HeroCard,
						}, cancellationToken) ;
					}
					catch(Exception e)
					{
						Console.WriteLine(e);
					}
				}

				return await stepContext.NextAsync(null, cancellationToken);
			}

			// Check on scores between Luis and Qna.
			if (luisResult.TopIntent().score >= (qnaResult.FirstOrDefault()?.Score ?? 0))
			{
				// Block proactive messaging while in LUIS dialog.
				Globals.connector.BlockProactiveMessagingForUser(Globals.userID);
				// Start the Luis Weather dialog.
				return await stepContext.BeginDialogAsync(nameof(LuisContactDialog), luisResult, cancellationToken);
			}

		
			else {
			// Show a Qna message.
			var qnaMessage = MessageFactory.Text(qnaResult.First().Answer, qnaResult.First().Answer,
				InputHints.ExpectingInput);

			await stepContext.Context.SendActivityAsync(qnaMessage, cancellationToken);
			return await stepContext.NextAsync(null, cancellationToken);

				
			}		
		}

		private async Task<DialogTurnResult> FeedbackStepAsync(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{
			string yesorno = stepContext.Context.Activity.Text;
			int feedbackCode;
			if (yesorno == "Yes")
				feedbackCode = (int)FeedbackStatusCode.Positive;
			else if (yesorno == "No")
				feedbackCode = (int)FeedbackStatusCode.Positive;
			else
				return await stepContext.NextAsync(null, cancellationToken);

			Globals.connector.SendFeedbackOnAnswer(Globals.userID, this._answer_id, this._question_id, feedbackCode);
			return await stepContext.NextAsync(null, cancellationToken);
		}

		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{
			string answerMsgSingle = "By the way, I think I have found an answer to one of your previous questions.";
			string answerMsgMultiple = "By the way, I think I found an answer to some of your earlier questions!";

		// Reenable proactive messages for user if they were blocked during dialog.
		Globals.connector.UnblockProactiveMessagingForUser(Globals.userID);
			if (Globals.connector.MissedProactiveMessagesForUser(Globals.userID))
			{
				var answers = Globals.connector.GetNewAnswersForUser(Globals.userID);
				if (answers.Count > 0)
				{
					if (answers.Count == 1)
						await stepContext.Context.SendActivityAsync(MessageFactory.Text(answerMsgSingle), cancellationToken);
					else
						await stepContext.Context.SendActivityAsync(MessageFactory.Text(answerMsgMultiple), cancellationToken);
					foreach (ServerAnswer answer in answers)
					{
						var message = "You recently asked: '" + answer.question + "'. The answer should be: " + answer.answer;
						await stepContext.Context.SendActivityAsync(MessageFactory.Text(message), cancellationToken);
					}
				}
				
			}
			var i = random.Next(100);
			if (i < 16)
			{
				if(Globals.connector.RequestAndRetrieveUnansweredQuestions(Globals.userID, 2).Count > 0)
					return await stepContext.BeginDialogAsync(nameof(QuestionDialog), null, cancellationToken);
			}
			var msg = "What else can I do for you?";
			return await stepContext.ReplaceDialogAsync(InitialDialogId, msg, cancellationToken);
		}

	}
}
