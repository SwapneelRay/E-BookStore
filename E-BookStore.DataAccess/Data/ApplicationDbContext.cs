﻿using E_BookStore.Models;
using Microsoft.EntityFrameworkCore;


namespace E_BookStore.DataAccess.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Romantic", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 }
                );
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, ISBN = "art001", Title = "Ender's Game", Author = "Ender", Description = "mast novel hai", ListPrice = 50.00, Price = 50.00, Price50 = 45, Price100 = 35, CategoryId= 1, ImageURL = "" },
                new Product { Id = 2, ISBN = "art002", Title = "Hunger games", Author = "Jennifer White", Description = "secrect society", ListPrice = 80.00, Price = 80.00, Price50 = 70, Price100 = 55, CategoryId = 1, ImageURL = "" },
                new Product { Id = 3, ISBN = "art003", Title = "Halo", Author = "John Snow", Description = "digital Edition", ListPrice = 50.00, Price = 50, Price50 = 45, Price100 = 35, CategoryId = 3, ImageURL = "" }
                );
        }
    }
}
