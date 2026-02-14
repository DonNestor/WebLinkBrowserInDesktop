using Microsoft.Win32;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Controls;
using WebLinkBrowserInDesktop.Models;
using WebLinkBrowserInDesktop.Services;
using WebLinkBrowserInDesktop.Views;
using System.Diagnostics;
using System.IO.Enumeration;

namespace WebLinkBrowserInDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppConfig _currentConfig;
        private readonly DatabaseService _databaseService;
        public MainWindow(DatabaseService databaseService, AppConfig appConfig)
        {
            InitializeComponent();

            _currentConfig = appConfig;
            _databaseService = databaseService;

            this.Title = $"Przeglądarka Linków - Profil: {_currentConfig.LastUser ?? "Domyślny"}";

            InitializeBrowser();
            RefreshLinkList();
        }

        private async void InitializeBrowser()
        {
            await MyWebView.EnsureCoreWebView2Async();
        }

        #region Load & Refresh
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
        }

        private void LinkListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LinkListBox.SelectedItem is WebLinkModel selectedLink)
            {
                try
                {
                    LoadPage(selectedLink.Url);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Niepoprawny format adresu URL: " + ex.Message);
                }
            }
        }

        private void LoadPage(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return;

                MyWebView.Source = new Uri(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd ładowania strony: " + ex.Message);
            }
        }
        #endregion

        #region CRUD
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new LinkEditWindow();
            editWindow.Owner = this;

            if(editWindow.ShowDialog() == true)
            {
                _databaseService.AddLink(editWindow.Links);
                RefreshLinkList();
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if(LinkListBox.SelectedItem is WebLinkModel selectedLink)
            {
                var editWindow = new Views.LinkEditWindow(selectedLink);
                editWindow.Owner = this;

                if (editWindow.ShowDialog() == true)
                {
                    _databaseService.UpdateLink(editWindow.Links);
                    RefreshLinkList();

                    LoadPage(editWindow.Links.Url);
                }
            }
            else
            {
                MessageBox.Show("Nie wybrano żadnego linku do edycji.");
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
        #endregion

        #region Toolbar
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (MyWebView.CanGoBack) MyWebView.GoBack();
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (MyWebView.CanGoForward) MyWebView.GoForward();
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            MyWebView.Reload();
        }

        private void Go_Click(object sender, RoutedEventArgs e)
        {
            LoadPage(txtCurrentUrl.Text);
        }

        private void txtCurrentUrl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                LoadPage(txtCurrentUrl.Text);
            }
        }

        private void MyWebView_SourceChanged(object sender, Microsoft.Web.WebView2.Core.CoreWebView2SourceChangedEventArgs e)
        {
            txtCurrentUrl.Text = MyWebView.Source?.ToString() ?? "";
        }

        private void SaveLink_Click(object sender, RoutedEventArgs e)
        {
            string currentUrl = MyWebView.Source?.ToString() ?? "";
            if(string.IsNullOrEmpty(currentUrl))
            {
                MessageBox.Show("Nie mozna zapisać pustego adresu URL.");
                return;
            }

            var newLink = new WebLinkModel
            {
                Name = currentUrl, // Można tu dodać logikę do generowania nazwy na podstawie URL lub pozostawić puste
                Url = currentUrl,
                BrowserType = "Chrome" // Można tu dodać logikę do określania typu przeglądarki, jeśli jest taka potrzeba
            };

            _databaseService.AddLink(newLink);
            RefreshLinkList();
        }

        private void OpenExternal_Click(object seneder, RoutedEventArgs e)
        {
            if(LinkListBox.SelectedItem is WebLinkModel selectedLink)
            {
                OpenInSpecificBrowser(selectedLink.Url, selectedLink.BrowserType);
            }
            else if (!string.IsNullOrEmpty(txtCurrentUrl.Text))
            {
                OpenUrlInDefaultBrowser(txtCurrentUrl.Text);
            }
        }

        private void OpenInSpecificBrowser(string url, string browserType)
        {
            try
            {
                string fileName = "";

                switch(browserType)
                {
                    case "Chrome":
                        fileName = "chrome.exe";
                        break;
                    case "Opera":
                        fileName = "opera.exe";
                        break;
                    case "Firefox":
                        fileName = "filefox.exe";
                        break;
                    case "Tor":
                        fileName = "tor.exe";
                        break;
                    default:
                        OpenUrlInDefaultBrowser(url);
                        break;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie udało się otworzyć przeglądarki: " + ex.Message);
            }
        }

        private void OpenUrlInDefaultBrowser(string url)
        {
           try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Nie udało się otworzyć przeglądarki: " + ex.Message);
            }
        }
        #endregion



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