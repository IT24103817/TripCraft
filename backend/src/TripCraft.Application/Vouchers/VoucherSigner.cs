using System.Security.Cryptography;
using System.Text;

namespace TripCraft.Application.Vouchers;

/// <summary>
/// Signs and checks voucher codes with HMAC-SHA256 (key: VOUCHER_SIGNING_KEY, never stored in code).
/// Code = "1.{voucherId}.{tripId}.{T|H}.{yyyyMMdd|-}.{signature}". The QR code holds "TRIPCRAFT-VOUCHER:" + code.
/// Verify recomputes the signature and compares in constant time, so a changed id, trip, type or night fails.
/// </summary>
public class VoucherSigner
{
    public const string QrPrefix = "TRIPCRAFT-VOUCHER:";
    private const int SignatureLength = 22; // 22 base64url characters = 132 bits of the HMAC
    private readonly byte[] _key;

    public VoucherSigner(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || Encoding.UTF8.GetByteCount(key) < 32)
            throw new InvalidOperationException("VOUCHER_SIGNING_KEY must be set and at least 32 bytes long.");
        _key = Encoding.UTF8.GetBytes(key);
    }

    public string Sign(Guid voucherId, Guid tripRequestId, VoucherType type, DateOnly? night)
    {
        var payload = $"1.{voucherId:N}.{tripRequestId:N}.{(type == VoucherType.Trip ? "T" : "H")}.{night?.ToString("yyyyMMdd") ?? "-"}";
        return $"{payload}.{Signature(payload)}";
    }

    /// <summary>The claims of a valid code (with or without the QR prefix), or null when it is malformed or forged.</summary>
    public VoucherClaims? Verify(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;
        var text = code.Trim();
        if (text.StartsWith(QrPrefix, StringComparison.OrdinalIgnoreCase))
            text = text[QrPrefix.Length..];

        var parts = text.Split('.');
        if (parts.Length != 6 || parts[0] != "1")
            return null;
        var payload = string.Join('.', parts[..5]);
        var expected = Encoding.ASCII.GetBytes(Signature(payload));
        var given = Encoding.ASCII.GetBytes(parts[5]);
        if (!CryptographicOperations.FixedTimeEquals(expected, given))
            return null;

        if (!Guid.TryParseExact(parts[1], "N", out var voucherId) || !Guid.TryParseExact(parts[2], "N", out var tripId))
            return null;
        VoucherType? type = parts[3] switch { "T" => VoucherType.Trip, "H" => VoucherType.HotelNight, _ => null };
        if (type is null)
            return null;
        DateOnly? night = null;
        if (parts[4] != "-")
        {
            if (!DateOnly.TryParseExact(parts[4], "yyyyMMdd", out var parsed))
                return null;
            night = parsed;
        }
        return new VoucherClaims(voucherId, tripId, type.Value, night);
    }

    private string Signature(string payload)
    {
        var hash = HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(payload));
        return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_')[..SignatureLength];
    }
}

public record VoucherClaims(Guid VoucherId, Guid TripRequestId, VoucherType Type, DateOnly? Night);
