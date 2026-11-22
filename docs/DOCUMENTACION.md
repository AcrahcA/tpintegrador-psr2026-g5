# Documentación técnica – Refugio de Mascotas (Patitas)

Sistema para administrar mascotas rescatadas, cuidadores, tratamientos, recursos del refugio y procesos de adopción
(ver [`consigna(2).md`](consigna(2).md) y el enunciado enlazado en el [README](../README.md)).

Este documento explica **qué tiene el software**, **cómo está conformado** (estructura y capas), **su lógica**
(estados, reglas de negocio, seguridad) y el **historial de cambios**. Se actualiza con cada cambio importante.

---

## 1. Resumen de funcionalidades

| Área | Qué permite |
|---|---|
| **Sitio público** (`/`) | Ver las mascotas disponibles con **todas sus fotos** (galería), filtrarlas, **pedir adoptar** (con cuenta) y **reportar un animal** que necesita ayuda (con o sin cuenta). |
| **Cuentas** | Crear una cuenta, iniciar y cerrar sesión. Las cuentas nuevas son siempre de rol **Usuario**. |
| **Buzón de mensajes** | Cada persona con cuenta recibe en su buzón (ícono de sobre) el resultado de sus trámites: solicitud **aprobada / rechazada / cancelada**, **adopción concretada**, aviso **aceptado / descartado / rescatado**. Al rechazar, cancelar o descartar, el administrador puede escribir un **motivo opcional**. |
| **Panel del refugio** (`/admin.html`) | Sólo para el rol **Administrador**: resumen con indicadores, mascotas (ficha, fotos, estados, cuidador, historial sanitario), avisos de rescate, tratamientos, cuidadores, adoptantes, solicitudes, adopciones y configuración del refugio. |
| **Adaptabilidad** | Temas para baja visión y daltonismo, tamaño de texto, espaciado y reducir animaciones. |
| **Accesibilidad** | Diseño para celular y computadora con criterios **WCAG 2.1 AA** (Ley 26.653). |

---

## 2. Tecnologías

| Capa | Tecnología |
|---|---|
| Backend | C# / **.NET 10**, ASP.NET Core Web API, Entity Framework Core 10 con **Npgsql** |
| Base de datos | **PostgreSQL** 14+ (tipos `ENUM` nativos) |
| Frontend | **React 19** + **TypeScript** + **Vite** (sin librerías de UI) |
| Pruebas | xUnit (backend); auditorías con axe-core y recorridos con Playwright (frontend) |
| Tipografías | Comfortaa y Nunito incluidas en el proyecto (`@fontsource`) |

---

## 3. Estructura del repositorio

```text
/
├── README.md                     Puesta en marcha y guía general
├── package.json                  Atajos: npm run dev / build / test (ejecutan dotnet)
├── database/
│   ├── refugio_db.sql            Estructura completa + datos de prueba (re-ejecutable)
│   └── crear_admin.sql           Convierte una cuenta en Administrador
├── docs/
│   ├── DOCUMENTACION.md          Este documento
│   ├── endpoints.md              Todos los endpoints de la API y sus permisos
│   ├── requerimientos.md
│   ├── consigna(2).md            Pedido del rediseño del frontend
│   └── DiagramaUML.drawio.png    Diagrama de clases
├── backend/
│   ├── tpintegrador_psr2026.sln
│   ├── tpintegrador_psr2026.Api/
│   │   ├── Program.cs            Configuración: CORS, sesión, límites, EF Core, inyección de dependencias
│   │   ├── ManejadorErrores.cs   Excepciones → respuestas ProblemDetails (400/401/403/404/409/503)
│   │   ├── Controllers/          Endpoints HTTP (sin lógica de negocio)
│   │   ├── DTOs/                 Objetos de entrada/salida de la API
│   │   ├── Services/             Reglas de negocio
│   │   │   └── Seguridad/        Contraseñas (PBKDF2), sesión y roles
│   │   ├── Repositories/         Acceso a datos (los "DAO" de la consigna)
│   │   ├── Domain/               Entidades, enums y reglas propias de cada entidad
│   │   ├── Data/                 RefugioDbContext (mapeo EF Core) y UnidadDeTrabajo (transacciones)
│   │   └── appsettings.json
│   └── tpintegrador_psr2026.Tests/  Pruebas unitarias con repositorios en memoria
└── frontend/
    ├── index.html / admin.html   Sitio público / panel del refugio
    ├── public/assets/img/        Fotos de ejemplo
    └── src/
        ├── api/                  Cliente HTTP tipado, tipos y formatos de texto
        ├── components/           Componentes accesibles reutilizables
        ├── public/               Sitio público
        ├── admin/                Panel del refugio y sus vistas
        └── styles/               Tokens de diseño, temas, componentes y páginas
```

