using System;
using System.Collections.Generic;

namespace WMS_Application.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public long PhoneNumber { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? ProfileImgPath { get; set; }

    public string? Designation { get; set; }

    public string? AdminRef { get; set; }
}
