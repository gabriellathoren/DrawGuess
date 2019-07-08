using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.Models
{
    public class GameRoom 
    {
        public String RoomName { get; set; }
        public int NumberOfPlayers
        {
            get { return _numberOfPlayers; }
            set
            {
                _numberOfPlayers = value;
                if (this.NumberOfPlayers >= 10) { Full = true; }
                else { Full = false; }
            }
        }
        private int _numberOfPlayers;
   
        public Boolean Full { get; set; }


        public void GetRooms()
        {
            var gameRooms = new ObservableCollection<GameRoom>();

            const string query =
                "SELECT Id, Name " +
                "FROM dbo.GameRoom ";

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
            
        }
    }
}
