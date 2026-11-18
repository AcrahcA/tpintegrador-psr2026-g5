import type { SVGProps } from 'react';

const paths: Record<string, string[]> = {
  grid: ['M3 3h7v7H3z', 'M14 3h7v7h-7z', 'M3 14h7v7H3z', 'M14 14h7v7h-7z'],
  heart: ['M20.8 4.6a5.5 5.5 0 0 0-7.8 0L12 5.7l-1.1-1.1a5.5 5.5 0 0 0-7.8 7.8L12 21l8.8-8.6a5.5 5.5 0 0 0 0-7.8z'],
  clipboard: ['M9 4h6', 'M9 2h6v4H9z', 'M5 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2h-1', 'M7 11h10', 'M7 15h10'],
  users: ['M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2', 'M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z', 'M22 21v-2a4 4 0 0 0-3-3.87', 'M16 3.13a4 4 0 0 1 0 7.75'],
  plus: ['M12 5v14', 'M5 12h14'],
  edit: ['M12 20h9', 'M16.5 3.5a2.1 2.1 0 0 1 3 3L10 16l-4 1 1-4z'],
  shield: ['M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z', 'm9 12 2 2 4-4'],
  briefcase: ['M3 7h18v14H3z', 'M8 7V5a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2', 'M3 12h18'],
  file: ['M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z', 'M14 2v6h6', 'M8 13h8', 'M8 17h8'],
  building: ['M4 21h16', 'M6 21V4h12v17', 'M9 8h2', 'M13 8h2', 'M9 12h2', 'M13 12h2', 'M11 21v-5h2v5'],
  bell: ['M18 8a6 6 0 0 0-12 0c0 7-3 8-3 9h18c0-1-3-2-3-9', 'M10 21h4'],
  search: ['M11 19a8 8 0 1 0 0-16 8 8 0 0 0 0 16z', 'm21 21-4.4-4.4'],
  arrow: ['M5 12h14', 'm12 5 7 7-7 7'],
  chevron: ['m9 18 6-6-6-6'],
  x: ['M18 6 6 18', 'm6 6 12 12'],
  menu: ['M4 7h16', 'M4 12h16', 'M4 17h16'],
  check: ['m4 12 5 5L20 6'],
  clock: ['M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20z', 'M12 6v6l4 2'],
  pin: ['M20 10c0 5-8 12-8 12S4 15 4 10a8 8 0 1 1 16 0z', 'M12 10a2 2 0 1 0 0-4 2 2 0 0 0 0 4z'],
  tag: ['M20.6 13.4 13.4 20.6a2 2 0 0 1-2.8 0L2 12V2h10l8.6 8.6a2 2 0 0 1 0 2.8z', 'M7 7h.01'],
  trash: ['M3 6h18', 'M8 6V4h8v2', 'M6 6l1 16h10l1-16', 'M10 10v8', 'M14 10v8'],
  eye: ['M2 12s3.6-7 10-7 10 7 10 7-3.6 7-10 7-10-7-10-7z', 'M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6z'],
  calendar: ['M4 5h16v17H4z', 'M8 2v6', 'M16 2v6', 'M4 10h16'],
  star: ['m12 2 3 6 7 .9-5 4.9 1.2 7-6.2-3.3L5.8 21l1.2-7L2 9l7-.9z'],
  activity: ['M22 12h-4l-3 9L9 3l-3 9H2'],
  house: ['M3 10 12 3l9 7', 'M5 9v12h14V9', 'M9 21v-7h6v7'],
  refresh: ['M20 7v5h-5', 'M4 17v-5h5', 'M20 12a8 8 0 0 0-14-5L4 12', 'M4 12a8 8 0 0 0 14 5l2-5'],
  info: ['M12 22a10 10 0 1 0 0-20 10 10 0 0 0 0 20z', 'M12 11v6', 'M12 7h.01'],
  download: ['M12 3v12', 'm7 10 5 5 5-5', 'M4 20h16'],
  chart: ['M3 3v18h18', 'M7 15l5-6 4 3 5-7'],
  logout: ['M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4', 'M16 17l5-5-5-5', 'M21 12H9'],
  paw: ['M9 8a1 1 0 0 0 0-2 1 1 0 0 0 0 2z', 'M15 8a1 1 0 0 0 0-2 1 1 0 0 0 0 2z', 'M5 12a1 1 0 0 0 0-2 1 1 0 0 0 0 2z', 'M19 12a1 1 0 0 0 0-2 1 1 0 0 0 0 2z', 'M12 12c-2 0-2.7 3-4.6 4.5a2.6 2.6 0 0 0 2.5 4.4c1.2-.5 3-.5 4.2 0a2.6 2.6 0 0 0 2.5-4.4C14.7 15 14 12 12 12z']
};

export function Icon({
  name,
  size = 18,
  ...props
}: SVGProps<SVGSVGElement> & { name: string; size?: number }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.7"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      {...props}
    >
      {(paths[name] || paths.grid).map((d, i) => (
        <path d={d} key={i} />
      ))}
    </svg>
  );
}