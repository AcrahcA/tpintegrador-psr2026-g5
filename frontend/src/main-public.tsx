import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { NotifierProvider } from './components/Notifier';
import { PublicApp } from './public/PublicApp';
import './styles/base.css';
import './styles/components.css';
import './styles/public.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <NotifierProvider>
      <PublicApp />
    </NotifierProvider>
  </StrictMode>
);
