import { aplicarPreferencias, leerPreferencias } from './components/Adaptabilidad';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { AdminApp } from './admin/AdminApp';
import { LectorVoz } from './components/LectorVoz';
import { NotifierProvider } from './components/Notifier';
import { SesionProvider } from './components/Sesion';
import '@fontsource/comfortaa/400.css';
import '@fontsource/comfortaa/700.css';
import '@fontsource/nunito/400.css';
import '@fontsource/nunito/600.css';
import '@fontsource/nunito/700.css';
import './styles/base.css';
import './styles/components.css';
import './styles/themes.css';
import './styles/admin.css';

aplicarPreferencias(leerPreferencias());

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <NotifierProvider>
      <SesionProvider>
        {/* Primer elemento al que se llega con Tab: activa la lectura en voz alta */}
        <LectorVoz />
        <AdminApp />
      </SesionProvider>
    </NotifierProvider>
  </StrictMode>
);
