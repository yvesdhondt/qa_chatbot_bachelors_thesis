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

namespace PenoBot.Dialogs
{
	public class QuestionDialog : ComponentDialog
	{
		//private readonly IBotServices _botServices;
		protected readonly ILogger Logger;
		//private readonly ContactRecognizer _luisRecognizer; 
		private readonly IBotServices _botServices;
		//

		public QuestionDialog(String id/**ContactRecognizer contactRecognizer**/ /**ILogger<LuisContactDialog> logger*/) :
			base(id)
		{

			//			Logger = logger;

			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				retrieveQuestions,
				answerQuestion
			}));

			InitialDialogId = nameof(WaterfallDialog);

		}

		private async Task<DialogTurnResult> retrieveQuestions(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{

			await stepContext.Context.SendActivityAsync($"In deze stap tonen we vragen om te beantwoorden");
			return await stepContext.NextAsync("useLuis", cancellationToken);



		}


		private Connection connec = new Connection();
		private String email;

		private async Task<DialogTurnResult> answerQuestion(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{

			await stepContext.Context.SendActivityAsync($"In deze stap geef jij het antwoord op de vraag");
			return await stepContext.EndDialogAsync(this, cancellationToken);

		}

	}
}
