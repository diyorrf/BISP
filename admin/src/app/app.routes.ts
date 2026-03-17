import { Routes } from '@angular/router';
import { adminGuard } from './guards/admin.guard';
import { LayoutComponent } from './components/layout/layout.component';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [adminGuard],
    children: [
      { path: '', loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'users', loadComponent: () => import('./pages/users/users.component').then(m => m.UsersComponent) },
      { path: 'users/:id', loadComponent: () => import('./pages/user-detail/user-detail.component').then(m => m.UserDetailComponent) },
      { path: 'documents', loadComponent: () => import('./pages/documents/documents.component').then(m => m.DocumentsComponent) },
      { path: 'regulatory', loadComponent: () => import('./pages/regulatory/regulatory.component').then(m => m.RegulatoryComponent) },
      { path: 'payments', loadComponent: () => import('./pages/payments/payments.component').then(m => m.PaymentsComponent) },
    ]
  }
];
