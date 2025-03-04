using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class TblCollab
{
    public int CollabId { get; set; }

    public int? CompanyId { get; set; }

    public int? ShopId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual TblCompany? Company { get; set; }

    public virtual TblShop? Shop { get; set; }
}
