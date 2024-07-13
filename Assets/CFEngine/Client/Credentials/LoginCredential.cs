
using System;

namespace CrystalFrost.Client.Credentials
{
    /// <summary>
    /// Defines the information needed login.
    /// </summary>
    public class LoginCredential
    {
        public string LoginServer { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime LastUsed { get; set; } = DateTime.MinValue;
    }
}
