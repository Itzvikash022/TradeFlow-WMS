using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class TblCompany
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public int? AddedBy { get; set; }

    public string? ContactPerson { get; set; }

    public long? PhoneNumber { get; set; }

    public DateTime? LastOrderDate { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? CompanyLogo { get; set; }
    [NotMapped]
    public IFormFile? LogoFile { get; set; }

    public string? Gst { get; set; }

    public int Pincode { get; set; }
    public string City { get; set; }
    public string State { get; set; }

    public DateTime? CreatedAt { get; set; }
}
