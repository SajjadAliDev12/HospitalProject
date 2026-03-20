using Hospital.Desktop.Services;
using Hospital.Core.DTOs;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Hospital.Desktop.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        public string Username { get; set; }
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }
        private bool _isRememberMe;
        public bool IsRememberMe
        {
            get => _isRememberMe;
            set { _isRememberMe = value; OnPropertyChanged(nameof(IsRememberMe)); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _apiService = new ApiService();
            LoginCommand = new RelayCommand(async (param) => await ExecuteLogin(param));
            if (Properties.Settings.Default.IsRemembered)
            {
                Username = Properties.Settings.Default.SavedUsername;
                IsRememberMe = true;
                // ملاحظة: الـ PasswordBox يحتاج تعاملاً خاصاً في الـ Code Behind لتعبئته تلقائياً
            }
        }

        private async Task ExecuteLogin(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(password))
            {
                ErrorMessage = "يرجى إدخال كافة الحقول";
                return;
            }

            try
            {
                var loginDto = new LoginDto { UserName = Username, Password = password };
                var response = await _apiService.PostAsync<dynamic>("Auth/Login", loginDto);

                if (response != null)
                {
                    if (IsRememberMe)
                    {
                        Properties.Settings.Default.SavedUsername = Username;
                        Properties.Settings.Default.SavedPassword = EncryptionHelper.Encrypt( password); 
                        Properties.Settings.Default.IsRemembered = true;
                    }
                    else
                    {
                        Properties.Settings.Default.SavedUsername = string.Empty;
                        Properties.Settings.Default.SavedPassword = string.Empty;
                        Properties.Settings.Default.IsRemembered = false;
                    }
                    Properties.Settings.Default.Save();
                    // حفظ التوكن في الخدمة لاستخدامه لاحقاً
                    _apiService.SetToken(response.token.ToString());

                    // الانتقال للنافذة الرئيسية (MainWindow)
                    var mainWin = new MainWindow();
                    mainWin.Show();

                    Application.Current.Windows[0].Close(); // إغلاق نافذة الدخول
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // كلاس بسيط لتنفيذ الأوامر (Command)
    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _execute;
        public RelayCommand(Func<object, Task> execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public async void Execute(object parameter) => await _execute(parameter);
        public event EventHandler CanExecuteChanged;
    }
}