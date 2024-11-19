namespace SqlAccountRestAPI.Controllers;
public class BizObjectTransferRequest
{
    public string FromEntityType { get; set; } = null!;
    public string ToEntityType { get; set; } = null!;
    public string DocNo { get; set; } = null!;
}
