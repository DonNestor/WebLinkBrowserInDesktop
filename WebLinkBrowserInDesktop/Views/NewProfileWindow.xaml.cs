using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using WebLinkBrowserInDesktop.Helpers;
using WebLinkBrowserInDesktop.Models;


namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Interaction logic for the NewProfileWindow.xaml class
    /// </summary>
    public partial class NewProfileWindow : Window
    {
        public string CreatedConfigPath { get; private set; }

        public NewProfileWindow()
        {
            InitializeComponent();
            txtProfileName.Focus(); // Set focus to the profile name textbox when the window opens
        }
        private void DbSource_Changed(object sender, RoutedEventArgs e)
        {
            if (pnlExistingDb == null) return; // Protection during window initialization

            if (rbExistingDb.IsChecked == true)
            {
                pnlExistingDb.Visibility = Visibility.Visible;
            }
            else
            {
                rbExistingDb.Visibility = Visibility.Collapsed; // Hide the existing DB option if not selected
            }
        }
        private void BrowseExistingDb_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "SQLite Database Files (*.db)|*.db|All Files (*.*)|*.*",
                Title = "Select an Existing Database"
            };

            if (ofd.ShowDialog() == true)
            {
                txtExistingDbPath.Text = ofd.FileName;
            }
        }
        private void Create_Click(object sender, RoutedEventArgs e)
        {
            // Validate the profile name input
            string name = txtProfileName.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Enter a profile name");
                return;
            }
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("Profile name contains invalid characters. Please enter a valid name.");
                return;
            }
            if (name.ToLower() == "default" || name.ToLower() == "template")
            {
                MessageBox.Show("This name is reserved. Please choose another one.");
                return;
            }
            if (rbExistingDb.IsChecked == true && string.IsNullOrWhiteSpace(txtExistingDbPath.Text))
            {
                MessageBox.Show("Choose an existing database.");
                return;
            }

            try
            {
                string newDbPath = Path.Combine(AppPaths.DatabaseDir, $"{name}.db");
                string newConfigPath = Path.Combine(AppPaths.ProfileDir, $"{name}.json");

                if (File.Exists(newConfigPath))
                {
                    MessageBox.Show("A profile with this name already exists. Please choose another name.");
                    return; 
                }

                string sourceDbPath;
                if (rbExistingDb.IsChecked == true)
                {
                    sourceDbPath = txtExistingDbPath.Text;
                    if (!File.Exists(sourceDbPath))
                    {
                        throw new Exception("The selected database file does not exist.");
                    }
                }
                else
                {
                    sourceDbPath = AppPaths.TemplateDbFile;
                    if (!File.Exists(sourceDbPath))
                    {
                        throw new Exception($"Missing database template file:\n{sourceDbPath}");
                    }
                }

                // Copy the selected or template database to the new location
                File.Copy(sourceDbPath, newDbPath, true);

                // Create the new configuration based on the template or default values
                AppConfigModel newConfig;
                if (File.Exists(AppPaths.TemplateConfigFile))
                {
                    newConfig = JsonConvert.DeserializeObject<AppConfigModel>(File.ReadAllText(AppPaths.TemplateConfigFile));
                }
                else
                {
                    newConfig = new AppConfigModel();
                }

                newConfig.DatabasePath = newDbPath;
                newConfig.LastUser = name;

                FileHelper.SafeWriteConfig(newConfigPath, newConfig);

                CreatedConfigPath = newConfigPath;
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating profile: {ex.Message}");
            }
        }
    }
}

