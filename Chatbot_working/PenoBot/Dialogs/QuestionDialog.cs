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

namespace PenoBot.Dialogs
{
	public class QuestionDialog : ComponentDialog
	{
		private const string yes = "Yes, I'm happy to help!";
		private const string no  = "No, I have my onw questions";

		public QuestionDialog(String id) :
			base(id)
		{
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				WantToAnswerQuestions,
				RetrieveQuestions,
				AnswerQuestion
			}));

			InitialDialogId = nameof(WaterfallDialog);

		}

		
		private async Task<DialogTurnResult> WantToAnswerQuestions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
		{
			return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
			{
				Prompt = MessageFactory.Text("Would you like to answer some questions?"),
				RetryPrompt = MessageFactory.Text("Please make a valid choice."),
				Choices = ChoiceFactory.ToChoices(new List<String>(new string[] { yes, no })),
				Style = ListStyle.HeroCard,
			}, cancellationToken);
		}

		private async Task<DialogTurnResult> RetrieveQuestions(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{
			switch (stepContext.Context.Activity.Text)
			{
				case yes:
					// Retrieve the actual questions
					await stepContext.Context.SendActivityAsync("Great! Let's help some coworkers out.");

					return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
					{
						Prompt = MessageFactory.Text("Which question would you like to answer?"),
						RetryPrompt = MessageFactory.Text("Please make a valid choice."),
						Choices = ChoiceFactory.ToChoices(new List<String>(new string[] { "question 1", "question 2", "question 3" })),
						Style = ListStyle.HeroCard,
					}, cancellationToken);
				case no:
					// End QuestionDialog
					return await stepContext.EndDialogAsync(this, cancellationToken);
				default:
					// Ask again for an answer
					return await stepContext.NextAsync("useLuis", cancellationToken);
			}
		}


		private Connection connec = new Connection();
		private String email;

		private async Task<DialogTurnResult> AnswerQuestion(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{

			await stepContext.Context.SendActivityAsync($"In deze stap geef jij het antwoord op de vraag");
			return await stepContext.EndDialogAsync(this, cancellationToken);

		}

	}
}
