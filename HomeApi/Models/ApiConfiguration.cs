namespace HomeApi;

public class ApiConfiguration
{
    public Keys Keys { get; set; } = new();
    public BaseUrls BaseUrls { get; set; } = new();
    public string DefaultCity { get; set; } = "Vega stockholms lan";
}

public class BaseUrls
{
    public string Weather { get; set; } = string.Empty;
    public string SL { get; set; } = string.Empty;
}

public class Keys
{
    public string Weather { get; set; } = string.Empty;
    public string SL { get; set; } = string.Empty;
}