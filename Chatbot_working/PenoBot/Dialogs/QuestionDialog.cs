using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PenoBot.CognitiveModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Dialogs.Choices;
using ClusterClient;
using ClusterClient.Models;
using System.Diagnostics;
using System.Net.WebSockets;

namespace PenoBot.Dialogs
{
	
	public class QuestionDialog : ComponentDialog
	{
		// Some strings to display on the buttons
		private const string yes = "Yes, I'm happy to help!";
		private const string no  = "No, I have my own questions";
		private const string cancel = "I don't know the answer to these questions";
		private const string noQuestionsAvailable = "I'm glad you'd like to help me, but I'm currently out of questions.";


		// Variables to keep track of the current questions and ids retrieved from the server
		//private const int nbQuestions = 3;
		private List<string> QuestionList = new List<string>();
		private List<int> QuestionIds = new List<int>();
		private int currentQuestionIndex = 0;
		public static Connector conchatbot = Globals.connector;
		public IStatePropertyAccessor<UserProfile> UserProfileAccessor { get; }



		public QuestionDialog(string id /*IStatePropertyAccessor<UserProfile> userProfileStateAccessor*/) :
			base(id)
		{
			//UserProfileAccessor = userProfileStateAccessor;
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				WantToAnswerQuestions,		// Ask user if he/she wants to answer questions
				RetrieveQuestions,			// If so, get questions from server and prompt them to the user
				AnswerQuestion,				// Ask the user for the answer of the selected question (if the user cancelled, skip this step)
				AnswerToServer,				// Send that answer to the server and ask user if they want to answer more questions
				Redo,						// If so, repreat. Else, end the dialog
			}));

			InitialDialogId = nameof(WaterfallDialog);

		}

		private async Task<DialogTurnResult> WantToAnswerQuestions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			// Check if this is the first time entering this function or not 
			// If you're looping, that means the user already indicated that he/she wants to answer some more questions
			if ((string)stepContext.Options == "Redo")
			{
				return await stepContext.NextAsync(null, cancellationToken);
			}
			else
			{
				// If this is the first time, then ask the user whether he/she wants to answer some questions
				return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
				{
					Prompt = MessageFactory.Text("Would you like to answer some questions?"),
					RetryPrompt = MessageFactory.Text("Please press one of the following buttons."),
					Choices = ChoiceFactory.ToChoices(new List<String>(new string[] { yes, no })),
					Style = ListStyle.HeroCard,
				}, cancellationToken);
			}
		}

		private async Task<DialogTurnResult> RetrieveQuestions(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{
			// stepContext.Context.Activity.Text contains the answer to whether the user wants to answer 
			// questions.
			// If so, questions are retrieved from the server, stored locally and prompted to the user.
			// If not, the dialog is ended.
			switch (stepContext.Context.Activity.Text)
			{
				case yes:
					
					List<string> choices = new List<string>(); 
					QuestionList = new List<string>();
					QuestionIds = new List<int>();
					
					try
					{
						await RequestQuestionsAndAddToChoices(choices).ConfigureAwait(false);
					}
					catch (Exception e)
					{
						// Ignore WebSocketExceptions once, because the server could have been down for a moment but might be online now.
						if (e is WebSocketException || (e is AggregateException && e.InnerException is WebSocketException))
						{
							Debug.WriteLine("WebSocketException while requesting questions:\n" + e);
							try
							{
								await RequestQuestionsAndAddToChoices(choices);
							}
							catch (Exception e2)
							{
								Debug.WriteLine("Exception while requesting questions for 2nd time:\n" + e2);
								// If you want to send the exception to the user.
								//await stepContext.Context.SendActivityAsync(MessageFactory.Text(e.ToString()), cancellationToken);
								await stepContext.Context.SendActivityAsync(MessageFactory.Text(noQuestionsAvailable), cancellationToken);
								return await stepContext.NextAsync(null, cancellationToken);
							}
						} else {
							Debug.WriteLine("Exception while requesting questions:\n" + e);
							// If you want to send the exception to the user.
							//await stepContext.Context.SendActivityAsync(MessageFactory.Text(e.ToString()), cancellationToken);
							await stepContext.Context.SendActivityAsync(MessageFactory.Text(noQuestionsAvailable), cancellationToken);
							return await stepContext.NextAsync(null, cancellationToken);
						}
					}

					// Let the user choose a question to answer
					if (choices.Count > 0)
					{
						await stepContext.Context.SendActivityAsync("Great! Let's help some coworkers out.");
						choices.Add(cancel);
						return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
						{
							Prompt = MessageFactory.Text("Which question would you like to answer?"),
							RetryPrompt = MessageFactory.Text("Please press one of the following buttons."),
							Choices = ChoiceFactory.ToChoices(choices),
							Style = ListStyle.HeroCard,
						}, cancellationToken);
					}
					else
					{
						await stepContext.Context.SendActivityAsync(MessageFactory.Text(noQuestionsAvailable), cancellationToken);
						// End QuestionDialog
						return await stepContext.EndDialogAsync(this, cancellationToken);
					}
					
				case no:
					// End QuestionDialog
					return await stepContext.EndDialogAsync(this, cancellationToken);
				default:
					// Can never happen
					return null;
			}
		}

		private async Task<List<string>> RequestQuestionsAndAddToChoices(List <string> choices)
		{
			var questions = await Task.Run(() => Globals.connector.RequestAndRetrieveUnansweredQuestions(Globals.userID,10));

			if (questions.Count > 0)
			{
				foreach (ServerQuestion question in questions)
				{
					this.QuestionList.Add(question.question);
					choices.Add(question.question);
					this.QuestionIds.Add(question.question_id);
				}
			}
			Console.WriteLine("No of questions: " + questions.Count);
			return choices;
		}

		private async Task<DialogTurnResult> AnswerQuestion(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{
			// stepContext.Context.Activity.Text contains either the cancel string,
			// or the question that the user wants to answer
			if (stepContext.Context.Activity.Text == cancel)
			{
				return await stepContext.NextAsync("cancel", cancellationToken);
			}
			else
			{
				// Figure out which question was pressed
				Predicate<string> p = stepContext.Context.Activity.Text.Equals;
				currentQuestionIndex = QuestionList.FindIndex(p);

				// Let the user answer that question
				string ask = $"What is the answer to:\n {QuestionList[currentQuestionIndex]}";
				var askMsg = MessageFactory.Text(ask, ask, InputHints.ExpectingInput);
				return await stepContext.PromptAsync(nameof(TextPrompt),
						new PromptOptions() { Prompt = askMsg }, cancellationToken);
			}

		}

		private async Task<DialogTurnResult> AnswerToServer(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			// Check if the user has actually answered a question.
			// If so, send that answer to the server
			if (!((string)stepContext.Result == "cancel"))
			{ 
				//The answer to a question
				var answer = stepContext.Context.Activity.Text;
				
				//sending the answer to the server
				try
					{
						Globals.connector.AnswerQuestion(Globals.userID, QuestionIds[currentQuestionIndex],answer);
					}
					catch (Exception e)
					{
					//await stepContext.Context.SendActivityAsync(MessageFactory.Text(e.ToString()), cancellationToken);
					Debug.WriteLine("Exception found when answering question:\n" + e.Message);
						return await stepContext.NextAsync(null, cancellationToken);
					}
					

				await stepContext.Context.SendActivityAsync("Thank you for your help!");
			}
			
			// Ask the user if he/she wants to answer another question
			return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
			{
				Prompt = MessageFactory.Text("Would you like to see some other questions?"),
				RetryPrompt = MessageFactory.Text("Please press one of the following buttons."),
				Choices = ChoiceFactory.ToChoices(new List<String>(new string[] { yes, no })),
				Style = ListStyle.HeroCard,
			}, cancellationToken);

		}

		private async Task<DialogTurnResult> Redo(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			// Check to see if the user wants new questions.
			// If so, restart the dialog.
			// Else, end this dialog.
			switch (stepContext.Context.Activity.Text)
			{
				case yes:
					return await stepContext.ReplaceDialogAsync(InitialDialogId, "Redo", cancellationToken);
				case no:
					return await stepContext.EndDialogAsync(this, cancellationToken);
				default:
					// Will never happen
					return null;
			}
		}

	}
}
