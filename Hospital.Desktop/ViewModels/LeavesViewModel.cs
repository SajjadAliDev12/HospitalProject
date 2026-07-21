using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class LeavesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private string _searchEmp = "";
        private string _searchSub = "";
        private DateTime? _searchDate;
        private int _currentPage = 1;
        private int _totalPages = 1;

        public ObservableCollection<LeaveFullDto> Leaves { get; set; } = new();

        // فلاتر البحث
        public string SearchEmployee { get => _searchEmp; set { _searchEmp = value; OnPropertyChanged(); CurrentPage = 1; LoadLeaves(); } }
        public string SearchSubEmployee { get => _searchSub; set { _searchSub = value; OnPropertyChanged(); CurrentPage = 1; LoadLeaves(); } }
        public DateTime? SearchDate { get => _searchDate; set { _searchDate = value; OnPropertyChanged(); CurrentPage = 1; LoadLeaves(); } }

        public int CurrentPage { get => _currentPage; set { _currentPage = value; OnPropertyChanged(); } }
        public int TotalPages { get => _totalPages; set { _totalPages = value; OnPropertyChanged(); } }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        public LeavesViewModel()
        {
            _apiService = new ApiService();
            AddCommand = new RelayCommand((p) => OpenForm(null));
            EditCommand = new RelayCommand((p) => OpenForm(p as LeaveFullDto));
            DeleteCommand = new RelayCommand(async (p) => await HandleDelete(p as LeaveFullDto));

            NextPageCommand = new RelayCommand((p) => { if (CurrentPage < TotalPages) { CurrentPage++; LoadLeaves(); } });
            PrevPageCommand = new RelayCommand((p) => { if (CurrentPage > 1) { CurrentPage--; LoadLeaves(); } });

            LoadLeaves();
        }

        public async void LoadLeaves()
        {
            try
            {
                string dateStr = SearchDate.HasValue ? $"&date={SearchDate.Value:yyyy-MM-dd}" : "";
                string url = $"Leaves?employeeName={SearchEmployee}&subEmployeeName={SearchSubEmployee}{dateStr}&page={CurrentPage}&pageSize=15";

                var result = await _apiService.GetAsync<PagedResult<LeaveFullDto>>(url);
                if (result != null)
                {
                    Leaves.Clear();
                    foreach (var item in result.Items)
                    { 
                        if(!item.IsDeleted)
                        Leaves.Add(item); 
                    }
                    TotalPages = result.TotalPages;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void OpenForm(LeaveFullDto leave)
        {
            var form = new Views.LeaveFormView();
            var vm = new LeaveFormViewModel(leave);
            vm.RequestClose += () => { form.Close(); LoadLeaves(); };
            form.DataContext = vm;
            form.ShowDialog();
        }

        private async Task HandleDelete(LeaveFullDto leave)
        {
            if (leave == null) return;
            if (MessageBox.Show("عند حذف الإجازة سيتم إعادة الرصيد للموظف تلقائياً، هل أنت متأكد؟", "تأكيد الحذف", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var res = await _apiService.DeleteAsync<dynamic>($"Leaves/{leave.Id}");
                LoadLeaves();
            }
        }
    }
}