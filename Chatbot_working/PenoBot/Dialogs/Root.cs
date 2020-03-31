using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using PenoBot.CognitiveModels;


namespace PenoBot.Dialogs
{
	public class RootDialog : ComponentDialog
	{
		//private readonly IBotServices _botServices;
		protected readonly ILogger Logger;
		//private readonly ContactRecognizer _luisRecognizer; 
		private readonly IBotServices _botServices;


		public RootDialog(IBotServices botServices /**ContactRecognizer contactRecognizer**/, ILogger<RootDialog> logger) :
			base(nameof(RootDialog))
		{
			_botServices = botServices;
			Logger = logger;
			//_luisRecognizer = contactRecognizer; 

			// Register all the dialogs that will be called (Prompts, LuisWeather, Waterfall Steps).

			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new QuestionDialog(nameof(QuestionDialog)));
			AddDialog(new MainDialog(nameof(MainDialog), _botServices)); 

			// Define the steps for the waterfall dialog.
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				QuestionStep,
				MainStep
			}));

			InitialDialogId = nameof(WaterfallDialog);

		}

		private async Task<DialogTurnResult> QuestionStep(WaterfallStepContext stepContext,
			System.Threading.CancellationToken cancellationToken)
		{

			return await stepContext.BeginDialogAsync(nameof(QuestionDialog), null, cancellationToken);

		}

		private async Task<DialogTurnResult> MainStep(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{

			return await stepContext.BeginDialogAsync(nameof(MainDialog), null, cancellationToken);

		}

	}
}
