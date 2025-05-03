using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaPOS
{
    internal class DatabaseInitializer
    {
        static string dbpath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Databases", "AquaPOS-Database.db");
        public static string ConnectionString = $"Data Source={dbpath};Version=3;";


        public static void InitializeDatabase()
        {
            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open(); // Open Connection

                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username TEXT NOT NULL UNIQUE,
                            Password TEXT NOT NULL,
                            UserRole TEXT CHECK(UserRole IN ('Admin', 'Cashier')) NOT NULL
                        );";

                    // StockItem table
                    string createStockTableQuery = @"
                        CREATE TABLE IF NOT EXISTS StockItems (
                            ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Category TEXT NOT NULL,
                            ProductName TEXT NOT NULL UNIQUE,
                            UnitPrice REAL NOT NULL,
                            Quantity INTEGER NOT NULL,
                            DateUpdated TEXT NOT NULL
                        );";

                    using (var cmd = new SQLiteCommand(createTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SQLiteCommand(createStockTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

    }
}
