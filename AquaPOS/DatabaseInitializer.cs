using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace AquaPOS
{
    internal class DatabaseInitializer
    {
        static string dbpath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Databases", "AquaPOS-Database.db");
        public static string ConnectionString { get; } = $"Data Source={dbpath};Version=3;";

        public static void InitializeDatabase()
        {
            try
            {
                if (!File.Exists(dbpath))
                {
                    SQLiteConnection.CreateFile(dbpath);
                }

                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open(); // Open Connection

                    //Create User Table
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Users (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Username TEXT NOT NULL UNIQUE,
                            Password TEXT NOT NULL,
                            UserRole TEXT CHECK(UserRole IN ('Admin', 'Cashier')) NOT NULL
                        );";

                    //Create StockItems Table
                    string createStockTableQuery = @"
                        CREATE TABLE IF NOT EXISTS StockItems (
                            ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Category TEXT NOT NULL,
                            ProductName TEXT NOT NULL UNIQUE,
                            CostPrice REAL NOT NULL,
                            UnitPrice REAL NOT NULL,
                            Quantity INTEGER NOT NULL,
                            TotalCostPrice REAL NOT NULL,
                            DateUpdated DATETIME NOT NULL
                        );";

                    // Create Sales table: record each bill with overall info
                    string createSalesTableQuery = @"
                        CREATE TABLE IF NOT EXISTS Sales (
                            ID INTEGER PRIMARY KEY AUTOINCREMENT,
                            SaleID INTEGER NOT NULL,
                            ProductID INTEGER NOT NULL,
                            ProductName TEXT NOT NULL,
                            Quantity INTEGER NOT NULL,
                            TotalPrice REAL NOT NULL,
                            FOREIGN KEY (SaleID) REFERENCES SalesDetails(SaleID),
                            FOREIGN KEY(ProductID) REFERENCES StockItems(ProductID)
                        );";

                    // Create Sales Details table
                    string createSalesDetailsTable = @"
                        CREATE TABLE IF NOT EXISTS SalesDetails (
                            SaleID INTEGER PRIMARY KEY AUTOINCREMENT,
                            TotalAmount REAL NOT NULL,
                            SaleDate TEXT NOT NULL
                        );";

                    using (var cmd = new SQLiteCommand(createTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SQLiteCommand(createStockTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SQLiteCommand(createSalesTableQuery, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SQLiteCommand(createSalesDetailsTable, conn))
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

        // STOCK METHODS --------------------------------------
        public static List<StockItem> GetStockItems()
        {
            List<StockItem> stockItems = new List<StockItem>();

            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM StockItems;";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                stockItems.Add(new StockItem
                                {
                                    ProductID = reader.GetInt32(0),
                                    Category = reader.GetString(1),
                                    ProductName = reader.GetString(2),
                                    CostPrice = reader.GetDouble(3),
                                    UnitPrice = reader.GetDouble(4),
                                    Quantity = reader.GetInt32(5),
                                    TotalCostPrice = reader.GetDouble(6),
                                    DateUpdated = reader.GetString(7)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return stockItems;
        }

        public static StockItem GetStockItemByProductName(string productName)
        {
            StockItem item = null;

            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM StockItems WHERE ProductName = @ProductName;";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", productName);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                item = new StockItem
                                {
                                    ProductID = reader.GetInt32(0),
                                    Category = reader.GetString(1),
                                    ProductName = reader.GetString(2),
                                    UnitPrice = reader.GetDouble(3),
                                    Quantity = reader.GetInt32(4),
                                    DateUpdated = reader.GetString(5)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return item;
        }

        public static void AddStockItem(StockItem item)
        {
            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO StockItems (Category, ProductName, CostPrice, UnitPrice, Quantity, TotalCostPrice, DateUpdated) " +
                        "VALUES (@Category, @ProductName, @CostPrice, @UnitPrice, @Quantity, @TotalCostPrice, @DateUpdated)";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Category", item.Category);
                        cmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                        cmd.Parameters.AddWithValue("@CostPrice", item.CostPrice);
                        cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@TotalCostPrice", item.CostPrice * item.Quantity);
                        cmd.Parameters.AddWithValue("@DateUpdated", item.DateUpdated);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // STOCK UPDATE METHOD ---------------------------------------
        public static void UpdateStockItem(StockItem item)
        {
            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        UPDATE StockItems SET 
                            Category = @Category, 
                            ProductName = @ProductName,
                            CostPrice = @CostPrice,
                            UnitPrice = @UnitPrice, 
                            Quantity = @Quantity,
                            TotalCostPrice = @TotalCostPrice,
                            DateUpdated = @DateUpdated
                        WHERE ProductID = @ProductID";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Category", item.Category);
                        cmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                        cmd.Parameters.AddWithValue("@CostPrice", item.CostPrice);
                        cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@TotalCostPrice", item.CostPrice * item.Quantity);
                        cmd.Parameters.AddWithValue("@DateUpdated", item.DateUpdated);
                        cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void DeleteStockItem(int productID)
        {
            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM StockItems WHERE ProductID = @ProductID";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", productID);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        // SALES METHODS --------------------------------------
        public static void RecordSale(List<Sale> saleItems, double totalAmount, string saleDate)
        {
            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        // Insert into SalesDetails table (main bill)
                        long saleID;
                        string insertSaleDetails = "INSERT INTO SalesDetails (TotalAmount, SaleDate) VALUES (@TotalAmount, @SaleDate);";
                        using (var cmd = new SQLiteCommand(insertSaleDetails, conn))
                        {
                            cmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            cmd.Parameters.AddWithValue("@SaleDate", saleDate);
                            cmd.ExecuteNonQuery();

                            // Get the newly created SaleID
                            saleID = conn.LastInsertRowId;
                        }

                        // Insert each item into Sales table
                        foreach (var item in saleItems)
                        {
                            string insertSale = "INSERT INTO Sales (SaleID, ProductID, ProductName, Quantity, TotalPrice) VALUES (@SaleID, @ProductID, @ProductName, @Quantity, @TotalPrice);";
                            using (var cmd = new SQLiteCommand(insertSale, conn))
                            {
                                cmd.Parameters.AddWithValue("@SaleID", saleID);
                                cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                                cmd.Parameters.AddWithValue("@ProductName", item.ProductName);
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@TotalPrice", item.TotalPrice);
                                cmd.ExecuteNonQuery();
                            }

                            // Update stock for each item
                            string updateStock = "UPDATE StockItems SET Quantity = Quantity - @Quantity WHERE ProductID = @ProductID;";
                            using (var cmd = new SQLiteCommand(updateStock, conn))
                            {
                                cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                                cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        
                        transaction.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recording sale: {ex.Message}");
            }
        }

        // TOTAL SALES REVENUE CALCULATION METHOD ------------------------------
        public static double GetTotalSalesRevenue()
        {
            double totalRevenue = 0;

            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT SUM(TotalAmount) FROM SalesDetails;";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            totalRevenue = Convert.ToDouble(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving total revenue: {ex.Message}");
            }

            return totalRevenue;
        }

        // TOTAL COST CALCULATION METHOD ----------------------------
        public static double GetTotalCostPrice()
        {
            double totalCost = 0;

            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT SUM(TotalCostPrice) FROM StockItems;";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        var result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            totalCost = Convert.ToDouble(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving total cost price: {ex.Message}");
            }

            return totalCost;
        }

        // TOTAL INCOME CALCULATION METHOD ------------------------------
        public static double GetTotalIncome()
        {
            double totalRevenue = GetTotalSalesRevenue();
            double totalCost = GetTotalCostPrice();
            return totalRevenue - totalCost;
        }

        // TODAY'S SALES REVENUE CALCULATION METHOD ---------------------
        public static double GetTodaysSalesRevenue()
        {
            double todaysRevenue = 0;

            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();

                    string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

                    string query = "SELECT SUM(TotalAmount) FROM SalesDetails WHERE DATE(SaleDate) = @TodayDate;";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TodayDate", todayDate);

                        var result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            todaysRevenue = Convert.ToDouble(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving today's revenue: {ex.Message}");
            }

            return todaysRevenue;
        }

        // LOW STOCK METHOD ------------------------------------
        public static List<StockItem> GetLowStockItems(int threshold = 5)
        {
            var lowStockItems = new List<StockItem>();

            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT Category, ProductName, Quantity FROM StockItems WHERE Quantity <= @Threshold;";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Threshold", threshold);
                        using (var reader = cmd.ExecuteReader())

                        {
                            while (reader.Read())
                            {
                                lowStockItems.Add(new StockItem
                                {
                                    Category = reader.GetString(0),
                                    ProductName = reader.GetString(1),
                                    Quantity = reader.GetInt32(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving low stock items: {ex.Message}");
            }

            return lowStockItems;
        }

        // PRODUCT NAME LIST METHOD ------------------------------------
        public static List<string> GetAllProductNames()
        {
            var productNames = new List<string>();

            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT ProductName FROM StockItems ORDER BY ProductName ASC;";
                    using (var cmd = new SQLiteCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productNames.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching product names: {ex.Message}");
            }

            return productNames;
        }

        // SOLD & AVAILABLE PRODUCT QUANTITY METHOD -----------------------------
        public static int GetAvailableQuantity(string productName)
        {
            int quantity = 0;
            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT Quantity FROM StockItems WHERE ProductName = @ProductName";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", productName);
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            quantity = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting available quantity: {ex.Message}");
            }
            return quantity;
        }

        public static int GetSoldQuantity(string productName)
        {
            int soldQuantity = 0;
            try
            {
                using (var conn = new SQLiteConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT SUM(Quantity) FROM Sales WHERE ProductName = @ProductName";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", productName);
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            soldQuantity = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting sold quantity: {ex.Message}");
            }
            return soldQuantity;
        }
    }
}

