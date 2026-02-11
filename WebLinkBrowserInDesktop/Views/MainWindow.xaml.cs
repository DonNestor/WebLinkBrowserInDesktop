using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebLinkBrowserInDesktop.Models;
using WebLinkBrowserInDesktop.Services;

namespace WebLinkBrowserInDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly DatabaseService _databaseService;
        public MainWindow(DatabaseService databaseService)
        {
            InitializeComponent();

            _databaseService = databaseService;

            //LoadLinks();
            RefreshLinkList();
        }

        private void LoadLinks()
        {
            List<WebLinkModel> links = new List<WebLinkModel>
            {
                new WebLinkModel { Name = "Google", Url = "https://www.google.com" },
                new WebLinkModel { Name = "Microsoft", Url = "https://www.microsoft.com" },
                new WebLinkModel { Name = "GitHub", Url = "https://www.github.com" }
            };

            LinkListBox.ItemsSource = links;
        }

        private void RefreshLinkList()
        {
            var links = _databaseService.GetAllLinks();
            LinkListBox.ItemsSource = links;
        }

        private void LinkListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LinkListBox.SelectedItem is WebLinkModel selectedLink)
            {
                BrowserControl.Navigate(selectedLink.Url);
            }
        }
    }
}