using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace DrawGuess.Model
{
    public class Model
    {
        public class BloggingContext : DbContext
        {

            public DbSet<User> Users { get; set; }
            //public DbSet<GameRoom> GameRooms { get; set; }
            //public DbSet<Game> Games { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer("Data Source=DrawGuess");
            }

        }
        public class User
        {
            public int Id { get; set; }
            public String FirstName { get; set; }
            public String LastName { get; set; }
            public String Email { get; set; }
            public String ProfilePicture { get; set; }
        }

    }
}
