using MauiApp5.Models;
using System.Drawing;
using IronPdf;
using System.Text;
using Svg;
using Microsoft.Maui.Controls.Xaml;
using System.Drawing.Printing;
using SkiaSharp;

namespace MauiApp5;

public partial class MainPage : ContentPage
{
    

    public MainPage()
    {
        InitializeComponent();
        RowsListView.ItemsSource = importedProducts;
        RowsListView.BindingContext = this;
        RowsListView.ItemSelected += RowsListView_ItemSelected;
        AddRow.Clicked += AddRowButton_Clicked;
        DeleteRow.Clicked += DeleteRowButton_Clicked;
        ClearTable.Clicked += ClearTableButton_Clicked;
        GenerateSVG.Clicked += GenerateSVGButton_Clicked;
        LoadFolders();
    }

    private List<ProductModel> importedProducts = new List<ProductModel>();
    int count = 0;
    private async void ImportCSV_Clicked(object sender, EventArgs e)
    {
        try 
        {
            var result = await FilePicker.PickAsync();

            if (result != null)
            {
                using (var stream = await result.OpenReadAsync())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        importedProducts.Clear(); // Clear the existing data
                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            var values = line.Split(',');

                            if (values.Length >= 2)
                            {
                                if (!string.IsNullOrWhiteSpace(values[0]) || string.IsNullOrWhiteSpace(values[1]))
                                {

                                    importedProducts.Add(new ProductModel { Name = values[0], Qty = values[1] });
                                }
                            }
                        }

                        RowsListView.ItemsSource = importedProducts.ToArray();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }

        //UpdateQty();
    }

    private void RowsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            if (e.SelectedItem != null)
            {
                // Handle row selection here if needed
                var selectedProduct = (ProductModel)e.SelectedItem;
                //DisplayAlert("Selected", $"Selected item: {selectedProduct.Name} - {selectedProduct.Qty}", "OK");

            }
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private void AddRowButton_Clicked(object sender, EventArgs e)
    {
        // Add a new row to the importedProducts list
        importedProducts.Add(new ProductModel { Name = $"New Product {count}", Qty = $"{count}" });
        count++;

        // Update the ListView's item source
        RowsListView.ItemsSource = importedProducts.ToArray();
    }

    private void DeleteRowButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (RowsListView.SelectedItem != null)
            {
                var selectedProduct = (ProductModel)RowsListView.SelectedItem;
                importedProducts.Remove(selectedProduct);

                // Update the ListView's item source
                RowsListView.ItemsSource = importedProducts.ToArray();


                // Clear the selection after deletion
                RowsListView.SelectedItem = null;
            }
        }
        catch (Exception ex)
        {
             DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private void ClearTableButton_Clicked(object sender, EventArgs e)
    {
        // Clear the entire importedProducts list
        importedProducts.Clear();

        // Reset the count variable to 0
        count = 0;

        // Update the ListView's item source
        RowsListView.ItemsSource = importedProducts.ToArray();
    }

    private void GeneratePDF(object sender, EventArgs e)
    {
        try
        {
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<html><body>");
            htmlBuilder.Append("<h1>Imported Products</h1>");
            htmlBuilder.Append("<ul>");

            foreach (var product in importedProducts)
            {
                htmlBuilder.AppendFormat("<li>{0} - {1:C}</li>", product.Name, product.Qty);
            }

            htmlBuilder.Append("</ul>");
            htmlBuilder.Append("</body></html>");

            string htmlContent = htmlBuilder.ToString();

            HtmlToPdf renderer = new HtmlToPdf();
            IronPdf.PdfDocument pdfDocument = renderer.RenderHtmlAsPdf(htmlContent);

            string customDirectory = @"D:\Ortigo\ExportPdf";
            string fileName = $"ExportJob_{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf"; // Include timestamp in the filename

            string outputPath = Path.Combine(customDirectory, fileName);
            pdfDocument.SaveAs(outputPath);

            Console.WriteLine($"PDF saved to: {outputPath}");
            DisplayAlert("Success", "Object data converted and saved as PDF.", "OK");
        }

        catch (Exception ex)
        {
            DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private void GenerateSVGButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var svgDocument = new SvgDocument();

            float yOffset = 0;

            foreach (var product in importedProducts)
            {
                var text = new SvgText
                {
                    FontSize = new SvgUnit(12),
                    Fill = new SvgColourServer(System.Drawing.Color.Black),
                    X = new SvgUnitCollection { GetXPosition(product.Name) },
                    Y = new SvgUnitCollection { yOffset },
                    Text = $"{product.Name}         {product.Qty}" // Adding line break
                };

                svgDocument.Children.Add(text);

                yOffset += CalculateTextHeight(text) + 0; // Adjust spacing
            }

            string customDirectory = @"D:\Ortigo\ExportSVG";
            Directory.CreateDirectory(customDirectory);

            string fileName = $"SaveSVG_{DateTime.Now.ToString("yyyyMMddHHmmss")}.svg";
            string outputPath = Path.Combine(customDirectory, fileName);

            svgDocument.Write(outputPath);

            Console.WriteLine($"SVG saved to: {outputPath}");
            DisplayAlert("Success", "Object data converted and saved as SVG.", "OK");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            DisplayAlert("Error", "An error occurred while generating the SVG.", "OK");
        }
    }

    private float CalculateTextHeight(SvgText text)
    {
        using (var font = new System.Drawing.Font("Arial", (float)text.FontSize.Value))
        using (var bmp = new Bitmap(1, 1))
        using (var graphics = Graphics.FromImage(bmp))
        {
            var size = graphics.MeasureString(text.Text, font);
            return size.Height;
        }
    }

    private static float GetXPosition(string Name)
    {
        return Name.Length * 1;
    }

    private void LoadFolders()
    {
        string basePath = @"C:\Users\anup.ghodake\Desktop\TestCSV";

        if (Directory.Exists(basePath))
        {
            List<string> folderNames = Directory.GetDirectories(basePath)
                                               .Select(Path.GetFileName)
                                               .ToList();

            folderPicker.ItemsSource = folderNames;
        }
        else
        {
            folderPicker.IsEnabled = false;
        }
    }

    //private void LoadFolders()
    //{
    //    string basePath = @"C:\Users\anup.ghodake\Desktop\TestCSV";

    //    if (Directory.Exists(basePath))
    //    {
    //        List<string> folderNames = Directory.GetDirectories(basePath)
    //                                           .Select(Path.GetFileName)
    //                                           .ToList();

    //        folderPicker.ItemsSource = folderNames;
    //        placeholderLabel.IsVisible = false;
    //        folderPicker.IsVisible = true;
    //    }
    //    else
    //    {
    //        folderPicker.IsEnabled = false;
    //        placeholderLabel.IsVisible = true;
    //        folderPicker.IsVisible = false;
    //    }
    //}

}
