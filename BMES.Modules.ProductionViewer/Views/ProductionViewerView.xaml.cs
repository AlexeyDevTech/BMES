using BMES.Modules.ProductionViewer.ViewModels;
using System.Windows.Controls;
using Unity;

namespace BMES.Modules.ProductionViewer.Views
{
    /// <summary>
    /// Interaction logic for ProductionViewerView
    /// </summary>
    public partial class ProductionViewerView : UserControl
    {
        public ProductionViewerView(IUnityContainer container)
        {
            InitializeComponent();
            DataContext = container.Resolve<ProductionViewerViewModel>();
        }
    }
}
