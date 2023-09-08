using MauiApp5.Models;
using System.Drawing;
using System.Text;
using Svg;
using Newtonsoft.Json; 
using System.Reflection;
using DevExpress.Data.Browsing;
using System.Configuration;
using System.ComponentModel;

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
        GeneratePDF.Clicked += GeneratePDFButton_Clicked;
        GenerateCSV.Clicked += SaveCSVButton_Clicked;
        browse.Clicked += OnBrowseClicked;
        LoadFolders();
        LoadDataFromJson();
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
                        bool isFirstRow = true;

                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();
                            var values = line.Split(',');

                            if (values.Length >= 2)
                            {
                                if (!string.IsNullOrWhiteSpace(values[0]) || !string.IsNullOrWhiteSpace(values[1]))
                                {
                                    // Check if this is the first row with headers
                                    if (isFirstRow && values[0].Trim().Equals("Name", StringComparison.OrdinalIgnoreCase) &&
                                        values[1].Trim().Equals("Qty", StringComparison.OrdinalIgnoreCase))
                                    {
                                        isFirstRow = false; // Skip the first row with headers
                                        continue;
                                    }

                                    importedProducts.Add(new ProductModel { Name = values[0], Qty = values[1] });
                                }
                            }
                        }

                        RowsListView.ItemsSource = importedProducts.ToArray();
                    }
                }
            }
            UpdateTotals();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
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
        UpdateTotals();
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

                UpdateTotals();
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
        UpdateTotals();
    }

    private void UpdateTotals()
    {
        // Calculate the total count of Names and the total sum of Qty
        int totalCount = importedProducts.Count;
        int totalQty = importedProducts.Sum(product => int.Parse(product.Qty));

        // Update the labels with the calculated values
        totalCountLabel.Text = totalCount.ToString();
        totalQtyLabel.Text = totalQty.ToString();
    }

    private void SaveCSVButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            StringBuilder csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Name,Qty");

            foreach (var product in importedProducts)
            {
                csvBuilder.AppendLine($"{product.Name},{product.Qty}");
            }

            string csvContent = csvBuilder.ToString();

            string customDirectory = @"D:\Ortigo\ExportCSV";
            Directory.CreateDirectory(customDirectory);

            string fileName = $"ExportedCSV_{DateTime.Now.ToString("yyyyMMddHHmmss")}.csv";
            string outputPath = Path.Combine(customDirectory, fileName);

            File.WriteAllText(outputPath, csvContent);

            //Console.WriteLine($"CSV saved to: {outputPath}");
            DisplayAlert("Success", "Data converted and saved as CSV.", "OK");
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"An error occurred: {ex.Message}");
            DisplayAlert("Error", "An error occurred while saving the CSV.", "OK");
        }
    }

    [Obsolete]
    private void GeneratePDFButton_Clicked(object sender, EventArgs e)
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

    private void LoadDataFromJson() 
    {
        //string jsonFilePath = ConfigurationManager.AppSettings["Template.json"]; 
        string jsonFilePath = "D:\\Projects\\MauiApp5\\MauiApp5\\Resources\\Template.json";

        List<string> myList = new List<string>(); 
        try 
        { 
            string jsonContent = File.ReadAllText(jsonFilePath); 
            myList = JsonConvert.DeserializeObject<List<string>>(jsonContent);

            selecttemplate.ItemsSource = myList;
        } 
        catch (Exception ex) 
        {
            selecttemplate.IsEnabled = false;
            Console.WriteLine("An error occurred: " + ex.Message); 
        } 
    }

    private async void OnBrowseClicked(object sender, EventArgs e)
    {
        try
        {
            List<string> dataToSave = importedProducts.Select(product => $"{product.Name}, {product.Qty}").ToList();

            // Get the selected radio button
            string selectedFolder = "";

            if (Legend.IsChecked)
            {
                selectedFolder = "Legend";
            }
            else if (Radius.IsChecked)
            {
                selectedFolder = "Radius";
            }
            else
            {
                await DisplayAlert("Error", "Please select a radio button.", "OK");
                return;
            }

            // Get the selected template from the Picker
            string selectedTemplate = selecttemplate.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedTemplate))
            {
                await DisplayAlert("Error", "Please select a template.", "OK");
                return;
            }

            // Get the file name from the "Jobs #" entry
            string jobsNumber = jobs.Text.Trim();

            if (string.IsNullOrEmpty(jobsNumber))
            {
                await DisplayAlert("Error", "Please enter a Jobs #.", "OK");
                return;
            }

            // Use the "Jobs #" as part of the file name
            string fileName = $"{jobsNumber}.txt"; // Modify the file name construction here

            // Get the folder path based on the selected radio button
            string folderPath = Path.Combine(@"D:\Ortigo\", selectedFolder);

            // Create the template folder if it doesn't exist
            Directory.CreateDirectory(Path.Combine(folderPath, selectedTemplate));

            string filePath = Path.Combine(folderPath, selectedTemplate, fileName);

            File.WriteAllLines(filePath, dataToSave);

            await DisplayAlert("Success", "Data saved to local storage.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "Error while saving Data to local storage.", "OK");
        }
    }


}
