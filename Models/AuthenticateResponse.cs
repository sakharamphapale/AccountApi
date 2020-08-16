namespace AccountApi.Models
{
    public class AuthenticateResponse
    {
        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Token { get; set; }
    }
}
