using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using Hospital.Desktop.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalCount { get; set; }
    }

    public class TransferLogViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private int _currentPage = 1;
        private int _totalPages = 1;
        private int _pageSize = 15;
        private TransferLogDto _selectedLog = null!;

        public ObservableCollection<TransferLogDto> Logs { get; set; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                CurrentPage = 1; // العودة للصفحة الأولى عند بدء بحث جديد
                LoadLogs();
            }
        }

        public int CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(); }
        }

        public int TotalPages
        {
            get => _totalPages;
            set { _totalPages = value; OnPropertyChanged(); }
        }

        public TransferLogDto SelectedLog
        {
            get => _selectedLog;
            set { _selectedLog = value; OnPropertyChanged(); }
        }

        // الأوامر (Commands)
        public ICommand RefreshCommand { get; }
        public ICommand NewTransferCommand { get; }
        public ICommand ViewDetailsCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }

        public TransferLogViewModel()
        {
            _apiService = new ApiService();

            // تعريف الأوامر
            RefreshCommand = new RelayCommand((p) => LoadLogs());

            NewTransferCommand = new RelayCommand((p) => OpenTransferForm());

            ViewDetailsCommand = new RelayCommand((p) =>
            {
                var log = p as TransferLogDto;
                if (log != null) OpenDetailsForm(log);
            });

            NextPageCommand = new RelayCommand((p) =>
            {
                if (CurrentPage < TotalPages)
                {
                    CurrentPage++;
                    LoadLogs();
                }
            });

            PrevPageCommand = new RelayCommand((p) =>
            {
                if (CurrentPage > 1)
                {
                    CurrentPage--;
                    LoadLogs();
                }
            });

            LoadLogs();
        }

        public async void LoadLogs()
        {
            try
            {
                IsLoading = true;

                // بناء الرابط مع بارامترات البحث والترقيم للسيرفر
                string url = $"TransferLog?searchTerm={SearchText}&page={CurrentPage}&pageSize={_pageSize}";

                var result = await _apiService.GetAsync<PagedResult<TransferLogDto>>(url);

                if (result != null && result.Items != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Logs.Clear();
                        foreach (var log in result.Items)
                        {
                            Logs.Add(log);
                        }
                        TotalPages = result.TotalPages;
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في جلب البيانات: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenTransferForm()
        {
            var form = new AddTransferView();
            var vm = new AddTransferViewModel();
            vm.RequestClose += () => { form.Close(); LoadLogs(); };
            form.DataContext = vm;
            form.ShowDialog();
        }

        private void OpenDetailsForm(TransferLogDto log)
        {
            if (log == null) return;
            var form = new TransferDetailsView();
            form.DataContext = log;
            form.ShowDialog();
        }
    }
}
