using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using Hospital.Desktop.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class JobTitlesViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private bool _isLoading;

        public ObservableCollection<JobTitleVeiwDTO> JobTitles { get; set; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public JobTitlesViewModel()
        {
            _apiService = new ApiService();

            RefreshCommand = new RelayCommand((p) => LoadJobTitles());
            AddCommand = new RelayCommand((p) => OpenJobTitleForm(null));
            EditCommand = new RelayCommand((p) => OpenJobTitleForm(p as JobTitleVeiwDTO));
            DeleteCommand = new RelayCommand(async (p) => await HandleDelete(p as JobTitleVeiwDTO));

            LoadJobTitles();
        }

        public async void LoadJobTitles()
        {
            try
            {
                IsLoading = true;
                var result = await _apiService.GetAsync<List<JobTitleVeiwDTO>>("JobTitles");
                JobTitles.Clear();
                if (result != null)
                    foreach (var job in result) JobTitles.Add(job);
            }
            catch (Exception ex) { MessageBox.Show("فشل الجلب: " + ex.Message); }
            finally { IsLoading = false; }
        }

        private void OpenJobTitleForm(JobTitleVeiwDTO job)
        {
            var form = new JobTitleFormView();
            var vm = new JobTitleFormViewModel(job);
            vm.RequestClose += () => { form.Close(); LoadJobTitles(); };
            form.DataContext = vm;
            form.ShowDialog();
        }

        private async Task HandleDelete(JobTitleVeiwDTO job)
        {
            if (job == null) return;
            if (MessageBox.Show($"هل أنت متأكد من حذف العنوان الوظيفي: {job.Title}؟", "تأكيد", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    await _apiService.DeleteAsync<dynamic>($"JobTitles/{job.Id}");
                    LoadJobTitles();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
    }
}