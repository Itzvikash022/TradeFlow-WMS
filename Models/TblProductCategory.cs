using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class TblProductCategory
{
    public int ProdCatId { get; set; }

    public string? ProductCategory { get; set; }

    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
}
