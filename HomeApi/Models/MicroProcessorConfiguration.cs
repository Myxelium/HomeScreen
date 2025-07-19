namespace HomeApi.Models;

public class MicroProcessorConfiguration
{
    public string InformationBoardImageUrl { get; set; } = string.Empty;
    public int UpdateIntervalMinutes { get; set; } = 2;
    public int BlackTextThreshold { get; set; } = 190; // (0-255)
    public bool EnableDithering { get; set; } = true;
    public int DitheringStrength { get; set; } = 8; // (8-32)
    public bool EnhanceContrast { get; set; } = true;
    public int ContrastStrength { get; set; } = 10; // (0-100)
}