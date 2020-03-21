using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenoBot.CognitiveModels
{
    public partial class LuisContactModel
    {
        public string personFirstName => PersonFirstName();

        public string personLastName => PersonLastName(); 

        public string personName => PersonName();

        public String PersonLastName()
        {
            if (Entities?._instance?.Last_name != null )
            {
                return Entities?._instance?.Last_name.FirstOrDefault()?.Text;
            }
            else
            {
                return " "; 
            }
        }

        public String PersonFirstName()
        {
            if (Entities?._instance?.First_name != null)
            {
                return Entities?._instance?.First_name.FirstOrDefault()?.Text;
            }
            else
            {
                return " ";
            }
        }

        public String PersonName()
        {
            if (Entities?._instance?.personName != null)
            {
                return Entities?._instance?.personName.FirstOrDefault()?.Text;
            }
            else
            {
                return " ";
            }
        }

    }
}
