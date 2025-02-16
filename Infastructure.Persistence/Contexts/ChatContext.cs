using Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Contexts
{
    public class ChatContext : DbContext
    {
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=C:\\Users\\jiejo\\source\\repos\\ClassLibrary1\\Infastructure.Persistence\\DB\\chat.db;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigurateUsers(modelBuilder);
            ConfigurateMessages(modelBuilder);


            base.OnModelCreating(modelBuilder);
        }

        private static void ConfigurateMessages(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MessageEntity>().HasKey(x => x.Id);

            modelBuilder.Entity<MessageEntity>()
                .Property(x => x.Id)
                .HasColumnType("INTEGER")
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<MessageEntity>()
                .HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(x => x.SenderId);



        }

        private static void ConfigurateUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<UserEntity>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UserEntity>().HasIndex(x=>x.Name).IsUnique();
        }
    }
}
