using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PenoBot
{
    public class Connection
    {
        private string connectionString = "Data Source=clusterbot.database.windows.net;Initial Catalog=Cluster;Persist Security Info=True;User ID=Martijn;Password=sY6WRDL2pY7qmsY3";

        public String getEmail(String firstName, String lastName)
        {
            return performQuery("Address", firstName, lastName);
        }

        public String getPhoneNumber(String firstName, String lastName)
        {
            return performQuery("Number", firstName, lastName);
        }

        private String performQuery(String column, String firstName, String lastName)
        {
            var result = "";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String query = "Select " + column + " from dbo.Persons where FirstName = '" + firstName + "' and LastName = '" + lastName + "'";
                using (var command = new SqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        try
                        {
                            result = reader.GetString(0);
                        }
                        catch 
                        {
                            result = null;
                            break;
                        }
                        
                    }
                    reader.Close();

                }
                connection.Close();
            }
            return result;
        }
    }
}
