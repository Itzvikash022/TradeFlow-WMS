using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class TblUnregCompany
{
    public int UnregCompanyId { get; set; }

    public string? UnregCompanyName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? AddedBy { get; set; }
}
