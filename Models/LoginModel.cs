﻿namespace WMS_Application.Models
{
    public class LoginModel
    {
            public string? EmailOrUsername { get; set; }
            public string? Password { get; set; }

            public bool RememberMe { get; set; }
    }
}
