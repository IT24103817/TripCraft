using System.Security.Cryptography;

namespace TripCraft.Application.Identity.Services;

/// <summary>
/// A random one-time password that meets the password rules (upper, lower, digit, 8+ characters).
/// Look-alike characters (0/O, 1/l/I) are left out so it can be read out over the phone.
/// </summary>
public static class TemporaryPassword
{
    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnpqrstuvwxyz";
    private const string Digits = "23456789";

    public static string Create()
    {
        var all = Upper + Lower + Digits;
        var chars = new List<char>
        {
            Upper[RandomNumberGenerator.GetInt32(Upper.Length)],
            Lower[RandomNumberGenerator.GetInt32(Lower.Length)],
            Digits[RandomNumberGenerator.GetInt32(Digits.Length)]
        };
        while (chars.Count < 12)
            chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
        // Shuffle so the first three are not always upper, lower, digit.
        return new string(chars.OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue)).ToArray());
    }
}
