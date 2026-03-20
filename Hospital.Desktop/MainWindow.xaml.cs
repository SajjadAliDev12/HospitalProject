using Hospital.Desktop.Views; 
using System.Windows;

namespace Hospital.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainViewModel();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            
            var result = MessageBox.Show("هل أنت متأكد من تسجيل الخروج؟", "تأكيد",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginView loginView = new LoginView();
                loginView.Show();

                this.Close();
            }
        }
    }
}