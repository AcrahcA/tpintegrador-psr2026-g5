# Documentación de Endpoints REST API

## Mascotas (`/api/mascotas`)
- `GET /api/mascotas` - Consultar todas las mascotas.
- `GET /api/mascotas?estado={estado}` - Filtrar mascotas por estado.
- `POST /api/mascotas` - Registrar una nueva mascota.

## Avisos de Rescate (`/api/avisosrescate`)
- `POST /api/avisosrescate` - Registrar un aviso de rescate.
- `GET /api/avisosrescate/pendientes` - Consultar avisos pendientes.
- `PUT /api/avisosrescate/{id}/aceptar` - Aceptar aviso.
- `PUT /api/avisosrescate/{id}/descartar` - Descartar aviso.

## Cuidadores (`/api/cuidadores`)
- `POST /api/cuidadores` - Registrar un nuevo cuidador.
- `POST /api/cuidadores/{id}/asignar-mascota` - Asignar mascota a un cuidador.

## Tratamientos (`/api/tratamientos`)
- `POST /api/tratamientos` - Registrar tratamiento.
- `GET /api/tratamientos/mascota/{mascotaId}` - Consultar tratamientos de una mascota.
- `PUT /api/tratamientos/{id}/estado` - Cambiar estado de un tratamiento.

## Adoptantes y Solicitudes (`/api/adoptantes`, `/api/solicitudesadopcion`)
- `POST /api/adoptantes` - Registrar posible adoptante.
- `POST /api/solicitudesadopcion` - Crear solicitud de adopción.
- `GET /api/solicitudesadopcion/pendientes` - Consultar solicitudes pendientes.
- `PUT /api/solicitudesadopcion/{id}/estado` - Aprobar o rechazar solicitud.

## Adopciones (`/api/adopciones`)
- `GET /api/adopciones` - Consultar adopciones concretadas.
