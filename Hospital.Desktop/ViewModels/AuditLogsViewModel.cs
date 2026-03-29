using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class AuditLogsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private int _currentPage = 1;
        private int _totalPages = 1;

        public ObservableCollection<AuditLogDTO> Logs { get; set; } = new();
        public int CurrentPage { get => _currentPage; set { _currentPage = value; OnPropertyChanged(); } }
        public int TotalPages { get => _totalPages; set { _totalPages = value; OnPropertyChanged(); } }

        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand RefreshCommand { get; }

        public AuditLogsViewModel()
        {
            _apiService = new ApiService();
            RefreshCommand = new RelayCommand((p) => { CurrentPage = 1; LoadLogs(); });
            NextPageCommand = new RelayCommand((p) => { if (CurrentPage < TotalPages) { CurrentPage++; LoadLogs(); } });
            PrevPageCommand = new RelayCommand((p) => { if (CurrentPage > 1) { CurrentPage--; LoadLogs(); } });

            LoadLogs();
        }

        public async void LoadLogs()
        {
            try
            {
                var result = await _apiService.GetAsync<PagedResult<AuditLogDTO>>($"AuditLogs?page={CurrentPage}&pageSize=20");
                if (result != null)
                {
                    Logs.Clear();
                    foreach (var log in result.Items) Logs.Add(log);
                    TotalPages = result.TotalPages;
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}