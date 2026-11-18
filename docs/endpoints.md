# Documentación de Endpoints REST API

Base: `http://localhost:5080`. Todas las respuestas son JSON y los enums viajan como texto (`"DisponibleAdopcion"`).
Para cambiar un estado se envía el valor como string JSON en el body, p. ej. `"Finalizado"`.

**Errores:** cuando una regla de negocio impide la operación se responde `400` con un `ProblemDetails`
(`{ "title": "...", "status": 400, "detail": "motivo" }`). Recurso inexistente → `404`; conflicto en la base → `409`;
PostgreSQL inaccesible → `503`.

## Refugio (`/api/Refugio`)
- `GET /api/Refugio` - Datos del refugio.
- `POST /api/Refugio` (o `PUT`) - Configurar nombre y capacidad máxima (no puede ser menor a las mascotas alojadas).
- `GET /api/Refugio/capacidad` - Capacidad, mascotas alojadas (las adoptadas no ocupan plaza) y plazas libres.

## Mascotas (`/api/Mascotas`)
- `GET /api/Mascotas` - Todas las mascotas, incluidas las adoptadas (historial del refugio).
- `GET /api/Mascotas?estado={estado}` y `GET /api/Mascotas/estado/{estado}` - **Mascotas según su estado**.
- `GET /api/Mascotas/alojadas` - **Mascotas alojadas** (las que ocupan plaza).
- `GET /api/Mascotas/{id}` - Una mascota.
- `POST /api/Mascotas` - **Registrar una mascota** (valida la capacidad del refugio; queda `Ingresada` y con historial sanitario).
- `PUT /api/Mascotas/{id}` - Modificar datos descriptivos (no el estado).
- `PUT /api/Mascotas/{id}/estado` - Cambiar estado. Manualmente sólo `EnTratamiento`, `EnRecuperacion` o `DisponibleAdopcion`
  (este último exige las condiciones sanitarias). `Reservada` y `Adoptada` sólo se alcanzan por el proceso de adopción.
- `PUT /api/Mascotas/{id}/cuidador/{cuidadorId}` - Asignar cuidador.
- `DELETE /api/Mascotas/{id}/cuidador` - Quitar el cuidador.
- `GET /api/Mascotas/{id}/disponibilidad-adopcion` - Indica si está en condiciones de ser adoptada y los motivos si no lo está.
- `GET /api/Mascotas/{id}/tratamientos` - Tratamientos de la mascota.
- `GET /api/Mascotas/{id}/historial` - Historial sanitario completo (también para mascotas adoptadas).
- `POST /api/Mascotas/{id}/foto` - Subir foto (multipart, campo `foto`; JPG/PNG/WebP hasta 5 MB).
- `DELETE /api/Mascotas/{id}/foto` - Quitar foto.
- `GET /api/Mascotas/imagenes/{archivo}` - Descargar una foto.
- `DELETE /api/Mascotas/{id}` - Sólo ingresos cargados por error (estado `Ingresada`, sin tratamientos, solicitudes ni aviso de origen).

## Avisos de Rescate (`/api/avisos-rescate`)
- `GET /api/avisos-rescate` (`?estado={estado}` opcional) - Avisos registrados.
- `GET /api/avisos-rescate/pendientes` - **Avisos pendientes** (`Reportado` o `EnEvaluacion`, los más urgentes primero).
- `GET /api/avisos-rescate/{id}` - Un aviso.
- `POST /api/avisos-rescate` - **Registrar aviso** (queda `Reportado`).
- `PUT /api/avisos-rescate/{id}` - Modificar datos de un aviso no cerrado.
- `PUT /api/avisos-rescate/{id}/aceptar` - **Aceptar** (requiere lugar disponible en el refugio).
- `PUT /api/avisos-rescate/{id}/descartar` - **Descartar**.
- `PUT /api/avisos-rescate/{id}/estado` - Cambio de estado genérico (`Reportado → EnEvaluacion → Aceptado`, o `Descartado`).
- `POST /api/avisos-rescate/{id}/mascotas` - Rescate realizado: **genera el ingreso de la mascota** a partir de un aviso
  `Aceptado` (valida capacidad) y el aviso pasa a `Atendido`. Si no se envía `especie`, se usa el tipo de animal del aviso.
- `DELETE /api/avisos-rescate/{id}` - Eliminar (no permitido para avisos atendidos).

## Cuidadores (`/api/Cuidadores`)
- `GET /api/Cuidadores` - Cuidadores (incluye cantidad de mascotas asignadas).
- `GET /api/Cuidadores/{id}` - Un cuidador.
- `POST /api/Cuidadores` - **Registrar cuidador** (`especializacion` libre: "Perros", "Gatos y aves"; vacío = generalista).
- `PUT /api/Cuidadores/{id}` - Modificar (capacidad no menor a las asignadas; `disponible` opcional).
- `GET /api/Cuidadores/{id}/mascotas` - **Mascotas asignadas** (`?incluirAdoptadas=true` para el historial).
- `PUT /api/Cuidadores/{id}/mascotas/{mascotaId}` - **Asignar mascota** (valida disponibilidad, especialización y capacidad máxima).
- `GET /api/Cuidadores/{id}/disponibilidad` - Capacidad, asignadas, tipos que atiende y si tiene disponibilidad.
- `DELETE /api/Cuidadores/{id}` - Eliminar (sólo sin mascotas asignadas).

