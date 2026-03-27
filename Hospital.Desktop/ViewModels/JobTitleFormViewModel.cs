using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class JobTitleFormViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private readonly int? _jobTitleId;
        private string _title;

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public bool IsEditMode => _jobTitleId.HasValue;
        public event Action RequestClose;
        public ICommand SaveCommand { get; }

        public JobTitleFormViewModel(JobTitleVeiwDTO job = null)
        {
            _apiService = new ApiService();
            if (job != null)
            {
                _jobTitleId = job.Id;
                Title = job.Title;
            }
            SaveCommand = new RelayCommand(async (p) => await Save());
        }

        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Title)) return;

            try
            {
                if (IsEditMode)
                {
                    var dto = new JobTitleVeiwDTO { Id = _jobTitleId.Value, Title = Title };
                    await _apiService.PutAsync<dynamic>($"JobTitles/{_jobTitleId}", dto);
                }
                else
                {
                    var dto = new JobTitleDTO { Title = Title };
                    await _apiService.PostAsync<dynamic>("JobTitles", dto);
                }
                RequestClose?.Invoke();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}