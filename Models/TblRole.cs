using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class TblRole
{
    public int RoleId { get; set; }

    public string? RoleName { get; set; }

    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
