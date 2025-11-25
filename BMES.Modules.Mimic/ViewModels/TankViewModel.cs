namespace BMES.Modules.Mimic.ViewModels
{
    public class TankViewModel : EquipmentViewModel
    {
        private double _level;
        public double Level
        {
            get { return _level; }
            set { SetProperty(ref _level, value); }
        }
    }
}
