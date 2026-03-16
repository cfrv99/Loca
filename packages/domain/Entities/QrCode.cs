namespace Loca.Domain.Entities;

/// <summary>
/// Value object representing a venue's QR TOTP configuration.
/// QR codes rotate every 60 seconds using TOTP algorithm.
/// </summary>
public static class QrCodeGenerator
{
    /// <summary>
    /// Generate a TOTP code from a venue's secret key.
    /// Uses a 60-second time step.
    /// </summary>
    public static string GenerateTotp(string secretKey, DateTime? timestamp = null)
    {
        var time = timestamp ?? DateTime.UtcNow;
        var timeStep = (long)(time - DateTime.UnixEpoch).TotalSeconds / 60;
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(secretKey);
        var timeBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian) Array.Reverse(timeBytes);

        using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(timeBytes);
        var offset = hash[^1] & 0x0F;
        var code = ((hash[offset] & 0x7F) << 24) |
                   ((hash[offset + 1] & 0xFF) << 16) |
                   ((hash[offset + 2] & 0xFF) << 8) |
                   (hash[offset + 3] & 0xFF);
        return (code % 1000000).ToString("D6");
    }

    /// <summary>
    /// Validate a TOTP code. Accepts current + previous window (120s total).
    /// </summary>
    public static bool ValidateTotp(string secretKey, string code)
    {
        var current = GenerateTotp(secretKey);
        var previous = GenerateTotp(secretKey, DateTime.UtcNow.AddSeconds(-60));
        return code == current || code == previous;
    }
}
