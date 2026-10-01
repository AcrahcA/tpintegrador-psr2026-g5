# Documentación de Endpoints REST API

## Mascotas (`/api/Mascotas`)
- `GET /api/Mascotas` - Consultar todas las mascotas.
- `POST /api/Mascotas` - Registrar una nueva mascota.
- `GET /api/Mascotas/{id}` - Consultar una mascota específica por su ID.
- `DELETE /api/Mascotas/{id}` - Eliminar una mascota por su ID.
- `PUT /api/Mascotas/{id}/estado` - Cambiar el estado de una mascota.
- `PUT /api/Mascotas/{id}/cuidador/{cuidadorId}` - Asignar un cuidador a una mascota.
- `GET /api/Mascotas/{id}/disponibilidad-adopcion` - Consultar la disponibilidad de adopción de una mascota.

## Avisos de Rescate (`/api/avisos-rescate`)
- `GET /api/avisos-rescate` - Consultar todos los avisos de rescate.
- `POST /api/avisos-rescate` - Registrar un nuevo aviso de rescate.
- `GET /api/avisos-rescate/pendientes` - Consultar los avisos de rescate pendientes.
- `GET /api/avisos-rescate/{id}` - Consultar un aviso de rescate por su ID.
- `DELETE /api/avisos-rescate/{id}` - Eliminar un aviso de rescate por su ID.
- `PUT /api/avisos-rescate/{id}/estado` - Cambiar/actualizar el estado de un aviso de rescate.
- `POST /api/avisos-rescate/{id}/mascotas` - Registrar o asociar una mascota a un aviso de rescate.

## Cuidadores (`/api/Cuidadores`)
- `GET /api/Cuidadores` - Consultar todos los cuidadores.
- `POST /api/Cuidadores` - Registrar un nuevo cuidador.
- `GET /api/Cuidadores/{id}` - Consultar un cuidador por su ID.
- `PUT /api/Cuidadores/{id}` - Actualizar los datos de un cuidador.
- `DELETE /api/Cuidadores/{id}` - Eliminar un cuidador por su ID.
- `GET /api/Cuidadores/{id}/disponibilidad` - Consultar la disponibilidad de un cuidador.

## Historial Sanitario (`/api/historiales-sanitarios`)
- `GET /api/historiales-sanitarios` - Consultar todos los historiales sanitarios.
- `POST /api/historiales-sanitarios` - Registrar un nuevo historial sanitario.
- `GET /api/historiales-sanitarios/{id}` - Consultar un historial sanitario por su ID.
- `PUT /api/historiales-sanitarios/{id}` - Actualizar un historial sanitario.
- `DELETE /api/historiales-sanitarios/{id}` - Eliminar un historial sanitario.
- `GET /api/historiales-sanitarios/mascotas/{mascotaId}` - Consultar el historial sanitario asociado a una mascota.

## Tratamientos (`/api/Tratamientos`)
- `GET /api/Tratamientos` - Consultar todos los tratamientos registrados.
- `GET /api/Tratamientos/mascotas/{mascotaId}` - Consultar tratamientos asociados a una mascota.
- `POST /api/Tratamientos/mascotas/{mascotaId}` - Registrar un tratamiento para una mascota.
- `GET /api/Tratamientos/{id}` - Consultar un tratamiento por su ID.
- `DELETE /api/Tratamientos/{id}` - Eliminar un tratamiento.
- `PUT /api/Tratamientos/{id}/estado` - Cambiar el estado de un tratamiento.

## Refugio (`/api/Refugio`)
- `GET /api/Refugio` - Consultar la información general del refugio.
- `POST /api/Refugio` - Registrar/configurar datos del refugio.
- `GET /api/Refugio/capacidad` - Consultar la capacidad disponible u ocupación del refugio.

## Adoptantes (`/api/Adoptantes`)
- `GET /api/Adoptantes` - Consultar todos los adoptantes.
- `POST /api/Adoptantes` - Registrar un posible adoptante.
- `GET /api/Adoptantes/{id}` - Consultar datos de un adoptante específico.
- `DELETE /api/Adoptantes/{id}` - Eliminar un adoptante por su ID.

## Solicitudes de Adopción (`/api/solicitudes-adopcion`)
- `GET /api/solicitudes-adopcion` - Consultar todas las solicitudes de adopción.
- `POST /api/solicitudes-adopcion` - Crear una nueva solicitud de adopción.
- `GET /api/solicitudes-adopcion/pendientes` - Consultar solicitudes de adopción pendientes.
- `GET /api/solicitudes-adopcion/{id}` - Consultar una solicitud de adopción específica.
- `DELETE /api/solicitudes-adopcion/{id}` - Eliminar una solicitud de adopción.
- `PUT /api/solicitudes-adopcion/{id}/estado` - Aprobar, rechazar o cambiar el estado de una solicitud.

## Adopciones (`/api/Adopciones`)
- `GET /api/Adopciones` - Consultar todas las adopciones concretadas.
- `POST /api/Adopciones` - Registrar/concretar una nueva adopción.
- `GET /api/Adopciones/{id}` - Consultar el detalle de una adopción concretada por su ID.
