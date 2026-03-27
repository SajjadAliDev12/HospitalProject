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
            if (!(param is UserViewDTO user)) return;

            string actionText = user.IsDeleted ? "استعادة" : "حذف";
            var confirm = MessageBox.Show($"هل أنت متأكد من {actionText} المستخدم {user.UserName}؟",
                                        "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                if (user.IsDeleted)
                {
                    // منطق الاستعادة: تغيير الحالة وإرسال طلب تحديث
                    user.IsDeleted = false;
                    user.IsActive = true;

                    // ملاحظة: تأكد أن الـ API يتوقع UserViewDTO أو قم بتحويله لـ UserFormDTO إذا لزم الأمر
                    var result = await _apiService.PutAsync<dynamic>("Auth/UpdateUser", user);

                    if (result != null)
                        MessageBox.Show("تمت استعادة الحساب بنجاح.");
                    else
                        { user.IsDeleted = true;
                        user.IsActive = false;  }// إعادة الحالة للأصل في حال فشل السيرفر
                }
                else
                {
                    // منطق الحذف العادي (Soft Delete)
                    string endpoint = $"Auth/{user.Id}";
                    var result = await _apiService.DeleteAsync<dynamic>(endpoint);

                    if (result != null)
                        MessageBox.Show("تم الحذف بنجاح.");
                }

                LoadUsers(); // تحديث الجدول
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في العملية: {ex.Message}");
                LoadUsers(); // إعادة جلب البيانات لضمان دقة الحالة المعروضة
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