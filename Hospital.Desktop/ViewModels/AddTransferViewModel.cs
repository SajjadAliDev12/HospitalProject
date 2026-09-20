using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using Hospital.Desktop.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class AddTransferViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private CancellationTokenSource _cts = null!;

        public CreateTransferLogDto Transfer { get; set; } = new() { TransferDate = DateOnly.FromDateTime(DateTime.Now) };
        public ObservableCollection<EmployeeLookupDto> Employees { get; set; } = new();
        public ObservableCollection<DepartmentDto> Departments { get; set; } = new();
        public Array ShiftTypes => Enum.GetValues(typeof(enShiftType));

        private string _searchText = null!;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();

                // 1. جلب الموظف المختار حالياً من القائمة
                var selectedEmp = Employees.FirstOrDefault(e => e.Id == Transfer.EmployeeId);

                // 2. إذا كان النص يطابق اسم الموظف المختار، توقف ولا تشغل البحث
                if (selectedEmp != null && selectedEmp.Name == value)
                    return;

                // 3. شروط البحث (على الأقل 3 أحرف)
                if (!string.IsNullOrWhiteSpace(value) && value.Length >= 3)
                    SearchEmployees(value);
            }
        }

        public event Action? RequestClose;
        public ICommand SaveCommand { get; }

        public AddTransferViewModel()
        {
            _apiService = new ApiService();
            LoadDepartments();
            SaveCommand = new RelayCommand(async (p) => await Save());
        }

        private async void LoadDepartments()
        {
            var res = await _apiService.GetAsync<List<DepartmentDto>>("Departments?IsDeleted=false");
            if (res != null) foreach (var d in res) Departments.Add(d);
        }

        private async void SearchEmployees(string term)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(400, _cts.Token);
                var res = await _apiService.GetAsync<List<EmployeeLookupDto>>($"Employees/Search?term={term}");

                App.Current.Dispatcher.Invoke(() => {
                    // حفظ الموظف المختار حالياً قبل المسح
                    var currentId = Transfer.EmployeeId;
                    var currentEmp = Employees.FirstOrDefault(e => e.Id == currentId);

                    Employees.Clear();

                    // إعادة الموظف المختار أولاً لكي لا يختفي من الواجهة
                    if (currentEmp != null)
                        Employees.Add(currentEmp);

                    if (res != null)
                    {
                        foreach (var e in res)
                        {
                            // تجنب إضافة الموظف المختار مرتين إذا ظهر في نتائج البحث
                            if (e.Id != currentId)
                                Employees.Add(e);
                        }
                    }
                });
            }
            catch (TaskCanceledException) { }
        }

        private async Task Save()
        {
            if (Transfer.EmployeeId == 0 || Transfer.NewDepartmentId == 0 || string.IsNullOrEmpty(Transfer.AdOrderNumber))
            {
                MessageBox.Show("يرجى ملء جميع الحقول الأساسية ورقم الأمر الإداري.");
                return;
            }

            try
            {
                await _apiService.PostAsync<dynamic>("TransferLog", Transfer);
                MessageBox.Show("تمت عملية النقل وتحديث ملف الموظف بنجاح.");
                RequestClose?.Invoke();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
