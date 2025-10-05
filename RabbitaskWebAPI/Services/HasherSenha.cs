using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public static class HasherSenha
{
    public static string Hash(string password)
    {
        // eu queria mt poder fazer salt aq... mas precisa armazernar no banco... ç - ç
        byte[] salt = new byte[0];
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 32));
        return hashed;
    }

    public static bool Verify(string password, string stored)
    {
        byte[] salt = new byte[0];
        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 10000,
            numBytesRequested: 32));
        return hash == stored;
    }
}