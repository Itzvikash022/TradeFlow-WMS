using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class TblAdminInfo
{
    public int InfoId { get; set; }

    public int AdminId { get; set; }

    public string IdentityDocPath { get; set; } = null!;

    public string IdentityDocType { get; set; } = null!;

    [NotMapped]
    public IFormFile IdentityDoc { get; set; }
    [NotMapped]
    public IFormFile ShopLicense { get; set; }
    [NotMapped]
    public IFormFile AddressProof { get; set; } 
    public string IdentityDocNo { get; set; } = null!;

    public string AddressProofPath { get; set; } = null!;

    public string ShopLicensePath { get; set; } = null!;

    public string ShopLicenseNo { get; set; } = null!;

    public string VerificationStatus { get; set; } = null!;

    public string? VerifiedBy { get; set; }

    public DateOnly? CreatedAt { get; set; }
}
