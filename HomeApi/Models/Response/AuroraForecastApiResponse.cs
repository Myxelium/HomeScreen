using System.Text.Json.Serialization;

namespace HomeApi.Models.Response;

public class AuroraForecastApiResponse
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("ace")]
    public AceData Ace { get; set; }

    [JsonPropertyName("weather")]
    public bool Weather { get; set; }

    [JsonPropertyName("probability")]
    public ProbabilityData Probability { get; set; }

    [JsonPropertyName("message")]
    public List<string> Message { get; set; }
}

public class AceData
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("bz")]
    public string Bz { get; set; }

    [JsonPropertyName("density")]
    public string Density { get; set; }

    [JsonPropertyName("speed")]
    public string Speed { get; set; }

    [JsonPropertyName("kp1hour")]
    public string Kp1Hour { get; set; }

    [JsonPropertyName("kp4hour")]
    public string Kp4Hour { get; set; }

    [JsonPropertyName("kp")]
    public string Kp { get; set; }

    [JsonPropertyName("colour")]
    public ColourData Colour { get; set; }
}

public class ColourData
{
    [JsonPropertyName("bz")]
    public string Bz { get; set; }

    [JsonPropertyName("density")]
    public string Density { get; set; }

    [JsonPropertyName("speed")]
    public string Speed { get; set; }

    [JsonPropertyName("kp1hour")]
    public string Kp1Hour { get; set; }

    [JsonPropertyName("kp4hour")]
    public string Kp4Hour { get; set; }

    [JsonPropertyName("kp")]
    public string Kp { get; set; }
}

public class ProbabilityData
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("calculated")]
    public CalculatedProbability Calculated { get; set; }

    [JsonPropertyName("colour")]
    public string Colour { get; set; }

    [JsonPropertyName("lat")]
    public string Lat { get; set; }

    [JsonPropertyName("long")]
    public string Long { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("highest")]
    public HighestProbability Highest { get; set; }
}

public class CalculatedProbability
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("long")]
    public double Long { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }

    [JsonPropertyName("colour")]
    public string Colour { get; set; }
}

public class HighestProbability
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("colour")]
    public string Colour { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("long")]
    public double Long { get; set; }

    [JsonPropertyName("value")]
    public int Value { get; set; }
}
