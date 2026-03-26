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
        private CancellationTokenSource _cts;
        public UserFormDTO User { get; set; }
        public ObservableCollection<string> Roles { get; set; } = new() { "Admin", "User", "Manager" };
        public ObservableCollection<EmployeeLookupDto> Employees { get; set; } = new();

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();

                if (IsViewMode) return;
                var currentEmployee = Employees.FirstOrDefault(e => e.Id == User.EmployeeId);

                // شرط الحماية: إذا كان النص يطابق اسم الموظف المختار، لا تطلق البحث
                if (currentEmployee != null && currentEmployee.Name == value)
                    return;

                if (!string.IsNullOrWhiteSpace(value) && value.Length > 1)
                    SearchEmployeesDebounced(value);
            }
        }
        public bool IsAddMode { get; set; }
        public bool IsEditMode { get; set; }
        public bool IsViewMode { get; set; }

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

            if (LinkEmployee) LoadCurrentEmployee(User.EmployeeId);

            SaveCommand = new RelayCommand(async (p) => await SaveUser(p), (p) => !IsViewMode);
        }
        private async void SearchEmployeesDebounced(string term)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(400, _cts.Token);

                var results = await _apiService.GetAsync<List<EmployeeLookupDto>>($"Employees/Search?term={term}");

                App.Current.Dispatcher.Invoke(() => {
                    // حفظ الموظف المختار حالياً قبل مسح القائمة لضمان عدم اختفائه من ComboBox
                    var selectedId = User.EmployeeId;
                    var selectedEmp = Employees.FirstOrDefault(e => e.Id == selectedId);

                    Employees.Clear();

                    // إعادة الموظف المختار أولاً (إذا لم يكن ضمن النتائج الجديدة)
                    if (selectedEmp != null)
                        Employees.Add(selectedEmp);

                    if (results != null)
                    {
                        foreach (var emp in results)
                        {
                            // تجنب إضافة الموظف المختار مرتين
                            if (emp.Id != selectedId)
                                Employees.Add(emp);
                        }
                    }
                });
            }
            catch (TaskCanceledException) { }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        private async void LoadCurrentEmployee(int? id)
        {
            if (!id.HasValue) return;

            var emp = await _apiService.GetAsync<EmployeeFullDTO>($"Employees/{id}");
            if (emp != null)
            {
                App.Current.Dispatcher.Invoke(() => {
                    Employees.Clear();
                    var lookup = new EmployeeLookupDto { Id = emp.Id, Name = emp.Name };
                    Employees.Add(lookup);

                    // تحديث النص الظاهري دون تفعيل البحث
                    _searchText = lookup.Name;
                    OnPropertyChanged(nameof(SearchText));
                });
            }
        }

        private async Task SaveUser(object parameter)
        {
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
                RequestClose?.Invoke();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}