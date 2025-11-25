using System.Collections.Generic;

namespace BMES.Core.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<RecipeParameter> Parameters { get; set; } = new List<RecipeParameter>();
    }

    public class RecipeParameter
    {
        public int Id { get; set; }
        public string? TagName { get; set; }
        public object? Value { get; set; }
    }
}
