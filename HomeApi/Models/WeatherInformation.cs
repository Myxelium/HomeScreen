using HomeApi.Models.Response;

namespace HomeApi.Models;

public class WeatherInformation
{
    public string CityName { get; set; } = string.Empty;
    public Current Current { get; set; } = new();
    public List<Forecast> Forecast { get; set; }
}

public class Current
{
    public string Date { get; set; }
    public double Feelslike { get; set; }
    public int IsDay { get; set; }
    public double WindPerMeterSecond { get; set; } = 0;
    public double WindGustPerMeterSecond { get; set; } = 0;
    public double Temperature { get; set; } = 0;
    public string LastUpdated { get; set; } = string.Empty;
    public int Cloud { get; set; }
    public string WindDirection { get; set; } = string.Empty;
    public Location WeatherDataLocation { get; set; } = new();
    public AirQuality AirQuality { get; set; }

    public Probability AuroraProbability { get; set; } = new();
}

public class Location
{
    public string Name { get; set; }
    public string Region { get; set; }
    public string Country { get; set; }
    public double Lat { get; set; }
    public double Lon { get; set; }
}

public class Probability
{
    public DateTime Date { get; set; }
    public CalculatedProbability Calculated { get; set; } // my location?
    public string Colour { get; set; }
    public int Value { get; set; }
    public Highest HighestProbability { get; set; }
}

public class Highest
{
    public DateTime Date { get; set; }
    public string Colour { get; set; }
    public double Lat { get; set; }
    public double Long { get; set; }
    public int Value { get; set; }
}

public class Forecast
{
    public string Date { get; set; }
    public double MinTempC { get; set; }
    public double MaxTempC { get; set; }
    public string DayIcon { get; set; }
    public WeatherSummary? Day { get; set; }
    public WeatherSummary? Night { get; set; }
    public Astro Astro { get; set; }
}
public class Astro
{
    public string Sunrise { get; set; }
    public string Sunset { get; set; }
    public string Moonrise { get; set; }
    public string Moonset { get; set; }
    public string Moon_Phase { get; set; }
    public double? Moon_Illumination { get; set; }
}

public class AirQuality
{
    public double Co { get; set; }
    public double No2 { get; set; }
    public double O3 { get; set; }
    public double So2 { get; set; }
    public double Pm2_5 { get; set; }
    public double Pm10 { get; set; }
    public int Us_Epa_Index { get; set; }
    public int Gb_Defra_Index { get; set; }
}

public class WeatherSummary
{
    public string ConditionText { get; set; }
    public string ConditionIcon { get; set; }
    public double AvgTempC { get; set; }
    public double AvgFeelslikeC { get; set; }
    public int TotalChanceOfRain { get; set; }
    public int TotalChanceOfSnow { get; set; }
}