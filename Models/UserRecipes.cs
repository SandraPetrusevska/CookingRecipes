using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CookingRecipes.Models
{
    public class UserRecipes
    {
        public int Id { get; set; }

        [StringLength(450)]
        public string AppUser { get; set; }

        [Display(Name = "Recipe")]
        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }
    }
}
