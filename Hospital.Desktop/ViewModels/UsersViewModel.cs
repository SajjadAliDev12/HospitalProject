using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
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
        public UsersViewModel()
        {
            _apiService = new ApiService();
            Users = new ObservableCollection<UserViewDTO>();
            RefreshCommand = new RelayCommand((p) => LoadUsers());
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
                    LoadUsers(); // إعادة تحديث القائمة لرؤية التغيير
                }
            }
        });

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}