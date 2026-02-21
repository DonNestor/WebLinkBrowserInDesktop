using System.Windows;
using System.Windows.Controls;
using WebLinkBrowserInDesktop.Models;

namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Interaction logic for the LinkEditWindow.xaml class
    /// </summary>
    public partial class LinkEditWindow : Window
    {
        public WebLinkModel Links { get; private set; }

        public LinkEditWindow(WebLinkModel links = null)
        {
            InitializeComponent();

            if (links != null)
            {
                Links = links;
                txtName.Text = links.Name;
                txtUrl.Text = links.Url;
                cbx.Text = links.BrowserType;
            }
            else
            {
                Links = new WebLinkModel();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string url = txtUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(url))
            {
                MessageBox.Show("URL cannot be empty!");
                return;
            }

            if(!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) 
                && !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            Links.Url = url;
            Links.Name = txtName.Text;
            Links.BrowserType = (cbx.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Chrome";
            
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
