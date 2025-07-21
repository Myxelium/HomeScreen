namespace HomeApi.Models.Response;

public class TrafikLabsApiResponse
{
    public List<Departure> Departure { get; set; }
}

public class Departure
{
    public JourneyDetailRef JourneyDetailRef { get; set; }
    public string JourneyStatus { get; set; }
    public ProductDetail ProductAtStop { get; set; }
    public List<ProductDetail> Product { get; set; }
    public Notes Notes { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Stop { get; set; }
    public string Stopid { get; set; }
    public string StopExtId { get; set; }
    public double Lon { get; set; }
    public double Lat { get; set; }
    public string Time { get; set; }
    public string Date { get; set; }
    public bool Reachable { get; set; }
    public string Direction { get; set; }
    public string DirectionFlag { get; set; }
}

public class JourneyDetailRef
{
    public string Ref { get; set; }
}

public class ProductDetail
{
    public Icon Icon { get; set; }
    public OperatorInfo OperatorInfo { get; set; }
    public string Name { get; set; }
    public string InternalName { get; set; }
    public string DisplayNumber { get; set; }
    public string Num { get; set; }
    public string Line { get; set; }
    public string LineId { get; set; }
    public string CatOut { get; set; }
    public string CatIn { get; set; }
    public string CatCode { get; set; }
    public string Cls { get; set; }
    public string CatOutS { get; set; }
    public string CatOutL { get; set; }
    public string OperatorCode { get; set; }
    public string Operator { get; set; }
    public string Admin { get; set; }
    public string MatchId { get; set; }
    public int? RouteIdxFrom { get; set; }
    public int? RouteIdxTo { get; set; }
}

public class Icon
{
    public string Res { get; set; }
}

public class OperatorInfo
{
    public string Name { get; set; }
    public string NameS { get; set; }
    public string NameN { get; set; }
    public string NameL { get; set; }
    public string Id { get; set; }
}

public class Notes
{
    public List<Note> Note { get; set; }
}

public class Note
{
    public string Value { get; set; }
    public string Key { get; set; }
    public string Type { get; set; }
    public int RouteIdxFrom { get; set; }
    public int RouteIdxTo { get; set; }
    public string TxtN { get; set; }
}
