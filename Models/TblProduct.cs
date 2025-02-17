using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class TblProduct
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Category { get; set; }

    public int? CompanyId { get; set; }

    public decimal PricePerUnit { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? LastUpdateDate { get; set; }

    public string? Manufacturer { get; set; }

    public string? ProductImagePath { get; set; }
    [NotMapped]
    public IFormFile? ProductImage { get; set; }

    public virtual ICollection<TblOrderDetail> TblOrderDetails { get; set; } = new List<TblOrderDetail>();

    public virtual ICollection<TblStock> TblStocks { get; set; } = new List<TblStock>();

    [NotMapped]
    public string CompanyName { get; set; }
    public int ProductQty { get; set; }
}
