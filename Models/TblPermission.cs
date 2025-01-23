using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class TblPermission
{
    public int PermissionId { get; set; }

    public int RoleId { get; set; }

    public int TabId { get; set; }

    public virtual TblTab Tab { get; set; } = null!;
}
