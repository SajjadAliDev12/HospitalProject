using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class DepartmentFormViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly int? _departmentId;

        private string _departmentName;
        private int? _managerId;
        private string? _managerOrderNumber;
        private DateOnly? _managerStartDate;
        private string _employeeSearchText;

        // الخصائص المرتبطة بالواجهة (Binding)
        public string DepartmentName
        {
            get => _departmentName;
            set { _departmentName = value; OnPropertyChanged(); }
        }

        public int? ManagerId
        {
            get => _managerId;
            set { _managerId = value; OnPropertyChanged(); }
        }

        public string? ManagerOrderNumber
        {
            get => _managerOrderNumber;
            set { _managerOrderNumber = value; OnPropertyChanged(); }
        }
        private string _managerSearchText;
        private bool _isManagerDropDownOpen;

        public string ManagerSearchText
        {
            get => _managerSearchText;
            set
            {
                if (_managerSearchText == value) return;
                _managerSearchText = value;
                OnPropertyChanged();

                // تجنب البحث إذا كان النص هو نفسه اسم المدير المختار حالياً
                var selected = Employees.FirstOrDefault(e => e.Id == ManagerId);
                if (selected != null && selected.Name == value) return;

                // ابدأ البحث بعد 3 أحرف
                if (value?.Length >= 3)
                {
                    _ = SearchEmployees(value);
                    IsManagerDropDownOpen = true;
                }
                else
                {
                    IsManagerDropDownOpen = false;
                }
            }
        }

        public bool IsManagerDropDownOpen
        {
            get => _isManagerDropDownOpen;
            set { _isManagerDropDownOpen = value; OnPropertyChanged(); }
        }
        public DateOnly? ManagerStartDate
        {
            get => _managerStartDate;
            set { _managerStartDate = value; OnPropertyChanged(); }
        }
        private async Task SearchEmployees(string term)
        {
            try
            {
                var res = await _apiService.GetAsync<List<EmployeeLookupDto>>($"Employees/Search?term={term}");

                App.Current.Dispatcher.Invoke(() =>
                {
                    Employees.Clear();
                    if (res != null)
                    {
                        foreach (var emp in res) Employees.Add(emp);
                    }
                });
            }
            catch (Exception ex)
            {
                
            }
        }
        public string EmployeeSearchText
        {
            get => _employeeSearchText;
            set { _employeeSearchText = value; OnPropertyChanged(); }
        }

        // قائمة الموظفين لاختيار المدير
        public ObservableCollection<EmployeeLookupDto> Employees { get; set; } = new();

        public bool IsEditMode => _departmentId.HasValue;
        public event Action RequestClose;
        public ICommand SaveCommand { get; }

        public DepartmentFormViewModel(DepartmentDto department = null)
        {
            _apiService = new ApiService();

            if (department != null)
            {
                _departmentId = department.Id;
                DepartmentName = department.Name;
                ManagerId = department.ManagerId;
                ManagerOrderNumber = department.ManagerOrderNumber;
                ManagerStartDate = department.ManagerStartDate;

                // إذا كان هناك مدير، نضيفه للقائمة مؤقتاً ونضع اسمه في مربع البحث
                if (department.ManagerId.HasValue && !string.IsNullOrEmpty(department.ManagerName))
                {
                    var currentManager = new EmployeeLookupDto { Id = department.ManagerId.Value, Name = department.ManagerName };
                    Employees.Add(currentManager);
                    _managerSearchText = department.ManagerName; 
                    OnPropertyChanged(nameof(ManagerSearchText));
                }
            }

            SaveCommand = new RelayCommand(async (p) => await Save());
        }

        

        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(DepartmentName))
            {
                MessageBox.Show("يرجى إدخال اسم القسم.");
                return;
            }

            try
            {
                if (IsEditMode)
                {
                    var dto = new DepartmentDto
                    {
                        Id = _departmentId.Value,
                        Name = DepartmentName,
                        ManagerId = ManagerId,
                        ManagerOrderNumber = ManagerOrderNumber,
                        ManagerStartDate = ManagerStartDate
                    };
                    await _apiService.PutAsync<dynamic>($"Departments/{_departmentId}", dto);
                }
                else
                {
                    var dto = new CreateDepartmentDto
                    {
                        Name = DepartmentName,
                        ManagerId = ManagerId,
                        ManagerOrderNumber = ManagerOrderNumber,
                        ManagerStartDate = ManagerStartDate
                    };
                    await _apiService.PostAsync<dynamic>("Departments", dto);
                }

                MessageBox.Show("تم حفظ بيانات القسم بنجاح.");
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في الحفظ: " + ex.Message);
            }
        }
    }
}