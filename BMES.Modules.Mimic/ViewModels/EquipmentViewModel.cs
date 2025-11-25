using Prism.Mvvm;

namespace BMES.Modules.Mimic.ViewModels
{
    public abstract class EquipmentViewModel : BindableBase
    {
        private double _x;
        public double X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        private double _y;
        public double Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }

        private string _state;
        public string State
        {
            get { return _state; }
            set { SetProperty(ref _state, value); }
        }
    }
}
