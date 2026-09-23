using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    /// <summary>
    /// Admin dashboard: aggregates counts + recents + today's night-shift team.
    /// Every call is independently guarded — the dashboard renders partial data
    /// (or a friendly error) instead of crashing when the API is unreachable.
    /// </summary>
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly Action<string>? _navigate;

        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }

        private string? _errorMessage;
        public string? ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }

        private int _totalEmployees;
        public int TotalEmployees { get => _totalEmployees; set { _totalEmployees = value; OnPropertyChanged(); } }

        private int _totalDepartments;
        public int TotalDepartments { get => _totalDepartments; set { _totalDepartments = value; OnPropertyChanged(); } }

        private int _totalLeaves;
        public int TotalLeaves { get => _totalLeaves; set { _totalLeaves = value; OnPropertyChanged(); } }

        private int _totalAbsents;
        public int TotalAbsents { get => _totalAbsents; set { _totalAbsents = value; OnPropertyChanged(); } }

        private int _todayTeamId;
        public int TodayTeamId { get => _todayTeamId; set { _todayTeamId = value; OnPropertyChanged(); } }

        private string _todaySupervisor = "—";
        public string TodaySupervisor { get => _todaySupervisor; set { _todaySupervisor = value; OnPropertyChanged(); } }

        public string TodayDateStr => DateTime.Now.ToString("yyyy/MM/dd");

        private int _morningCount;
        public int MorningCount { get => _morningCount; set { _morningCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(MorningPct)); OnPropertyChanged(nameof(NightPct)); } }

        private int _nightCount;
        public int NightCount { get => _nightCount; set { _nightCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(MorningPct)); OnPropertyChanged(nameof(NightPct)); } }

        public int MorningPct => MorningCount + NightCount == 0 ? 0 : (int)Math.Round(MorningCount * 100.0 / (MorningCount + NightCount));
        public int NightPct => MorningCount + NightCount == 0 ? 0 : 100 - MorningPct;

        public ObservableCollection<LeaveFullDto> RecentLeaves { get; } = new();
        public ObservableCollection<AbsentFullDto> RecentAbsents { get; } = new();
        public ObservableCollection<DepartmentDto> TopDepartments { get; } = new();

        public ICommand RefreshCommand { get; }
        public ICommand NavigateCommand { get; }

        public DashboardViewModel(Action<string>? navigate = null)
        {
            _navigate = navigate;
            _apiService = new ApiService();
            RefreshCommand = new RelayCommand(async (p) => await LoadAsync());
            NavigateCommand = new RelayCommand((p) => { if (p?.ToString() is string dest) _navigate?.Invoke(dest); });
            _ = LoadAsync();
        }

        public async Task LoadAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            ErrorMessage = null;
            try
            {
                await Task.WhenAll(
                    LoadEmployeesAsync(),
                    LoadDepartmentsAsync(),
                    LoadLeavesAsync(),
                    LoadAbsentsAsync(),
                    LoadShiftAsync());
            }
            catch (Exception ex)
            {
                ErrorMessage = "تعذر تحميل بيانات اللوحة: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadEmployeesAsync()
        {
            try
            {
                var emp = await _apiService.GetAsync<PagedResult<EmployeeSimpleDTO>>("Employees?IsDeleted=false&page=1&pageSize=1");
                if (emp != null) TotalEmployees = emp.TotalCount;
                var report = await _apiService.GetAsync<System.Collections.Generic.List<EmployeeReportDto>>("Employees/report-data");
                if (report != null)
                {
                    MorningCount = report.Count(e => e.ShiftType == Core.Enums.enShiftType.Morning);
                    NightCount = report.Count(e => e.ShiftType == Core.Enums.enShiftType.Night);
                }
            }
            catch { /* partial dashboard: keep defaults */ }
        }

        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var deps = await _apiService.GetAsync<System.Collections.Generic.List<DepartmentDto>>("Departments?IsDeleted=false");
                if (deps == null) return;
                TotalDepartments = deps.Count;
                TopDepartments.Clear();
                foreach (var d in deps.OrderByDescending(d => d.StaffCount).Take(5))
                    TopDepartments.Add(d);
            }
            catch { }
        }

        private async Task LoadLeavesAsync()
        {
            try
            {
                var res = await _apiService.GetAsync<PagedResult<LeaveFullDto>>("Leaves?IsDeleted=false&page=1&pageSize=5");
                if (res == null) return;
                TotalLeaves = res.TotalCount;
                RecentLeaves.Clear();
                foreach (var l in res.Items) RecentLeaves.Add(l);
            }
            catch { }
        }

        private async Task LoadAbsentsAsync()
        {
            try
            {
                var res = await _apiService.GetAsync<PagedResult<AbsentFullDto>>("Absents?IsDeleted=false&page=1&pageSize=5");
                if (res == null) return;
                TotalAbsents = res.TotalCount;
                RecentAbsents.Clear();
                foreach (var a in res.Items) RecentAbsents.Add(a);
            }
            catch { }
        }

        private async Task LoadShiftAsync()
        {
            try
            {
                var res = await _apiService.GetAsync<JObject>($"Shifts/calculate?date={DateTime.Now:yyyy-MM-dd}");
                if (res == null) return;
                TodayTeamId = res["TeamId"]?.Value<int>() ?? res["teamId"]?.Value<int>() ?? 0;
                TodaySupervisor = res["SupervisorName"]?.Value<string>() ?? res["supervisorName"]?.Value<string>() ?? "لم يحدد";
            }
            catch { }
        }
    }
}
