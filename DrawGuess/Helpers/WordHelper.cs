using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrawGuess.Helpers
{
    public class WordHelper
    {
        public static string RandomizeSecretWord()
        {
            string query = "SELECT Top 1 Word FROM SecretWords ORDER BY NEWID()";

            string word = "";

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
                                    word = reader.GetString(0);
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

            return word.ToUpper();
        }

        public static string SetRandomLetters(string secretWord)
        {
            string randomLetters = "";
            //Get letters from secret word to add to hinting letters
            for (int i = 0; i < secretWord.Length; i++)
            {
                randomLetters += secretWord[i].ToString();
            }

            //Add random letters
            int noOfLetters = 20 - secretWord.Length;

            for (int i = 0; i < noOfLetters; i++)
            {
                randomLetters += GetRandomLetter();
            }

            //Randomize order of letters
            return Shuffle(randomLetters.ToCharArray());
        }

        public static string Shuffle(char[] randomLetters)
        {
            Random Random = new Random();
            int n = randomLetters.Length;

            while (n > 1)
            {
                n--;
                int k = Random.Next(n + 1);
                var value = randomLetters[k];
                randomLetters[k] = randomLetters[n];
                randomLetters[n] = value;
            }

            return new string(randomLetters).Replace(" ", "");
        }

        public static string GetRandomLetter()
        {
            Random Random = new Random();
            string st = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(st, 1).Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}
