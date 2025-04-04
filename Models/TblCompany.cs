using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class TblCompany
{
    public int CompanyId { get; set; }

    public string CompanyName { get; set; } = null!;

    public long? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public string? CompanyLogo { get; set; }

    public string? PasswordHash { get; set; }

    public string? State { get; set; }

    public string? City { get; set; }

    public int? Pincode { get; set; }

    public string? Address { get; set; }

    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public string? Gst { get; set; }
    [NotMapped]
    public IFormFile? LogoFile { get; set; }

    public DateTime? CreatedAt { get; set; }

    [NotMapped]
    public string Path { get; set; }
    [NotMapped]
    public int OrderCount { get; set; }
    
    [NotMapped]
    public string Designation { get; set; }
    public virtual ICollection<TblCollab> TblCollabs { get; set; } = new List<TblCollab>();
}
