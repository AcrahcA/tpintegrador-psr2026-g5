-- =====================================================================
--  Convertir una cuenta en ADMINISTRADOR del refugio
--
--  Por seguridad, desde el sitio sólo se pueden crear cuentas comunes.
--  Para tener un administrador:
--
--   1. Creá la cuenta desde el sitio: botón "Iniciar sesión" → "Crear cuenta".
--   2. Ejecutá este script indicando el correo de esa cuenta:
--
--        psql -U postgres -d refugio_db -v email=tu.correo@ejemplo.com -f database/crear_admin.sql
--
--      (En pgAdmin: abrí la Query Tool, reemplazá :'email' por 'tu.correo@ejemplo.com' y ejecutá.)
--   3. Recargá el sitio: aparece el botón "Panel del refugio" (no hace falta volver a iniciar sesión).
--
--  Para quitarle el rol, cambiá 'Administrador' por 'Usuario' en la consulta.
-- =====================================================================

UPDATE public."Usuarios"
SET "Rol" = 'Administrador'
WHERE "Email" = lower(trim(:'email'))
RETURNING "Id", "Nombre", "Email", "Rol";
