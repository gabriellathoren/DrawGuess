using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess
{
    public static class DatabaseConnection
    {
        public static SqlDataReader DatabaseReader(string query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = query;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                return reader;

                            }
                        }
                    }
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        public static SqlDataReader DatabaseEdit(string query)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand
                    {
                        CommandType = System.Data.CommandType.Text,
                        CommandText = query,
                        Connection = conn
                    };

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception eSql)
            {
                Debug.WriteLine("Exception: " + eSql.Message);
                throw eSql;
            }
            return null;
        }
    }
}
