using Microsoft.Data.Sqlite;
using Dapper;
using WebLinkBrowserInDesktop.Models;
using System.Windows;


namespace WebLinkBrowserInDesktop.Services
{
    public class DatabaseService
    {
        private SqliteConnection _connection;

        private void EnsureConnection()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized. Call Initialize() with a valid database path first.");
            }
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }
        }
        public void Initialize(string dbPath)
        {
            CloseConection();
            string connectionString = $"Data Source={dbPath}";

            _connection = new SqliteConnection(connectionString);
            _connection.Open();

            InitializeTables();
        }
        private void InitializeTables()
        {
            string createLinksTableSql = @"
                CREATE TABLE IF NOT EXISTS Links (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Url TEXT NOT NULL,
                    BrowserType TEXT NOT NULL,
                    CategoryId INTEGER
                )";

            _connection.Execute(createLinksTableSql);

            string createCategoriesTableSql = @"
                CREATE TABLE IF NOT EXISTS Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    ParentId INTEGER,
                FOREIGN KEY (ParentId) REFERENCES Categories(Id)
                )";

            _connection.Execute(createCategoriesTableSql);

            try
            {
                _connection.Execute("ALTER TABLE Links ADD COLUMN CategoryId INTEGER");
            }
            catch { }
            finally 
            { 
                //for logging
            }
        }
        public void CloseConection()
        {
            if (_connection != null)
            {
                try
                {
                    // Close the connection
                    if (_connection.State == System.Data.ConnectionState.Open)
                    {
                        _connection.Close();
                    }

                    // Destroy the object (we release resources)
                    _connection.Dispose();
                    _connection = null;

                    //Force garbage collection to ensure SQLite releases the file. 
                    // This helps with "Database is locked" errors when switching quickly.
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error closing database {ex.Message}");
                }
            }
        }

        #region CRUD for Links
        public List<WebLinkModel> GetAllLinks()
        {
            try
            {
                EnsureConnection();
                return _connection.Query<WebLinkModel>("SELECT * FROM Links").ToList();
            }
            catch (SqliteException sqlEx)
            {
                MessageBox.Show("Database error: " + sqlEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<WebLinkModel>(); // Return an empty list on error for application continuity 

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<WebLinkModel>(); // Return an empty list on error for application continuity 
            }
            finally
            {
                // The finally block ALWAYS executes (even if an error occurs or we return).
                // NOTE: We don't close _connection.Close() here because we're using a persistent connection.
                // Here, we could, for example, disable the "spinning loading wheel" (spinner) in the interface
                // or write the log to a text file.
            }
        }
        public void AddLink(WebLinkModel linkModel)
        {
            try
            {
                EnsureConnection();
                string sql = @"INSERT INTO Links (
                                Name, 
                                Url, 
                                BrowserType, CategoryId) 
                              VALUES(
                                @Name, 
                                @Url, 
                                @BrowserType, 
                                @CategoryId)";
                _connection.Execute(sql, linkModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                //For logging
            }
        }
        public void UpdateLink(WebLinkModel linkModel)
        {
            try
            {
                EnsureConnection();
                string sql = @"UPDATE Links 
                                SET Name = @Name, 
                                    Url = @Url, 
                                    BrowserType = @BrowserType, 
                                    CategoryId = @CategoryId 
                                WHERE Id = @Id";
                _connection.Execute(sql, linkModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                //For logging
            }
        }
        public void DeleteLink(int id)
        {
            try
            {
                EnsureConnection();
                string sql = "DELETE FROM Links WHERE Id = @Id";
                _connection.Execute(sql, new { Id = id });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting link: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                //For logging
            }
        }
        #endregion

        #region CRUD for Categories
        public List<CategoryModel> GetAllCategories()
        {
            try
            {
                EnsureConnection();
                return _connection.Query<CategoryModel>("SELECT * FROM Categories").ToList();
            }
            catch (SqliteException sqlEx)
            {
                MessageBox.Show("Database error: " + sqlEx.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<CategoryModel>(); // Return an empty list on error for application continuity 
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unexpected error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<CategoryModel>(); // Return an empty list on error for application continuity
            }
            finally
            {
                // For logging

            }
        }
        public void AddCategory(CategoryModel categoryModel)
        {
            try
            {
                EnsureConnection();
                string sql = "INSERT INTO Categories (Name, ParentId) VALUES(@Name, @ParentId)";
                _connection.Execute(sql, categoryModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding category: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // For logging
            }
        }
        public void UpdateCategory(CategoryModel categoryModel)
        {
            try
            {
                EnsureConnection();
                string sql = @"UPDATE Categories 
                              SET Name = @Name, 
                                  ParentId = @ParentId 
                              WHERE Id = @Id";
                _connection.Execute(sql, categoryModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating category: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // For logging
            }
        }
        public void DeleteCategory(int categoryId)
        {
            try
            {
                EnsureConnection();

                string updateLinksSql = "UPDATE Links SET CategoryId = NULL WHERE CategoryId = @CategoryId";
                _connection.Execute(updateLinksSql, new { CategoryId = categoryId });

                string udateSubCategoriesSql = "UPDATE Categories SET ParentId = NULL WHERE ParentId = @CategoryId";
                _connection.Execute(udateSubCategoriesSql, new { CategoryId = categoryId });

                string deleteSql = "DELETE FROM Categories WHERE Id = @CategoryId";
                _connection.Execute(deleteSql, new { CategoryId = categoryId });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting category: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // For logging
            }
        }
        #endregion
    }
}
