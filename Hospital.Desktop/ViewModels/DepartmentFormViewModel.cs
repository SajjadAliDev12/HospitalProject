using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class DepartmentFormViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly int? _departmentId;
        private string _departmentName;

        public string DepartmentName
        {
            get => _departmentName;
            set { _departmentName = value; OnPropertyChanged(); }
        }

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
                    var dto = new DepartmentDto { Id = _departmentId.Value, Name = DepartmentName };
                    await _apiService.PutAsync<dynamic>($"Departments/{_departmentId}", dto);
                }
                else
                {
                    var dto = new CreateDepartmentDto { Name = DepartmentName };
                    await _apiService.PostAsync<dynamic>("Departments", dto);
                }

                MessageBox.Show("تم حفظ البيانات بنجاح.");
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في الحفظ: " + ex.Message);
            }
        }
    }
}