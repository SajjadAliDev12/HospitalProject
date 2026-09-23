using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class AbsentsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private DateTime? _searchDate;
        private int _selectedStatusFilter = 1; // 0: الكل, 1: الفعال, 2: المحذوف
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _pageSize = 15;

        public int CurrentPage { get => _currentPage; set { _currentPage = value; OnPropertyChanged(); } }
        public int TotalPages { get => _totalPages; set { _totalPages = value; OnPropertyChanged(); } }

        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ObservableCollection<AbsentFullDto> Absents { get; set; } = new();

        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); LoadAbsents(); } }
        public DateTime? SearchDate { get => _searchDate; set { _searchDate = value; OnPropertyChanged(); LoadAbsents(); } }
        public int SelectedStatusFilter { get => _selectedStatusFilter; set { _selectedStatusFilter = value; OnPropertyChanged(); LoadAbsents(); } }
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); } }

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand DeleteCommand { get; }

        public AbsentsViewModel()
        {
            _apiService = new ApiService();
            RefreshCommand = new RelayCommand((p) => LoadAbsents());
            AddCommand = new RelayCommand((p) => OpenAbsentForm());
            ViewDetailsCommand = new RelayCommand((p) => OpenDetails(p as AbsentFullDto));
            DeleteCommand = new RelayCommand(async (p) => await HandleDelete(p as AbsentFullDto));
            NextPageCommand = new RelayCommand((p) => { if (CurrentPage < TotalPages) { CurrentPage++; LoadAbsents(); } });
            PrevPageCommand = new RelayCommand((p) => { if (CurrentPage > 1) { CurrentPage--; LoadAbsents(); } });
            LoadAbsents();
        }

        public async void LoadAbsents()
        {
            try
            {
                IsLoading = true;
                string dateFilter = SearchDate.HasValue ? $"&date={SearchDate.Value:yyyy-MM-dd}" : "";
                string statusFilter = SelectedStatusFilter == 0 ? "" : (SelectedStatusFilter == 1 ? "&IsDeleted=false" : "&IsDeleted=true");

                // إضافة بارامترات الصفحة
                string url = $"Absents?searchTerm={SearchText}{dateFilter}{statusFilter}&page={CurrentPage}&pageSize={_pageSize}";

                var result = await _apiService.GetAsync<PagedResult<AbsentFullDto>>(url);

                Absents.Clear();
                if (result != null)
                {
                    foreach (var item in result.Items) Absents.Add(item);
                    TotalPages = result.TotalPages;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private void OpenAbsentForm()
        {
            var form = new Views.AbsentFormView();
            var vm = new AbsentFormViewModel();
            vm.RequestClose += () => { form.Close(); LoadAbsents(); };
            form.DataContext = vm;
            form.ShowDialog();
        }

        private void OpenDetails(AbsentFullDto? absent)
        {
            if (absent == null) return;
            var details = new Views.AbsentDetailsView();
            details.DataContext = absent;
            details.ShowDialog();
        }

        private async Task HandleDelete(AbsentFullDto? absent)
        {
            if (absent == null) return;
            string action = absent.IsDeleted ? "استعادة" : "حذف";
            if (MessageBox.Show($"هل أنت متأكد من {action} سجل الغياب؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (absent.IsDeleted)
                {
                    absent.IsDeleted = false;
                    await _apiService.PutAsync<dynamic>($"Absents/{absent.Id}", absent);
                }
                else await _apiService.DeleteAsync<dynamic>($"Absents/{absent.Id}");
                LoadAbsents();
            }
        }
    }
}