using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class LeaveFormViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _empSearchText = null!;
        private string _subSearchText = null!;
        private bool _isEmpDropDownOpen;
        private bool _isSubDropDownOpen;
        private CancellationTokenSource _ctsEmp = null!;
        private CancellationTokenSource _ctsSub = null!;

        public LeaveFullDto Leave { get; set; }
        public bool IsEditMode { get; }
        public ObservableCollection<EmployeeLookupDto> Employees { get; set; } = new();
        public ObservableCollection<EmployeeLookupDto> SubEmployees { get; set; } = new();

        public bool IsEmpDropDownOpen
        {
            get => _isEmpDropDownOpen;
            set { _isEmpDropDownOpen = value; OnPropertyChanged(); }
        }

        public bool IsSubDropDownOpen
        {
            get => _isSubDropDownOpen;
            set { _isSubDropDownOpen = value; OnPropertyChanged(); }
        }

        public string EmpSearchText
        {
            get => _empSearchText;
            set
            {
                if (_empSearchText == value) return;
                _empSearchText = value;
                OnPropertyChanged();

                // منع البحث إذا كان النص يطابق الاسم المختار فعلاً
                var selected = Employees.FirstOrDefault(e => e.Id == Leave.EmployeeId);
                if (selected != null && selected.Name == value) return;

                if (value?.Length >= 3) { _ = SearchEmployees(value, true); IsEmpDropDownOpen = true; }
                else IsEmpDropDownOpen = false;
            }
        }

        public string SubSearchText
        {
            get => _subSearchText;
            set
            {
                if (_subSearchText == value) return;
                _subSearchText = value;
                OnPropertyChanged();

                var selected = SubEmployees.FirstOrDefault(e => e.Id == Leave.SubEmployeeId);
                if (selected != null && selected.Name == value) return;

                if (value?.Length >= 3) { _ = SearchEmployees(value, false); IsSubDropDownOpen = true; }
                else IsSubDropDownOpen = false;
            }
        }

        public event Action? RequestClose;
        public ICommand SaveCommand { get; }

        public LeaveFormViewModel(LeaveFullDto existingLeave = null)
        {
            _apiService = new ApiService();
            IsEditMode = existingLeave != null;
            Leave = existingLeave ?? new LeaveFullDto { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(1), Duration = 1 };

            // في حالة التعديل، نحتاج إضافة الأسماء الحالية للقوائم لكي لا تظهر فارغة
            if (IsEditMode)
            {
                Employees.Add(new EmployeeLookupDto { Id = Leave.EmployeeId, Name = Leave.EmployeeName });
                SubEmployees.Add(new EmployeeLookupDto { Id = Leave.SubEmployeeId, Name = Leave.SubEmployeeName });
                _empSearchText = Leave.EmployeeName;
                _subSearchText = Leave.SubEmployeeName;
            }

            SaveCommand = new RelayCommand(async (p) => await Save());
        }

        private async Task SearchEmployees(string term, bool isMain)
        {
            // إلغاء الطلب السابق
            if (isMain) { _ctsEmp?.Cancel(); _ctsEmp = new CancellationTokenSource(); }
            else { _ctsSub?.Cancel(); _ctsSub = new CancellationTokenSource(); }

            try
            {
                await Task.Delay(400); // Debounce

                var res = await _apiService.GetAsync<List<EmployeeLookupDto>>($"Employees/Search?term={term}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (isMain) UpdateCollection(Employees, res, Leave.EmployeeId);
                    else UpdateCollection(SubEmployees, res, Leave.SubEmployeeId);
                });
            }
            catch (Exception) { }
        }

        private void UpdateCollection(ObservableCollection<EmployeeLookupDto> collection, List<EmployeeLookupDto> results, int currentId)
        {
            // حفظ العنصر المختار حالياً
            var currentItem = collection.FirstOrDefault(e => e.Id == currentId);

            collection.Clear();

            // إعادة العنصر المختار أولاً لمنع اختفاء النص
            if (currentItem != null) collection.Add(currentItem);

            if (results != null)
            {
                foreach (var emp in results)
                {
                    if (emp.Id != currentId) collection.Add(emp);
                }
            }
        }

        private async Task Save()
        {
            if (Leave.EmployeeId == 0 || Leave.SubEmployeeId == 0)
            {
                MessageBox.Show("يرجى اختيار الموظف والبديل أولاً");
                return;
            }
            try
            {
                if (IsEditMode) await _apiService.PutAsync<dynamic>($"Leaves/{Leave.Id}", Leave);
                else await _apiService.PostAsync<dynamic>("Leaves", Leave);
                RequestClose?.Invoke();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
