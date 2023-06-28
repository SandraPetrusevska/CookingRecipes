namespace CookingRecipes.Models
{
    public class RecipeCategory
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
