using System.Security.Cryptography;

namespace tpintegrador_psr2026.Api.Services.Seguridad;

/// <summary>
/// Hash de contraseñas con PBKDF2-SHA256 (100.000 iteraciones y sal aleatoria de 16 bytes).
/// Nunca se guarda la contraseña: sólo "pbkdf2-sha256$iteraciones$sal$hash" en Base64.
/// </summary>
public static class Contraseñas
{
    private const int Iteraciones = 100_000;
    private const int LargoSal = 16;
    private const int LargoHash = 32;
    private const string Prefijo = "pbkdf2-sha256";

    public static string Hashear(string contraseña)
    {
        var sal = RandomNumberGenerator.GetBytes(LargoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, Iteraciones, HashAlgorithmName.SHA256, LargoHash);
        return $"{Prefijo}${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string contraseña, string guardado)
    {
        var partes = guardado.Split('$');
        if (partes.Length != 4 || partes[0] != Prefijo || !int.TryParse(partes[1], out var iteraciones)) return false;
        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(contraseña, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
