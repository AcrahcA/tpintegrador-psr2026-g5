# Frontend – Patitas (React + TypeScript + Vite)

Dos páginas que consumen la API del backend (`VITE_API_URL`, por defecto `http://localhost:5080`):

| Página | Para quién | Qué permite |
|---|---|---|
| `index.html` (sitio público) | Cualquier persona | Ver las mascotas disponibles y **todas sus fotos** (galería), filtrar, **solicitar una adopción** y **reportar un animal** que necesita ayuda (con ubicación por dirección, GPS del celular o coordenadas). |
| `admin.html` (panel del refugio) | Equipo del refugio | Resumen con indicadores, mascotas (ficha, **cargar y ordenar fotos**, estados, cuidador, historial sanitario), avisos de rescate (aceptar, descartar, registrar ingreso), tratamientos, cuidadores, adoptantes, solicitudes (aprobar, rechazar, registrar adopción), adopciones y datos del refugio. |

```bash
npm install
npm run dev      # http://localhost:5173
npm run build    # verificación de tipos + build de producción en dist/
```

## Estructura

```text
src/
├── api/          cliente HTTP tipado, tipos de la API y formato de textos/estados
├── components/   componentes accesibles reutilizables (Dialog, campos, galería, gestor de fotos, avisos)
├── public/       sitio público
├── admin/        panel del refugio (navegación por #/ruta) y sus vistas
└── styles/       tokens de diseño, componentes, estilos de cada página
```

## Celular y computadora

- Diseño *mobile first*: se adapta desde 320 px de ancho sin desplazamiento horizontal (WCAG 1.4.10).
- En pantallas angostas el menú se despliega con un botón y las tablas se muestran como tarjetas.
- Los diálogos ocupan toda la pantalla en el celular.
- Para reportar un animal se puede usar la ubicación del celular (“Usar mi ubicación actual”).

## Accesibilidad (WCAG 2.1 AA – Ley 26.653)

| Criterio | Cómo se cumple |
|---|---|
| 1.1.1 Contenido no textual | Cada foto tiene texto alternativo (la descripción que carga el refugio; si no se carga, “Foto de {nombre}”). Los íconos son decorativos (`aria-hidden`). |
| 1.3.1 Información y relaciones | Encabezados jerárquicos, regiones (`header`, `nav`, `main`, `footer`), listas, tablas con encabezados y `fieldset/legend` en grupos de opciones. |
| 1.4.1 Uso del color | Los estados siempre se muestran con texto, no sólo con color. |
| 1.4.3 / 1.4.11 Contraste | Texto ≥ 4.5:1 y bordes de campos / foco ≥ 3:1 (verificados con cálculo de contraste). |
| 1.4.4 / 1.4.10 Cambio de tamaño y reflow | Tipografía en `rem`, sin scroll horizontal a 320 px ni con zoom de 400 %. |
| 2.1.1 Teclado | Todo se opera con teclado; la galería también responde a las flechas ← →. |
| 2.4.1 Saltar bloques | Enlace “Saltar al contenido principal”. |
| 2.4.2 / 2.4.3 Título y orden del foco | El título de la pestaña cambia en cada sección del panel y el foco va al encabezado de la vista. Los diálogos atrapan el foco, se cierran con Esc y lo devuelven al botón que los abrió. |
| 2.4.7 Foco visible | Anillo de foco azul de 3 px en todos los controles. |
| 2.5.5 (AAA, aplicado) | Botones y campos de al menos 44 × 44 px. |
| 3.1.1 Idioma | `lang="es-AR"`. |
| 3.3.1 / 3.3.2 / 3.3.3 Formularios | Etiquetas visibles, indicación “(obligatorio)” en texto, ayudas, mensajes de error junto al campo y un resumen de errores que recibe el foco con enlaces a cada campo. |
| 4.1.2 / 4.1.3 Nombre, rol, valor y mensajes de estado | `aria-expanded`, `aria-current`, `aria-pressed`, `aria-invalid`; confirmaciones anunciadas con `role="status"` y errores con `role="alert"`. |

Además respeta `prefers-reduced-motion` y el modo de alto contraste de Windows (`forced-colors`).

**Verificación:** se auditó con [axe-core](https://github.com/dequelabs/axe-core) (reglas WCAG 2.0/2.1 A y AA + buenas prácticas)
las 13 pantallas en escritorio y en celular (Pixel 7), incluidos los diálogos y un formulario con errores: 0 problemas.
Las auditorías automáticas no reemplazan una prueba con lectores de pantalla (NVDA, TalkBack, VoiceOver).
