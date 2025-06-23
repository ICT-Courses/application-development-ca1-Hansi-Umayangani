using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;
using LvcWpf = LiveCharts.Wpf;

namespace AquaPOS
{
    /// <summary>
    /// Interaction logic for Dashboard.xaml
    /// </summary>
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();

            this.DataContext = this;

            LoadTotalSalesRevenue();
            LoadTotalIncome();
            LoadTodaysSalesRevenue();
            LoadLowStockItemCount();
            LoadStockDoughnutChart();
        }

        private void LoadTotalSalesRevenue()
        {
            double totalRevenue = DatabaseInitializer.GetTotalSalesRevenue();
            TotalSalesRevenueText.Text = $"Rs. {totalRevenue:N2}";
        }

        private void LoadTotalIncome()
        {
            double totalIncome = DatabaseInitializer.GetTotalIncome();
            TotalIncomeText.Text = $"Rs. {totalIncome:N2}";
        }

        private void LoadTodaysSalesRevenue()
        {
            double todaysRevenue = DatabaseInitializer.GetTodaysSalesRevenue();
            TodaysSalesText.Text = $"Rs. {todaysRevenue:N2}";
        }

        private void LoadLowStockItemCount()
        {
            var lowStockItems = DatabaseInitializer.GetLowStockItems();
            int count = lowStockItems.Count;
            LowStockCountText.Text = $"{count} Item{(count != 1 ? "s" : "")}";
        }

        private void LowStockSummaryCard_Click(object sender, MouseButtonEventArgs e)
        {
            LowStockWindow lowStockWindow = new LowStockWindow();
            lowStockWindow.ShowDialog();
        }

        private void SearchProduct_Click(object sender, RoutedEventArgs e)
        {
            string selectedProduct = cmbProductSearch.Text;
            if (!string.IsNullOrWhiteSpace(selectedProduct))
            {
                ShowProductComparisonChart(selectedProduct);
            }
        }

        private void CmbProductSearch_Loaded(object sender, RoutedEventArgs e)
        {
            var productNames = DatabaseInitializer.GetAllProductNames();
            cmbProductSearch.ItemsSource = productNames;
            ProductListView.ItemsSource = productNames;

            string defaultProduct = "Coca-Cola"; // 🟦 Use a valid product name
            if (productNames.Contains(defaultProduct))
            {
                cmbProductSearch.SelectedItem = defaultProduct;
                ProductListView.SelectedItem = defaultProduct;
            }
        }

        private void CmbProductSearch_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            string typedText = cmbProductSearch.Text + e.Text;

            var filtered = DatabaseInitializer.GetAllProductNames()
                              .Where(p => p.ToLower().Contains(typedText.ToLower()))
                              .ToList();

            cmbProductSearch.ItemsSource = filtered;
            cmbProductSearch.IsDropDownOpen = true;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CmbProductSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ProductListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductListView.SelectedItem != null)
            {
                string selectedProduct = ProductListView.SelectedItem.ToString();
                ShowProductComparisonChart(selectedProduct);
            }
        }

        private void ShowProductComparisonChart(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                return;

            int availableQty = DatabaseInitializer.GetAvailableQuantity(productName);
            int soldQty = DatabaseInitializer.GetSoldQuantity(productName);

            ProductStockBarChart.Series = new LiveCharts.SeriesCollection
            {
                new LvcWpf.ColumnSeries
                {
                    Title = "Available",
                    Values = new ChartValues<int> { availableQty },
                    Fill = new SolidColorBrush(Color.FromRgb(149,237,100)),
                    Stroke = Brushes.White, // border color of bars
                    StrokeThickness = 0.5
                },
                new LvcWpf.ColumnSeries
                {
                    Title = "Sold",
                    Values = new ChartValues<int> { soldQty },
                    Fill = new SolidColorBrush(Color.FromRgb(38,255,255)),
                    Stroke = Brushes.White,
                    StrokeThickness = 0.5

                }
            };

            ProductStockBarChart.AxisX.Clear();
            ProductStockBarChart.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Product",
                Labels = new[] { productName },
                Foreground = Brushes.White,                   // Axis line color
                Separator = new LiveCharts.Wpf.Separator  // Grid line customization
                {
                    Step = 1,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.5
                }
            });


            ProductStockBarChart.AxisY.Clear();
            ProductStockBarChart.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Quantity",
                LabelFormatter = value => value.ToString("N0"),
                Foreground = Brushes.White,
                Separator = new LiveCharts.Wpf.Separator
                {
                    Stroke = Brushes.White,
                    StrokeThickness = 0.5
                }
            });

            // Customize tooltip foreground color to White
            var defaultTooltip = ProductStockBarChart.DataTooltip as LiveCharts.Wpf.DefaultTooltip;
            if (defaultTooltip == null)
            {
                defaultTooltip = new LiveCharts.Wpf.DefaultTooltip();
            }
            defaultTooltip.Foreground = Brushes.Black;
            ProductStockBarChart.DataTooltip = defaultTooltip;
        }

        private void ProductStockBarChart_Loaded(object sender, RoutedEventArgs e)
        {
            string defaultProduct = "Goldfish"; // 🔁 Replace with a real product in your DB
            ShowProductComparisonChart(defaultProduct);

            // Optionally preselect it in the ComboBox and ListView
            cmbProductSearch.SelectedItem = defaultProduct;
            ProductListView.SelectedItem = defaultProduct;
        }

        private void LoadStockDoughnutChart()
        {
            var categoryData = DatabaseInitializer.GetStockDistributionByCategory();

            var pieSeriesCollection = new LiveCharts.SeriesCollection();

            var colors = new[]
            {
                Color.FromRgb(30, 182, 255),
                Color.FromRgb(146,217,0),
                Color.FromRgb(233, 150, 122),
                Color.FromRgb(255, 182, 0),
                Color.FromRgb(115, 155, 255),
                Color.FromRgb(217,65,109),
                Color.FromRgb(134, 217, 7),
                Color.FromRgb(152,85,255),
                Color.FromRgb(255,118,172)
            };

            int colorIndex = 0;

            foreach (var item in categoryData)
            {
                pieSeriesCollection.Add(new PieSeries
                {
                    Title = item.Key,
                    Values = new ChartValues<int> { item.Value },
                    DataLabels = true,
                    LabelPoint = chartPoint => chartPoint.Participation.ToString("P0"),
                    Fill = new SolidColorBrush(colors[colorIndex % colors.Length]),

                    Stroke = Brushes.White,
                    StrokeThickness = 1,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Normal
                });

                colorIndex++;
            }

            StockDoughnutChart.Series = pieSeriesCollection;

            // Customize the default tooltip's foreground color to Black
            var defaultTooltip = StockDoughnutChart.DataTooltip as LiveCharts.Wpf.DefaultTooltip;
            if (defaultTooltip == null)
            {
                defaultTooltip = new LiveCharts.Wpf.DefaultTooltip();
            }
            defaultTooltip.Foreground = Brushes.Black;
            StockDoughnutChart.DataTooltip = defaultTooltip;
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
    }
}
