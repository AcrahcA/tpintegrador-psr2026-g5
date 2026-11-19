// Ilustraciones originales del proyecto (SVG). Todas son decorativas: aria-hidden.
// Estilo: trazo oscuro redondeado, relleno naranja y blanco, como un dibujo a mano.

const TRAZO = '#262b30';
const NARANJA = '#ff9600';
const NARANJA_CLARO = '#ffc27a';

type P = { className?: string; size?: number };
const svgProps = (className?: string) => ({ className, 'aria-hidden': true as const, focusable: 'false' as const });

/** Logo: corazón azul con una huella. */
export function Logo({ size = 44, className }: P) {
  return (
    <svg {...svgProps(className)} width={size} height={size} viewBox="0 0 64 64">
      <path d="M32 58C14 45 4 35 4 21 4 11 12 4 21 4c5 0 9 2.5 11 6.5C34 6.5 38 4 43 4c9 0 17 7 17 17 0 14-10 24-28 37z" fill="#2e76e4" />
      <path d="M32 58C14 45 4 35 4 21 4 11 12 4 21 4c5 0 9 2.5 11 6.5" fill="none" stroke="#1d5fc9" strokeWidth="2" />
      <g fill="#fff">
        <ellipse cx="32" cy="36" rx="9" ry="7.5" />
        <ellipse cx="21.5" cy="25" rx="3.6" ry="4.6" transform="rotate(-20 21.5 25)" />
        <ellipse cx="28" cy="19.5" rx="3.6" ry="4.8" />
        <ellipse cx="36" cy="19.5" rx="3.6" ry="4.8" />
        <ellipse cx="42.5" cy="25" rx="3.6" ry="4.6" transform="rotate(20 42.5 25)" />
      </g>
    </svg>
  );
}

/** Gato enroscado en forma de anillo: gira en la pantalla de carga. */
export function GatoAnillo({ size = 140, className }: P) {
  const arco = 'M 30.5 86.5 A 42 42 0 1 1 89.5 86.5';
  return (
    <svg {...svgProps(className)} width={size} height={size} viewBox="0 0 120 120">
      <path d={arco} fill="none" stroke={TRAZO} strokeWidth="27" strokeLinecap="round" />
      <path d={arco} fill="none" stroke="#8fa4d8" strokeWidth="21" strokeLinecap="round" />
      <path d="M 36 82 A 34 34 0 0 1 30 40" fill="none" stroke="#e9eefb" strokeWidth="7" strokeLinecap="round" />
      {/* patitas hacia adentro */}
      <g fill="#e9eefb" stroke={TRAZO} strokeWidth="2.5">
        <circle cx="38" cy="30" r="5" /><circle cx="25" cy="48" r="5" />
      </g>
      {/* cola */}
      <path d="M 89.5 86.5 q 14 6 12 20" fill="none" stroke={TRAZO} strokeWidth="11" strokeLinecap="round" />
      <path d="M 89.5 86.5 q 14 6 12 20" fill="none" stroke="#8fa4d8" strokeWidth="6" strokeLinecap="round" />
      {/* cabeza */}
      <g transform="translate(30 88)">
        <path d="M -15 -6 l -4 -16 l 12 8 z M 15 -6 l 4 -16 l -12 8 z" fill="#8fa4d8" stroke={TRAZO} strokeWidth="2.5" strokeLinejoin="round" />
        <circle r="16" fill="#8fa4d8" stroke={TRAZO} strokeWidth="2.8" />
        <ellipse cy="6" rx="9" ry="6.5" fill="#e9eefb" />
        <circle cx="-6" cy="-2" r="2.6" fill={TRAZO} /><circle cx="6" cy="-2" r="2.6" fill={TRAZO} />
        <circle cx="-5.3" cy="-2.8" r=".9" fill="#fff" /><circle cx="6.7" cy="-2.8" r=".9" fill="#fff" />
        <path d="M -2.5 3 h 5 l -2.5 2.5 z" fill={TRAZO} />
        <path d="M -4 7.5 q 2 2 4 0 q 2 2 4 0" fill="none" stroke={TRAZO} strokeWidth="1.6" strokeLinecap="round" />
        <path d="M -9 4 h -9 M -9 7 l -8 2 M 9 4 h 9 M 9 7 l 8 2" stroke={TRAZO} strokeWidth="1.2" strokeLinecap="round" />
      </g>
    </svg>
  );
}

