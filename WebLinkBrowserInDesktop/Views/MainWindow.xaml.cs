using Microsoft.VisualBasic;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using WebLinkBrowserInDesktop.Helpers;
using WebLinkBrowserInDesktop.Models;
using WebLinkBrowserInDesktop.Services;
using WebLinkBrowserInDesktop.Views;


namespace WebLinkBrowserInDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppConfigModel _currentConfig;
        private readonly DatabaseService _databaseService;
        private int? _selectedCategoryId = null; // For future category implementation
        private WebLinkModel _currentEditingLink = null; // To track which link is being edited

        public MainWindow(DatabaseService databaseService, AppConfigModel appConfig)
        {
            InitializeComponent();

            _currentConfig = appConfig;
            _databaseService = databaseService;

            this.Title = $"Links Browser - Profile: {_currentConfig.LastUser ?? "Default"}";

            InitializeAsync(); // Call async initialization without awaiting to avoid blocking the UI thread

            LoadCategoriesTree();
        }

        #region Initialize & WebView2
        private async void InitializeAsync()
        {
            await MainWebView.EnsureCoreWebView2Async(null);

            MainWebView.NavigationCompleted += MainWebView_NavigationCompleted;
        }
        private void MainWebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (MainWebView.Source != null)
            {
                txtCurrentUrl.Text = MainWebView.Source.ToString();
            }
        }
        private void LoadUrlIntoBrowser(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) { return; }

                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "http://" + url;
                }
                MainWebView.Source = new Uri(url);
                txtCurrentUrl.Text = url;
            }
            catch (Exception ex)
            {
                MessageBox.Show("URL format error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
        private void BrowseManually()
        {
            CategoryViewGrid.Visibility = Visibility.Collapsed;
            LinkDetailGrid.Visibility = Visibility.Visible;
            _currentEditingLink = null; // Clear any editing state when loading a new URL

            txtEditLinkName.Text = "New ";
            txtEditLinkUrl.Text = txtCurrentUrl.Text;

            LoadUrlIntoBrowser(txtCurrentUrl.Text);
        }
        public void UpdateTitle()
        {
            this.Title = $"Links Browser - Profile: {_currentConfig.LastUser ?? "Default"} | Database: {Path.GetFileName(_currentConfig.DatabasePath)}";
        }
        #endregion

        #region TreeView & Vies (Left & Right)
        private void LoadCategoriesTree()
        {
            try
            {
                var allCategories = _databaseService.GetAllCategories();
                var allLinks = _databaseService.GetAllLinks();

                var rootItems = new ObservableCollection<object>();

                foreach (var link in allLinks)
                {
                    if (link.CategoryId.HasValue)
                    {
                        var parentCategory = allCategories.FirstOrDefault(c => c.Id == link.CategoryId.Value);
                        if (parentCategory != null)
                        {
                            parentCategory.Items.Add(link);
                        }
                    }
                    else
                    {
                        rootItems.Add(link);
                    }
                }

                foreach (var cat in allCategories)
                {
                    if (cat.ParentId == null)
                    {
                        rootItems.Insert(0, cat);
                    }
                    else
                    {
                        var parent = allCategories.FirstOrDefault(n => n.Id == cat.ParentId);
                        if (parent != null)
                        {
                            parent.Items.Insert(0, cat);
                        }
                    }
                }

                CategoriesTree.ItemsSource = rootItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message);
            }
        }
        private void CategoriesTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (CategoriesTree.SelectedItem is CategoryModel selectedCategory)
            {
                //User selected category - filter links by this category
                _selectedCategoryId = selectedCategory.Id;
                _currentEditingLink = null; // Clear any editing state when selecting a category

                //Show category view and hide link details
                CategoryViewGrid.Visibility = Visibility.Visible;
                LinkDetailGrid.Visibility = Visibility.Collapsed;

                RefreshLinkList();
            }
            else if (CategoriesTree.SelectedItem is WebLinkModel selectedLink)
            {
                _currentEditingLink = selectedLink; // Set the currently editing link to the selected one

                CategoryViewGrid.Visibility = Visibility.Collapsed;
                LinkDetailGrid.Visibility = Visibility.Visible;

                txtEditLinkName.Text = selectedLink.Name;
                txtEditLinkUrl.Text = selectedLink.Url;

                LoadCategoriesToComboBox();
                cmbEditLinkCategory.SelectedValue = selectedLink.CategoryId ?? -1; // Select "No Category" if null

                LoadUrlIntoBrowser(selectedLink.Url);
            }
        }
        private void RefreshLinkList()
        {
            var allLinks = _databaseService.GetAllLinks();
            if (_selectedCategoryId.HasValue)
            {
                var filterLinks = allLinks.Where(l => l.CategoryId == _selectedCategoryId.Value).ToList();
            }
        }
        private void ReloadAppLicationState()
        {
            try
            {
                _selectedCategoryId = null;
                _currentEditingLink = null;

                CategoryViewGrid.Visibility = Visibility.Visible;
                LinkDetailGrid.Visibility = Visibility.Collapsed;

                txtCurrentUrl.Text = string.Empty;

                if (MainWebView != null && MainWebView.CoreWebView2 != null)
                {
                    MainWebView.Source = new Uri("about:blank"); // Clear the browser view
                }

                LoadCategoriesTree();
                RefreshLinkList();
                UpdateTitle(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing the application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Toolbar
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (MainWebView.CanGoBack) MainWebView.GoBack();
        }
        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            if (MainWebView.CanGoForward) MainWebView.GoForward();
        }
        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            MainWebView.Reload();
        }
        private void Go_Click(object sender, RoutedEventArgs e)
        {
            BrowseManually();
        }
        private void txtCurrentUrl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BrowseManually();
            }
        }
        private void SaveLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentUrl = txtCurrentUrl.Text;
                if(string.IsNullOrWhiteSpace(currentUrl) || MainWebView.CoreWebView2 == null)
                {
                    MessageBox.Show("First load a page to the browser to save it as a link.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string pageTitle = MainWebView.CoreWebView2.DocumentTitle;
                if(string.IsNullOrWhiteSpace(pageTitle))
                {
                    pageTitle = "Nowy link";
                }

                var newLink = new WebLinkModel
                {
                    Name = pageTitle,
                    Url = currentUrl,
                    BrowserType = "Chrome",
                    CategoryId = _selectedCategoryId
                };


                _databaseService.AddLink(newLink);
                LoadCategoriesTree();
                RefreshLinkList();

                MessageBox.Show($"Save link: {pageTitle}", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Edit Panel
        private void SaveLinkChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEditingLink == null)
            {
                MessageBox.Show("No link selected for editing.");
                return;
            }

            _currentEditingLink.Name = txtEditLinkName.Text;
            _currentEditingLink.Url = txtEditLinkUrl.Text;


            if (string.IsNullOrEmpty(_currentEditingLink.Name) || string.IsNullOrEmpty(_currentEditingLink.Url))
            {
                MessageBox.Show("Name and URL cannot be empty.");
                return;
            }
            
            if (cmbEditLinkCategory.SelectedValue is int selectedCatId && selectedCatId != -1)
            {
                _currentEditingLink.CategoryId = selectedCatId;
            }
            else
            {
                _currentEditingLink.CategoryId = null; // No category
            }

            _databaseService.UpdateLink(_currentEditingLink);

            MessageBox.Show("Link updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            LoadCategoriesTree();

            // After saving changes, show the category view again
            CategoryViewGrid.Visibility = Visibility.Visible;
            LinkDetailGrid.Visibility = Visibility.Collapsed;
            _currentEditingLink = null;
        }
        private void OpenExternal_Click(object seneder, RoutedEventArgs e)
        {
            string urlToOpen = _currentEditingLink?.Url ?? txtCurrentUrl.Text;

            if (!string.IsNullOrWhiteSpace(urlToOpen))
            {
                if (!urlToOpen.StartsWith("http://") && !urlToOpen.StartsWith("https://"))
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = urlToOpen,
                        UseShellExecute = true
                    });
            }
        }
        private void LoadCategoriesToComboBox()
        {
            try
            {
                var categories = _databaseService.GetAllCategories();

                var noCategory = new CategoryModel { Id = -1, Name = "No Category" };

                categories.Insert(0, noCategory);

                cmbEditLinkCategory.ItemsSource = categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories for edit: " + ex.Message);
            }
        }
        #endregion Edit Panel

        #region Menu
        private void ImportDatabase_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Database SQLite (*.db)|*.db";
            ofd.Title = "Choose a database to import";

            if (ofd.ShowDialog() == true)
            {
                string newDbPath = ofd.FileName;

                if (newDbPath == _currentConfig.DatabasePath)
                {
                    MessageBox.Show("Choosed database is already in use.");
                    return;
                }

                try
                {
                    _databaseService.CloseConnection();

                    _currentConfig.DatabasePath = newDbPath;
                    FileHelper.SafeWriteConfig(AppPaths.ActiveProfileFile, _currentConfig);

                    _databaseService.Initialize(newDbPath);

                    ReloadAppLicationState();

                    MessageBox.Show("Successfully loaded new database!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error while importing databas: {ex.Message}");
                }
            }
        }
        private void ProfileManager_Click(object sender, RoutedEventArgs e)
        {

            _databaseService.CloseConnection(); // Close current database connection before switching profiles

            var selectionWindow = new ProfileManagerWindow();
            selectionWindow.Owner = this;

            if (selectionWindow.ShowDialog() == true)
            {
                try
                {
                    var launcherState = new LauncherConfigModel()
                    {
                        LastActiveProfilePath = selectionWindow.SelectedProfilePath,
                        AlwaysLoadLastProfile = selectionWindow.RememberProfile
                    };
                    FileHelper.SafeWriteConfig(AppPaths.ActiveProfileFile, launcherState);

                    LoadProfile(selectionWindow.SelectedProfilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving profile selection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                // If user canceled profile selection, re-initialize database connection with current config to ensure app remains functional
                _databaseService.Initialize(_currentConfig.DatabasePath);
            }
        }
        private void LoadProfile(string configPath)
        {
            try
            {
                string jsonContent = File.ReadAllText(configPath);
                _currentConfig = JsonConvert.DeserializeObject<AppConfigModel>(jsonContent);

                if (_currentConfig == null)
                {
                    throw new Exception("Empty config file.");
                }

                //Initialize now database connection with new path from loaded profile
                _databaseService.Initialize(_currentConfig.DatabasePath);

                ReloadAppLicationState();

                MessageBox.Show($"Switched to profile: {_currentConfig.LastUser}", "Success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profile: {ex.Message}", "Error");
            }
        }
        private void LoadPage(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return;

                MainWebView.Source = new Uri(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Page loading error: " + ex.Message);
            }
        }
        private void SettingsMenu_Click(object sender, RoutedEventArgs e)
        {
            var settingsWin = new SettingsWindow(_currentConfig);
            settingsWin.Owner = this;

            if (settingsWin.ShowDialog() == true)
            {
                try
                {
                    FileHelper.SafeWriteConfig(AppPaths.ActiveProfileFile, _currentConfig);

                    UpdateTitle();
                    MessageBox.Show("The configuration has been saved.");

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving settings: " + ex.Message);
                }
            }
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("LinksBrowser\nVersion 1.0\nAutor: ZB", "About...");
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region CRUD Menu & Category
        private void AddLink_Click(object sender, RoutedEventArgs e)
        {
            var addLinkWindow = new AddLinkWindow();
            addLinkWindow.Owner = this;

            if (addLinkWindow.ShowDialog() == true)
            {
                try
                {
                    var newLink = new WebLinkModel
                    {
                        Name = addLinkWindow.LinkName,
                        Url = addLinkWindow.LinkUrl,
                        BrowserType = "Chrome",
                        CategoryId = _selectedCategoryId
                    };

                    _databaseService.AddLink(newLink);

                    LoadCategoriesTree(); // Refresh the tree to show the new link in the correct category
                    RefreshLinkList(); // Refresh the list to show the new link if we're currently viewing its category
                }
                catch(Exception ex)
                { 
                    MessageBox.Show($"Error adding link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }
        private void EditLink_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesTree.SelectedItem is WebLinkModel selectedLink)
            {
                var editWindow = new LinkEditWindow(selectedLink);
                editWindow.Owner = this;

                if (editWindow.ShowDialog() == true)
                {
                    _databaseService.UpdateLink(editWindow.Links);

                    LoadCategoriesTree();
                    RefreshLinkList();

                    LoadPage(editWindow.Links.Url);
                }
            }
            else
            {
                MessageBox.Show("No link selected for editing.");
            }
        }
        private void DeleteLink_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesTree.SelectedItem is WebLinkModel selectedLink)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {selectedLink.Name}", "Confirmation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _databaseService.DeleteLink(selectedLink.Id);
                    RefreshLinkList();
                    MessageBox.Show("Data deleted");
                }

                LoadCategoriesTree();
                RefreshLinkList();
                LoadPage(selectedLink.Url);
            }
            else
            {
                MessageBox.Show("No link selected for editing.");
            }
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            int? parentId = null;
            string parentName = null;

            if (CategoriesTree.SelectedItem is CategoryModel selectedCategory)
            {
                parentId = selectedCategory.Id;
                parentName = selectedCategory.Name;
            }

            var addCatWin = new AddCategoryWindow(parentName);
            addCatWin.Owner = this;

            if (addCatWin.ShowDialog() == true)
            {
                try
                {
                    var newCategory = new CategoryModel
                    {
                        Name = addCatWin.CategoryName,
                        ParentId = parentId
                    };

                    _databaseService.AddCategory(newCategory);

                    LoadCategoriesTree();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error adding category: " + ex.Message);
                }
            }


        }
        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesTree.SelectedItem is CategoryModel selectedCategory)
            {
                var editCatWin = new EditCategoryWindow(selectedCategory.Name);
                editCatWin.Owner = this; // Pass current name to edit window

                if (editCatWin.ShowDialog() == true)
                {
                    try
                    {
                        selectedCategory.Name = editCatWin.CategoryName;
                        _databaseService.UpdateCategory(selectedCategory);
                        LoadCategoriesTree();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("First select a folder(category) from the tree on the left to edit it.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("No category selected for editing.");
            }
        }
        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesTree.SelectedItem is CategoryModel selectedCategory)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete category '{selectedCategory.Name}'?\n\n"
                    + "All links in this folder will be preserved (will go to the main view).",
                    "Confirmation",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _databaseService.DeleteCategory(selectedCategory.Id);
                        LoadCategoriesTree();

                        _selectedCategoryId = null; // Clear selected category to show all links in main view
                        CategoryViewGrid.Visibility = Visibility.Visible;
                        LinkDetailGrid.Visibility = Visibility.Collapsed;

                        LoadCategoriesTree();
                        RefreshLinkList();

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting category: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("First select a folder(category) from the tree on the left to delete it.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            //private void OpenInSpecificBrowser(string url, string browserType)
            //{
            //    try
            //    {
            //        string exePath = "";

            //        if (_currentConfig.BrowserPaths.TryGetValue(browserType, out string savePath) && !string.IsNullOrWhiteSpace(savePath))

            //        {
            //            exePath = savePath;
            //        }
            //        else
            //        {
            //            exePath = browserType switch
            //            {
            //                "Chrome" => "chrome.exe",
            //                "Opera" => "opera.exe",
            //                "Firefox" => "firefox.exe",
            //                "Tor" => "tor.exe",
            //                _ => ""
            //            };
            //        }

            //        if (string.IsNullOrEmpty(exePath))
            //        {
            //            OpenUrlInDefaultBrowser(url);
            //            return;
            //        }

            //        Process.Start(new ProcessStartInfo
            //        {
            //            FileName = exePath,
            //            Arguments = url,
            //            UseShellExecute = true
            //        });
            //    }
            //    catch
            //    {
            //        OpenUrlInDefaultBrowser(url);
            //    }
            //}

            //private void OpenUrlInDefaultBrowser(string url)
            //{
            //    try
            //    {
            //        Process.Start(new ProcessStartInfo
            //        {
            //            FileName = url,
            //            UseShellExecute = true
            //        });
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show("Failed to open browser: " + ex.Message);
            //    }
            //}

        }
        #endregion CRUD Menu & Category
    }
}