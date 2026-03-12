using Newtonsoft.Json;
using System.IO;
using System.Windows;
using WebLinkBrowserInDesktop.Models;
using WebLinkBrowserInDesktop.Services;
using WebLinkBrowserInDesktop.Views;
using WebLinkBrowserInDesktop.Helpers;

namespace WebLinkBrowserInDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string _configPath;
        private AppConfigModel _config;
        private DatabaseService _databaseService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppPaths.EnsureDirectoriesExist(); // Ensure the main directories exist before we do anything else

            // Ensure the template database exists. This is a blank database with the correct schema that we can copy for new profiles.
            if (!File.Exists(AppPaths.TemplateDbFile))
            {
                var tempService = new DatabaseService(); // Create a temporary instance of the DatabaseService to initialize the template database with the correct schema.
                tempService.Initialize(AppPaths.TemplateDbFile); //Create the file and initialize it with the correct schema
                tempService.CloseConnection(); //Close immediately since we just want to create the file with the correct schema. The MainWindow will create its own connection to this file when needed.
            }
            
            //ensure the template config exists. This is a blank config with default values that we can copy for new profiles.
            if (!File.Exists(AppPaths.TemplateConfigFile))
            {
                var defaultConfig = new AppConfigModel
                {
                    DatabasePath = AppPaths.TemplateDbFile,
                    LastUser = "New user"
                };
                FileHelper.SafeWriteConfig(AppPaths.TemplateConfigFile, defaultConfig);
            }

            bool showSelectionWindow = true;
            LauncherConfigModel launcherState = null;

            //Read the launcher state from active_profile.txt to determine if we should show the profile selection window or directly load the last profile
            if (File.Exists(AppPaths.ActiveProfileFile))
            {
                try
                {
                    string launcherJson = File.ReadAllText(AppPaths.ActiveProfileFile);
                    launcherState = JsonConvert.DeserializeObject<LauncherConfigModel>(launcherJson);

                    //If user has chosen to always load the last profile and the last active profile path exists, load it directly without showing the selection window
                    if (launcherState != null && launcherState.AlwaysLoadLastProfile && File.Exists(launcherState.LastActiveProfilePath))
                    {
                        _configPath = launcherState.LastActiveProfilePath;
                        showSelectionWindow = false;
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

            if (showSelectionWindow)
            {
                var selectionWindow = new ProfileManagerWindow();

                if (selectionWindow.ShowDialog() == true)
                {
                    //Save the selected profile path
                    _configPath = selectionWindow.SelectedProfilePath;

                    //Save the selected profile path to active_profile.txt for next time
                    launcherState = new LauncherConfigModel
                    {
                        LastActiveProfilePath = _configPath,
                        AlwaysLoadLastProfile = selectionWindow.RememberProfile
                    };
                    FileHelper.SafeWriteConfig(AppPaths.ActiveProfileFile, launcherState);
                }
                else
                {
                    // If the user cancels, shut down the application
                    Shutdown();
                    return;
                }
            }

            //Start the main window with the loaded or default config
            try
            {
                //Load the selected profile's config into memory
                string json = File.ReadAllText(_configPath);

                string dbDirectory = Path.GetDirectoryName(_configPath);

                // Ensure the directory for the database exists
                if (!string.IsNullOrEmpty(dbDirectory) && !Directory.Exists(dbDirectory))
                {
                    Directory.CreateDirectory(dbDirectory);
                }

                _config = JsonConvert.DeserializeObject<AppConfigModel>(json) ?? new AppConfigModel(); // If deserialization fails, we create a new config with default values to avoid null reference exceptions later on.
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
                    _config = JsonConvert.DeserializeObject<AppConfigModel>(File.ReadAllText(_configPath)) ?? new AppConfigModel();
                }
                catch
                {
                    // If there's an error reading/parsing the config, create a new one with defaults
                    _config = new AppConfigModel();
                }
            }
            else
            {
                //First launch ever, create default config
                _config = new AppConfigModel();
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

                FileHelper.SafeWriteConfig(_configPath, _config);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Configuration saving error: {ex.Message}");
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Check if the service has been created (the question mark protects against an error if it were null)
            _databaseService?.CloseConnection(); //Closing the base upon exit

            base.OnExit(e);
        }
    }
}
