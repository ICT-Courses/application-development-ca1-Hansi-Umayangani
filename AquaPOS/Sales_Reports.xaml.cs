using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Diagnostics;

namespace AquaPOS
{
    public partial class Sales_Reports : Window
    {
        private List<SalesReportItem> reportData = new List<SalesReportItem>();

        public Sales_Reports(string activeSection = "Sales")
        {
            InitializeComponent();

            SetActiveButton(activeSection);
        }

        private void SetActiveButton(string activeSection)
        {
            DashboardButton.Tag = null;
            InventoryManagementButton.Tag = null;
            SalesReportsButton.Tag = null;

            switch (activeSection)
            {
                case "Dashboard":
                    DashboardButton.Tag = "Active";
                    break;
                case "Inventory_Management":
                    InventoryManagementButton.Tag = "Active";
                    break;
                case "Sales":
                    SalesReportsButton.Tag = "Active";
                    break;
            }
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            Dashboard dashboard = new Dashboard();
            dashboard.Show();
            this.Close();
        }

        private void InventoryManagementButton_Click(object sender, RoutedEventArgs e)
        {
            Inventory_Management inventory_Management = new Inventory_Management();
            inventory_Management.Show();
            this.Close();
        }

        private void SalesReportsButton_Click(object sender, RoutedEventArgs e)
        {
            Sales_Reports sales_Reports = new Sales_Reports();
            sales_Reports.Show();
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            User_Login user_Login = new User_Login();
            user_Login.Show(); // Show the login window
            this.Hide(); // Close the current window
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime fromDate = FromDatePicker.SelectedDate ?? DateTime.MinValue;
            DateTime toDate = ToDatePicker.SelectedDate ?? DateTime.MaxValue;

            reportData = GetSalesReportData(fromDate, toDate);
            SalesReportsDataGrid.ItemsSource = reportData;
            TotalSalesTextBlock.Text = $"Rs. {reportData.Sum(item => item.TotalPrice):F2}";
        }

