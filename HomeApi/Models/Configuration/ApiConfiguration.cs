namespace HomeApi.Models.Configuration;

public class ApiConfiguration
{
    public Keys Keys { get; set; } = new();
    public BaseUrls BaseUrls { get; set; } = new();
    public string DefaultCity { get; set; } = "Vega stockholms lan";
    public string DefaultStation { get; set; } = "Vega station";
}

public class BaseUrls
{
    public string Weather { get; set; } = string.Empty;
    public string Nominatim { get; set; } = string.Empty;
    public string Aurora { get; set; } = string.Empty;
    public string ResRobot { get; set; } = string.Empty;
}

public class Keys
{
    public string Weather { get; set; } = string.Empty;
    public string Nominatim { get; set; } = string.Empty;
    public string Aurora { get; set; } = string.Empty;
    public string ResRobot { get; set; } = string.Empty;
}