import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { AdminApp } from './admin/AdminApp';
import { NotifierProvider } from './components/Notifier';
import '@fontsource/comfortaa/400.css';
import '@fontsource/comfortaa/700.css';
import '@fontsource/nunito/400.css';
import '@fontsource/nunito/600.css';
import '@fontsource/nunito/700.css';
import './styles/base.css';
import './styles/components.css';
import './styles/admin.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <NotifierProvider>
      <AdminApp />
    </NotifierProvider>
  </StrictMode>
);
