using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenoBot
{
    public class BotServices : IBotServices
    {
        public LuisRecognizer LuisService { get; private set; }
		public QnAMaker QnAMakerService { get; private set; }

		public BotServices(IConfiguration configuration)
		{
			//Read the settings for cognitive services (LUIS, QnA) from the appsettings.json
			//If includeApiResults is set to true, the full response from the LUIS api (LuisResult)
			//will be made available in the properties collection of the RecognizerResult
			LuisService = new LuisRecognizer(
				new LuisApplication(configuration["LuisAppId"], configuration["LuisAPIKey"],
					$"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com"),
				new LuisPredictionOptions { IncludeAllIntents = true, IncludeInstanceData = true },
				includeApiResults: true);

			QnAMakerService = new QnAMaker(new QnAMakerEndpoint
			{
				KnowledgeBaseId = configuration["QnAKnowledgebaseId"],
				EndpointKey = configuration["QnAEndpointKey"],
				Host = configuration["QnAEndpointHostName"]
			});

			/**
			WeatherStackServices =
				new WeatherStackServices(configuration["WeatherStackHost"], configuration["WeatherStackApiKey"]);
			**/	
	}

	}
}
