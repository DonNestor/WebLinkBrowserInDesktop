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
            //Download the application folder and create a path to the "Data" subfolder
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Db");

            //Check if the "Data" folder exists, if not, create it
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = dataFolder; // Set the initial directory to the "Data" folder
            sfd.Filter = "Baza danych SQLite (*.db)|*.db";
            sfd.FileName = "WebLink.db"; // Default file name
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
