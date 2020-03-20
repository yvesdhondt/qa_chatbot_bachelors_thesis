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
    public class MainDialog : ComponentDialog
    {
		//private readonly IBotServices _botServices;
		protected readonly ILogger Logger;
		//private readonly ContactRecognizer _luisRecognizer; 
		private readonly IBotServices _botServices; 


		public MainDialog(IBotServices botServices /**ContactRecognizer contactRecognizer**/, ILogger<MainDialog> logger) :
			base(nameof(MainDialog))
		{
			_botServices = botServices;
			Logger = logger;
			//_luisRecognizer = contactRecognizer; 

			// Register all the dialogs that will be called (Prompts, LuisWeather, Waterfall Steps).
			
			AddDialog(new TextPrompt(nameof(TextPrompt)));
			//AddDialog(new LuisWeatherDialog(nameof(LuisWeatherDialog), botServices));

			// Define the steps for the waterfall dialog.
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				IntroStepAsync,
				DispatchStepAsync,
				FinalStepAsync
			}));

			InitialDialogId = nameof(WaterfallDialog);
		
		}

		private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext,
			System.Threading.CancellationToken cancellationToken)
		{

			//var luisResults = await _botServices.LuisService.RecognizeAsync(stepContext.Context, cancellationToken);
			//var topScoringIntent = luisResults?.GetTopScoringIntent();
			//var topIntent = topScoringIntent.Value.intent; 

			//return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(topIntent) }, cancellationToken);


			/**if (!_luisRecognizer.IsConfigured)
			{
				await stepContext.Context.SendActivityAsync(
					MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

				return await stepContext.NextAsync(null, cancellationToken);
			}**/

			// Use the text provided in FinalStepAsync or the default if it is the first time.
			var messageText = stepContext.Options?.ToString() ?? "What can I help you with today?\nSay something like \"Book a flight from Paris to Berlin on March 22, 2020\"";
			var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);

			/**
			// Check if message from the final step present. If so, display it as Prompt, else skip to next step.
			var greeting = stepContext.Options?.ToString();
			if (string.IsNullOrEmpty(greeting))
			{
				return await stepContext.NextAsync(null, cancellationToken);
			}

			var promptText = MessageFactory.Text(greeting, greeting, InputHints.ExpectingInput);
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions() { Prompt = promptText },
				cancellationToken);
			**/
		}

		private async Task<DialogTurnResult> DispatchStepAsync(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{

			/**
			if (!_luisRecognizer.IsConfigured)
			{
				// LUIS is not configured, we just run the BookingDialog path with an empty BookingDetailsInstance.
				return await stepContext.BeginDialogAsync(nameof(BookingDialog), new BookingDetails(), cancellationToken);
			}
			*/

			// Call LUIS and gather any potential booking details. (Note the TurnContext has the response to the prompt.)
			var luisResult = await _botServices.LuisService.RecognizeAsync<LuisContactModel>(stepContext.Context, cancellationToken);
			
			switch (luisResult.TopIntent().intent)
			{
				case LuisContactModel.Intent.getEmail:

					await stepContext.Context.SendActivityAsync(
					MessageFactory.Text("intent is email"));

					await stepContext.Context.SendActivityAsync(
					MessageFactory.Text(luisResult.personFirstName));

					break; 

				/**
				await ShowWarningForUnsupportedCities(stepContext.Context, luisResult, cancellationToken);

				// Initialize BookingDetails with any entities we may have found in the response.
				var bookingDetails = new BookingDetails()
				{
					// Get destination and origin from the composite entities arrays.
					Destination = luisResult.ToEntities.Airport,
					Origin = luisResult.FromEntities.Airport,
					TravelDate = luisResult.TravelDate,
				};

				// Run the BookingDialog giving it whatever details we have from the LUIS call, it will fill out the remainder.
				return await stepContext.BeginDialogAsync(nameof(BookingDialog), bookingDetails, cancellationToken);
				**/

				/**
				case FlightBooking.Intent.GetWeather:
					// We haven't implemented the GetWeatherDialog so we just display a TODO message.
					var getWeatherMessageText = "TODO: get weather flow here";
					var getWeatherMessage = MessageFactory.Text(getWeatherMessageText, getWeatherMessageText, InputHints.IgnoringInput);
					await stepContext.Context.SendActivityAsync(getWeatherMessage, cancellationToken);
					break;
				**/

				default:
					// Catch all for unhandled intents
					var didntUnderstandMessageText = $"Sorry, I didn't get that. Please try asking in a different way (intent was {luisResult.TopIntent().intent})";
					var didntUnderstandMessage = MessageFactory.Text(didntUnderstandMessageText, didntUnderstandMessageText, InputHints.IgnoringInput);
					await stepContext.Context.SendActivityAsync(didntUnderstandMessage, cancellationToken);
					break;
			}

			var messageText = stepContext.Options?.ToString() ?? "Ask me something nice!";
			var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
			//return await stepContext.NextAsync(null, cancellationToken);

			/**
			// Call the services to dispatch to the correct dialog.
			var luisResult =
				await _botServices.LuisService.RecognizeAsync<WeatherLuis>(stepContext.Context, cancellationToken);
			var qnaResult = await _botServices.QnAMakerService.GetAnswersAsync(stepContext.Context);

			var thresholdScore = 0.70;

			// Check if score is too low, then it is not understood.
			if (luisResult.TopIntent().score < thresholdScore &&
				(qnaResult.FirstOrDefault()?.Score ?? 0) < thresholdScore)
			{
				var notUnderstood = "I'm sorry but I didn't understand your message. Please try to rephrase it";
				var notUnderstoodMessage = MessageFactory.Text(notUnderstood, notUnderstood, InputHints.ExpectingInput);

				return await stepContext.PromptAsync(nameof(TextPrompt),
					new PromptOptions() { Prompt = notUnderstoodMessage }, cancellationToken);
			}

			// Check on scores between Luis and Qna.
			if (luisResult.TopIntent().score >= (qnaResult.FirstOrDefault()?.Score ?? 0))
			{
				switch (luisResult.TopIntent().intent)
				{
					case WeatherLuis.Intent.GetWeather:
						// Start the Luis Weather dialog.
						return await stepContext.BeginDialogAsync(nameof(LuisWeatherDialog), luisResult, cancellationToken);

					default:
						// Display a not understood message.
						var notUnderstood = "I'm sorry but I didn't understand your message. Please try to rephrase it";
						var notUnderstoodMessage =
							MessageFactory.Text(notUnderstood, notUnderstood, InputHints.ExpectingInput);
						return await stepContext.PromptAsync(nameof(TextPrompt),
							new PromptOptions() { Prompt = notUnderstoodMessage }, cancellationToken);
				}
			}

			// Show a Qna message.
			var qnaMessage = MessageFactory.Text(qnaResult.First().Answer, qnaResult.First().Answer,
				InputHints.ExpectingInput);
			return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions() { Prompt = qnaMessage },
				cancellationToken);
			**/
		}

		private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{
			
			
			var options = new QnAMakerOptions { Top = 1 };

			// The actual call to the QnA Maker service.
			var response = await _botServices.QnAMakerService.GetAnswersAsync(stepContext.Context, options);
			if (response != null && response.Length > 0)
			{
				await stepContext.Context.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
			}
			else
			{
				await stepContext.Context.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
			}
			

				return await stepContext.NextAsync(null, cancellationToken);

			/**
			// Check that we ended the LUIS dialog.
			if (stepContext.Result != null && stepContext.Result is LuisWeatherDialog)
			{
				// End the dialog by replacing the current one with the root one.
				// We also pass the sentence that we will display in the initial step dialog.
				var msg = "What else can I do for you?";
				return await stepContext.ReplaceDialogAsync(InitialDialogId, msg, cancellationToken);
			}

			return await stepContext.ReplaceDialogAsync(InitialDialogId, cancellationToken: cancellationToken);
	**/
		}

	}
}
