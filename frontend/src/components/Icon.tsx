// Íconos decorativos: siempre aria-hidden. El significado lo da el texto que los acompaña.
const PATHS: Record<string, string> = {
  menu: 'M4 6h16M4 12h16M4 18h16',
  close: 'M6 6l12 12M18 6L6 18',
  paw: 'M12 13c-2.8 0-5 2.6-5 4.6 0 1.5 1.3 2.4 2.7 2.4.9 0 1.5-.4 2.3-.4s1.4.4 2.3.4c1.4 0 2.7-.9 2.7-2.4 0-2-2.2-4.6-5-4.6zM6.5 11.5a2 2.5 0 1 0 0-.01M17.5 11.5a2 2.5 0 1 0 0-.01M9.3 7a2 2.6 0 1 0 0-.01M14.7 7a2 2.6 0 1 0 0-.01',
  heart: 'M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.7l-1.1-1.1a5.5 5.5 0 0 0-7.8 7.8L12 21l8.8-8.6a5.5 5.5 0 0 0 0-7.8z',
  alert: 'M12 3l10 18H2L12 3zM12 10v5M12 18h.01',
  home: 'M3 11l9-8 9 8M5 10v10h14V10',
  list: 'M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01',
  bell: 'M18 8a6 6 0 0 0-12 0c0 7-3 8-3 9h18c0-1-3-2-3-9M10 21h4',
  users: 'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8zM22 21v-2a4 4 0 0 0-3-3.9M16 3.1a4 4 0 0 1 0 7.8',
  med: 'M10 3h4v7h7v4h-7v7h-4v-7H3v-4h7z',
  file: 'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8zM14 2v6h6M8 13h8M8 17h8',
  check: 'M4 12l5 5L20 6',
  building: 'M4 21h16M6 21V4h12v17M9 8h2M13 8h2M9 12h2M13 12h2M11 21v-5h2v5',
  plus: 'M12 5v14M5 12h14',
  edit: 'M12 20h9M16.5 3.5a2.1 2.1 0 0 1 3 3L10 16l-4 1 1-4z',
  trash: 'M3 6h18M8 6V4h8v2M6 6l1 15h10l1-15',
  camera: 'M3 8h4l2-3h6l2 3h4v12H3zM12 17a4 4 0 1 0 0-8 4 4 0 0 0 0 8z',
  left: 'M15 18l-6-6 6-6',
  right: 'M9 18l6-6-6-6',
  up: 'M18 15l-6-6-6 6',
  down: 'M6 9l6 6 6-6',
  star: 'M12 3l2.8 5.7 6.2.9-4.5 4.4 1 6.2L12 17.3 6.5 20.2l1-6.2L3 9.6l6.2-.9z',
  map: 'M9 4L3 6v14l6-2 6 2 6-2V4l-6 2-6-2zM9 4v14M15 6v14',
  locate: 'M12 2v3M12 19v3M2 12h3M19 12h3M12 17a5 5 0 1 0 0-10 5 5 0 0 0 0 10z',
  external: 'M14 4h6v6M20 4l-9 9M18 14v6H4V6h6',
  search: 'M11 19a8 8 0 1 0 0-16 8 8 0 0 0 0 16zM21 21l-4.4-4.4',
  refresh: 'M20 11a8 8 0 0 0-14.9-3M4 4v4h4M4 13a8 8 0 0 0 14.9 3M20 20v-4h-4',
  arrow: 'M5 12h14M13 6l6 6-6 6',
  settings: 'M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6zM19.4 15a1.7 1.7 0 0 0 .3 1.8l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-1.8-.3 1.7 1.7 0 0 0-1 1.5V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-1.1-1.5 1.7 1.7 0 0 0-1.8.3l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0 .3-1.8 1.7 1.7 0 0 0-1.5-1H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.5-1.1 1.7 1.7 0 0 0-.3-1.8l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.8.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.8-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.8V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1z',
  mail: 'M3 6h18v12H3zM3 7l9 6 9-6',
  user: 'M12 12a4 4 0 1 0 0-8 4 4 0 0 0 0 8zM4 21v-1a6 6 0 0 1 6-6h4a6 6 0 0 1 6 6v1',
  logout: 'M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4M16 17l5-5-5-5M21 12H9',
  clock:'M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18zM12 7v5l3 2',
  info: 'M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18zM12 11v6M12 7.5h.01',
  minus: 'M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18zM8 12h8',
  megaphone:'M3 10v4h3l7 5V5L6 10H3zM17 9a4 4 0 0 1 0 6M19.5 6.5a7.5 7.5 0 0 1 0 11'
};

export type NombreIcono = keyof typeof PATHS;

export function Icon({ name, size = 20, className }: { name: NombreIcono; size?: number; className?: string }) {
  return (
    <svg className={className} width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor"
      strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" focusable="false">
      <path d={PATHS[name]} />
    </svg>
  );
}
