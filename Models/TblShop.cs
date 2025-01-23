using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class TblShop
{
    public int ShopId { get; set; }

    public string? ShopName { get; set; }

    public int AdminId { get; set; }

    public string? ShopImagePath { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public long? Pincode { get; set; }

    public string? Address { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? ClosingTime { get; set; }
    [NotMapped]
    public IFormFile? ShopImage { get; set; }
}
