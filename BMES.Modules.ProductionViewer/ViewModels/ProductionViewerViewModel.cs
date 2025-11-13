using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BMES.Modules.ProductionViewer.ViewModels
{
    public class ProductionViewerViewModel : BindableBase, INavigationAware
    {
        public ProductionViewerViewModel()
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        //из...
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }
        
         //в...
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }
    }
}
