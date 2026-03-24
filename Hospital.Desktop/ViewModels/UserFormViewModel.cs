using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace Hospital.Desktop.ViewModels
{
    public class UserFormViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public UserFormDTO User { get; set; }
        public ObservableCollection<string> Roles { get; set; } = new() { "Admin", "User", "Manager" };
        public ObservableCollection<dynamic> Employees { get; set; } = new();

        public bool IsAddMode { get; set; }
        public bool IsEditMode { get; set; }
        public bool IsViewMode { get; set; }

        // حدث لإخبار الواجهة بإغلاق النافذة
        public event Action RequestClose;

        private bool _linkEmployee;
        public bool LinkEmployee
        {
            get => _linkEmployee;
            set { _linkEmployee = value; if (!value) User.EmployeeId = null; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }

        public UserFormViewModel(UserFormDTO user = null, string mode = "Add")
        {
            _apiService = new ApiService();
            IsAddMode = mode == "Add";
            IsEditMode = mode == "Edit";
            IsViewMode = mode == "View";

            User = user ?? new UserFormDTO { IsActive = true, IsDeleted = false };
            LinkEmployee = User.EmployeeId.HasValue;

            LoadEmployees();

            // تصحيح: تمرير الـ PasswordBox كبارامتر
            SaveCommand = new RelayCommand(async (p) => await SaveUser(p), (p) => !IsViewMode);
        }

        private async void LoadEmployees()
        {
            var result = await _apiService.GetAsync<List<dynamic>>("Employees");
            if (result != null)
                foreach (var emp in result) Employees.Add(emp);
        }

        private async Task SaveUser(object parameter)
        {
            // جلب كلمة المرور يدوياً في حالة الإضافة فقط
            if (IsAddMode && parameter is PasswordBox passBox)
            {
                User.Password = passBox.Password;
                if (string.IsNullOrWhiteSpace(User.Password))
                {
                    MessageBox.Show("كلمة المرور مطلوبة للحسابات الجديدة.");
                    return;
                }
            }

            try
            {
                if (IsAddMode)
                    await _apiService.PostAsync<dynamic>("Auth/Register", User);
                else
                    await _apiService.PutAsync<dynamic>("Auth/UpdateUser", User);

                MessageBox.Show("تمت العملية بنجاح.");
                RequestClose?.Invoke(); // تنفيذ الإغلاق
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}