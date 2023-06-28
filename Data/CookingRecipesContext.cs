using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CookingRecipes.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CookingRecipes.Areas.Identity.Data;

namespace CookingRecipes.Data
{
    public class CookingRecipesContext : IdentityDbContext<CookingRecipesUser>
    {
        public CookingRecipesContext (DbContextOptions<CookingRecipesContext> options)
            : base(options)
        {
        }

        public DbSet<CookingRecipes.Models.Chef> Chef { get; set; } = default!;

        public DbSet<CookingRecipes.Models.Recipe>? Recipe { get; set; }

        public DbSet<CookingRecipes.Models.Category>? Category { get; set; }

        public DbSet<CookingRecipes.Models.Review>? Review { get; set; }
        public DbSet<CookingRecipes.Models.RecipeCategory>? RecipeCategory { get; set; }
        public DbSet<CookingRecipes.Models.UserRecipes>? UserRecipes { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

    }
}
