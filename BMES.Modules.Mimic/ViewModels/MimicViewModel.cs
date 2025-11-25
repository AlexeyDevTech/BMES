using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace BMES.Modules.Mimic.ViewModels
{
    public class MimicViewModel : BindableBase
    {
        public ObservableCollection<EquipmentViewModel> Equipments { get; set; }

        public MimicViewModel()
        {
            Equipments = new ObservableCollection<EquipmentViewModel>
            {
                new PumpViewModel { X = 100, Y = 100, State = "On" },
                new TankViewModel { X = 200, Y = 200, Level = 50 }
            };
        }
    }
}
