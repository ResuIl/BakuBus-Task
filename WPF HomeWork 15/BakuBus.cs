using Microsoft.Maps.MapControl.WPF;
using System.Collections.Generic;
using System.Text.Json.Serialization;
#nullable disable

namespace WPF_HomeWork_15;

public class Attributes
{
    public string BUS_ID { get; set; }
    public string PLATE { get; set; }
    public string DRIVER_NAME { get; set; }
    public string PREV_STOP { get; set; }
    public string CURRENT_STOP { get; set; }
    public string SPEED { get; set; }
    public string BUS_MODEL { get; set; }
    public string LATITUDE { get; set; }
    public string LONGITUDE { get; set; }
    public string ROUTE_NAME { get; set; }
    public int LAST_UPDATE_TIME { get; set; }
    public string DISPLAY_ROUTE_CODE { get; set; }
    public Location Coordinates { get; set; }
}

public class Bus
{
    [JsonPropertyName("@attributes")]
    public Attributes Attributes { get; set; }
    public override string ToString() => Attributes.DISPLAY_ROUTE_CODE;
}

public class BakuBus
{
    [JsonPropertyName("BUS")]
    public List<Bus> Buses { get; set; }
}
