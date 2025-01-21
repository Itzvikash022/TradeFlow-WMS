using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WMS_Application.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string PasswordHash { get; set; } = null!;
    [NotMapped]
    public string? ConfirmPassword { get; set; }
    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? ProfileImgPath { get; set; }

    [NotMapped]
    public IFormFile? ProfileImage { get; set; }
    public int? DesignationIdRef { get; set; }

    public string? AdminRef { get; set; }

    public string? Otp { get; set; }

    public DateTime? OtpExpiry { get; set; }

    public bool? IsVerified { get; set; }
}
