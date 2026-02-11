using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;
using System.IO;
using Dapper;
using WebLinkBrowserInDesktop.Models;


namespace WebLinkBrowserInDesktop.Services
{
    public class DatabaseService
    {
        private string _connectionString;

        public void Initialize(string dbPath)
        {
            _connectionString = $"Data Source={dbPath}";

            using var connection = new SqliteConnection(_connectionString);
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Links (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Url TEXT NOT NULL,
                BrowserType TEXT NOT NULL
                )");
        }

        public List<WebLinkModel> GetAllLinks() 
        {
            using var connection = new SqliteConnection(_connectionString);
            return connection.Query<WebLinkModel>("SELECT * FROM Links").ToList();
        }

        //Insert / Updatte / Delete
    }
}
