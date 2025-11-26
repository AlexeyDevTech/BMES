using Prism.Mvvm;
using System.Collections.ObjectModel;
using BMES.Core.Models;
using Prism.Events;
using BMES.Contracts.Events;
using System.Linq;

namespace BMES.Modules.Recipes.ViewModels
{
    public class RecipeParametersViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        private ObservableCollection<RecipeParameter> _recipeParameters;
        public ObservableCollection<RecipeParameter> RecipeParameters
        {
            get => _recipeParameters;
            set => SetProperty(ref _recipeParameters, value);
        }

        public RecipeParametersViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            RecipeParameters = new ObservableCollection<RecipeParameter>();
            _eventAggregator.GetEvent<RecipeSelectedEvent>().Subscribe(OnRecipeSelected);
        }

        private void OnRecipeSelected(Recipe selectedRecipe)
        {
            RecipeParameters.Clear();
            if (selectedRecipe?.Parameters != null)
            {
                foreach (var parameter in selectedRecipe.Parameters)
                {
                    RecipeParameters.Add(parameter);
                }
            }
        }
    }
}
