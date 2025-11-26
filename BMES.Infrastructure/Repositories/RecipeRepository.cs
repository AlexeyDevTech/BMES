using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using BMES.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMES.Infrastructure.Repositories
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly BmesDbContext _context;

        public RecipeRepository(BmesDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesAsync()
        {
            return await _context.Recipes.Include(r => r.Parameters).ToListAsync();
        }

        public async Task<Recipe> GetRecipeByIdAsync(int id)
        {
            return (await _context.Recipes.Include(r => r.Parameters).FirstOrDefaultAsync(r => r.Id == id))!;
        }

        public async Task AddRecipeAsync(Recipe recipe)
        {
            await _context.Recipes.AddAsync(recipe);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {
            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRecipeAsync(int id)
        {
            var recipeToDelete = await _context.Recipes.FindAsync(id);
            if (recipeToDelete != null)
            {
                _context.Recipes.Remove(recipeToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}
