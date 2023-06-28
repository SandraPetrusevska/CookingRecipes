using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace CookingRecipes.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Category")]
        public string CategoryName {get; set; }
        public ICollection<RecipeCategory>? RecipeCategories { get; set; }
    }
}