/** Perro sentado, de dibujo con trazo negro y manchas naranjas. */
export function PerroSentado({ size = 200, className }: P) {
  return (
    <svg {...svgProps(className)} width={size} height={size} viewBox="0 0 200 200">
      <g stroke={TRAZO} strokeWidth="4" strokeLinecap="round" strokeLinejoin="round">
        {/* cola */}
        <path d="M58 150 C 34 146, 28 118, 40 104" fill="none" />
        {/* cuerpo */}
        <path d="M70 182 C 52 182, 48 150, 60 126 C 70 104, 92 96, 108 100 C 120 120, 126 150, 122 182 Z" fill="#fff" />
        <path d="M62 130 C 70 116, 84 112, 94 118 C 92 136, 78 148, 64 150 Z" fill={NARANJA} stroke="none" />
        {/* patas delanteras */}
        <path d="M104 120 L 108 184 M 120 122 L 126 184" fill="none" />
        <path d="M100 184 h 14 M 120 184 h 14" fill="none" />
        {/* pata trasera */}
        <path d="M66 182 C 66 166, 84 160, 90 172 C 92 178, 88 184, 80 184 Z" fill="#fff" />
        {/* cabeza */}
        <path d="M100 60 C 100 34, 124 26, 142 34 C 156 40, 166 50, 176 54 C 184 58, 184 70, 174 74 C 160 80, 146 84, 132 92 C 116 100, 100 88, 100 60 Z" fill="#fff" />
        {/* oreja */}
        <path d="M112 40 C 96 36, 86 52, 88 72 C 90 82, 100 84, 104 76 C 108 64, 114 54, 116 46 Z" fill={NARANJA} />
        <path d="M128 30 C 136 26, 148 30, 150 40 C 140 44, 132 42, 128 30 Z" fill={NARANJA} stroke="none" />
        {/* hocico */}
        <ellipse cx="178" cy="60" rx="7" ry="6" fill={TRAZO} />
        <path d="M150 78 C 156 84, 166 82, 170 76" fill="none" />
      </g>
      <circle cx="140" cy="50" r="5" fill={TRAZO} />
      <circle cx="141.5" cy="48.5" r="1.6" fill="#fff" />
      {/* huellitas */}
      <g fill={NARANJA}>
        <Huella x={40} y={36} s={0.55} /><Huella x={22} y={70} s={0.45} /><Huella x={168} y={130} s={0.5} /><Huella x={158} y={168} s={0.6} />
      </g>
    </svg>
  );
}

/** Perro salchicha parado, naranja. */
export function PerroSalchicha({ size = 260, className }: P) {
  return (
    <svg {...svgProps(className)} width={size} height={size * 0.6} viewBox="0 0 260 156">
      <g stroke={TRAZO} strokeWidth="4" strokeLinecap="round" strokeLinejoin="round">
        {/* cola */}
        <path d="M212 66 C 228 60, 238 48, 250 40" fill="none" />
        {/* patas (detrás del cuerpo) */}
        <g fill={NARANJA}>
          <rect x="84" y="84" width="17" height="52" rx="8" /><rect x="106" y="84" width="17" height="50" rx="8" />
          <rect x="176" y="84" width="17" height="50" rx="8" /><rect x="198" y="84" width="17" height="52" rx="8" />
        </g>
        {/* cuerpo largo */}
        <rect x="68" y="50" width="152" height="50" rx="25" fill={NARANJA} />
        {/* cabeza con hocico largo */}
        <path d="M86 52 C 86 30, 70 20, 52 22 C 38 24, 30 34, 26 42 C 16 44, 6 50, 6 58 C 6 66, 16 70, 30 68 C 40 74, 56 76, 70 72 C 82 68, 86 62, 86 52 Z" fill={NARANJA} />
        {/* oreja colgando */}
        <path d="M60 26 C 76 26, 84 44, 80 66 C 78 80, 64 82, 62 72 C 60 60, 56 42, 60 26 Z" fill={TRAZO} />
        <ellipse cx="9" cy="55" rx="6.5" ry="6" fill={TRAZO} />
        <path d="M14 66 C 22 70, 30 70, 36 66" fill="none" strokeWidth="3" />
        {/* manchitas del lomo */}
        <path d="M120 62 q 6 6 12 0 M 150 60 q 6 6 12 0" fill="none" stroke="#fff" strokeWidth="3" />
      </g>
      <circle cx="44" cy="42" r="4" fill={TRAZO} />
      <circle cx="45.3" cy="40.7" r="1.3" fill="#fff" />
    </svg>
  );
}

/** Perro echado, panza abajo, con manchas. */
export function PerroEchado({ size = 260, className }: P) {
  return (
    <svg {...svgProps(className)} width={size} height={size * 0.62} viewBox="0 0 260 160">
      <g stroke={TRAZO} strokeWidth="4" strokeLinecap="round" strokeLinejoin="round">
        <path d="M20 112 C 4 104, 6 84, 22 82" fill="none" />
        <path d="M22 120 C 18 96, 40 80, 80 80 L 160 84 C 190 88, 196 132, 170 138 L 46 138 C 30 138, 24 132, 22 120 Z" fill={NARANJA} />
        <path d="M150 128 C 176 130, 214 132, 232 138 C 238 142, 232 148, 222 146 L 150 140 Z" fill={NARANJA} />
        <path d="M140 74 C 136 46, 160 30, 190 34 C 220 38, 236 58, 240 72 C 244 86, 232 96, 214 98 C 186 102, 146 100, 140 74 Z" fill={NARANJA} />
        <path d="M170 40 C 156 50, 154 76, 164 96 C 172 106, 184 102, 182 90 C 180 74, 182 56, 180 42 Z" fill={TRAZO} />
        <ellipse cx="240" cy="70" rx="7" ry="6" fill={TRAZO} />
      </g>
      <circle cx="208" cy="58" r="4.5" fill={TRAZO} />
      <g fill={TRAZO}><circle cx="70" cy="100" r="7" /><circle cx="88" cy="110" r="5" /><circle cx="60" cy="116" r="4" /></g>
      <g fill="#fff"><ellipse cx="100" cy="128" rx="40" ry="6" opacity=".5" /></g>
    </svg>
  );
}

