using Newtonsoft.Json;
using System.IO;
using System.Windows;
using WebLinkBrowserInDesktop.Models;
using WebLinkBrowserInDesktop.Services;
using WebLinkBrowserInDesktop.Views;

namespace WebLinkBrowserInDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string _configPath;
        private AppConfig _config;
        private DatabaseService _databaseService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //Path and profile configuration
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string templateDir = Path.Combine(baseDir, "Templates");
            string profileDir = Path.Combine(baseDir, "Profiles");
            string databaseDir = Path.Combine(baseDir, "Db");
            string activeProfileFile = Path.Combine(baseDir, "active_profile.txt");

            Directory.CreateDirectory(templateDir);
            Directory.CreateDirectory(profileDir);
            Directory.CreateDirectory(databaseDir);

            string templateDbPath = Path.Combine(templateDir, "empty_template.db");
            if (!File.Exists(templateDbPath))
            {
                var tempService = new DatabaseService(); // Create a temporary instance of the DatabaseService to initialize the template database with the correct schema.
                tempService.Initialize(templateDbPath); //Create the file and initialize it with the correct schema
                tempService.CloseConection(); //Close immediately since we just want to create the file with the correct schema. The MainWindow will create its own connection to this file when needed.
            }

            string templateConfigPath = Path.Combine(templateDir, "default_template.json");
            if (!File.Exists(templateConfigPath))
            {
                var defaultConfig = new AppConfig
                {
                    LastUser = "New user"
                };
                File.WriteAllText(templateConfigPath, JsonConvert.SerializeObject(defaultConfig, Formatting.Indented));
            }

            //By default, we set the configuration path to "default.json" in the Profiles folder
            _configPath = Path.Combine(profileDir, "default.json");

            //Attempting to load the last profile from active_profile.txt
            if (File.Exists(activeProfileFile))
            {
                try
                {
                    string fileContent = File.ReadAllText(activeProfileFile);
                    var launcherState = JsonConvert.DeserializeObject<LauncherConfig>(fileContent);

                    if (launcherState != null && File.Exists(launcherState.LastActiveProfilePath))
                    {
                        _configPath = launcherState.LastActiveProfilePath;
                        // If successful, we end the check in this block
                        goto ConfigurationLoaded;
                    }


                    // Attempting to read as "bare track" (backward compatibility)
                    string rawPath = fileContent.Trim();
                    if (File.Exists(rawPath))
                    {
                        _configPath = rawPath;
                    }
                }
                catch (Exception)
                {
                    // FALLBACK:
                    // If the file is locked, corrupted, permission denied, etc. -> we come in here.
                    // We don't do anything. The code will simply continue, and _configPath
                    // will remain set to "default.json" (from step 1).

                    System.Diagnostics.Debug.WriteLine("Error reading active_profile. Acts like first run");
                }
            }

            ConfigurationLoaded: // Label for goto to jump out of nests on success

            LoadOrCreateConfig();

            // Check if the database path is set and valid, if not, show the first run window
            if (string.IsNullOrEmpty(_config.DatabasePath) || !File.Exists(_config.DatabasePath))
            {
                var newProfileWindow = new NewProfileWindow();

                if (newProfileWindow.ShowDialog() == true)
                {
                    _config.DatabasePath = newProfileWindow.CreatedConfigPath;

                    string json = File.ReadAllText(_configPath);
                    _config = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();

                    var launcherState = new LauncherConfig
                    {
                        LastActiveProfilePath = _configPath
                    };
                    File.WriteAllText(Path.Combine(baseDir, "avtive_profile.json"), JsonConvert.SerializeObject(launcherState));

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
            
            try
            {
                string dbDirectory = Path.GetDirectoryName(_config.DatabasePath);
                // Ensure the directory for the database exists
                if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
                {
                    Directory.CreateDirectory(dbDirectory);
                }

                _databaseService = new DatabaseService();
                _databaseService.Initialize(_config.DatabasePath);

                var mainWindow = new MainWindow(_databaseService, _config);
                this.MainWindow = mainWindow; // We set the main application window, which is important for managing the window lifecycle
                this.ShutdownMode = ShutdownMode.OnMainWindowClose;
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database error: {ex.Message}");
                Shutdown();
            }
        }

        private void LoadOrCreateConfig()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    _config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(_configPath)) ?? new AppConfig();
                }
                catch
                {
                    // If there's an error reading/parsing the config, create a new one with defaults
                    _config = new AppConfig();
                }
            }
            else
            {
                //First launch ever, create default config
                _config = new AppConfig();
            }
        }

        private void SaveConfig()
        {
            try
            {
                string fileName = Path.GetFileName(_configPath).ToLower();

                if (fileName.Contains("template") || fileName == "default.json")
                {
                    System.Diagnostics.Debug.WriteLine("Skipping saving for templates or default config"); 
                    return; // Don't save if it's a template or default config
                }

                string json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Configuration saving error: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Check if the service has been created (the question mark protects against an error if it were null)
            _databaseService?.CloseConection(); //Closing the base upon exit

            base.OnExit(e);
        }
    }
}
