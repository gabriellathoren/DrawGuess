using DrawGuess.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.Helpers
{
    public class RoomHelper
    {
        public static string RandomizeRoomName(ObservableCollection<Game> currentGames)
        {

            string query = "SELECT Top 1 Name FROM GameNames ";

            int counter = 1;
            foreach (Game game in currentGames)
            {
                if (counter == 1)
                {
                    query += "WHERE Name != '" + game.Name + "' ";
                }
                else
                {
                    query += "AND Name != '" + game.Name + "' ";
                }
                counter++;
            }

            query += "ORDER BY NEWID()";

            string randomGameName = "";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = query;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    randomGameName = reader.GetString(0);
                                }
                            }
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return randomGameName;
        }
    }
}
