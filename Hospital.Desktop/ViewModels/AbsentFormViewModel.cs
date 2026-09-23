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
    public class AbsentFormViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _searchText = null!;
        private CancellationTokenSource _cts = null!; // لمنع تداخل الطلبات

        public CreateAbsentDto Absent { get; set; } = new() { Date = DateTime.Now };
        public ObservableCollection<EmployeeLookupDto> Employees { get; set; } = new();

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText == value) return;
                _searchText = value;
                OnPropertyChanged();

                // حماية: إذا كان النص يطابق اسم الموظف المختار حالياً، لا تشغل البحث
                var selectedEmp = Employees.FirstOrDefault(e => e.Id == Absent.EmployeeId);
                if (selectedEmp != null && selectedEmp.Name == value) return;

                SearchEmployees(value);
            }
        }
        public bool IsSelecting = true;
        public event Action? RequestClose;
        public ICommand SaveCommand { get; }

        public AbsentFormViewModel()
        {
            _apiService = new ApiService();
            SaveCommand = new RelayCommand(async (p) => await Save());
        }

        private async void SearchEmployees(string term)
        {
            // إلغاء أي طلب بحث سابق لم يكتمل
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (string.IsNullOrWhiteSpace(term) || term.Length < 3) return;

            try
            {
                // تأخير بسيط (Debounce) لتقليل الضغط على السيرفر
                await Task.Delay(400, _cts.Token);

                var res = await _apiService.GetAsync<List<EmployeeLookupDto>>($"Employees/Search?term={term}");

                App.Current.Dispatcher.Invoke(() =>
                {
                    // حفظ الموظف المختار حالياً قبل مسح القائمة
                    var currentId = Absent.EmployeeId;
                    var currentEmp = Employees.FirstOrDefault(e => e.Id == currentId);

                    Employees.Clear();

                    // إعادة الموظف المختار أولاً لكي لا يختفي النص من الكومبو
                    if (currentEmp != null) Employees.Add(currentEmp);

                    if (res != null)
                    {
                        foreach (var e in res)
                        {
                            // تجنب تكرار الموظف المختار إذا ظهر في نتائج البحث
                            if (e.Id != currentId) Employees.Add(e);
                        }
                    }
                });
            }
            catch (TaskCanceledException) { /* تجاهل الخطأ عند الإلغاء */ }
            catch (Exception) { /* معالجة الأخطاء الأخرى */ }
        }

        private async Task Save()
        {
            if (Absent.EmployeeId == 0)
            {
                MessageBox.Show("يرجى اختيار الموظف أولاً.");
                return;
            }

            try
            {
                await _apiService.PostAsync<dynamic>("Absents", Absent);
                MessageBox.Show("تم تسجيل الغياب بنجاح.");
                RequestClose?.Invoke();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}
