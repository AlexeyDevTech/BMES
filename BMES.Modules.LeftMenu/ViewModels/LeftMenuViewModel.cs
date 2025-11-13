using BMES.Regions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace BMES.Modules.LeftMenu.ViewModels
{
    public class LeftMenuViewModel : BindableBase
    {
        private IRegionManager _reg;

        public ICommand SelectMenuCommand { get; set; }
        public LeftMenuViewModel(IRegionManager reg)
        {
            _reg = reg;
            SelectMenuCommand = new DelegateCommand<object>(SelectMenu);

        }
        private void SelectMenu(object obj)
        {
            var menuItem = Int32.Parse(obj as string);
            switch (menuItem)
            {
                case 1:
                    _reg.RequestNavigate(RegionNames.ContentRegion, "ProductionViewer");
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
            }
        }
    }
}
