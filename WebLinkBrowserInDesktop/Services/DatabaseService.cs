using Microsoft.Data.Sqlite;
using Dapper;
using WebLinkBrowserInDesktop.Models;


namespace WebLinkBrowserInDesktop.Services
{
    public class DatabaseService
    {
        private SqliteConnection _connection;

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
            string sql = @"
                CREATE TABLE IF NOT EXISTS Links (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Url TEXT NOT NULL,
                BrowserType TEXT NOT NULL
                )";

            using (var command = new SqliteCommand(sql, _connection))
            {
                command.ExecuteNonQuery();
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


        public List<WebLinkModel> GetAllLinks() 
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection.Query<WebLinkModel>("SELECT * FROM Links").ToList();
        }

        //Insert / Update / Delete
        public void AddLink(WebLinkModel linkModel)
        {
            string sql = "INSERT INTO Links (Name, Url, BrowserType) " 
                   + "    VALUES(@Name, @Url, @BrowserType)";
            _connection.Execute(sql, linkModel);
        }

        public void UpdateLink(WebLinkModel linkModel)
        {
            string sql = "UPDATE Links SET Name = @Name, Url = @Url, BrowserType = @BrowserType WHERE Id = @Id";
            _connection.Execute(sql, linkModel);
        }

        public void DeleteLink(int id)
        {
            string sql = "DELETE FROM Links WHERE Id = @Id";
            _connection.Execute(sql, new { Id = id });
        }

    }
}