## Tratamientos (`/api/Tratamientos`)
- `GET /api/Tratamientos` - Todos los tratamientos.
- `GET /api/Tratamientos/mascotas/{mascotaId}` - **Tratamientos de una mascota**.
- `GET /api/Tratamientos/{id}` - Un tratamiento.
- `POST /api/Tratamientos/mascotas/{mascotaId}` - **Registrar tratamiento** en el historial de la mascota. Con un tratamiento
  activo la mascota pasa a `EnTratamiento`. Se puede registrar como `Finalizado` (p. ej. una vacuna ya aplicada).
- `PUT /api/Tratamientos/{id}` - Modificar tipo, descripción y fecha de inicio (no si está finalizado).
- `PUT /api/Tratamientos/{id}/estado` - **Cambiar estado** (`Pendiente → EnCurso → Finalizado`, `Suspendido`). Al no quedar
  tratamientos activos, la mascota pasa a `EnRecuperacion`.
- `DELETE /api/Tratamientos/{id}` - Sólo tratamientos `Pendiente` cargados por error.

## Historial Sanitario (`/api/historiales-sanitarios`)
- `GET /api/historiales-sanitarios` - Todos los historiales.
- `GET /api/historiales-sanitarios/{id}` - Un historial.
- `GET /api/historiales-sanitarios/mascotas/{mascotaId}` - Historial de la mascota con tratamientos y evaluación sanitaria.
- `POST /api/historiales-sanitarios` - Crear historial (si ya existe, actualiza sus observaciones).
- `PUT /api/historiales-sanitarios/{id}` - Actualizar observaciones.
- `DELETE /api/historiales-sanitarios/{id}` - Siempre rechazado: el historial no se elimina.

## Adoptantes (`/api/Adoptantes`)
- `GET /api/Adoptantes` - Adoptantes.
- `GET /api/Adoptantes/{id}` - Un adoptante.
- `POST /api/Adoptantes` - **Registrar posible adoptante** (DNI único).
- `PUT /api/Adoptantes/{id}` - Modificar.
- `DELETE /api/Adoptantes/{id}` - Eliminar (sólo sin solicitudes registradas).

## Solicitudes de Adopción (`/api/solicitudes-adopcion`)
- `GET /api/solicitudes-adopcion` (`?estado={estado}` opcional) - **Solicitudes según su estado**.
- `GET /api/solicitudes-adopcion/pendientes` - **Solicitudes pendientes** (`Pendiente` o `EnEvaluacion`).
- `GET /api/solicitudes-adopcion/mascotas/{mascotaId}` - Solicitudes de una mascota.
- `GET /api/solicitudes-adopcion/{id}` - Una solicitud.
- `POST /api/solicitudes-adopcion` - **Crear solicitud** (`adoptanteId`, `mascotaId`): sólo sobre mascotas disponibles.
- `POST /api/solicitudes-adopcion/publica` - Solicitud desde el sitio público: datos del adoptante (se registra o se reutiliza por DNI) + `mascotaId`.
- `PUT /api/solicitudes-adopcion/{id}/aprobar` - **Aprobar**: verifica que la mascota siga disponible, la deja `Reservada`
  y rechaza las demás solicitudes pendientes de esa mascota.
- `PUT /api/solicitudes-adopcion/{id}/rechazar` - **Rechazar**.
- `PUT /api/solicitudes-adopcion/{id}/estado` - Cambio genérico (`EnEvaluacion`, `Aprobada`, `Rechazada`, `Cancelada`).
  Cancelar una solicitud aprobada libera la reserva de la mascota.
- `DELETE /api/solicitudes-adopcion/{id}` - Eliminar (no aprobadas ni finalizadas en adopción).

## Adopciones (`/api/Adopciones`)
- `GET /api/Adopciones` - **Adopciones realizadas**.
- `GET /api/Adopciones/{id}` - Una adopción.
- `GET /api/Adopciones/mascotas/{mascotaId}` - Adopción de una mascota.
- `POST /api/Adopciones` - **Adopción definitiva** (`solicitudAdopcionId`, `observaciones`, `fechaAdopcion` opcional). Verifica:
  solicitud aprobada, mascota disponible/reservada, sin tratamientos activos, con vacunas y controles requeridos y sin otra
  adopción previa. La mascota pasa automáticamente a `Adoptada` y conserva todo su historial.
  Las adopciones no se modifican ni se eliminan.

## Condiciones sanitarias para adopción
Una mascota cumple las condiciones sanitarias cuando no tiene tratamientos `Pendiente` / `EnCurso` y posee al menos una
`Vacunacion` y un `ControlVeterinario` en estado `Finalizado` (`Domain/RequisitosSanitarios.cs`).
