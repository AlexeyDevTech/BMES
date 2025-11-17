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
        private double _outdoorTemperature = -1.2;
        private double _indoorTemperature = 21.1;
        private int _tvoHumidity = 34;

        public double OutdoorTemperature
        {
            get => _outdoorTemperature;
            set { SetProperty(ref _outdoorTemperature, value); }
        }

        public double IndoorTemperature
        {
            get => _indoorTemperature;
            set { SetProperty(ref _indoorTemperature, value); }
        }

        public int TvoHumidity
        {
            get => _tvoHumidity;
            set { SetProperty(ref _tvoHumidity, value); }
        }

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
