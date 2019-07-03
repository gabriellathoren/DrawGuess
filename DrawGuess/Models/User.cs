using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;

namespace DrawGuess.Models
{
    public class User : INotifyPropertyChanged
    { 
        public int Id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String Email { get; set; }
        public String ProfilePicture { get; set; }

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
            const string query = "select id, firstname, lastname, email from user";

            SqlDataReader reader = DatabaseConnection.DatabaseReader(query);

            while (reader.Read())
            {
                var user = new User
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Email = reader.GetString(3)
                };
                users.Add(user);
            }

            return users;
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

            string query = "INSERT INTO dbo.User (FirstName, LastName, Email, PassWordSalt, PasswordHash, Points)" +
                "VALUES(" + user.FirstName + "," + user.LastName + ")";

            try
            {
                DatabaseConnection.DatabaseEdit(query);
            }
            catch(Exception e)
            {
                throw e; 
            }            
        }        
    }
}
