using Prism.Mvvm;
using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Events;
using BMES.Contracts.Events; // Added for RecipeSelectedEvent

namespace BMES.Modules.Recipes.ViewModels
{
    public class RecipesViewModel : BindableBase
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IEventAggregator _eventAggregator;

        private ObservableCollection<Recipe> _recipes;
        public ObservableCollection<Recipe> Recipes
        {
            get => _recipes;
            set => SetProperty(ref _recipes, value);
        }

        private Recipe _selectedRecipe;
        public Recipe SelectedRecipe
        {
            get => _selectedRecipe;
            set
            {
                if (SetProperty(ref _selectedRecipe, value))
                {
                    _eventAggregator.GetEvent<RecipeSelectedEvent>().Publish(value);
                }
            }
        }

        public DelegateCommand LoadRecipesCommand { get; private set; }

        public RecipesViewModel(IRecipeRepository recipeRepository, IEventAggregator eventAggregator)
        {
            _recipeRepository = recipeRepository;
            _eventAggregator = eventAggregator;
            Recipes = new ObservableCollection<Recipe>();
            LoadRecipesCommand = new DelegateCommand(async () => await ExecuteLoadRecipesCommand());
            
            LoadRecipesCommand.Execute();
        }

        private async Task ExecuteLoadRecipesCommand()
        {
            var recipes = await _recipeRepository.GetAllRecipesAsync();
            Recipes.Clear();
            foreach (var recipe in recipes)
            {
                Recipes.Add(recipe);
            }
        }
    }
}
