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
│   └── diagrama-clases/
│
├── backend/
│   ├── tpintegrador_psr2026.sln
│   ├── tpintegrador_psr2026.Api/
│       ├── Controllers/
│       ├── Services/
│       ├── /DTOs
│       ├── Domain/
│       ├── Program.cs
│       └── tpintegrador_psr2026.Api.csproj
│   └── tpintegrador_psr2026.Tests/
│
└── frontend/
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

- `http://localhost:5173/` → sitio público: mascotas disponibles con su galería de fotos, solicitud de adopción y
  formulario para **reportar un animal** que necesita ayuda.
- `http://localhost:5173/admin.html` → **panel del refugio**: resumen, mascotas (ficha, fotos, estados, cuidador,
  historial sanitario), avisos, tratamientos, cuidadores, adoptantes, solicitudes, adopciones y configuración.

El frontend se diseñó para celular y computadora y con criterios de accesibilidad **WCAG 2.1 nivel AA**, e incluye un panel de **Adaptabilidad** (temas para baja visión y daltonismo, tamaño de texto, espaciado y animaciones)
(Ley 26.653). Detalle en [`frontend/README.md`](frontend/README.md).

Otros comandos útiles:

```bash
npm run build
npm test
```

### `docs/`
Documentación del proyecto: requerimientos, endpoints y diagramas.

### `Domain/`
Clases principales del sistema.

Ejemplos: `Usuario`, `Producto`, `Pedido`, `Incidente`.

### `DAO/`
Acceso a los datos.

Se encarga de buscar, guardar, modificar y eliminar información.

### `Services/`
Lógica y reglas de negocio del sistema.

### `Controllers/`
Reciben las peticiones HTTP, utilizan los Services y devuelven las respuestas de la API.

---

## Flujo del backend

```text
Frontend
   ↓
Controller
   ↓
Service
   ↓
DAO
   ↓
Datos
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

Los endpoints desarrollados deberán documentarse en:

```text
docs/endpoints.md
```

---

## Reglas generales

- Mantener separadas las responsabilidades de cada capa.
- No colocar la lógica del sistema en los Controllers.
- El acceso a datos debe realizarse desde los DAO.
- Las reglas de negocio deben estar en los Services.
- El frontend debe comunicarse con el backend mediante la API REST.
- Mantener actualizada la documentación.
- No subir contraseñas, tokens ni credenciales al repositorio.
