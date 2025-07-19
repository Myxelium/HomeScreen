namespace HomeApi.Models;

public class TimeTable
{
    public string LineNumber { get; set; }          // e.g. "43", "832"
    public string LineName { get; set; }            // e.g. "Länstrafik - Tåg 43"
    public string TransportType { get; set; }       // e.g. "Tåg", "Buss"
    public string Operator { get; set; }            // e.g. "SL"
    public string StopName { get; set; }            // e.g. "Vega station (Haninge kn)"
    public string DepartureTime { get; set; }       // e.g. 2025-07-15 01:03
    public string Direction { get; set; }           // e.g. "Farsta Strand station"
    public string JourneyDetailRef { get; set; }    // e.g. "1|39437|0|1|15072025"
    public List<string> Notes { get; set; }         // e.g. "Pendeltåg", "Endast 2 klass"
    public string InternalTransportationName { get; set; } // e.g. "Pendeltåg 43"
}