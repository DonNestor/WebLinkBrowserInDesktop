using Microsoft.Win32;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Controls;
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

        private async void InitializeBrowser()
        {
            await MyWebView.EnsureCoreWebView2Async();
        }

        private void ImportDatabase_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Baza danych SQLite (*.db)|*.db";
            ofd.Title = "Wybierz bazę danych do zaimportowania";

            if (ofd.ShowDialog() == true)
            {
                string newDbPath = ofd.FileName;

                string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cofig.json");

                AppConfig config = new AppConfig { DatabasePath = newDbPath };
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            }
        }

        private void RefreshLinkList()
        {
            var links = _databaseService.GetAllLinks();
            LinkListBox.ItemsSource = links;

            txtName.Clear();
            txtUrl.Clear();
            comboBrowser.SelectedIndex = -1;
        }

        private void LinkListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LinkListBox.SelectedItem is WebLinkModel selectedLink)
            {
                txtName.Text = selectedLink.Name;
                txtUrl.Text = selectedLink.Url;


                try
                {
                    string url = selectedLink.Url;
                    if(!url.StartsWith("http") || !url.StartsWith("https"))
                    {
                        url = "https://" + url;
                    }

                    MyWebView.Source = new Uri(url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Niepoprawny format adresu URL: " + ex.Message);
                }
            }
        }

        private void AddLink_Click(object sender, RoutedEventArgs e)
        {
            var newLink = new WebLinkModel
            {
                Name = txtName.Text,
                Url = txtUrl.Text,
                BrowserType = comboBrowser.Text
            };

            _databaseService.AddLink(newLink);

            RefreshLinkList();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (LinkListBox.SelectedItem is WebLinkModel selected)
            {
                selected.Name = txtName.Text;
                selected.Url = txtUrl.Text;
                selected.BrowserType = (comboBrowser.SelectedItem as ComboBoxItem)?.Content.ToString();

                _databaseService.UpdateLink(selected);
                RefreshLinkList();
                MessageBox.Show("Zaktualizowano dane");
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (LinkListBox.SelectedItem is WebLinkModel selected)
            {
                var result = MessageBox.Show($"Czy na pewno usunać {selected.Name}", "Potwierdzenie", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _databaseService.DeleteLink(selected.Id);
                    RefreshLinkList();
                    MessageBox.Show("Usunięto dane");
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("WebLinkBrowserInDesktop\nWersja 1.0\nAutor: Your Name", "O programie");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}