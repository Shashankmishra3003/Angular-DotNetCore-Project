using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options){}

        public DbSet<Value> Values { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // creating primary Key
            builder.Entity<Like>()
                .HasKey(k => new { k.LikerId, k.LikeeId });

            // creating relationship using Fluent API and configuring EF
            builder.Entity<Like>()
                 .HasOne(u => u.Likee)
                 .WithMany(u => u.Likers)
                 .HasForeignKey(u => u.LikeeId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
                 .HasOne(u => u.Liker)
                 .WithMany(u => u.Likees)
                 .HasForeignKey(u => u.LikerId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(u => u.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
