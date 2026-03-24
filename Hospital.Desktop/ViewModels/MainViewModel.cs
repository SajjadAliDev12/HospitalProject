using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;


namespace Hospital.Desktop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }
        private string _selectedMenuTitle = "الرئيسية";
        public string SelectedMenuTitle
        {
            get => _selectedMenuTitle;
            set { _selectedMenuTitle = value; OnPropertyChanged(nameof(SelectedMenuTitle)); }
        }

        private string _currentTime;
        public string CurrentTime
        {
            get => _currentTime;
            set { _currentTime = value; OnPropertyChanged(nameof(CurrentTime)); }
        }
        public ICommand NavCommand { get; }
        public MainViewModel()
        {
            SelectedMenuTitle = "الرئيسية";

            // منطق التنقل
            NavCommand = new RelayCommand((param) =>
            {
                if (param == null) return;

                string destination = param.ToString();
                switch (destination)
                {
                    case "Users":
                        CurrentView = new UsersViewModel();
                        SelectedMenuTitle = "إدارة المستخدمين";
                        break;
                    case "Dashboard":
                        CurrentView = null;
                        SelectedMenuTitle = "الرئيسية";
                        break;
                    case "Leaves":
                        SelectedMenuTitle = "الأجازات";
                        break;
                    case "Absents":
                        SelectedMenuTitle = "الغيابات";
                        break;
                    case "Employees":
                        CurrentView = new EmployeesViewModel();
                        SelectedMenuTitle = "إدارة الموظفين";
                        break;
                    case "Transfers":
                        SelectedMenuTitle = "التنقلات";
                        break;
                    default:
                        SelectedMenuTitle = "الرئيسية";
                        break;
                }
            });
            // تحديث الساعة كل ثانية
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("yyyy/MM/dd  hh:mm:ss tt");
            timer.Start();
        }
        public void ShowUsersView()
        {
            SelectedMenuTitle = "إدارة المستخدمين";
            CurrentView = new UsersViewModel(); // سنحتاج لاستخدام DataTemplate لاحقاً
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
