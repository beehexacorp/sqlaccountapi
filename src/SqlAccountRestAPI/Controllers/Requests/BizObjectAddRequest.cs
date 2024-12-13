namespace SqlAccountRestAPI;
public class BizObjectRequest
{
    public IDictionary<string, object?> Data { get; set; } = null!;
}

public class BizObjectAddChildrenRequest
{
    public string EntityType { get; set; } = null!;
    public IEnumerable<IDictionary<string, object?>> Data { get; set; } = null!;
}