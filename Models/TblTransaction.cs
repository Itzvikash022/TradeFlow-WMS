using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class TblTransaction
{
    public int TransactionId { get; set; }

    public int? OrderId { get; set; }

    public string TransactionType { get; set; } = null!;
    public string ReferenceNo { get; set; }
    public string ReceiptPath { get; set; }
    [NotMapped]
    public IFormFile Receipt { get; set; }
    [NotMapped]
    public string BuyerName { get; set; }
    [NotMapped]
    public string SellerName { get; set; }
    public decimal? Amount { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? Remarks { get; set; }
}
