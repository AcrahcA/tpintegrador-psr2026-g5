import { aplicarPreferencias, leerPreferencias } from './components/Adaptabilidad';
import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { NotifierProvider } from './components/Notifier';
import { SesionProvider } from './components/Sesion';
import { PublicApp } from './public/PublicApp';
import '@fontsource/comfortaa/400.css';
import '@fontsource/comfortaa/700.css';
import '@fontsource/nunito/400.css';
import '@fontsource/nunito/600.css';
import '@fontsource/nunito/700.css';
import './styles/base.css';
import './styles/components.css';
import './styles/themes.css';
import './styles/public.css';

aplicarPreferencias(leerPreferencias());

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <NotifierProvider>
      <SesionProvider>
        <PublicApp />
      </SesionProvider>
    </NotifierProvider>
  </StrictMode>
);
