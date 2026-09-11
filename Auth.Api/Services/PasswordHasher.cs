using Auth.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Services
{
    public class PasswordHasher
    {
        private readonly PasswordHasher<User> _hasher = new();

        public string Hash(User user, string password) =>
            _hasher.HashPassword(user, password);

        public bool Verify(User user, string hash, string password) =>
            _hasher.VerifyHashedPassword(user, hash, password) == PasswordVerificationResult.Success;
    }
}