// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

using PenoBot.Bots;
using PenoBot.Dialogs;
using PenoBot.Middleware;
using System.Collections.Concurrent;

using ClusterClient;
using ClusterClient.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System;

namespace PenoBot
{
    public class Startup
    {

        private readonly string connectionString = "Data Source=clusterbot.database.windows.net;Initial Catalog=Cluster;" +
            "Persist Security Info=True;User ID=Martijn;Password=sY6WRDL2pY7qmsY3";
        public Startup(/**Microsoft.AspNetCore.Hosting.IHostingEnvironment env,*/ IConfiguration configuration)
        {
            //ContentRootPath = env.ContentRootPath;
            Configuration = configuration;
 
            // Initialize connection with server.
#if DEBUG
            Globals.connector = new Connector("843iu233d3m4pxb1", "ws://localhost:39160/api/Chatbot/WS", 10);
            Globals.connector.EndPointAddress = "http://localhost:3978/api/ClusterClient";
            Globals.connector.SurpressConnectionErrors();
            Globals.timeout = GetTimeout(connectionString).Result;
#else
            Globals.connector = new Connector("843iu233d3m4pxb1", "wss://clusterapi20200320113808.azurewebsites.net/api/Chatbot/WS", 10);
            Globals.connector.EndPointAddress = "https://penobot.azurewebsites.net/api/ClusterClient";
            Globals.connector.SurpressConnectionErrors();
            Globals.timeout = GetTimeout(connectionString).Result;
#endif
            // Only use the following line if you want constant websocket state checking (causes CPU usage increase)
            //Globals.connector.EnableWebSocketStateCheck(true);
        }
        //wss://clusterapi20200320113808.azurewebsites.net/api/Chatbot/WS
        //wss://clusterapidebug.azurewebsites.net/api/NLP/WS
        public IConfiguration Configuration { get; }
        //public string ContentRootPath { get; private set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.)
            services.AddSingleton<IStorage, MemoryStorage>();

            // Create the User state. (Used in this bot Dialog implementation.)
            services.AddSingleton<UserState>();

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>();

            //services.AddSingleton<ContactRecognizer>(); 

            // Create the bot services (LUIS, QnA) as a singleton.
            services.AddSingleton<IBotServices, BotServices>();

            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            // Register dialogs.
            //services.AddSingleton<LuisWeatherDialog>();
            services.AddSingleton<MainDialog>();
            services.AddSingleton<QuestionDialog>();
            services.AddSingleton<LuisContactDialog>();
            services.AddSingleton<RootDialog>();
            services.AddSingleton<Typing>();
            services.AddTransient<IBot, MyBot<RootDialog>>();

            // To get user IP
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            //app.UseHttpsRedirection();
            app.UseMvc();
        }

        private async Task<float> GetTimeout(string connectionString)
        {
            string commandText = "SELECT timeout FROM dbo.ChatbotSettings LIMIT 1;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(commandText, connection);

                try
                {
                    await connection.OpenAsync();
                    var result = command.BeginExecuteReader();
                    while (!result.IsCompleted)
                        await Task.Delay(5);
                    SqlDataReader reader = command.EndExecuteReader(result);
                    reader.Read();
                    return (float)reader.GetValue(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return 10;
            }
        }
    }
}
