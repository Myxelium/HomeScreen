using System.Text.Json.Serialization;

public class LocationNameResponse
{
    [JsonPropertyName("stopLocationOrCoordLocation")]
    public List<StopLocationOrCoordLocation> StopLocationOrCoordLocation { get; set; }

    [JsonPropertyName("TechnicalMessages")]
    public TechnicalMessages TechnicalMessages { get; set; }

    [JsonPropertyName("serverVersion")]
    public string ServerVersion { get; set; }

    [JsonPropertyName("dialectVersion")]
    public string DialectVersion { get; set; }

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; }
}

public class StopLocationOrCoordLocation
{
    [JsonPropertyName("StopLocation")]
    public StopLocation StopLocation { get; set; }
}

public class StopLocation
{
    [JsonPropertyName("productAtStop")]
    public List<ProductAtStop> ProductAtStop { get; set; }

    [JsonPropertyName("timezoneOffset")]
    public int TimezoneOffset { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("extId")]
    public string ExtId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("weight")]
    public int Weight { get; set; }

    [JsonPropertyName("products")]
    public int Products { get; set; }

    [JsonPropertyName("minimumChangeDuration")]
    public string MinimumChangeDuration { get; set; }
}

public class ProductAtStop
{
    [JsonPropertyName("icon")]
    public Icon Icon { get; set; }

    [JsonPropertyName("cls")]
    public string Cls { get; set; }
}

public class Icon
{
    [JsonPropertyName("res")]
    public string Res { get; set; }
}

public class TechnicalMessages
{
    [JsonPropertyName("TechnicalMessage")]
    public List<TechnicalMessage> TechnicalMessage { get; set; }
}

public class TechnicalMessage
{
    [JsonPropertyName("value")]
    public string Value { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }
}
