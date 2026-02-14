using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using WebLinkBrowserInDesktop.Models;
using WebLinkBrowserInDesktop.Views;

namespace WebLinkBrowserInDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cofig.json");
        private AppConfig _config;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadOrCreateConfig();

            // Check if the database path is set and valid, if not, show the first run window
            if (string.IsNullOrEmpty(_config.DatabasePath) || !File.Exists(_config.DatabasePath))
            {
                var firstRun = new FirstRunWindow();
                if (firstRun.ShowDialog() == true)
                {
                    _config.DatabasePath = firstRun.SelectedDbPath;
                    // Save the selected path to config.json
                    SaveConfig();
                }
                else
                {
                    // If the user cancels, shut down the application
                    Shutdown();
                    return;
                }
            }
            
            var dbService = new Services.DatabaseService();
            try
            {
                string directory = Path.GetDirectoryName(_config.DatabasePath);
                // Ensure the directory for the database exists
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                dbService.Initialize(_config.DatabasePath);

                var mainWindow = new MainWindow(dbService, _config);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błądbazy danych: {ex.Message}");
                Shutdown();
                return;
            }
        }

        private void LoadOrCreateConfig()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    string json = File.ReadAllText(_configPath);
                    _config = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                }
                catch (Exception ex)
                {
                    // If there's an error reading/parsing the config, create a new one with defaults
                    _config = new AppConfig { LastUser = "Użytkownik" };
                }
            }
            else
            {
                //First launch ever, create default config
                _config = new AppConfig { LastUser = "Użytkownik" };
            }
        }

        private void SaveConfig()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd zapisu konfiguracji: {ex.Message}");
            }
        }
    }
}
