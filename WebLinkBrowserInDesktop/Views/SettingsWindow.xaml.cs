using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using WebLinkBrowserInDesktop.Models;

namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Interaction logic for the SettingsWindow.xaml class
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private AppConfig _currentConfig;

        public List<BrowserItem> EditableBrowser { get; set; }

        public SettingsWindow(AppConfig currentConfig)
        {
            InitializeComponent();
            _currentConfig = currentConfig;

            EditableBrowser = _currentConfig.BrowserPaths.Select(x => new BrowserItem
            {
                Name = x.Key,
                Path = x.Value
            }).ToList();

            txtUserName.Text = _currentConfig.LastUser;
            BrowsersItemsControl.ItemsSource = EditableBrowser;
        }

        private void BrowsePath_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string browserName = button.Tag.ToString(); //Download the browser name from the button's Tag property ex. "Chrome", "Edge", "Firefox"

            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Executable files (*.exe)|*.exe" };
            if(ofd.ShowDialog() == true)
            {
                _currentConfig.BrowserPaths[browserName] = ofd.FileName;
                // Refresh the ItemsControl to show the updated path
                BrowsersItemsControl.ItemsSource = null;
                BrowsersItemsControl.ItemsSource = _currentConfig.BrowserPaths.ToList();
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            _currentConfig.LastUser = txtUserName.Text;

            foreach (var item in EditableBrowser)
            {
                if(_currentConfig.BrowserPaths.ContainsKey(item.Name))
                {
                    _currentConfig.BrowserPaths[item.Name] = item.Path; 
                }
            }

            this.DialogResult = true;
        }
    }
}
