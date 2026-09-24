using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Http;

namespace ISSTracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private static readonly HttpClient _http = new();
    private async Task<(double lat, double lon)> GetISSPosition()
    {
        var response = await _http.GetStringAsync("http://api.open-notify.org/iss-now.json");
        using var doc = JsonDocument.Parse(response);
        var pos = doc.RootElement.GetProperty("iss_position");
        double lat = double.Parse(pos.GetProperty("latitude").GetString()!);
        double lon = double.Parse(pos.GetProperty("longitude").GetString()!);
        return (lat, lon);
    }

    private async void StartTracking_Click(object sender, RoutedEventArgs e)
    {
        await LoadCrew();
        while (true)
        {
            var (lat, lon) = await GetISSPosition();
            MapCanvas.Children.Clear();
            var (x, y) = LatLonToCanvas(lat, lon, MapCanvas.ActualWidth, MapCanvas.ActualHeight);
            var dot = new Ellipse
            {
                Width = 12,
                Height = 12, 
                Fill = Brushes.OrangeRed
            };
            Canvas.SetLeft(dot, x = 6);
            Canvas.SetTop(dot, y - 6);
            MapCanvas.Children.Add(dot);
            LatLabel.Text = $"Latitude: {lat:F4}";
            LonLabel.Text = $"Longitude: {lon:F4}";
            TimeLabel.Text = $"Last Updated: {DateTime.Now:HH:mm:ss}";
            await Task.Delay(5000);
        }
    }

    private (double x, double y) LatLonToCanvas(double lat, double lon, double width, double height)
    {
        double x = (lon + 180.0) / 360.0 * width;
        double y = (90.0 - lat) / 180.0 * height;
        return (x, y);
    }

    private async Task LoadCrew()
    {
        var response = await _http.GetStringAsync("http://api.open-notify.org/astros.json"); 
        using var doc = JsonDocument.Parse(response);
        var people = doc.RootElement.GetProperty("people");
        var names = people.EnumerateArray().Select(people => people.GetProperty("name").GetString()).ToList();
        CrewList.ItemsSource = names;
    }
}