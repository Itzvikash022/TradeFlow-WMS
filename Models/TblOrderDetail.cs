using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class TblOrderDetail
{
    public int OrderDetailId { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal PricePerUnit { get; set; }

    public decimal Amount { get; set; }

    public virtual TblOrder? Order { get; set; }

    public virtual TblProduct? Product { get; set; }
}
