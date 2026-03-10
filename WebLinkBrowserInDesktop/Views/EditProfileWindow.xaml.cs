using Microsoft.Win32;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using WebLinkBrowserInDesktop.Helpers;
using WebLinkBrowserInDesktop.Models;

namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Logic for EditProfileWindow.xaml
    /// </summary>
    public partial class EditProfileWindow : Window
    {
        public string EditedConfigPath { get; private set; }

        // This property indicates whether the profile was loaded successfully. If false, the window will close immediately without allowing any edits.
        public bool IsLoadSuccessful { get; private set; } = true;

        private string _originalJsonPath;
        private AppConfigModel _config;

        public EditProfileWindow(string profileJsonPath)
        {
            InitializeComponent();
            _originalJsonPath = profileJsonPath;
            LoadProfileData();
        }
        private void LoadProfileData()
        {
            try
            {
                string jsonContent = File.ReadAllText(_originalJsonPath);
                _config = JsonConvert.DeserializeObject<AppConfigModel>(jsonContent);

                txtProfileName.Text = Path.GetFileNameWithoutExtension(_originalJsonPath);
                txtDbPath.Text = _config.DatabasePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load profile data: {ex.Message}");
                IsLoadSuccessful = false;
            }
        }
        private void ChangeDb_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "SQLite Database Files (*.db)|*.db|All Files (*.*)|*.*",
                Title = "Select a Database for this profile",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Db")
            };

            if(ofd.ShowDialog() == true)
            {
                txtDbPath.Text = ofd.FileName;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string newName = txtProfileName.Text.Trim();
            string newDbPath = txtDbPath.Text.Trim();

            if (string.IsNullOrEmpty(newName))
            {
                MessageBox.Show("Name cannot be empty. Enter a profile name"); 
                return;
            }
            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("Profile name contains invalid characters");
                return;
            }
            if (!File.Exists(newDbPath))
            {
                MessageBox.Show("Database file does not exist");
                return;
            }

            _config.DatabasePath = newDbPath;
            try
            {
                string newJsonPath = Path.Combine(Path.GetDirectoryName(_originalJsonPath), $"{newName}.json");
                
                if(newJsonPath != _originalJsonPath && File.Exists(newJsonPath))
                {
                    MessageBox.Show("A profile with this name already exists. Please choose another name.");
                    return;
                }
                
                _config.LastUser = newName;
                _config.DatabasePath = txtDbPath.Text.Trim();

                FileHelper.SafeWriteConfig(newJsonPath, _config);
                
                // If the profile name was changed, delete the old JSON file
                if (newJsonPath != _originalJsonPath)
                {
                    File.Delete(_originalJsonPath);
                }

                // Update the EditedConfigPath to the new path
                EditedConfigPath = newJsonPath;
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save profile: {ex.Message}");
                this.DialogResult = false;
            }
        }
    }
}
