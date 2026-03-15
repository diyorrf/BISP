import { Routes } from '@angular/router';
import { LayoutComponent } from './components/layout/layout.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/register/register.component').then(m => m.RegisterComponent)
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'scanner',
        loadComponent: () => import('./pages/contract-scanner/contract-scanner.component').then(m => m.ContractScannerComponent)
      },
      {
        path: 'chat',
        loadComponent: () => import('./pages/ai-chat/ai-chat.component').then(m => m.AiChatComponent)
      },
      {
        path: 'business-checker',
        loadComponent: () => import('./pages/business-checker/business-checker.component').then(m => m.BusinessCheckerComponent)
      },
      {
        path: 'documents',
        loadComponent: () => import('./pages/document-library/document-library.component').then(m => m.DocumentLibraryComponent)
      },
      {
        path: 'documents/:id',
        loadComponent: () => import('./pages/document-detail/document-detail.component').then(m => m.DocumentDetailComponent)
      },
      {
        path: 'alerts',
        loadComponent: () => import('./pages/alerts/alerts.component').then(m => m.AlertsComponent)
      },
      {
        path: 'pricing',
        loadComponent: () => import('./pages/pricing/pricing.component').then(m => m.PricingComponent)
      },
      {
        path: '**',
        loadComponent: () => import('./pages/not-found/not-found.component').then(m => m.NotFoundComponent)
      }
    ]
  }
];
