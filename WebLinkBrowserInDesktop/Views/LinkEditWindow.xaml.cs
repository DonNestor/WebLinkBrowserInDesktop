using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WebLinkBrowserInDesktop.Models;

namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Logika interakcji dla klasy LinkEditWindow.xaml
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
                MessageBox.Show("Adres URL nie może być pusty!");
                return;
            }

            if(!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) 
                && !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            Links.Name = txtName.Text;
            Links.Url = url;
            Links.BrowserType = (cbx.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Chrome";
            
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
