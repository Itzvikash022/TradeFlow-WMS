using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class TblTransaction
{
    public int TransactionId { get; set; }

    public int? ProductId { get; set; }

    public int? ShopId { get; set; }

    public int? SupplierId { get; set; }

    public int Quantity { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal? Amount { get; set; }

    public string? CreditOrDebit { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? Remarks { get; set; }
}
