using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class TblCustomer
{
    public int CustomerId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string? Email { get; set; }

    public string? Contact { get; set; }
}
