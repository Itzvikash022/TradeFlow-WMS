using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class TblActivityLog
{
    public int LogId { get; set; }

    public string? EntityType { get; set; }

    public int? EntityId { get; set; }

    public string? ActionType { get; set; }

    public DateTime? ActionDate { get; set; }

    public int? UserId { get; set; }

    public string? Remarks { get; set; }
}
