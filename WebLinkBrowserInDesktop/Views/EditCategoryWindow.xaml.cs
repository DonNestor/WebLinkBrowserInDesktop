using System.Windows;

namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Logic for EditCategoryWindow.xaml
    /// </summary>
    public partial class EditCategoryWindow : Window
    {
        public string CategoryName { get; private set; }
        public EditCategoryWindow(string currentName)
        {
            InitializeComponent();

            txtCategoryName.Text = currentName;
            txtCategoryName.SelectAll();
            txtCategoryName.Focus();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Category name cannot be empty.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            CategoryName = txtCategoryName.Text.Trim();
            this.DialogResult = true;
        }
    }
}
