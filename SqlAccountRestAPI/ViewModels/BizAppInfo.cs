using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlAccountRestAPI.ViewModels;
public class BizAppInfo
{
    public string Title { get; set; } = null!;
    public string ReleaseDate { get; set; } = null!;
    public string BuildNo { get; set; } = null!;
}
