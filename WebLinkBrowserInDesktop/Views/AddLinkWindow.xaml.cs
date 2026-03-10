using System.Windows;

namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Logic for AddLinkWindow.xaml
    /// </summary>
    public partial class AddLinkWindow : Window
    {
        public string LinkName { get; private set; }
        public string LinkUrl { get; private set; }

        public AddLinkWindow()
        {
            InitializeComponent();

            txtLinkName.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLinkUrl.Text))
            {
                MessageBox.Show("Adres URL jest wymagany!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string name = txtLinkName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = txtLinkUrl.Text.Trim();
            }

            LinkName = name;

            string url = txtLinkUrl.Text.Trim();
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }
            LinkUrl = url;
            this.DialogResult = true;
        }
    }
}