---

## 4. Arquitectura

```text
 Navegador (React)                         API ASP.NET Core                               PostgreSQL
┌───────────────────┐  HTTP + JSON   ┌──────────────┐  ┌────────────┐  ┌──────────────┐  ┌───────────┐
│ index.html        │ ─────────────▶ │ Controllers  │─▶│  Services  │─▶│ Repositories │─▶│ refugio_db│
│ admin.html        │  cookie de     │ (permisos)   │  │  (reglas)  │  │  (EF Core)   │  │           │
└───────────────────┘  sesión        └──────────────┘  └────────────┘  └──────────────┘  └───────────┘
                                            │                 │
                                     ManejadorErrores   Domain (entidades y reglas propias)
```

- **Controllers**: reciben el pedido, verifican permisos (`[Authorize]`), convierten DTO ↔ entidad y llaman al Service.
- **Services**: toda la lógica de negocio (capacidad, estados, condiciones sanitarias, notificaciones). Lanzan
  `ReglaNegocioException` (→ 400) o `RecursoNoEncontradoException` (→ 404).
- **Repositories**: consultas a la base con EF Core. Además del CRUD tienen las consultas que pide la Entrega 4
  (mascotas por estado, avisos pendientes, mascotas por cuidador, tratamientos por mascota, solicitudes por estado, etc.).
- **UnidadDeTrabajo**: las operaciones que modifican varias tablas (aprobar, adoptar, ingresar desde un aviso) se
  hacen en una transacción: o se guarda todo o nada.
- **Domain**: entidades con sus reglas propias (transiciones de estado válidas, validaciones).

---

## 5. Modelo de dominio

| Entidad | Descripción | Relaciones |
|---|---|---|
| `Refugio` | Nombre y capacidad máxima | 1 → N mascotas y cuidadores |
| `AvisoRescate` | Reporte de un animal: descripción, tipo, coordenadas, dirección, contacto, estado aparente, urgencia | 0..1 mascota generada; 0..1 `Usuario` que reportó |
| `Mascota` | Nombre, especie, raza, edad, sexo, tamaño, fecha de ingreso, estado | N fotos, 1 historial sanitario, 0..1 cuidador, 0..1 aviso de origen |
| `FotoMascota` | Foto con **texto alternativo** y orden (la primera es la principal) | N → 1 mascota |
| `Cuidador` | Nombre, especialización, capacidad máxima, disponible | 1 → N mascotas |
| `HistorialSanitario` | Observaciones de salud | 1 ↔ 1 mascota; 1 → N tratamientos |
| `Tratamiento` | Tipo, descripción, fechas, estado | N → 1 historial |
| `Adoptante` | Nombre, DNI, contacto | 0..1 `Usuario`; 1 → N solicitudes |
| `SolicitudAdopcion` | Adoptante + mascota + estado | 0..1 adopción |
| `Adopcion` | Adopción definitiva (permanente) | 1 ↔ 1 solicitud; 1 por mascota |
| `Usuario` | Cuenta del sitio: nombre, correo, hash de contraseña, rol | 0..1 adoptante; N notificaciones |
| `Notificacion` | Mensaje del buzón: tipo, título, mensaje, **motivo opcional**, leída | N → 1 usuario |

### 5.1 Estados y transiciones

| Entidad | Transiciones válidas |
|---|---|
| Aviso | Reportado → EnEvaluacion / Aceptado / Descartado · EnEvaluacion → Aceptado / Descartado · Aceptado → Descartado · **Atendido** sólo al registrar el ingreso de la mascota |
| Mascota | Manual: EnTratamiento, EnRecuperacion, DisponibleAdopcion. Automático: Ingresada (al ingresar), EnTratamiento / EnRecuperacion (según tratamientos), **Reservada** (al aprobar una solicitud), **Adoptada** (al registrar la adopción; estado final) |
| Tratamiento | Pendiente → EnCurso / Finalizado / Suspendido · EnCurso → Finalizado / Suspendido · Suspendido → EnCurso / Pendiente · Finalizado es final |
| Solicitud | Pendiente → EnEvaluacion / Aprobada / Rechazada / Cancelada · EnEvaluacion → Aprobada / Rechazada / Cancelada · Aprobada → Cancelada · Rechazada y Cancelada son finales |

