namespace SqlAccountRestAPI.Controllers;
public class BizObjectQueryRequest
{
    public string Sql { get; set; } = null!;
    public IDictionary<string, object?> Params { get; set; } = new Dictionary<string, object?>();
    public int Offset { get; set; } = 0;
    public int Limit { get; set; } = 100;
}