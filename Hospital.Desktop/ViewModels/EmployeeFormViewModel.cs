using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using Hospital.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class EmployeeFormViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        
        private EmployeeFullDTO _employee;
        public EmployeeFullDTO em
        {
            get => _employee;
            set
            {
                _employee = value;
                OnPropertyChanged(); 
            }
        }
        public ObservableCollection<JobTitleVeiwDTO> JobTitles { get; set; } = new();

        public bool IsAddMode { get; set; }
        public bool IsEditMode { get; set; }
        public bool IsViewMode { get; set; }

        public event Action RequestClose;

        public Array Genders => Enum.GetValues(typeof(enGender));
        public Array ShiftTypes => Enum.GetValues(typeof(enShiftType));
        public Array JobStatuses => Enum.GetValues(typeof(enJobStatus));
        public Array Certificates => Enum.GetValues(typeof(enCertificate));

        // قوائم يتم جلبها من الـ API
        public ObservableCollection<dynamic> Departments { get; set; } = new();
        

        public ICommand SaveCommand { get; }

        public EmployeeFormViewModel(EmployeeFullDTO employee = null, string mode = "Add")
        {
            _apiService = new ApiService();
            IsAddMode = mode == "Add";
            IsEditMode = mode == "Edit";
            IsViewMode = mode == "View";

            em = employee ?? new EmployeeFullDTO
            {
                BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-20)),
                HireDate = DateOnly.FromDateTime(DateTime.Now)
            };

            LoadLookups();

            SaveCommand = new RelayCommand(async (p) => await SaveEmployee(), (p) => !IsViewMode);
        }

        private async void LoadLookups()
        {
            try
            {
                var deps = await _apiService.GetAsync<List<DepartmentDto>>("Departments");
                if (deps != null)
                {
                    App.Current.Dispatcher.Invoke(() => {
                        Departments.Clear();
                        foreach (var d in deps) Departments.Add(d);
                    });
                }

                var jobs = await _apiService.GetAsync<List<JobTitleVeiwDTO>>("JobTitles");
                if (jobs != null)
                {
                    App.Current.Dispatcher.Invoke(() => {
                        JobTitles.Clear();
                        foreach (var j in jobs) JobTitles.Add(j);
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في تحميل القوائم: " + ex.Message);
            }
        }

        private async Task SaveEmployee()
        {
            try
            {
                if (IsAddMode)
                    await _apiService.PostAsync<dynamic>("Employees", em);
                else
                    await _apiService.PutAsync<dynamic>($"Employees/{em.Id}", em);

                MessageBox.Show("تم حفظ بيانات الموظف بنجاح");
                RequestClose?.Invoke();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}