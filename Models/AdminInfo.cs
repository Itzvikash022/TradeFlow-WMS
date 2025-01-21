using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class AdminInfo
{
    public int InfoId { get; set; }
    public int AdminId { get; set; }
    public string IdentityDocPath { get; set; } = null!;
    [NotMapped]
    public IFormFile? IdentityDoc { get; set; }
    public string IdentityDocType { get; set; } = null!;
    public string IdentityDocNo { get; set; } = null!;
    public string ShopLicenseNo { get; set; } = null!;
    public string ShopLicensePath { get; set; } = null!;
    [NotMapped]
    public IFormFile? ShopLicense { get; set; }
    public string AddressProofPath { get; set; } = null!;
    [NotMapped]
    public IFormFile? AddressProof { get; set; }
    public string VerificationStatus { get; set; } = null!;
    public string? VerifiedBy { get; set; }

    public DateOnly? CreatedAt { get; set; }

}
