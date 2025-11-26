using Prism.Mvvm;
using Prism.Commands;
using System.Threading.Tasks;

namespace BMES.Modules.Genealogy.ViewModels
{
    public class GenealogyViewModel : BindableBase
    {
        private string _productId;
        public string ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        private string _genealogyTree;
        public string GenealogyTree
        {
            get => _genealogyTree;
            set => SetProperty(ref _genealogyTree, value);
        }

        public DelegateCommand ShowGenealogyCommand { get; private set; }

        public GenealogyViewModel()
        {
            ShowGenealogyCommand = new DelegateCommand(async () => await ExecuteShowGenealogyCommand(), CanShowGenealogy);
        }

        private bool CanShowGenealogy()
        {
            return !string.IsNullOrWhiteSpace(ProductId);
        }

        private async Task ExecuteShowGenealogyCommand()
        {
            // Placeholder for genealogy logic
            GenealogyTree = $"Genealogy for Product ID: {ProductId}\n" +
                            "Product -> Order #123 -> Material Lot #XYZ (Placeholder)";
            await Task.CompletedTask; // Simulate async operation
        }
    }
}