### 5.2 Reglas de negocio principales

- **Capacidad**: no ingresan mascotas si el refugio está lleno; las adoptadas no ocupan plaza. Un aviso sólo se acepta si hay lugar.
- **Ingreso desde aviso**: sólo un aviso **Aceptado** genera una mascota; el aviso pasa a **Atendido** y queda vinculado.
- **Cuidadores**: se valida capacidad máxima, disponibilidad y especialización (ej. "Gatos" no atiende perros; vacío = generalista).
- **Condiciones sanitarias** para adoptar: sin tratamientos pendientes/en curso y con **Vacunación** y **Control veterinario** finalizados.
- **Solicitudes**: sólo sobre mascotas disponibles; al aprobar una, la mascota queda **Reservada** y las demás pendientes se rechazan.
- **Adopción**: exige solicitud aprobada, sin tratamientos activos y con condiciones sanitarias; la mascota pasa sola a **Adoptada**.
  La base impide dos adopciones de la misma mascota.
- **Historial**: no se borran mascotas adoptadas, historiales, tratamientos finalizados ni adopciones.
- **Fotos**: hasta 12 por mascota, 6 por carga, JPG/PNG/WebP de hasta 5 MB; se verifica el contenido real del archivo.

---

## 6. Cuentas, roles y seguridad

| Aspecto | Implementación |
|---|---|
| Roles | `Usuario` (cuentas del sitio) y `Administrador` (panel del refugio). Desde el sitio **sólo** se crean cuentas `Usuario`. |
| Contraseñas | Nunca se guardan: hash **PBKDF2-SHA256** con 100.000 iteraciones y sal aleatoria (`Services/Seguridad/Contraseñas.cs`). Mínimo 8 caracteres con letras y números. |
| Sesión | Cookie `patitas.sesion` **HttpOnly** (JavaScript no puede leerla), `SameSite=Lax`, 7 días renovables. |
| Rol actualizado | En cada pedido se relee el rol desde la base: si una cuenta pasa a ser administradora (o deja de serlo) vale al instante. |
| Permisos | Todos los controladores del panel exigen `Administrador`. Públicos: catálogo, fotos, estadísticas, reportar aviso, registro e inicio de sesión. Con sesión: solicitud de adopción y buzón. |
| Fuerza bruta | Límite de **10 intentos por minuto** por IP en registro e inicio de sesión (429). |
| Mensajes | Mismo mensaje si el correo no existe o la contraseña es incorrecta (no revela qué correos están registrados). |
| Privacidad | Cada persona sólo ve y modifica sus propios mensajes del buzón. Las observaciones internas de una adopción no se envían al buzón. |
| CORS | Sólo los orígenes de `Cors:OrigenesPermitidos`, con credenciales. |

### 6.1 Crear la cuenta de administrador (paso a paso)

1. Levantar la base, la API y el frontend (ver README).
2. En el sitio (`http://localhost:5173`), tocar **Iniciar sesión → Crear cuenta** y registrar la cuenta del administrador.
3. Asignarle el rol ejecutando, desde la raíz del repositorio:
   ```bash
   psql -U postgres -d refugio_db -v email=correo.del.admin@ejemplo.com -f database/crear_admin.sql
   ```
   En pgAdmin: abrir `database/crear_admin.sql` en la Query Tool, reemplazar `:'email'` por `'correo.del.admin@ejemplo.com'` y ejecutar.
4. Recargar el sitio: aparece el botón **Panel del refugio** (no hace falta volver a iniciar sesión).

Para quitarle el rol: `UPDATE "Usuarios" SET "Rol" = 'Usuario' WHERE "Email" = '...';`

---

## 7. Buzón de mensajes

| Evento | Tipo | ¿Motivo? |
|---|---|---|
| El administrador aprueba la solicitud | `SolicitudAprobada` | — |
| El administrador **rechaza** la solicitud | `SolicitudRechazada` | Opcional, lo escribe el administrador |
| Se aprueba **otra** solicitud o se adopta la mascota | `SolicitudRechazada` | Automático ("fue reservada / adoptada por otra familia") |
| El administrador **cancela** la solicitud | `SolicitudCancelada` | Opcional |
| Se registra la adopción | `AdopcionConcretada` | — |
| El administrador acepta el aviso | `AvisoAceptado` | — |
| El administrador **descarta** el aviso | `AvisoDescartado` | Opcional |
| Se registra el rescate (ingreso desde el aviso) | `AvisoAtendido` | — |

