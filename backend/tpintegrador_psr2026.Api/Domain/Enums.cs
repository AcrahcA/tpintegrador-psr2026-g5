namespace tpintegrador_psr2026.Api.Domain;

public enum EstadoAviso
{
    Reportado,
    EnEvaluacion,
    Aceptado,
    Atendido,
    Descartado
}

public enum EstadoMascota
{
    Ingresada,
    EnTratamiento,
    EnRecuperacion,
    DisponibleAdopcion,
    Reservada,
    Adoptada
}

public enum EstadoTratamiento
{
    Pendiente,
    EnCurso,
    Finalizado,
    Suspendido
}

public enum EstadoSolicitud
{
    Pendiente,
    EnEvaluacion,
    Aprobada,
    Rechazada,
    Cancelada
}

public enum TipoAnimal
{
    Perro,
    Gato,
    Ave,
    Otro
}

public enum EstadoAparente
{
    Sano,
    Herido,
    Enfermo,
    Desnutrido,
    Critico
}

public enum NivelUrgencia
{
    Baja,
    Media,
    Alta
}

public enum Sexo
{
    Macho,
    Hembra
}

public enum Tamaño
{
    Pequenio,
    Mediano,
    Grande
}

public enum TipoTratamiento
{
    Medicacion,
    Vacunacion,
    Curacion,
    Desparasitacion,
    RecuperacionPosquirurgica,
    ControlVeterinario
}