/** Gato sentado de perfil, naranja claro. */
export function GatoSentado({ size = 160, className }: P) {
  return (
    <svg {...svgProps(className)} width={size} height={size} viewBox="0 0 160 160">
      <g stroke={TRAZO} strokeWidth="4" strokeLinecap="round" strokeLinejoin="round">
        <path d="M104 146 C 140 146, 150 118, 136 100" fill="none" />
        <path d="M50 148 C 34 148, 32 118, 44 96 C 54 78, 70 74, 84 78 C 104 86, 112 120, 106 148 Z" fill={NARANJA_CLARO} />
        <path d="M70 148 v -26 M 88 148 v -26" fill="none" />
        <path d="M40 60 C 40 40, 54 30, 70 30 C 86 30, 100 40, 100 60 C 100 76, 86 84, 70 84 C 54 84, 40 76, 40 60 Z" fill={NARANJA_CLARO} />
        <path d="M44 46 L 42 18 L 62 32 Z M 96 46 L 98 18 L 78 32 Z" fill={NARANJA_CLARO} />
        <path d="M60 66 h 20 M 64 72 q 6 4 12 0" fill="none" strokeWidth="3" />
      </g>
      <ellipse cx="58" cy="56" rx="4" ry="5" fill={TRAZO} /><ellipse cx="82" cy="56" rx="4" ry="5" fill={TRAZO} />
      <path d="M66 63 h 8 l -4 4 z" fill={TRAZO} />
      <path d="M58 98 q 10 -6 20 0 M 56 110 q 12 -6 24 0" stroke={NARANJA} strokeWidth="5" fill="none" strokeLinecap="round" />
    </svg>
  );
}

/** Plato de comida con corazón. */
export function Plato({ size = 90, className }: P) {
  return (
    <svg {...svgProps(className)} width={size} height={size * 0.7} viewBox="0 0 90 63">
      <g stroke={TRAZO} strokeWidth="3.5" strokeLinecap="round" strokeLinejoin="round">
        <path d="M14 26 C 18 12, 72 12, 76 26" fill={NARANJA} />
        <circle cx="30" cy="18" r="4" fill={NARANJA_CLARO} /><circle cx="45" cy="14" r="4" fill={NARANJA_CLARO} /><circle cx="60" cy="18" r="4" fill={NARANJA_CLARO} />
        <path d="M6 26 h 78 l -10 30 H 16 Z" fill="#fff" />
      </g>
      <path d="M45 50 c -8 -6 -11 -9 -11 -13 c 0 -3 2 -5 5 -5 c 2 0 4 1 6 3 c 2 -2 4 -3 6 -3 c 3 0 5 2 5 5 c 0 4 -3 7 -11 13 z" fill={TRAZO} />
    </svg>
  );
}

function Huella({ x, y, s }: { x: number; y: number; s: number }) {
  return (
    <g transform={`translate(${x} ${y}) scale(${s})`}>
      <ellipse cx="0" cy="8" rx="11" ry="9" />
      <ellipse cx="-12" cy="-6" rx="4.5" ry="6" /><ellipse cx="-4" cy="-13" rx="4.5" ry="6" />
      <ellipse cx="5" cy="-13" rx="4.5" ry="6" /><ellipse cx="13" cy="-6" rx="4.5" ry="6" />
    </g>
  );
}

/** Garabatos sueltos (corazones y rulos) para acompañar las ilustraciones. */
export function Garabatos({ className }: { className?: string }) {
  return (
    <svg {...svgProps(className)} width="120" height="90" viewBox="0 0 120 90">
      <g fill="none" stroke={TRAZO} strokeWidth="3.5" strokeLinecap="round" strokeLinejoin="round">
        <path d="M30 40 c -10 -8 -14 -12 -14 -17 c 0 -4 3 -7 7 -7 c 3 0 5 1 7 4 c 2 -3 4 -4 7 -4 c 4 0 7 3 7 7 c 0 5 -4 9 -14 17 z" />
        <path d="M70 80 q 6 -12 12 0 q 6 -12 12 0 q 6 -12 12 0" />
      </g>
      <path d="M86 26 c -6 -4 -8 -6 -8 -9 c 0 -2 2 -4 4 -4 c 2 0 3 1 4 2 c 1 -1 2 -2 4 -2 c 2 0 4 2 4 4 c 0 3 -2 5 -8 9 z" fill={TRAZO} />
    </svg>
  );
}
