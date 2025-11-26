using Prism.Events;
using BMES.Core.Models;

namespace BMES.Contracts.Events
{
    public class RecipeSelectedEvent : PubSubEvent<Recipe>
    {
    }
}
