using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class TblOrder
{
    public int OrderId { get; set; }

    public DateTime? OrderDate { get; set; }

    public int? SellerId { get; set; }

    public int? BuyerId { get; set; }

    public string? OrderType { get; set; }

    public decimal TotalAmount { get; set; }

    public string OrderStatus { get; set; } = null!;

    public int TotalQty { get; set; }
    public string? Remarks { get; set; }

    [NotMapped]
    public string? SellerName { get; set; }
    [NotMapped]
    public string? BuyerName { get; set; }
    [NotMapped]
    public bool CanEditStatus { get; set; }
    public virtual ICollection<TblOrderDetail> TblOrderDetails { get; set; } = new List<TblOrderDetail>();
}
