# Trabajo Práctico Integrador 

## Datos del proyecto

**Tema:**  
Gestión de Rescates y Adopciones de Mascotas

**Integrantes:**
- Charca, Ariel
- Silicuana, Andy
- Herrera, Matias
- Ajhiagunca, Ezequiel

## Enunciado

[Ver enunciado del TP](https://docs.google.com/document/d/1TDWUL1pNOTdqh9Lj__R_p1-kE66S-5o61o-rEIEEyQ8/edit?usp=sharing)

---

## Estructura del repositorio

```text
/
├── docs/
│   ├── requerimientos.md
│   ├── endpoints.md
│   ├── DOCUMENTACION.md          ← estructura, lógica e historial de cambios del software
│   ├── consigna(2).md
│   └── DiagramaUML.drawio.png    (diagrama de clases)
│
├── database/
│   ├── refugio_db.sql            ← estructura + datos de prueba de PostgreSQL
│   └── crear_admin.sql           ← convierte una cuenta en Administrador
│
├── backend/
│   ├── tpintegrador_psr2026.sln
│   ├── tpintegrador_psr2026.Api/
│       ├── Controllers/
│       ├── Services/             (incluye Seguridad/: contraseñas, sesión y roles)
│       ├── DTOs/
│       ├── Domain/
│       ├── Repositories/         (los DAO: acceso a datos)
│       ├── Data/                 (RefugioDbContext y transacciones)
│       ├── Program.cs
│       └── tpintegrador_psr2026.Api.csproj
│   └── tpintegrador_psr2026.Tests/
│
└── frontend/                     (React + TypeScript + Vite)
    ├── index.html                ← sitio público
    ├── admin.html                ← panel del refugio (sólo administradores)
    └── src/                      (api/, components/, public/, admin/, styles/)
```

## Puesta en marcha

Requisitos: Node.js, npm, .NET SDK 10 y PostgreSQL 14 o superior.

### 1. Base de datos

El script `database/refugio_db.sql` crea los tipos, las tablas y datos de prueba (se puede volver a ejecutar: borra y recrea todo).
Si lo volvés a ejecutar con la API encendida, **reiniciá la API**: los tipos enum se recrean y la API debe volver a leerlos.

```bash
createdb refugio_db
psql -d refugio_db -f database/refugio_db.sql
```

### 2. Cadena de conexión

`appsettings.json` trae valores de ejemplo (`TU_USUARIO` / `TU_CONTRASEÑA`). Para no subir credenciales, crear
`backend/tpintegrador_psr2026.Api/appsettings.Development.json` (está en `.gitignore`):

```json
{
  "ConnectionStrings": {
    "PostgreSQLConnection": "Host=localhost;Database=refugio_db;Username=postgres;Password=tu_contraseña"
  }
}
```

### 3. Backend

Desde la raíz del repositorio:

```bash
npm run dev
```

La API queda disponible en `http://localhost:5080` (Swagger en `/swagger`). Para comprobarla:

- `GET /`
- `GET /api/health` → indica si la conexión con PostgreSQL funciona.

### 4. Frontend

```bash
cd frontend
npm install
npm run dev
```

El frontend corre en `http://localhost:5173` y usa `VITE_API_URL` (`frontend/.env`) para llegar a la API.
La API permite ese origen por CORS; si se usa otro puerto, agregarlo en `Cors:OrigenesPermitidos` de `appsettings.json`.

- `http://localhost:5173/` → sitio público: mascotas disponibles con su galería de fotos, solicitud de adopción
  (con cuenta), formulario para **reportar un animal** que necesita ayuda (con o sin cuenta), **inicio de sesión**
  y **buzón de mensajes** (ícono de sobre) con las respuestas del refugio.
- `http://localhost:5173/admin.html` → **panel del refugio** (sólo cuentas **Administrador**): resumen, mascotas
  (ficha, fotos, estados, cuidador, historial sanitario), avisos, tratamientos, cuidadores, adoptantes, solicitudes,
  adopciones y configuración. Al rechazar, cancelar o descartar se puede escribir un **motivo opcional** que le llega
  a la persona en su buzón.

El frontend se diseñó para celular y computadora y con criterios de accesibilidad **WCAG 2.1 nivel AA**, e incluye un panel de **Adaptabilidad** (temas para baja visión y daltonismo, tamaño de texto, espaciado y animaciones)
(Ley 26.653). Detalle en [`frontend/README.md`](frontend/README.md).

### 5. Cuentas y administrador

- Cualquier persona puede crear una cuenta desde el sitio (**Iniciar sesión → Crear cuenta**). Las cuentas nuevas son
  siempre de rol **Usuario**: pueden pedir adoptar y reciben las respuestas en su **buzón**.
- El botón **Panel del refugio** sólo aparece para cuentas **Administrador**, y la API rechaza (403) las acciones del
  panel hechas por otras cuentas.
- Cuentas de prueba que trae el script (sólo para desarrollo): `laura.perez@example.com` y `ana.torres@example.com`,
  contraseña `Patitas2026`. No hay ningún administrador precargado.

**Cómo crear la cuenta de administrador (paso a paso):**

1. Levantar la base de datos, el backend y el frontend (pasos 1 a 4).
2. Entrar a `http://localhost:5173`, tocar **Iniciar sesión → Crear cuenta** y registrar la cuenta del administrador
   (por ejemplo `admin@refugio.org`, con una contraseña de al menos 8 caracteres con letras y números).
3. Desde la raíz del repositorio, darle el rol de administrador:

   ```bash
   psql -U postgres -d refugio_db -v email=admin@refugio.org -f database/crear_admin.sql
   ```

   En **pgAdmin**: abrir `database/crear_admin.sql` en la *Query Tool*, reemplazar `:'email'` por `'admin@refugio.org'`
   y ejecutar.
4. Recargar el sitio: aparece el botón **Panel del refugio** (no hace falta volver a iniciar sesión).

Para quitarle el rol, cambiar `'Administrador'` por `'Usuario'` en la consulta del script.

Otros comandos útiles:

```bash
npm run build
npm test
```

### `docs/`
Documentación del proyecto: requerimientos, endpoints y diagramas.
La explicación completa de cómo está conformado el software (estructura, capas, reglas de negocio, seguridad, buzón,
base de datos) y el **historial de cambios** está en [`docs/DOCUMENTACION.md`](docs/DOCUMENTACION.md).
Los endpoints y sus permisos, en [`docs/endpoints.md`](docs/endpoints.md).

### `Domain/`
Clases principales del sistema.

Ejemplos: `Usuario`, `Mascota`, `AvisoRescate`, `Cuidador`, `Tratamiento`, `HistorialSanitario`, `Adoptante`,
`SolicitudAdopcion`, `Adopcion`, `FotoMascota`, `Notificacion` (mensaje del buzón) y `Refugio`.

### `DAO/`
Acceso a los datos.

Se encarga de buscar, guardar, modificar y eliminar información.

En este proyecto los DAO son los **Repositories** (`backend/tpintegrador_psr2026.Api/Repositories/`), que usan
Entity Framework Core sobre PostgreSQL. Además del CRUD tienen consultas específicas (por ejemplo, mascotas por
estado, avisos pendientes, mascotas de un cuidador o el buzón de un usuario).

### `Services/`
Lógica y reglas de negocio del sistema.

Por ejemplo: capacidad del refugio, condiciones sanitarias para adoptar, reserva al aprobar una solicitud, envío de
mensajes al buzón y, en `Services/Seguridad/`, el hash de contraseñas y la sesión.

### `Controllers/`
Reciben las peticiones HTTP, utilizan los Services y devuelven las respuestas de la API.

También indican quién puede usar cada endpoint: las acciones del panel exigen el rol **Administrador**.

---

## Flujo del backend

```text
Frontend (React: sitio público y panel)
   ↓   HTTP + JSON, con la cookie de sesión
Controller   (permisos: público / con sesión / Administrador)
   ↓
Service      (reglas de negocio y mensajes al buzón)
   ↓
DAO          (Repositories con Entity Framework Core)
   ↓
Datos        (PostgreSQL: database/refugio_db.sql)
```

El **frontend** es la interfaz que utiliza el usuario y puede desarrollarse con la tecnología elegida por el grupo.

El **backend** debe desarrollarse en **C# / .NET** y será responsable de la lógica, los datos y la API REST.

---

## API REST

El frontend deberá comunicarse con el backend mediante HTTP y JSON.

Ejemplo de endpoints:

| Método | Endpoint | Acción |
|---|---|---|
| GET | `/api/productos` | Obtener todos |
| GET | `/api/productos/{id}` | Obtener uno |
| POST | `/api/productos` | Crear |
| PUT | `/api/productos/{id}` | Modificar |
| DELETE | `/api/productos/{id}` | Eliminar |

Ejemplo de respuesta:

```json
{
  "id": 1,
  "nombre": "Producto"
}
```

Algunos endpoints de este proyecto:

| Método | Endpoint | Acción | Quién |
|---|---|---|---|
| GET | `/api/Mascotas/disponibles` | Mascotas que se pueden adoptar, con sus fotos | Público |
| POST | `/api/avisos-rescate` | Reportar un animal | Público |
| POST | `/api/auth/registro` / `/api/auth/login` | Crear cuenta / iniciar sesión | Público |
| POST | `/api/solicitudes-adopcion/publica` | Pedir adoptar | Con sesión |
| GET | `/api/buzon` | Mensajes del buzón | Con sesión |
| PUT | `/api/solicitudes-adopcion/{id}/rechazar` | Rechazar (motivo opcional) | Administrador |
| PUT | `/api/avisos-rescate/{id}/descartar` | Descartar (motivo opcional) | Administrador |

Ejemplo de respuesta (`GET /api/buzon`):

```json
[
  {
    "id": 7,
    "tipo": "SolicitudRechazada",
    "titulo": "Tu solicitud para adoptar a Toby fue rechazada",
    "mensaje": "Solicitud #5. Podés conocer a otras mascotas disponibles y enviar una nueva solicitud.",
    "motivo": "Toby necesita una casa con patio.",
    "fecha": "2026-11-22T00:10:34Z",
    "leida": false
  }
]
```

Los endpoints desarrollados deberán documentarse en:

```text
docs/endpoints.md
```

---

## Reglas generales

- Mantener separadas las responsabilidades de cada capa.
- No colocar la lógica del sistema en los Controllers.
- El acceso a datos debe realizarse desde los DAO (en este proyecto, los Repositories).
- Las reglas de negocio deben estar en los Services.
- El frontend debe comunicarse con el backend mediante la API REST.
- Mantener actualizada la documentación (incluido el historial de `docs/DOCUMENTACION.md`).
- No subir contraseñas, tokens ni credenciales al repositorio.
- Las contraseñas de las cuentas nunca se guardan en texto: sólo su hash (PBKDF2).
- El rol de administrador no se puede obtener desde el sitio: se asigna desde la base (`database/crear_admin.sql`).
