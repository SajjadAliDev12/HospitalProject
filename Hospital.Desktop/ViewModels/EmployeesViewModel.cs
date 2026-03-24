using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using Hospital.Desktop.Views; 
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class EmployeesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        public ObservableCollection<EmployeeSimpleDTO> Employees { get; set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
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

            
            RefreshCommand = new RelayCommand((p) => LoadEmployees());
            AddEmployeeCommand = new RelayCommand((p) => OpenEmployeeForm(null, "Add"));
            EditEmployeeCommand = new RelayCommand((p) => OpenEmployeeForm(p, "Edit"));
            ViewDetailsCommand = new RelayCommand((p) => OpenEmployeeForm(p, "View"));
            DeleteEmployeeCommand = new RelayCommand(async (p) => await DeleteEmployee(p));

            LoadEmployees();
        }

        public async void LoadEmployees()
        {
            try
            {
                IsLoading = true;
                // جلب القائمة المختصرة للجدول
                var result = await _apiService.GetAsync<List<EmployeeSimpleDTO>>("Employees");
                Employees.Clear();
                if (result != null)
                    foreach (var emp in result) Employees.Add(emp);
            }
            catch (Exception ex) { MessageBox.Show("فشل جلب الموظفين: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private async void OpenEmployeeForm(object parameter, string mode)
        {
            EmployeeFullDTO employeeData = null;
            if (parameter is EmployeeSimpleDTO simple)
            {
                try
                {
                    IsLoading = true; // تفعيل مؤشر التحميل

                    // جلب البيانات الكاملة من السيرفر باستخدام الـ ID
                    employeeData = await _apiService.GetAsync<EmployeeFullDTO>($"Employees/{simple.Id}");
                    IsLoading = false;

                    if (employeeData == null)
                    {
                        MessageBox.Show("فشل جلب بيانات الموظف من السيرفر.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    IsLoading = false;
                    MessageBox.Show("خطأ أثناء الاتصال: " + ex.Message);
                    return;
                }
            }
            else
            {
                // في حالة الإضافة ننشئ كائن جديد
                employeeData = new EmployeeFullDTO
                {
                    BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                    HireDate = DateOnly.FromDateTime(DateTime.Now)
                };
            }

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

            var confirm = MessageBox.Show($"هل أنت متأكد من حذف الموظف {emp.Name}؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm == MessageBoxResult.Yes)
            {
                try
                {
                    await _apiService.DeleteAsync<dynamic>($"Employees/{emp.Id}");
                    LoadEmployees();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

    }
}