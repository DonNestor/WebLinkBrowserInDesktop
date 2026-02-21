using System.Windows;
using Microsoft.Win32;
using System.IO;
using WebLinkBrowserInDesktop.Models;
using Newtonsoft.Json;


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

            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string templateDir = Path.Combine(baseDir, "Templates", "empty_template.db");
                string templateConfig = Path.Combine(baseDir, "Templates", "default_template.json");

                string newDbPath = Path.Combine(baseDir, "Db", $"{name}.db");
                string newConfigPath = Path.Combine(baseDir, "Profiles", $"{name}.json");

                if (File.Exists(newConfigPath))
                {
                    MessageBox.Show("A profile with this name already exists. Please choose another name.");
                }

                AppConfig newConfig;
                if (File.Exists(templateConfig))
                {
                    newConfig = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(templateConfig));
                }
                else
                {
                    newConfig = new AppConfig();
                }

                newConfig.DatabasePath = newDbPath;
                newConfig.LastUser = name;

                string jsonOutput = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                File.WriteAllText(newConfigPath, jsonOutput);

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