Sólo reciben mensajes las personas con cuenta: la solicitud se vincula por el adoptante (`Adoptantes.UsuarioId`) y el aviso
por quien lo reportó con sesión iniciada (`AvisosRescate.UsuarioId`). En el panel, cada solicitud/aviso indica "Con cuenta"
y el diálogo de rechazo avisa si la persona va a recibir el mensaje. El ícono de sobre muestra la cantidad de mensajes nuevos
(se consulta al volver a la pestaña y cada 60 s); al abrir el buzón los mensajes quedan leídos.

---

## 8. Base de datos (`database/refugio_db.sql`)

- **Tablas**: Usuarios, Refugios, Cuidadores, AvisosRescate, Mascotas, FotosMascotas, HistorialesSanitarios, Tratamientos,
  Adoptantes, SolicitudesAdopcion, Adopciones, Notificaciones.
- **Tipos ENUM** (mismos nombres que en C#): estados de aviso, mascota, tratamiento y solicitud; tipo de animal, estado aparente,
  urgencia, sexo, tamaño, tipo de tratamiento, **rol de usuario** y **tipo de notificación**.
- **Restricciones que protegen el historial**: claves foráneas `RESTRICT` hacia mascotas, solicitudes, adoptantes y tratamientos;
  una sola adopción por mascota (`UQ_Adopciones_MascotaId`); una sola solicitud aprobada por mascota (índice único parcial);
  correo de usuario único y en minúsculas; un adoptante por cuenta.
- **Datos de prueba**: 10 mascotas en distintos estados con fotos, avisos, cuidadores, solicitudes y una adopción, y dos cuentas
  de prueba (Laura y Ana, contraseña `Patitas2026`). **No** hay administrador precargado.
- Es **re-ejecutable** (borra y recrea todo). Si se ejecuta con la API encendida, hay que reiniciar la API.

---

## 9. Frontend

| Parte | Archivos |
|---|---|
| Cliente de la API (envía la cookie con `credentials: 'include'`) | `src/api/client.ts`, `types.ts`, `format.ts` |
| Sesión, inicio de sesión y buzón | `components/Sesion.tsx`, `LoginDialog.tsx`, `Buzon.tsx`, `MenuCuenta.tsx` |
| Rechazo con motivo opcional | `components/MotivoDialog.tsx` |
| Fotos | `components/Gallery.tsx` (pública), `PhotoManager.tsx` (panel) |
| Diseño | `components/Ilustraciones.tsx`, `PantallaCarga.tsx`, `Banner.tsx`; estilos en `src/styles/` |
| Adaptabilidad | `components/Adaptabilidad.tsx`, `styles/themes.css` |
| Panel | `src/admin/AdminApp.tsx` (puerta de acceso para administradores y navegación), `admin/views/*` |

**Encabezado del sitio**: sin sesión muestra *Iniciar sesión*; con sesión, el **buzón** (sobre con mensajes nuevos) y el menú de la
cuenta; el botón **Panel del refugio** aparece **sólo** para administradores.

**Tarjetas de mascotas** (`public/PetCard.tsx`): son grandes (mínimo ~21rem de ancho) y, al pasar el mouse o llegar con el
teclado, **flotan** suavemente. Se puede tocar **cualquier parte** de la tarjeta (foto, nombre o datos) para abrir la ficha:
el único botón “Conocer a …” se estira sobre toda la tarjeta con `::after`, así el teclado y el lector de pantalla siguen
encontrando un solo control por mascota. Con “Reducir animaciones” o `prefers-reduced-motion` no hay movimiento.

**Inicio de sesión** (`components/LoginDialog.tsx`): mismo estilo del sitio, con un costado naranja ilustrado (perro, gato y
plato dibujados, huellas y corazones de fondo) y la lista de lo que permite la cuenta. En el celular los dibujos quedan arriba
del formulario.

**Mascotas en el panel** (`admin/views/Mascotas.tsx`): se ven como una **grilla de tarjetas** con foto, estado, datos,
cuidador, fecha de ingreso y cantidad de fotos; toda la tarjeta lleva a la ficha. Un selector **Tarjetas / Lista** permite
volver a la tabla y la elección se recuerda en el navegador.
La grilla es un componente común (`admin/ListadoMascotas.tsx`) que también usan **Resumen → Mascotas en tratamiento** y
**Cuidadores → Ver mascotas** (abre un diálogo con las mascotas a cargo de ese cuidador), con el mismo selector.

**Etiquetas**: los estados (disponible, en tratamiento, urgencia, etc.) son rectángulos con esquinas suaves, sin borde ni forma
de píldora, siempre con ícono y texto. Las miniaturas de mascotas son cuadradas con esquinas redondeadas.

**Adoptantes**: la tabla muestra las **mascotas adoptadas** por cada persona. Toda persona que adopta queda registrada como
adoptante (la solicitud se crea a su nombre) y no se puede eliminar porque tiene solicitudes en el historial. *Editar* sirve
para corregir sus datos (DNI, teléfono, correo, dirección); no cambia la cuenta con la que inicia sesión.

**Accesibilidad**: textos alternativos en fotos, foco visible, diálogos accesibles (Esc, foco atrapado y devuelto), resumen de errores
en formularios, mensajes de estado anunciados, contraste AA en todos los temas, objetivos táctiles de 44 px, sin desplazamiento
horizontal desde 320 px. Detalle por criterio en [`frontend/README.md`](../frontend/README.md).

---

## 10. Pruebas

| Tipo | Resultado |
|---|---|
| Unitarias (`dotnet test`) | 48 pruebas: capacidad, avisos, cuidadores, tratamientos, adopción, fotos, cuentas, contraseñas y buzón |
| Accesibilidad (axe-core, WCAG 2.0/2.1 A y AA) | 0 problemas en las pantallas públicas y del panel, en los 6 temas, en computadora y celular |
| Recorridos en navegador (Playwright) | Fotos y galería, adopción, reporte, aviso e ingreso, registro, inicio de sesión, rechazo con motivo y buzón |

---

## 11. Historial de cambios

| Fecha | Commit / PR | Cambio |
|---|---|---|
| 07/09/2026 | `cccaecc` … `46e05a2` | Primera versión del código, `.gitignore`, corrección de Swagger en .NET 10, entidades y relaciones del UML |
| 09–10/09/2026 | `9703513`, `387521f` | Cuidador, Adoptante e historial sanitario; ajustes de consultas |
| 24/09 – 06/10/2026 | `3df61a9` … `2a7bf07` | Correcciones de compilación, estructura y reglas de negocio de mascotas y cuidadores |
| 05–09/11/2026 | `a8d5c02`, `9b5751a`, `b0716c2` | Persistencia con base de datos y primeras pruebas |
| 12–18/11/2026 | `565142d` … `3f496aa` | Script SQL con datos de prueba, actualización de la base y primera versión del frontend |
| 18/11/2026 | `4e4c7f9` · PR #1 | **Backend completo según la consigna**: CORS, enums nativos, fechas UTC, repositorios con consultas de la Entrega 4, reglas de negocio, transacciones, manejo de errores y script SQL re-ejecutable |
| 19/11/2026 | `35d625e` · PR #1 | **Rediseño del frontend** accesible y responsivo, panel del administrador, **varias fotos por mascota** con galería y reporte de animales |
| 19/11/2026 | `fa58131` · PR #2 | **Nuevo diseño** inspirado en Petsome y El Campito: pantalla de carga, ilustraciones propias, degradés y mosaicos |
| 19/11/2026 | `b791970` · PR #3 | Se quitan los mosaicos de acceso rápido del sitio público |
| 19/11/2026 | `fd1a1c1` · PR #3 | **Adaptabilidad**: temas para baja visión y daltonismo, tamaño de texto, espaciado, reducir animaciones y etiquetas de estado con texto e ícono |
| 22/11/2026 | (este cambio) | **Cuentas e inicio de sesión** (roles Usuario/Administrador), **panel sólo para administradores**, **buzón de mensajes** con motivo opcional al rechazar/cancelar/descartar, tablas `Usuarios` y `Notificaciones`, script `crear_admin.sql` y esta documentación |
| 22/11/2026 | (este cambio) | Tarjetas de mascotas **más grandes, que flotan** al pasar el mouse y se abren tocando en cualquier parte; **inicio de sesión ilustrado** con el diseño del sitio; mascotas del panel en **grilla de tarjetas** con selector Tarjetas / Lista |
| 22/11/2026 | (este cambio) | Tarjetas también en *Mascotas en tratamiento* y en *Cuidadores → Ver mascotas*; etiquetas de estado rectangulares sin borde; miniaturas cuadradas; columna *Mascotas adoptadas* en Adoptantes |
| 22/11/2026 | (este cambio) | El gato de la pantalla de carga vuelve a girar aunque la computadora tenga desactivadas las animaciones (gira más despacio) |
