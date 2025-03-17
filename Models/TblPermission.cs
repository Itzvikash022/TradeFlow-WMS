using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class TblPermission
{
    public int PermissionId { get; set; }

    public int RoleId { get; set; }

    public int TabId { get; set; }
    [NotMapped]
    public string TabName { get; set; }

    public bool IsActive { get; set; }
    public string PermissionType { get; set; }

    public virtual TblTab Tab { get; set; } = null!;
}
