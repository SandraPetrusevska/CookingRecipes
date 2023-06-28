using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CookingRecipes.Models
{
    public class Recipe
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [StringLength(int.MaxValue)]
        [Display(Name = "Ingredients")]
        public string Ingredients { get; set; }

        [StringLength(int.MaxValue)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(int.MaxValue)]
        [Display(Name = "Image")]
        public string? Image { get; set; }

        [StringLength(int.MaxValue)]
        [Display(Name = "Download Url")]
        public string? DownloadUrl { get; set; }

        [Display(Name = "Cooking time (in minutes)")]
        public int? Time { get; set; }

        [Display(Name = "Serves")]
        public int? Serves { get; set; }

        [Required]
        [Display(Name = "Chef")]
        public int ChefId { get; set; }
        public Chef? Chef { get; set; }

        public ICollection<RecipeCategory>? RecipeCategories { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<UserRecipes>? UserRecipes { get; set; }
    }
}
