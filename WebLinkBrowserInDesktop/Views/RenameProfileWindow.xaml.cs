using System.Windows;
using System.IO;

namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Logic for RenameProfileWindow.xaml
    /// </summary>
    public partial class RenameProfileWindow : Window
    {
        public string NewName { get; set; }
        public RenameProfileWindow(string currentName)
        {
            InitializeComponent();
            txtNewName.Text = currentName;
            txtNewName.SelectAll();
            txtNewName.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string name = txtNewName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Enter a profile name");
                return;
            }
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                MessageBox.Show("Profile name contains invalid characters");
                return;
            }

            NewName = name;
            DialogResult = true;
        }
    }
}
