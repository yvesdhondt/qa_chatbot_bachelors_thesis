using ClusterClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PenoBot
{
    public static class Globals
    {
        public static Connector connector;
        public static string userID;
        public static Dictionary<string, string> UserIdToActivityUserId = new Dictionary<string, string>();
        public static Dictionary<string, string> ActivityUserIdToUserId = new Dictionary<string, string>();
        public static float timeout;
    }
}
