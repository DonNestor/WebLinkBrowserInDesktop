using System.Diagnostics;
using Microsoft.Win32;
using System.Windows;
using Newtonsoft.Json;
using System.Windows.Controls;
using WebLinkBrowserInDesktop.Models;
using WebLinkBrowserInDesktop.Services;
using WebLinkBrowserInDesktop.Views;
using System.IO;
using System.Runtime.CompilerServices;

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

            this.Title = $"Links Browser - Profile: {_currentConfig.LastUser ?? "Default"}"; 

            InitializeBrowser();
            RefreshLinkList();

            this.Closed += MainWindow_Closed;
        }

        private async void InitializeBrowser()
        {
            await MyWebView.EnsureCoreWebView2Async();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _databaseService.CloseConection();
            Application.Current.Shutdown();
        }

        #region Load & Refresh
        private void ImportDatabase_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Database SQLite (*.db)|*.db"; 
            ofd.Title = "Choose a database to import"; 

            if (ofd.ShowDialog() == true)
            {
                string newDbPath = ofd.FileName;

                if (newDbPath == _currentConfig.DatabasePath)
                {
                    MessageBox.Show("Choosed database is already in use.");
                    return;
                }

                try
                {
                    _databaseService.CloseConection();

                    _currentConfig.DatabasePath = newDbPath;
                    string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                    string json = JsonConvert.SerializeObject(_currentConfig, Formatting.Indented);
                    File.WriteAllText(configPath, json);

                    _databaseService.Initialize(newDbPath);

                    RefreshLinkList();
                    UpdateTitle();

                    MessageBox.Show("Successfully loaded new database!"); 
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while importing databas: {ex.Message}");
                }
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
                    MessageBox.Show("Incorrect URL format: " + ex.Message);
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
                MessageBox.Show("Page loading error: " + ex.Message);
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
                MessageBox.Show("No link selected for editing.");
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (LinkListBox.SelectedItem is WebLinkModel selected)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selected.Name}", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _databaseService.DeleteLink(selected.Id);
                    RefreshLinkList();
                    MessageBox.Show("Data deleted");
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
                MessageBox.Show("Cannot save empty URL.");
                return;
            }

            var newLink = new WebLinkModel
            {
                Name = currentUrl,  
                Url = currentUrl,
                BrowserType = "Chrome" 
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
                string exePath = "";

                if (_currentConfig.BrowserPaths.TryGetValue(browserType, out string savePath) && !string.IsNullOrWhiteSpace(savePath))

                {
                    exePath = savePath;
                }
                else
                {
                    exePath = browserType switch
                    {
                        "Chrome" => "chrome.exe",
                        "Opera" => "opera.exe",
                        "Firefox" => "firefox.exe",
                        "Tor" => "tor.exe",
                        _ => ""
                    };
                }

                if(string.IsNullOrEmpty(exePath))
                {
                    OpenUrlInDefaultBrowser(url);
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = url,
                    UseShellExecute = true
                });
            }
            catch 
            {
                OpenUrlInDefaultBrowser(url);
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
                MessageBox.Show("Failed to open browser: " + ex.Message);
            }
        }
        #endregion

        public void UpdateTitle()
        {
            this.Title = $"Links Browser - Profile: {_currentConfig.LastUser ?? "Default"} | Database: {Path.GetFileName(_currentConfig.DatabasePath)}";
        }

        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            var settingsWin = new Views.SettingsWindow(_currentConfig);
            settingsWin.Owner = this;

            if(settingsWin.ShowDialog() == true)
            {
                try
                {
                    string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                    string json = JsonConvert.SerializeObject(_currentConfig, Formatting.Indented);
                    File.WriteAllText(configPath, json);

                    UpdateTitle();
                    MessageBox.Show("The configuration has been saved.");

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving settings: " + ex.Message);
                }
            }
        }
        private void ProfileManager_Click(object sender, RoutedEventArgs e)
        {
            var manager = new ProfileManagerWindow();
            manager.Owner = this;

            if(manager.ShowDialog() == true)
            {
                LoadProfile(manager.SelectedProfilePath);
            }
        }

        private void LoadProfile(string configPath)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string jsonContent = File.ReadAllText(configPath);
                var newConfig = JsonConvert.DeserializeObject<AppConfig>(jsonContent);

                if (newConfig == null)
                {
                    throw new Exception("Empty config file."); 
                }

                _databaseService.CloseConection();

                //Changing reference to new config object loaded from selected profile
                _currentConfig = newConfig;

                //Initialize now database connection with new path from loaded profile
                _databaseService.Initialize(_currentConfig.DatabasePath);


                var launcherConfig = new LauncherConfig()
                {
                    LastActiveProfilePath = configPath
                };

                //Serialize the launcher configuration to JSON with indentation for readability
                string jsonState = JsonConvert.SerializeObject(launcherConfig, Formatting.Indented);

                //Save state to file active_profile.txt - to remember which profile to load next time
                string activeProfileInfo = Path.Combine(baseDir, "active_profile.txt");
                File.WriteAllText(activeProfileInfo, jsonState);

                RefreshLinkList();
                UpdateTitle();

                MyWebView.Source = new Uri("about:blank");
                txtCurrentUrl.Text = "";

                MessageBox.Show($"Switched to profile: {_currentConfig.LastUser}", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profile: {ex.Message}", "Error");
            }
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("LinksBrowser\nVersion 1.0\nAutor: ZB", "About...");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}