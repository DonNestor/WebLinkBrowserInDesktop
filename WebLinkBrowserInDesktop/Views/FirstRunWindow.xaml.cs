using System.Windows;
using Microsoft.Win32;
using System.IO;


namespace WebLinkBrowserInDesktop.Views
{
    /// <summary>
    /// Logika interakcji dla klasy FirstRunWindow.xaml
    /// </summary>
    public partial class FirstRunWindow : Window
    {
        public string SelectedDbPath { get; private set; }

        public FirstRunWindow()
        {
            InitializeComponent();
        }

        private void CreateNewDb_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Baza danych SQLite (*.db)|*.db";
            sfd.Title = "Wskaż miejsce zapisu nowej bazy";

            if (sfd.ShowDialog() == true)
            {
                SelectedDbPath = sfd.FileName;
                this.DialogResult = true;
            }
        }

        private void LoadExistingDb_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Baza danych SQLite (*.db)|*.db";
       
            if (ofd.ShowDialog() == true)
            {
                SelectedDbPath = ofd.FileName;
                this.DialogResult = true;
            }
        }
    }
}
