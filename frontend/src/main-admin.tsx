import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { RefugioProvider } from './store/RefugioContext';
import { AdminApp } from './pages/AdminApp';

import './styles/admin.css';
import './styles/shared.css';
import './styles/nido-dashboard.css';
import './styles/mascota-fotos.css';

document.body.dataset.page = 'admin';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <RefugioProvider>
      <AdminApp />
    </RefugioProvider>
  </StrictMode>
);