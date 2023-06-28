using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CookingRecipes.Models
{
    public class Review
    {
        public int Id { get; set; }

        [StringLength(450)]
        public string AppUser { get; set; }

        [StringLength(500)]
        [Display(Name = "Comment")]
        public string Comment { get; set; }

        [Display(Name = "Rating")]
        public int? Rating { get; set; }

        [Display(Name = "Recipe")]
        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }
    }
}
