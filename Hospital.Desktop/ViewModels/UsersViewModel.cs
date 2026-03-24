using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using Hospital.Desktop.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class UsersViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        public ObservableCollection<UserViewDTO> Users { get; set; }
        private bool? _isActiveFilter = null; // null = الكل
        public bool? IsActiveFilter
        {
            get => _isActiveFilter;
            set { _isActiveFilter = value; LoadUsers(); OnPropertyChanged(nameof(IsActiveFilter)); }
        }

        private bool? _isDeletedFilter = false; // الافتراضي غير المحذوفين
        public bool? IsDeletedFilter
        {
            get => _isDeletedFilter;
            set { _isDeletedFilter = value; LoadUsers(); OnPropertyChanged(nameof(IsDeletedFilter)); }
        }
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); }
        }
        public ICommand RefreshCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand EditUserCommand { get; }
        public UsersViewModel()
        {
            _apiService = new ApiService();
            Users = new ObservableCollection<UserViewDTO>();
            RefreshCommand = new RelayCommand((p) => LoadUsers());
            AddUserCommand = new RelayCommand((p) => OpenUserForm(null, "Add"));

            ViewDetailsCommand = new RelayCommand((p) => OpenUserForm(p as UserViewDTO, "View"));

            EditUserCommand = new RelayCommand((p) => OpenUserForm(p as UserViewDTO, "Edit"));
            LoadUsers();
        }

        public async void LoadUsers()
        {
            try
            {
                IsLoading = true;
                string url = "Auth/Users?";
                if (IsActiveFilter.HasValue) url += $"IsActive={IsActiveFilter.Value}&";
                if (IsDeletedFilter.HasValue) url += $"IsDeleted={IsDeletedFilter.Value}";
                var result = await _apiService.GetAsync<List<UserViewDTO>>(url);

                Users.Clear();
                foreach (var user in result)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل جلب المستخدمين: " + ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
        public ICommand DeleteUserCommand => new RelayCommand(async (param) => {
            var user = param as UserViewDTO;
            if (user == null) return;

            var confirm = MessageBox.Show($"هل أنت متأكد من حذف {user.UserName}؟", "تأكيد", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                string endpoint = $"Auth/{user.Id}";
                var result = await _apiService.DeleteAsync<dynamic>(endpoint);
                if (result != null)
                {
                    MessageBox.Show("تم الحذف بنجاح");
                    LoadUsers(); 
                }
            }
        });
        private void OpenUserForm(UserViewDTO? selectedUser, string mode)
        {
            UserFormDTO formDto;

            if (selectedUser != null)
            {
                
                formDto = new UserFormDTO
                {
                    Id = selectedUser.Id,
                    UserName = selectedUser.UserName,
                    FullName = selectedUser.FullName,
                    Role = selectedUser.Role,
                    EmployeeId = selectedUser.EmployeeId,
                    IsActive = selectedUser.IsActive,
                    IsDeleted = selectedUser.IsDeleted
                };
            }
            else
            {
                formDto = new UserFormDTO();
            }

            // إنشاء النافذة وتمرير الـ ViewModel لها
            var formWindow = new UserFormView();
            formWindow.DataContext = new UserFormViewModel(formDto, mode);

            // عرض النافذة كـ Dialog (تتوقف الشاشة الخلفية حتى تُغلق)
            if (formWindow.ShowDialog() == true || mode != "View")
            {
                // تحديث القائمة بعد الإغلاق إذا حدث تغيير
                LoadUsers();
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}