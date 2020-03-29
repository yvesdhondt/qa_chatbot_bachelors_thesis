using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PenoBot
{
    public class Connection
    {
        private string connectionString = "Data Source=clusterbot.database.windows.net";

        public String getEmail(String firstName, String lastName)
        {
            var email = "";
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                String query = "Select address from dbo.Persons where FirstName = " + firstName;
                using (var command = new SqlCommand(query, connection))
                {
                    var reader = command.ExecuteReader();
                    while(reader.Read())
                    {
                        email = reader.GetString(1);
                    }
                    reader.Close();

                }
                connection.Close();
            }
            return email;
        }
    }
}
