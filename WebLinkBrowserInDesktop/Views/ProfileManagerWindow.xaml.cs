using Newtonsoft.Json;
using System.IO;
using System.Windows;
using WebLinkBrowserInDesktop.Helpers;
using WebLinkBrowserInDesktop.Models;

namespace WebLinkBrowserInDesktop.Views
{
    public partial class ProfileManagerWindow : Window
    {
        public string SelectedProfilePath { get; private set; }
        public bool RememberProfile => chkRememberProfile.IsChecked == true;
        public ProfileManagerWindow()
        {
            InitializeComponent();
            LoadProfiles();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            if (File.Exists(AppPaths.ActiveProfileFile))
            {
                try
                {
                    string launcherJson = File.ReadAllText(AppPaths.ActiveProfileFile);
                    LauncherConfigModel launcherState = JsonConvert.DeserializeObject<LauncherConfigModel>(launcherJson);
                    
                    if (launcherState != null)
                    {
                        chkRememberProfile.IsChecked = launcherState.AlwaysLoadLastProfile;
                    }
                }
                catch
                {
                    // If there's an error reading the file, we can ignore it and just leave the checkbox unchecked.
                }
            }
        }
        private void LoadProfiles()
        {
            var profiles = new List<ProfileItem>();

            if (Directory.Exists(AppPaths.ProfileDir))
            {
                var files = Directory.GetFiles(AppPaths.ProfileDir, "*.json");
                foreach (var file in files)
                {
                    string name = Path.GetFileNameWithoutExtension(file);

                    // Filter out templates and temporary files
                    if (name.ToLower().Contains("template") || name.ToLower() == "defaultprofile")
                        continue;

                    profiles.Add(new ProfileItem { Name = name, FullPath = file });
                }
            }

            lstProfiles.ItemsSource = profiles;

            // Select the first profile if available
            if (profiles.Count > 0) lstProfiles.SelectedIndex = 0;
        }
        private void LoadSelected_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is ProfileItem selected)
            {
                SelectedProfilePath = selected.FullPath;
                this.DialogResult = true; // Closes the window and signals readiness to MainWindow
            }
            else
            {
                MessageBox.Show("Please select a profile from the list.");
            }
        }
        private void CreateNew_Click(object sender, RoutedEventArgs e)
        {
            var newProfileWin = new NewProfileWindow();
            newProfileWin.Owner = this;

            if (newProfileWin.ShowDialog() == true)
            {
                // If a new profile was created, we set it immediately and proceed
                SelectedProfilePath = newProfileWin.CreatedConfigPath;
                this.DialogResult = true;
            }
        }
        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is ProfileItem selected)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the profile '{selected.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        string json = File.ReadAllText(selected.FullPath);
                        var config = JsonConvert.DeserializeObject<AppConfigModel>(json);

                        if(config != null && File.Exists(config.DatabasePath))
                        {
                            File.Delete(config.DatabasePath);
                        }

                        File.Delete(selected.FullPath);

                        MessageBox.Show($"Profile '{selected.Name}' has been deleted.");

                        LoadProfiles(); // Refresh the list after deletion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete profile. It might be currently open and in use.\n\nError: {ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a profile to delete.");
            }
        }

        private void RenameProfile_Click(object sender, RoutedEventArgs e)
        {
            if (lstProfiles.SelectedItem is ProfileItem selected)
            {
                var renameWin = new RenameProfileWindow(selected.Name);
                renameWin.Owner = this;

                if (renameWin.ShowDialog() == true)
                {
                    string newName = renameWin.NewName;
                    try
                    {
                        string oldJsonPath = selected.FullPath;
                        string newJsonPath = Path.Combine(AppPaths.ProfileDir, $"{newName}.json");

                        if (File.Exists(newJsonPath))
                        {
                            MessageBox.Show("A profile with that name already exists. Please choose a different name.");
                            return;
                        }

                        string json = File.ReadAllText(oldJsonPath);
                        var config = JsonConvert.DeserializeObject<AppConfigModel>(json);

                        if (config != null)
                        {
                            string oldDbPath = config.DatabasePath;
                            string newDbPath = Path.Combine(AppPaths.DatabaseDir, $"{newName}.db");

                            if (File.Exists(oldDbPath))
                            {
                                File.Move(oldDbPath, newDbPath);
                            }

                            config.DatabasePath = newDbPath;
                            config.LastUser = newName;

                            FileHelper.SafeWriteConfig(newJsonPath, config);
                            File.Delete(oldJsonPath);

                            MessageBox.Show("Profile renamed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadProfiles();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to rename profile. It might be currently open and in use.\n\nError: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a profile to rename.");
                }
            }
        }
    }
}