        private List<SalesReportItem> GetSalesReportData(DateTime from, DateTime to)
        {
            List<SalesReportItem> list = new List<SalesReportItem>();
            try
            {
                using (var conn = new SQLiteConnection(DatabaseInitializer.ConnectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT sd.SaleID, si.Category, s.ProductName, s.Quantity, 
                               (s.TotalPrice / s.Quantity) AS UnitPrice,
                               s.TotalPrice, sd.SaleDate
                        FROM Sales s
                        INNER JOIN SalesDetails sd ON s.SaleID = sd.SaleID
                        INNER JOIN StockItems si ON s.ProductID = si.ProductID
                        WHERE DATE(sd.SaleDate) BETWEEN DATE(@From) AND DATE(@To);";

                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@From", from.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@To", to.ToString("yyyy-MM-dd"));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new SalesReportItem
                                {
                                    SaleID = reader.GetInt32(0),
                                    Category = reader.GetString(1),
                                    ProductName = reader.GetString(2),
                                    Quantity = reader.GetInt32(3),
                                    UnitPrice = reader.GetDouble(4),
                                    TotalPrice = reader.GetDouble(5),
                                    SaleDate = reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading sales report: " + ex.Message);
            }
            return list;
        }

        private void SalesReportsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PrintReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Sales Report";
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);

                XFont titleFont = new XFont("Times New Roman", 16, XFontStyleEx.Bold);
                XFont headerFont = new XFont("Times New Roman", 11, XFontStyleEx.Bold);
                XFont regularFont = new XFont("Times New Roman", 10, XFontStyleEx.Regular);

                // Margins and metrics
                XUnit leftMargin = XUnit.FromPoint(40);
                XUnit y = XUnit.FromPoint(40);
                double pageWidthPt = page.Width.Point;
                double usableWidth = pageWidthPt - 2 * leftMargin.Point;
                int columns = 7;
                double colWidthPt = usableWidth / columns;

                // Draw report title
                gfx.DrawString("Sales Report", titleFont, XBrushes.Black,
                    new XRect(0, y.Point, page.Width.Point, XUnit.FromPoint(20).Point), XStringFormats.TopCenter);

                y += XUnit.FromPoint(30);

                // Prepare date range parts
                string fromDate = FromDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
                string toDate = ToDatePicker.SelectedDate?.ToString("yyyy-MM-dd") ?? "";
                string label = "Date Range: ";
                string dateText = $"{fromDate} to {toDate}";

                string fullText = label + dateText;

                XSize fullTextSize = gfx.MeasureString(fullText, regularFont); // Approx width of entire line
                XSize labelSize = gfx.MeasureString(label, headerFont);        // Width of bold part

                double xStart = leftMargin.Point + usableWidth - fullTextSize.Width;
                double yPos = y.Point;

                gfx.DrawString(label, headerFont, XBrushes.Black,
                    new XRect(xStart, yPos, fullTextSize.Width, 20), XStringFormats.TopLeft);

                gfx.DrawString(dateText, regularFont, XBrushes.Black,
                    new XRect(xStart + labelSize.Width, yPos, fullTextSize.Width - labelSize.Width, 20), XStringFormats.TopLeft);

                y += XUnit.FromPoint(30);

                // Table headers
                string[] headers = { "Bill No.", "Category", "Product Name", "Qty", "Unit Price (Rs.)", "Total Price (Rs.)", "Bill Date" };
                double[] columnWidths = {
                    40,  // Bill No.
                    60,  // Category
                    100, // Product Name
                    40,  // Quantity
                    100,  // Unit Price
                    100,  // Total Price
                    100   // Bill Date
                };
                columns = headers.Length;

                double xPos = leftMargin.Point;

                for (int i = 0; i < columns; i++)
                {
                    gfx.DrawString(headers[i], headerFont, XBrushes.Black,
                        new XRect(xPos, y.Point, columnWidths[i], 20),
                        XStringFormats.TopCenter);

                    xPos += columnWidths[i];
                }
                y += XUnit.FromPoint(20);

                double tableStartX = leftMargin.Point;
                double tableWidth = columnWidths.Sum();

                // Underline header row
                gfx.DrawLine(XPens.Black, tableStartX, y.Point, tableStartX + tableWidth, y.Point);
                y += XUnit.FromPoint(10);

                int rowCount = reportData.Count;
                int currentRowIndex = 0;

                // Table rows
                foreach (var item in reportData)
                {
                    string[] row = {
                        item.SaleID.ToString(),
                        item.Category ?? "",
                        item.ProductName ?? "",
                        item.Quantity.ToString(),
                        $"Rs. {item.UnitPrice:F2}",
                        $"Rs. {item.TotalPrice:F2}",
                        item.SaleDate
                    };

                    double rowX = leftMargin.Point;

                    for (int i = 0; i < columns; i++)
                    {
                        gfx.DrawString(
                            row[i],
                            regularFont,
                            XBrushes.Black,
                            new XRect(rowX, y.Point, columnWidths[i], 20),
                            XStringFormats.TopCenter);

                        rowX += columnWidths[i];
                    }

                    y += XUnit.FromPoint(15);

                    // Draw horizontal line UNDER each row — only if NOT the last row
                    if (currentRowIndex < rowCount - 1)
                    {
                        gfx.DrawLine(
                            XPens.LightGray,
                            new XPoint(tableStartX, y.Point),
                            new XPoint(tableStartX + tableWidth, y.Point)
                        );
                    }

                    currentRowIndex++;

                    // Page break check
                    if (y.Point > page.Height.Point - 60)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        y = XUnit.FromPoint(40);

                        // Re-draw table header on new page
                        double headerX = leftMargin.Point;
                        for (int i = 0; i < columns; i++)
                        {
                            gfx.DrawString(headers[i], headerFont, XBrushes.Black,
                                new XRect(headerX, y.Point, columnWidths[i], 20),
                                XStringFormats.TopCenter);
                            headerX += columnWidths[i];
                        }
                        y += XUnit.FromPoint(30);
                    }
                }

                // Draw a horizontal line above the total section
                y += XUnit.FromPoint(20); 

                gfx.DrawLine(XPens.Black, tableStartX, y.Point, tableStartX + tableWidth, y.Point);
                y += XUnit.FromPoint(10);

                string totalText = $"Total Sales: Rs. {reportData.Sum(s => s.TotalPrice):F2}";

                XSize totalTextSize = gfx.MeasureString(totalText, headerFont);

                double amountX = tableStartX + tableWidth - totalTextSize.Width;
                double amountY = y.Point;

                gfx.DrawString(
                    totalText,
                    headerFont,
                    XBrushes.Black,
                    new XRect(amountX, amountY, totalTextSize.Width, totalTextSize.Height),
                    XStringFormats.TopLeft);

                y += XUnit.FromPoint(totalTextSize.Height + 2);

                // Draw double underline under the total amount
                double underlineY1 = y.Point;             
                double underlineY2 = underlineY1 + 2;     

                gfx.DrawLine(XPens.Black, amountX, underlineY1, amountX + totalTextSize.Width, underlineY1);
                gfx.DrawLine(XPens.Black, amountX, underlineY2, amountX + totalTextSize.Width, underlineY2);

                y += XUnit.FromPoint(10);

                // Save and open the document
                string filename = "StockReport.pdf";
                document.Save(filename);
                Process.Start(new ProcessStartInfo(filename) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error printing report: " + ex.Message);
            }
        }

    }

    public class SalesReportItem
    {
        public int SaleID { get; set; }
        public string Category { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
        public string SaleDate { get; set; }
    }
}
