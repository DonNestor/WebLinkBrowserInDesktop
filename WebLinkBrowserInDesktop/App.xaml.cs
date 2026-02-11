using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace WebLinkBrowserInDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string configPath = "config.json";
            string dbPath = "";

            if (!File.Exists(configPath))
            {
                var firstRun = new Views.FirstRunWindow();
                if (firstRun.ShowDialog() == true)
                {
                    dbPath = firstRun.SelectedDbPath;
                    // Save the selected path to config.json
                    File.WriteAllText(configPath, dbPath);
                }
                else
                {
                    // If the user cancels, shut down the application
                    Shutdown();
                    return;
                }
            }
            else
            {
                dbPath = File.ReadAllText(configPath);
            }

            var dbService = new Services.DatabaseService();
            dbService.Initialize(dbPath);

            var mainWindow = new MainWindow(dbService);
            mainWindow.Show();

        }
    }
   
}
