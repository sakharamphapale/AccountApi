using System;

namespace AccountApi.Models
{
    public class RegistrationRequest
    {
        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }
    }
}
