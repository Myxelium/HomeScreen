namespace HomeApi.Models;

public class Home
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
    public double WindPerHour { get; set; } = new();
    public double WindGustPerHour { get; set; } = new();
    public double Temperature { get; set; } = new();
    public Probability AuroraProbability { get; set; } = new();
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
    public WeatherSummary Day { get; set; }
    public WeatherSummary Night { get; set; }
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

public class WeatherSummary
{
    public string ConditionText { get; set; }
    public string ConditionIcon { get; set; }
    public double AvgTempC { get; set; }
    public double AvgFeelslikeC { get; set; }
    public int TotalChanceOfRain { get; set; }
    public int TotalChanceOfSnow { get; set; }
}