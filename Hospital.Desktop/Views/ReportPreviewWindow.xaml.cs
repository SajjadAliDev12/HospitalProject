using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Hospital.Desktop.Views
{
    public partial class ReportPreviewWindow : Window
    {
        public ReportPreviewWindow(FlowDocument doc)
        {
            InitializeComponent();
            // ربط التقرير بالأداة الجديدة
            FlowDocViewer.Document = doc;
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            // أمر الطباعة المباشر للـ FlowDocument
            if (FlowDocViewer.Document != null)
            {
                PrintDialog pd = new PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    pd.PrintDocument(((IDocumentPaginatorSource)FlowDocViewer.Document).DocumentPaginator, "Shift Report");
                }
            }
        }
    }
}