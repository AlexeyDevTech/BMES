using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BMES.Modules.Header.ViewModels
{
    public class HeaderViewModel : BindableBase
    {
        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }

        private string _userRole;
        public string UserRole
        {
            get { return _userRole; }
            set { SetProperty(ref _userRole, value); }
        }

        private string _userInitials;
        public string UserInitials
        {
            get { return _userInitials; }
            set { SetProperty(ref _userInitials, value); }
        }

        public HeaderViewModel()
        {
            UserName = "Иванов Иван Иванович";
            UserRole = "Исполнительный директор";
            UserInitials = "ИИ";
        }
    }
}
