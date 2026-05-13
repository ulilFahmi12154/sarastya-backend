using Microsoft.AspNetCore.Identity;
using TaskFlow.Core.Domain.Entities;

namespace TaskFlow.Infrastructure.Security;

public sealed class BcryptPasswordHasher : IPasswordHasher<User>
{
    private const int WorkFactor = 12;

    public string HashPassword(User user, string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public PasswordVerificationResult VerifyHashedPassword(User user, string hashedPassword, string providedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            return PasswordVerificationResult.Failed;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Treat malformed or legacy hashes as failed credentials without leaking details.
            return PasswordVerificationResult.Failed;
        }
    }
}
