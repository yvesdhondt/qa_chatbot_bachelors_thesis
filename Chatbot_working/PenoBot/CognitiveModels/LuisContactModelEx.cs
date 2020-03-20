using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenoBot.CognitiveModels
{
    public partial class LuisContactModel
    {
        public string personFirstName => Entities?._instance?.First_name.FirstOrDefault()?.Text;
        public string personLastName => Entities?._instance?.Last_name.FirstOrDefault()?.Text;
        public string personName => Entities?._instance?.personName.FirstOrDefault()?.Text; 

    }
}
