using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class TblUser
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;


    [NotMapped]
    public string? Designation { get; set; }

    [NotMapped]
    public string? ConfirmPassword { get; set; }
    [NotMapped]
    public string? ShopName { get; set; }
    [NotMapped]
    public string? Head { get; set; }

    [NotMapped]
    public IFormFile? ProfileImage { get; set; }

    public string? PhoneNumber { get; set; }

    public int RoleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? ProfileImgPath { get; set; }

    public int AdminRef { get; set; }

    public string? Otp { get; set; }

    public DateTime? OtpExpiry { get; set; }

    public bool IsVerified { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public bool? IsGoogleAccount { get; set; }

    public string? VerifiedBy { get; set; }

    public string? VerificationStatus { get; set; }
}
