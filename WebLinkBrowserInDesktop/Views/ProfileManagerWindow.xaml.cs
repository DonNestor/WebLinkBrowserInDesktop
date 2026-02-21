using System.IO;
using System.Windows;
using System.Windows.Markup;
using Newtonsoft.Json;
using WebLinkBrowserInDesktop.Models;

namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Logika interakcji dla klasy ProfileManagerWindow.xaml
    /// </summary>
    public partial class ProfileManagerWindow : Window
    {
        public string SelectedProfilePath { get; private set; }
        private string _profilesDir;

        public ProfileManagerWindow()
        {
            InitializeComponent();

            _profilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Profiles");

            if(!Directory.Exists(_profilesDir))
            {
                Directory.CreateDirectory(_profilesDir); // Refactor this function - there is also a way to create this in App.xaml.cs.
            }

            RefreshProfiles();
        }

        private void RefreshProfiles()
        {
            var files = Directory.GetFiles(_profilesDir, "*.json");
            ProfilesListBox.ItemsSource = files.Select(Path.GetFileName).ToList();
        }

        private void NewProfile_Click(object sender, RoutedEventArgs e)
        {
            var newProfileWindow = new NewProfileWindow();
            newProfileWindow.Owner = this;

            if (newProfileWindow.ShowDialog() == true)
            {
                RefreshProfiles();
                MessageBox.Show("Profile created successfully. You can now select it from the list and load it.");
            }
        }

        private void LoadProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ProfilesListBox.SelectedItem != null)
            {
                SelectedProfilePath = Path.Combine(_profilesDir, ProfilesListBox.SelectedItem.ToString());
                this.DialogResult = true;
            }
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs args)
        {
            if (ProfilesListBox.SelectedItem != null)
            {
                string fileName = ProfilesListBox.SelectedItem.ToString();
                if (fileName == "config.json")
                {
                    MessageBox.Show("Unable to delete the main system profile.");
                    return;
                }

                if (MessageBox.Show($"Delete profile {fileName}?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    File.Delete(Path.Combine(_profilesDir, fileName));
                    RefreshProfiles();
                }
            }
        }
    }
}
