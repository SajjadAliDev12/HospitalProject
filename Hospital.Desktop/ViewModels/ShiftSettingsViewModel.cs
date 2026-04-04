using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using Hospital.Core.Models;
using Hospital.Desktop.Services;
using Hospital.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class ShiftSettingsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private DateOnly _referenceDate;

        public ObservableCollection<TeamItemViewModel> Teams { get; set; } = new();
        public ObservableCollection<EmployeeLookupDto> Employees { get; set; } = new();
        public ObservableCollection<DepartmentDto> Departments { get; set; } = new();

        public DateOnly ReferenceDate
        {
            get => _referenceDate;
            set { _referenceDate = value; OnPropertyChanged(); }
        }

        private int _selectedPrintScope;
        public int SelectedPrintScope
        {
            get => _selectedPrintScope;
            set
            {
                _selectedPrintScope = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDeptSelectorVisible));
            }
        }

        public bool IsDeptSelectorVisible => SelectedPrintScope == 0;

        private int _selectedPrintShiftType;
        public int SelectedPrintShiftType
        {
            get => _selectedPrintShiftType;
            set { _selectedPrintShiftType = value; OnPropertyChanged(); }
        }

        private int? _selectedPrintDeptId;
        public int? SelectedPrintDeptId
        {
            get => _selectedPrintDeptId;
            set { _selectedPrintDeptId = value; OnPropertyChanged(); }
        }

        private DateTime _reportMonth = DateTime.Now;
        public DateTime ReportMonth
        {
            get => _reportMonth;
            set { _reportMonth = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand PreviewReportCommand { get; }
        public ICommand PrintReportCommand { get; }

        public ShiftSettingsViewModel()
        {
            _apiService = new ApiService();
            SaveCommand = new RelayCommand(async (p) => await SaveSettings());
            RefreshCommand = new RelayCommand((p) => LoadInitialData());
            PreviewReportCommand = new RelayCommand(async (p) => await GenerateReport(true));
            PrintReportCommand = new RelayCommand(async (p) => await GenerateReport(false));

            LoadInitialData();
            LoadDepartments();
        }
        private Dictionary<int, List<int>> CalculateMonthlyShifts(int year, int month)
        {
            // إنشاء قاموس يحتوي على 4 مفاتيح (تمثل الفرق 1، 2، 3، 4)
            var groups = new Dictionary<int, List<int>>
    {
        { 1, new List<int>() },
        { 2, new List<int>() },
        { 3, new List<int>() },
        { 4, new List<int>() }
    };

            // عدد أيام الشهر المختار (مثلاً 30 أو 31)
            int daysInMonth = DateTime.DaysInMonth(year, month);

            // تحويل التاريخ المرجعي إلى DateTime للقيام بالحسابات
            // ملاحظة: ReferenceDate هو الخاصية التي نربطها بالـ DatePicker في الواجهة
            DateTime start = ReferenceDate.ToDateTime(TimeOnly.MinValue);

            for (int day = 1; day <= daysInMonth; day++)
            {
                // التاريخ الحالي الذي نقوم بفحصه داخل الحلقة
                DateTime currentDay = new DateTime(year, month, day);

                // حساب الفرق بالأيام بين اليوم الحالي وبداية الدورة (التاريخ المرجعي)
                int daysDifference = (currentDay - start).Days;

                // تطبيق منطق الـ Modulo 4 للحصول على ترتيب الفريق المستلم (0, 1, 2, 3)
                // أضفنا +4 ثم %4 لضمان التعامل مع التواريخ التي تسبق التاريخ المرجعي (نتائج سالبة)
                int teamIndex = ((daysDifference % 4) + 4) % 4;

                // إضافة رقم اليوم (day) إلى قائمة الفريق المناسب (نضيف 1 لأن الـ Index يبدأ من 0)
                groups[teamIndex + 1].Add(day);
            }

            return groups;
        }
        private async void LoadInitialData()
        {
            try
            {
                var setting = await _apiService.GetAsync<SystemSettingDto>("Shifts/settings");
                if (setting != null)
                    ReferenceDate = setting.ShiftReferenceDate;

                var result = await _apiService.GetAsync<List<NightShiftTeamDto>>("NightShiftTeams");
                if (result != null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Teams.Clear();
                        foreach (var dto in result)
                        {
                            var teamVM = new TeamItemViewModel(dto, this);
                            teamVM.SearchTextChanged += (item, text) => HandleTeamSearch(item, text);
                            Teams.Add(teamVM);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل الاتصال أو تحميل البيانات: " + ex.Message);
            }
        }

        private async void LoadDepartments()
        {
            try
            {
                var res = await _apiService.GetAsync<List<DepartmentDto>>("Departments");
                if (res != null)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Departments.Clear();
                        foreach (var d in res) Departments.Add(d);
                    });
                }
            }
            catch { }
        }

        private void HandleTeamSearch(TeamItemViewModel item, string text)
        {
            if (item.Data.SupervisorName == text) return;

            if (text?.Length >= 3)
            {
                _ = SearchEmployees(text);
                item.IsLocalDropDownOpen = true;
            }
            else
            {
                item.IsLocalDropDownOpen = false;
            }
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
            catch { }
        }

        private async Task SaveSettings()
        {
            var supervisorIds = Teams.Where(t => t.SupervisorId.HasValue)
                                     .Select(t => t.SupervisorId.Value)
                                     .ToList();

            if (supervisorIds.Count != supervisorIds.Distinct().Count())
            {
                MessageBox.Show("خطأ: لا يمكن تعيين نفس الموظف كمسؤول لأكثر من خفرة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                await _apiService.PutAsync<dynamic>($"Shifts/settings?newDate={ReferenceDate:yyyy-MM-dd}", null);

                foreach (var teamItem in Teams)
                {
                    await _apiService.PutAsync<dynamic>($"NightShiftTeams/{teamItem.Id}", teamItem.Data);
                }

                MessageBox.Show("تم حفظ إعدادات النظام وتوزيع المشرفين بنجاح.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء الحفظ: " + ex.Message);
            }
        }

        private async Task GenerateReport(bool isPreview)
        {
            try
            {
                // 1. تجهيز الباراميترات لإرسالها للسيرفر بناءً على خيارات المستخدم في الواجهة
                string url = $"Employees/report-data?month={ReportMonth:yyyy-MM}";

                if (SelectedPrintScope == 0 && SelectedPrintDeptId.HasValue) // قسم محدد
                {
                    url += $"&departmentId={SelectedPrintDeptId}";
                }

                if (SelectedPrintShiftType == 0) url += $"&shiftType={(int)enShiftType.Morning}";
                else if (SelectedPrintShiftType == 1) url += $"&shiftType={(int)enShiftType.Night}";
                // إذا كان النوع (الكل) لا نرسل shiftType لكي يجلب السيرفر الجميع

                // 2. جلب الموظفين المفلترين من السيرفر
                var employeesForReport = await _apiService.GetAsync<List<EmployeeReportDto>>(url);

                if (employeesForReport == null || !employeesForReport.Any())
                {
                    MessageBox.Show("لا توجد بيانات موظفين مطابقة للخيارات المختارة.", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 3. حساب توزيع أيام الشهر على الفرق الأربعة (المحرك الرياضي)
                var shiftCalendar = CalculateMonthlyShifts(ReportMonth.Year, ReportMonth.Month);

                // 4. تحديد عنوان التقرير
                string deptName = SelectedPrintScope == 0 ?
                    Departments.FirstOrDefault(d => d.Id == SelectedPrintDeptId)?.Name : "كافة الأقسام";

                string title = $"جدول دوام ({deptName}) لشهر {ReportMonth:MMMM yyyy}";

                // 5. توليد المستند (FlowDocument) باستخدام المصنع الذي بنيناه في الخطوة 2
                var document = ReportGenerator.CreateShiftReport(title, employeesForReport, shiftCalendar);

                // 6. عرض النتيجة
                if (isPreview)
                {
                    // فتح نافذة المعاينة التي أنشأناها في الخطوة 2
                    var previewWindow = new ReportPreviewWindow(document);
                    previewWindow.ShowDialog();
                }
                else
                {
                    // طباعة فورية بدون معاينة
                    PrintDialog printDialog = new PrintDialog();
                    if (printDialog.ShowDialog() == true)
                    {
                        printDialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "Printing Shift Report");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء توليد التقرير: " + ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class TeamItemViewModel : BaseViewModel
    {
        private readonly ShiftSettingsViewModel _parent;
        public NightShiftTeamDto Data { get; set; }
        public int Id => Data.Id;

        private bool _isUpdatingInternally;

        public int? SupervisorId
        {
            get => Data.SupervisorId;
            set
            {
                if (Data.SupervisorId == value) return;
                Data.SupervisorId = value;
                OnPropertyChanged();

                if (value.HasValue && !_isUpdatingInternally)
                {
                    var emp = _parent.Employees.FirstOrDefault(e => e.Id == value);
                    if (emp != null)
                    {
                        _isUpdatingInternally = true;
                        LocalSearchText = emp.Name;
                        Data.SupervisorName = emp.Name;
                        _isUpdatingInternally = false;
                    }
                }
            }
        }

        private string _localSearchText;
        public string LocalSearchText
        {
            get => _localSearchText;
            set
            {
                if (_localSearchText == value) return;
                _localSearchText = value;
                OnPropertyChanged();

                if (!_isUpdatingInternally)
                {
                    SearchTextChanged?.Invoke(this, value);
                }
            }
        }

        private bool _isLocalDropDownOpen;
        public bool IsLocalDropDownOpen
        {
            get => _isLocalDropDownOpen;
            set { _isLocalDropDownOpen = value; OnPropertyChanged(); }
        }

        public event Action<TeamItemViewModel, string> SearchTextChanged;

        public TeamItemViewModel(NightShiftTeamDto data, ShiftSettingsViewModel parent)
        {
            Data = data;
            _parent = parent;
            _localSearchText = data.SupervisorName;
        }
    }
}