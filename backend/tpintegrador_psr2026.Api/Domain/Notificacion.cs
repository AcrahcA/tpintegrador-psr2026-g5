namespace tpintegrador_psr2026.Api.Domain;

/// <summary>
/// Mensaje del buzón de una persona: por ejemplo, que su solicitud de adopción fue rechazada.
/// El motivo es opcional: lo escribe el administrador si quiere explicar la decisión.
/// </summary>
public class Notificacion : IEntidad
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public TipoNotificacion Tipo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Motivo { get; set; }
    public DateTime Fecha { get; set; }
    public bool Leida { get; set; }

    // Referencias opcionales al trámite que originó el mensaje
    public int? SolicitudAdopcionId { get; set; }
    public int? AvisoRescateId { get; set; }

    protected Notificacion() { }

    public Notificacion(int usuarioId, TipoNotificacion tipo, string titulo, string mensaje, string? motivo = null,
        int? solicitudAdopcionId = null, int? avisoRescateId = null)
    {
        UsuarioId = usuarioId;
        Tipo = tipo;
        Titulo = titulo;
        Mensaje = mensaje;
        Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
        SolicitudAdopcionId = solicitudAdopcionId;
        AvisoRescateId = avisoRescateId;
        Fecha = DateTime.UtcNow;
        Leida = false;
    }

    public const int LargoMaximoMotivo = 500;

    public static string? NormalizarMotivo(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo)) return null;
        var texto = motivo.Trim();
        if (texto.Length > LargoMaximoMotivo)
            throw new ReglaNegocioException($"El motivo no puede superar los {LargoMaximoMotivo} caracteres.");
        return texto;
    }
}
