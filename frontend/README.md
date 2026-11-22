# Frontend – Patitas (React + TypeScript + Vite)

Dos páginas que consumen la API del backend (`VITE_API_URL`, por defecto `http://localhost:5080`):

| Página | Para quién | Qué permite |
|---|---|---|
| `index.html` (sitio público) | Cualquier persona | Ver las mascotas disponibles y **todas sus fotos** (galería), filtrar, **solicitar una adopción** (con cuenta) y **reportar un animal** que necesita ayuda (con o sin cuenta; ubicación por dirección, GPS del celular o coordenadas). **Iniciar sesión / crear cuenta** y **buzón de mensajes**. |
| `admin.html` (panel del refugio) | Sólo cuentas **Administrador** | Resumen con indicadores, mascotas (ficha, **cargar y ordenar fotos**, estados, cuidador, historial sanitario), avisos de rescate (aceptar, descartar, registrar ingreso), tratamientos, cuidadores, adoptantes, solicitudes (aprobar, rechazar, registrar adopción), adopciones y datos del refugio. Al rechazar, cancelar o descartar se puede escribir un **motivo opcional** que llega al buzón de la persona. |

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

## Cuentas, panel y buzón

- **Sin sesión** el encabezado muestra *Iniciar sesión* (en el celular, *Ingresar*). Desde ahí se puede **crear una cuenta**
  (siempre de rol Usuario) o ingresar (`components/LoginDialog.tsx`).
- **Con sesión** aparecen el **buzón** (ícono de sobre con la cantidad de mensajes nuevos) y el menú de la cuenta con
  *Cerrar sesión* (`components/MenuCuenta.tsx`, `components/Buzon.tsx`).
- El botón **Panel del refugio** aparece **sólo para administradores**. Si alguien entra a `admin.html` sin sesión se le pide
  iniciarla, y si su cuenta no es de administrador se le informa que no tiene permiso (`admin/AdminApp.tsx`).
- Para **pedir adoptar** hay que iniciar sesión: si no la hay, se abre el inicio de sesión y después se sigue con el
  formulario, completado con los datos de la cuenta. **Reportar** un animal no requiere cuenta, pero con sesión la
  respuesta llega al buzón.
- La sesión la maneja la API con una cookie HttpOnly; el frontend nunca guarda contraseñas ni tokens
  (`components/Sesion.tsx`, `api/client.ts` con `credentials: 'include'`).

## Diseño

Inspirado en dos sitios de referencia, con ilustraciones y logo propios (no se copian marcas ni dibujos ajenos):

- **Estilo Petsome:** fondo gris claro, azul como color principal, mosaicos de colores como indicadores del resumen
  del panel, franjas con degradé azul → violeta y una palabra gigante de fondo, tarjetas blancas centradas, interruptores
  tipo píldora, pie oscuro y una **pantalla de carga** con el logo y un gato enroscado que gira.
- **Estilo El Campito:** títulos naranjas, ilustraciones de perros y gatos con trazo negro y relleno naranja
  (`src/components/Ilustraciones.tsx`), sección naranja con garabatos de huellas, huesos y corazones, y sección crema
  con perros a los costados.
- **Tarjetas de mascotas grandes que flotan** al pasar el mouse (o al llegar con el teclado); se abren tocando cualquier parte
  de la tarjeta. Sin movimiento si se eligió “Reducir animaciones”.
- **Inicio de sesión ilustrado** con el mismo estilo: costado naranja con perro, gato y plato dibujados.
- **Mascotas del panel en grilla de tarjetas** (foto, estado, cuidador, ingreso), con selector **Tarjetas / Lista** que se recuerda.
  La misma grilla se usa en *Resumen → Mascotas en tratamiento* y en *Cuidadores → Ver mascotas* (`src/admin/ListadoMascotas.tsx`).
- **Etiquetas de estado** rectangulares con esquinas suaves, sin borde ni forma de píldora (siempre con ícono y texto).
- **Tipografías:** Comfortaa para títulos y Nunito para el texto, incluidas en el proyecto (`@fontsource`), sin depender de servicios externos.
- Los colores de referencia se ajustaron para cumplir el contraste AA: azul `#1d5fc9`, naranja de títulos `#a85300`,
  texto oscuro sobre los mosaicos claros y sobre el botón naranja.

## Celular y computadora

- Diseño *mobile first*: se adapta desde 320 px de ancho sin desplazamiento horizontal (WCAG 1.4.10).
- En pantallas angostas el menú se despliega con un botón y las tablas se muestran como tarjetas.
- Los diálogos ocupan toda la pantalla en el celular.
- Para reportar un animal se puede usar la ubicación del celular (“Usar mi ubicación actual”).

## Adaptabilidad

Botón **“Adaptabilidad”** en el menú del sitio público (y al final del menú del panel) que abre un panel de ajustes.
Las preferencias se guardan en el navegador (`localStorage`) y se aplican antes de mostrar la página, en el sitio y en el panel
(`src/components/Adaptabilidad.tsx` y `src/styles/themes.css`):

| Ajuste | Opciones | Para quién |
|---|---|---|
| Tema de colores | Predeterminado · **Alto contraste** · **Oscuro** · **Daltonismo rojo-verde** (protanopía/deuteranopía, paleta Okabe-Ito) · **Daltonismo azul-amarillo** (tritanopía) · **Escala de grises** | Baja visión, sensibilidad a la luz, daltonismo, acromatopsia |
| Tamaño del texto | Normal (100 %) · Mediano (112 %) · Grande (125 %) · Muy grande (150 %) | Baja visión |
| Más espacio entre líneas y letras | Activado / desactivado (WCAG 1.4.12) | Dislexia, dificultades de lectura |
| Reducir animaciones | Activado / desactivado (además respeta la preferencia del sistema). El gato de la pantalla de carga sigue girando, más despacio, porque indica que la página está cargando | Sensibilidad al movimiento |

**Etiquetas de estado accesibles:** el estado nunca se indica sólo con color. Cada etiqueta lleva **texto visible y un ícono**
(✓ disponible/aprobado/finalizado, reloj para pendiente/reservada, ⚠ para en tratamiento/rechazada, “i” para en curso o en
evaluación, corazón para recuperación/adoptada, “–” para cancelado/descartado). La urgencia de los avisos también se distingue por
el estilo del borde (doble, discontinuo, continuo) además del texto “Urgencia alta/media/baja”.

Cada tema se auditó con axe-core (9 pantallas por tema, incluido el propio panel de ajustes): 0 problemas.
Con texto al 150 % y espaciado amplio no hay desplazamiento horizontal a 320 px.

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
