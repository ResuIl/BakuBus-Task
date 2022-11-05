using Microsoft.Maps.MapControl.WPF;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace WPF_HomeWork_15;

public partial class MainWindow : Window
{
    Dictionary<string, string> BakuBusLines = new Dictionary<string, string>() 
    {
        {"M8",  "11156"}, {"M3", "11050"}, {"M2", "11049"}, {"M1", "11048"}, {"Q1", "11155"}, {"1", "11032"}, 
        {"2", "11035"}, {"3", "11037"}, {"5", "11031"}, {"6", "11033"}, {"7B", "11046"}, {"7A", "11045"}, {"10", "11150"},
        {"11", "11056"}, {"13", "11039"}, {"14", "11036"}, {"17", "11043"}, {"21", "11040"}, {"24", "11151"}, {"30", "11055"},
        {"32", "11152"}, {"35", "11153"}, {"88", "11047"}, {"88A", "11034"}, {"125", "11041"}, {"175", "11044"}, {"205", "11158"},
        {"211", "11157"}
    };

    Random rnd = new Random();
    DispatcherTimer timer = new DispatcherTimer();
    LocationCollection locs = new LocationCollection();

    private string SelectedBus = "-1";
    private int SelectedIndex = -1;
    private bool canRequest = true;

    private BakuBus? _bakuBus;
    public BakuBus? BakuBus
    {
        get { return _bakuBus; }
        set { _bakuBus = value; LoadBus(); }
    }

    public MainWindow()
    {
        InitializeComponent();
        BingMap.CredentialsProvider = new ApplicationIdCredentialsProvider(System.Configuration.ConfigurationManager.AppSettings["mapApi"]);   
        DataContext = this;
        timer.Interval = new TimeSpan(0, 0, 0, 1);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        timer.Tick += new EventHandler(timerTick_event);
        GetBusListAsync();
        timer.Start();
    }

    async void LoadBus()
    {
        BingMap.Children.Clear();
        lBox.Items.Clear();
        SelectedIndex = -1;
        Pushpin pin123 = new Pushpin() { Location = new Location(40.4583, 49.7522), Content = "Resul", ToolTip = (ControlTemplate)this.FindResource("customPushPinToolTip") };
        BingMap.Children.Add(pin123);
        for (int i = 0; i < BakuBus.Buses.Count; i++)
        {
            if (SelectedBus != "-1")
            {
                if (SelectedBus == BakuBus.Buses[i].Attributes.DISPLAY_ROUTE_CODE)
                {
                    if (canRequest)
                    {
                        try
                        {
                            locs.Clear();
                            var jsonString = (JsonConvert.DeserializeObject(await new HttpClient().GetStringAsync("https://www.bakubus.az/az/ajax/getPaths/" + BakuBusLines[SelectedBus])) as JObject)["Forward"]["busstops"];
                            foreach (var item in jsonString)
                                locs.Add(new Location(double.Parse(item["latitude"].ToString(), NumberStyles.Float, CultureInfo.CreateSpecificCulture("fr-FR")), double.Parse(item["longitude"].ToString(), NumberStyles.Float, CultureInfo.CreateSpecificCulture("fr-FR"))));
                            
                            canRequest = false;
                        }
                        catch (Exception)
                        {
                            canRequest = false;
                            MessageBox.Show("Bus Route Not Found");
                        }
                    }

                    if (locs is not null)
                    {
                        MapPolyline routeLine = new MapPolyline() { Locations = locs, Stroke = new SolidColorBrush(Colors.Blue), StrokeThickness = 5 };
                        BingMap.Children.Add(routeLine);
                    } 

                    Pushpin pin = new Pushpin() { Location = new Location(double.Parse(BakuBus.Buses[i].Attributes.LATITUDE, NumberStyles.Float, CultureInfo.CreateSpecificCulture("fr-FR")), double.Parse(BakuBus.Buses[i].Attributes.LONGITUDE, NumberStyles.Float, CultureInfo.CreateSpecificCulture("fr-FR"))), Template = (ControlTemplate)this.FindResource("customPushPin"), Content = BakuBus.Buses[i].Attributes.DISPLAY_ROUTE_CODE, Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256))) };
                    BingMap.Children.Add(pin);
                }
            } 
            else
            {
                if (!lBox.Items.Contains(BakuBus.Buses[i].Attributes.DISPLAY_ROUTE_CODE))
                    lBox.Items.Add(BakuBus.Buses[i].Attributes.DISPLAY_ROUTE_CODE);
                lBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("",System.ComponentModel.ListSortDirection.Ascending));
                Pushpin pin = new Pushpin() { Location = new Location(double.Parse(BakuBus.Buses[i].Attributes.LATITUDE, NumberStyles.Float, CultureInfo.CreateSpecificCulture("fr-FR")), double.Parse(BakuBus.Buses[i].Attributes.LONGITUDE, NumberStyles.Float, CultureInfo.CreateSpecificCulture("fr-FR"))), Template = (ControlTemplate)this.FindResource("customPushPin"), Content = BakuBus.Buses[i].Attributes.DISPLAY_ROUTE_CODE, Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, (byte)rnd.Next(256), (byte)rnd.Next(256), (byte)rnd.Next(256))) };
                BingMap.Children.Add(pin);
            } 
        }
    }

    async void GetBusListAsync()
    {
        var jsonString = "";
        if (bool.Parse(System.Configuration.ConfigurationManager.AppSettings["UseApi"]))
            jsonString = await new HttpClient().GetStringAsync("https://www.bakubus.az/az/ajax/apiNew1");
        else
            jsonString = await File.ReadAllTextAsync(Path.Combine(new DirectoryInfo($"../../../").FullName, "bakubusApi.json"));
        BakuBus = JsonSerializer.Deserialize<BakuBus>(jsonString);
    }

    private void timerTick_event(object sender, EventArgs e) => GetBusListAsync();

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        
        if (SelectedIndex == -1)
        {
            SelectedBus = lBox.SelectedValue.ToString();
            SelectedIndex = lBox.SelectedIndex;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        SelectedBus = "-1";
        canRequest = true;
    }
}
