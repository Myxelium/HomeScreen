namespace HomeApi.Models.Configuration;

public class ApiConfiguration
{
    public Keys Keys { get; set; } = new();
    public BaseUrls BaseUrls { get; set; } = new();
    public string DefaultCity { get; set; } = "Vega stockholms lan";
    public string DefaultStation { get; set; } = "Vega station";
    public EspConfig EspConfiguration { get; set; } = new();
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

public class EspConfig
{
    public string InformationBoardImageUrl { get; set; } = string.Empty;
    public int UpdateIntervalMinutes { get; set; } = 2;
    public int BlackTextThreshold { get; set; } = 190; // (0-255)
    public bool EnableDithering { get; set; } = true;
    public int DitheringStrength { get; set; } = 8; // (8-32)
    public bool EnhanceContrast { get; set; } = true;
    public int ContrastStrength { get; set; } = 10; // (0-100)
    public bool IsHighContrastMode { get; set; } = true;
}