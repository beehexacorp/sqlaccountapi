using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlAccountRestAPI.ViewModels;
public class SqlAccountingAppInfo
{
    public string Title { get; set; } = null!;
    public string ReleaseDate { get; set; } = null!;
    public string BuildNo { get; set; } = null!;
    public IDictionary<string, object> ApplicationInfo { get; set; } = null!;
}
