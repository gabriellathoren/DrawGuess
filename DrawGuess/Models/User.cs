using DrawGuess.Exceptions;
using DrawGuess.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;

namespace DrawGuess.Models
{
    public class User : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Email { get; set; }
        public String ProfilePicture { get; set; }
        public int Points { get; set; }

        public User()
        {
            ProfilePicture = "";
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ObservableCollection<User> GetUsers()
        {
            var users = new ObservableCollection<User>();
            //const string query =
            //    "SELECT Id, FirstName, LastName, Email, Points " +
            //    "FROM dbo.Users ";

            //IEnumerable<IDataRecord> data = DatabaseConnection.DatabaseReader(query);

            //while (reader.Read())
            //{
            //    var user = new User
            //    {
            //        Id = reader.GetInt32(0),
            //        FirstName = reader.GetString(1),
            //        LastName = reader.GetString(2),
            //        Email = reader.GetString(3),
            //        Points = reader.GetInt32(4)
            //    };
            //    users.Add(user);
            //}

            return users;
        }


        public static bool DoesUserExists(string email)
        {
            string query =
                "SELECT Id, FirstName, LastName, Email, PasswordSalt, PasswordHash, Points " +
                "FROM dbo.Users " +
                "WHERE Email = '" + email + "' ";

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
                                if(reader.HasRows)
                                {
                                    return true;
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

            return false;
        }


        public static User GetUser(string email, string password)
        {
            var user = new User();

            string query =
                "SELECT Id, FirstName, LastName, Email, PasswordSalt, PasswordHash, Points " +
                "FROM dbo.Users " +
                "WHERE Email = '" + email + "' ";

            string PasswordSalt = "";
            string PasswordHash = "";

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
                                while (reader.Read())
                                {
                                    user.Id = reader.GetInt32(0);
                                    user.FirstName = reader.GetString(1);
                                    user.LastName = reader.GetString(2);
                                    user.Email = reader.GetString(3);
                                    PasswordSalt = reader.GetString(4);
                                    PasswordHash = reader.GetString(5);
                                    user.Points = reader.GetInt32(6);
                                }
                            }
                        }
                    }
                    conn.Close();
                }

                if (Crypting.VerifyPassword(password, PasswordHash, PasswordSalt))
                {
                    return user;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (SqlException sqle)
            {
                throw sqle;
            }
            catch (Exception e)
            {
                throw new UserNotFoundException("User could not be found in database.", e);
            }
        }

        public static void AddUser(User user, string password)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            byte[] salt = Crypting.CreateSalt();
            String passwordHash = Crypting.CreatePassword(salt, password);

            string query = "INSERT INTO dbo.Users (FirstName, LastName, Email, PassWordSalt, PasswordHash, Points) " +
                "VALUES('" + user.FirstName + "','" + user.LastName + "','" + user.Email + "','" + Convert.ToBase64String(salt) +
                "','" + passwordHash + "'," + 0 + ")";

            try
            {
                using (SqlConnection conn = new SqlConnection((App.Current as App).ConnectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand
                    {
                        CommandType = System.Data.CommandType.Text,
                        CommandText = query,
                        Connection = conn
                    };

                    cmd.ExecuteNonQuery();

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
