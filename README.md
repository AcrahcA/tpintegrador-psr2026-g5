# Trabajo Práctico Integrador

## Datos del proyecto

**Tema:**  
Completar

**Integrantes:**
- Apellido, Nombre
- Apellido, Nombre
- Apellido, Nombre

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
│       ├── DAO/
│       ├── Domain/
│       ├── Program.cs
│       └── tpintegrador_psr2026.Api.csproj
│   └── tpintegrador_psr2026.Tests/
│
└── frontend/
```

## Puesta en marcha

Requisitos: Node.js, npm y .NET SDK 10.

Desde la raíz del repositorio:

```bash
npm run dev
```

La API queda disponible en `http://localhost:5080`. Para comprobarla:

- `GET /`
- `GET /api/health`

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
