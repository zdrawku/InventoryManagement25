import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/dashboard',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./components/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./components/auth/register.component').then(m => m.RegisterComponent)
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard]
  },
  {
    path: 'equipment',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./components/equipment/equipment-list.component').then(m => m.EquipmentListComponent)
      },
      {
        path: 'new',
        loadComponent: () => import('./components/equipment/equipment-form.component').then(m => m.EquipmentFormComponent),
        canActivate: [adminGuard]
      },
      {
        path: ':id',
        loadComponent: () => import('./components/equipment/equipment-detail.component').then(m => m.EquipmentDetailComponent)
      },
      {
        path: ':id/edit',
        loadComponent: () => import('./components/equipment/equipment-form.component').then(m => m.EquipmentFormComponent),
        canActivate: [adminGuard]
      }
    ]
  },
  {
    path: 'requests',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./components/requests/request-list.component').then(m => m.RequestListComponent)
      },
      {
        path: 'new',
        loadComponent: () => import('./components/requests/request-form.component').then(m => m.RequestFormComponent)
      }
    ]
  },
  {
    path: 'admin/requests',
    loadComponent: () => import('./components/requests/request-list.component').then(m => m.RequestListComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: 'reports',
    loadComponent: () => import('./components/reports/reports.component').then(m => m.ReportsComponent),
    canActivate: [authGuard, adminGuard]
  },
  {
    path: '**',
    redirectTo: '/dashboard'
  }
];
