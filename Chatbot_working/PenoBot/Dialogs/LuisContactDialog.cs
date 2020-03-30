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
    public class LuisContactDialog : ComponentDialog
	{
		//private readonly IBotServices _botServices;
		//protected readonly ILogger Logger;
		//private readonly ContactRecognizer _luisRecognizer; 
		//private readonly IBotServices _botServices;
		//

		public LuisContactDialog(String id/**ContactRecognizer contactRecognizer**/ /**ILogger<LuisContactDialog> logger*/) :
			base(id)
		{
			
//			Logger = logger;

			AddDialog(new TextPrompt(nameof(TextPrompt)));
			AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
			{
				checkNameAsync,
				performQueryAsync
			}));

			InitialDialogId = nameof(WaterfallDialog);

		}

		private async Task<DialogTurnResult> checkNameAsync(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{

			var luisresult = (LuisContactModel)stepContext.Options;

			try
			{
					if (luisresult.personName.Length == (luisresult.personFirstName.Length + luisresult.personLastName.Length + 1))
					{
						return await stepContext.NextAsync("useLuis", cancellationToken);
					}

			}
			catch
			{
			
			}

			var notSure = "I'm not sure I've got the full name correctly. Can you give me the full name once more?";
			var notSureMessage = MessageFactory.Text(notSure, notSure, InputHints.ExpectingInput);
			return await stepContext.PromptAsync(nameof(TextPrompt),
				new PromptOptions() { Prompt = notSureMessage }, cancellationToken);
			//return await stepContext.NextAsync(null, cancellationToken);


		}


		//private Connection connec = new Connection();
		//private String email;

		private async Task<DialogTurnResult> performQueryAsync(WaterfallStepContext stepContext,
			CancellationToken cancellationToken)
		{

			var luisresult = (LuisContactModel)stepContext.Options;

			string dbFirstName;
			string dbLastName; 

			if(stepContext.Result == "useLuis")
			{
				dbFirstName = luisresult.personFirstName;
				dbLastName = luisresult.personLastName; 
			}
			else
			{
				string answer = stepContext.Context.Activity.Text;
				dbFirstName = answer.Substring(0, answer.IndexOf(" "));
				dbLastName = answer.Substring(answer.IndexOf(" ") + 1);
			}


			//perform query op naam 
			
			//email = connec.getEmail(dbFirstName, dbLastName);

			var replyText = $"Query op voornaam = {dbFirstName} en achternaam = {dbLastName}";
			//var replyText = $"Query levert: {email}";
			await stepContext.Context.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
   
			return await stepContext.EndDialogAsync(this, cancellationToken);


		}

	}
}
