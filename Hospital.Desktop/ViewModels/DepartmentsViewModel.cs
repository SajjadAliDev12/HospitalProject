using Hospital.Core.DTOs;
using Hospital.Desktop.Services;
using Hospital.Desktop.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Hospital.Desktop.ViewModels
{
    public class DepartmentsViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;
        private bool _isLoading;
        private int _selectedStatusFilter = 1; // الافتراضي: غير المحذوف

        public ObservableCollection<DepartmentDto> Departments { get; set; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public int SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { _selectedStatusFilter = value; OnPropertyChanged(); LoadDepartments(); }
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public DepartmentsViewModel()
        {
            _apiService = new ApiService();

            RefreshCommand = new RelayCommand((p) => LoadDepartments());
            AddCommand = new RelayCommand((p) => OpenDepartmentForm(null));
            EditCommand = new RelayCommand((p) => OpenDepartmentForm(p as DepartmentDto));
            DeleteCommand = new RelayCommand(async (p) => await HandleDelete(p as DepartmentDto));

            LoadDepartments();
        }

        public async void LoadDepartments()
        {
            try
            {
                IsLoading = true;
                // 0: الكل، 1: غير المحذوف، 2: المحذوف
                string filter = SelectedStatusFilter == 0 ? "" : (SelectedStatusFilter == 1 ? "?IsDeleted=false" : "?IsDeleted=true");

                var result = await _apiService.GetAsync<List<DepartmentDto>>($"Departments{filter}");
                Departments.Clear();
                if (result != null)
                    foreach (var dep in result) Departments.Add(dep);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            finally { IsLoading = false; }
        }

        private void OpenDepartmentForm(DepartmentDto dep)
        {
            var form = new DepartmentFormView();
            var vm = new DepartmentFormViewModel(dep);
            vm.RequestClose += () => { form.Close(); LoadDepartments(); };
            form.DataContext = vm;
            form.ShowDialog();
        }

        private async Task HandleDelete(DepartmentDto dep)
        {
            if (dep == null) return;
            string action = dep.IsDeleted ? "استعادة" : "حذف";
            if (MessageBox.Show($"هل أنت متأكد من {action} القسم: {dep.Name}؟", "تأكيد", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (dep.IsDeleted) // استعادة
                {
                    dep.IsDeleted = false;
                    await _apiService.PutAsync<dynamic>($"Departments/{dep.Id}", dep);
                }
                else // حذف
                {
                    await _apiService.DeleteAsync<dynamic>($"Departments/{dep.Id}");
                }
                LoadDepartments();
            }
        }
    }
}