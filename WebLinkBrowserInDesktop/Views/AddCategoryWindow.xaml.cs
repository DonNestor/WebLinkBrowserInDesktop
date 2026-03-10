using System.Windows;

namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Logic for AddCategoryWindow.xaml
    /// </summary>
    public partial class AddCategoryWindow : Window
    {
        public string CategoryName { get; set; }
        public AddCategoryWindow(string parentCategoryName = null)
        {
            InitializeComponent();
            txtCategoryName.Focus();

            if(!string.IsNullOrEmpty(parentCategoryName))
            {
                txtParentInfo.Text = $"Add subcategory to '{parentCategoryName}'";
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Category name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CategoryName = txtCategoryName.Text.Trim();
            this.DialogResult = true;
        }
    }
}
