using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using Hospital.Desktop.Services;
using Hospital.Desktop.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class EmployeesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _searchText;
        private int _selectedStatusFilter = 0; // 0: All (Active), 1: Deleted
        private int _selectedGenderFilter = 0; // 0: All, 1: Male, 2: Female
        private bool _isLoading;

        public ObservableCollection<EmployeeSimpleDTO> Employees { get; set; }
        public ICollectionView FilteredEmployees { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }
        private int _currentPage = 1;
        private int _totalPages;
        private int _pageSize = 15;

        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); }
        }

        public int TotalPages
        {
            get => _totalPages;
            set { _totalPages = value; OnPropertyChanged(); }
        }

        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();

                // تنفيذ البحث فقط إذا كان النص فارغاً (لإعادة الكل) أو أكبر من أو يساوي 3 أحرف
                if (string.IsNullOrEmpty(value) || value.Length >= 3)
                {
                    CurrentPage = 1; // العودة للصفحة الأولى عند البحث
                    LoadEmployees();
                }
            }
        }

        public int SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                _selectedStatusFilter = value;
                OnPropertyChanged();
                LoadEmployees(); // إعادة الجلب من السيرفر لأن الـ API يدعم فلتر الحذف
            }
        }

        public int SelectedGenderFilter
        {
            get => _selectedGenderFilter;
            set { _selectedGenderFilter = value; OnPropertyChanged(); FilteredEmployees.Refresh(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddEmployeeCommand { get; }
        public ICommand EditEmployeeCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }

        public EmployeesViewModel()
        {
            _apiService = new ApiService();
            Employees = new ObservableCollection<EmployeeSimpleDTO>();
            NextPageCommand = new RelayCommand((p) => { CurrentPage++; LoadEmployees(); }, (p) => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand((p) => { CurrentPage--; LoadEmployees(); }, (p) => CurrentPage > 1);
            // إعداد الفلترة المحلية
            FilteredEmployees = CollectionViewSource.GetDefaultView(Employees);
            FilteredEmployees.Filter = FilterLogic;

            RefreshCommand = new RelayCommand((p) => LoadEmployees());
            AddEmployeeCommand = new RelayCommand((p) => OpenEmployeeForm(null, "Add"));
            EditEmployeeCommand = new RelayCommand((p) => OpenEmployeeForm(p, "Edit"));
            ViewDetailsCommand = new RelayCommand((p) => OpenEmployeeForm(p, "View"));
            DeleteEmployeeCommand = new RelayCommand(async (p) => await DeleteEmployee(p));

            LoadEmployees();
        }

        private bool FilterLogic(object obj)
        {
            if (!(obj is EmployeeSimpleDTO emp)) return false;

            // 1. فلتر البحث بالاسم
            bool matchesName = string.IsNullOrEmpty(SearchText) ||
                               emp.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

            // 2. فلتر الجنس
            bool matchesGender = true;
            if (SelectedGenderFilter == 1) matchesGender = emp.Gender == enGender.Male;
            else if (SelectedGenderFilter == 2) matchesGender = emp.Gender == enGender.Female;

            return matchesName && matchesGender;
        }

        public async void LoadEmployees()
        {
            try
            {
                IsLoading = true;
                bool isDeletedParam = SelectedStatusFilter == 1;

                // إرسال searchTerm للسيرفر
                string url = $"Employees?IsDeleted={isDeletedParam}&page={CurrentPage}&pageSize={_pageSize}&searchTerm={SearchText}";

                var result = await _apiService.GetAsync<PagedResult<EmployeeSimpleDTO>>(url);

                Employees.Clear();
                if (result != null)
                {
                    foreach (var emp in result.Items) Employees.Add(emp);
                    TotalPages = result.TotalPages;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private async void OpenEmployeeForm(object parameter, string mode)
        {
            if (mode == "View" && parameter == null) return; // حماية للنقر المزدوج على فراغ

            EmployeeFullDTO employeeData = null;
            if (parameter is EmployeeSimpleDTO simple)
            {
                try
                {
                    IsLoading = true;
                    employeeData = await _apiService.GetAsync<EmployeeFullDTO>($"Employees/{simple.Id}");
                }
                catch (Exception ex) { MessageBox.Show("خطأ أثناء الاتصال: " + ex.Message); return; }
                finally { IsLoading = false; }
            }
            else
            {
                employeeData = new EmployeeFullDTO
                {
                    BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                    HireDate = DateOnly.FromDateTime(DateTime.Now)
                };
            }

            if (employeeData == null && mode != "Add") return;

            var form = new EmployeeFormView();
            var viewModel = new EmployeeFormViewModel(employeeData, mode);
            viewModel.RequestClose += () => form.Close();
            form.DataContext = viewModel;
            form.ShowDialog();

            if (mode != "View") LoadEmployees();
        }

        private async Task DeleteEmployee(object parameter)
        {
            if (!(parameter is EmployeeSimpleDTO emp)) return;

            // تغيير الرسالة بناءً على الحالة
            string actionText = emp.IsDeleted ? "استعادة" : "حذف";
            var confirm = MessageBox.Show($"هل أنت متأكد من {actionText} الموظف {emp.Name}؟", "تأكيد", MessageBoxButton.YesNo);

            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    if (emp.IsDeleted)
                    {
                        // منطق الاستعادة: نستخدم Put لتحديث حالة isDeleted إلى false
                        var fullEmp = await _apiService.GetAsync<EmployeeFullDTO>($"Employees/{emp.Id}");
                        if (fullEmp != null)
                        {
                            fullEmp.IsDeleted = false;
                            await _apiService.PutAsync<dynamic>($"Employees/{emp.Id}", fullEmp);
                        }
                    }
                    else
                    {
                        // منطق الحذف العادي
                        await _apiService.DeleteAsync<dynamic>($"Employees/{emp.Id}");
                    }
                    LoadEmployees();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
    }
}