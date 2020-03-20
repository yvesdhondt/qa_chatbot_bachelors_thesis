using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;

namespace PenoBot
{
    public interface IBotServices
    {
        LuisRecognizer LuisService { get; }
        QnAMaker QnAMakerService { get; }
    }
}
