﻿namespace AccountApi.Models
{
    public class PasswordResetRequest
    {
        public string Username { get; set; }

        public string OldPassword { get; set; }

        public string NewPassword { get; set; }
    }
}
