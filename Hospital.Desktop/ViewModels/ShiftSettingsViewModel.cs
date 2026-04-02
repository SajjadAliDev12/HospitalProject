using Hospital.Core.DTOs;
using Hospital.Core.Models;
using Hospital.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class ShiftSettingsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private DateOnly _referenceDate;

        public ObservableCollection<TeamItemViewModel> Teams { get; set; } = new();
        public ObservableCollection<EmployeeLookupDto> Employees { get; set; } = new();

        public DateOnly ReferenceDate
        {
            get => _referenceDate;
            set { _referenceDate = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }

        public ShiftSettingsViewModel()
        {
            _apiService = new ApiService();
            SaveCommand = new RelayCommand(async (p) => await SaveSettings());
            RefreshCommand = new RelayCommand((p) => LoadInitialData());

            LoadInitialData();
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

        private void HandleTeamSearch(TeamItemViewModel item, string text)
        {
            // تجنب البحث إذا كان النص هو نفسه اسم المشرف المختار حالياً
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
                    // لتجنب اختفاء النص، نقوم بتحديث القائمة دون مسحها بالكامل إذا كانت النتائج متطابقة
                    // أو التأكد من أن التحديث لا يؤثر على النص المكتوب
                    Employees.Clear();
                    if (res != null)
                    {
                        foreach (var emp in res) Employees.Add(emp);
                    }
                });
            }
            catch { /* تجاهل أخطاء البحث */ }
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

                // تحديث نص البحث ليطابق الاسم المختار عند اختيار عنصر من القائمة
                if (value.HasValue && !_isUpdatingInternally)
                {
                    var emp = _parent.Employees.FirstOrDefault(e => e.Id == value);
                    if (emp != null)
                    {
                        _isUpdatingInternally = true;
                        LocalSearchText = emp.Name;
                        Data.SupervisorName = emp.Name; // تحديث الاسم في الـ DTO لضمان عدم البحث عنه مجدداً
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