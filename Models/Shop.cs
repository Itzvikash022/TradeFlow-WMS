using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class Shop
{
    public int ShopId { get; set; }

    public string ShopName { get; set; } = null!;

    public string? OwnerName { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? LastOrderDate { get; set; }

    public string? Location { get; set; }

    public string? Gst { get; set; }

    public DateTime? CreatedAt { get; set; }
}
