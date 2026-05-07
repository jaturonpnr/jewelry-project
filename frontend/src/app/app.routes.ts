import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./layouts/auth-layout/auth-layout').then((m) => m.AuthLayout),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
      },
    ],
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layouts/main-layout/main-layout').then((m) => m.MainLayout),
    children: [
      {
        path: '',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'customers',
        loadComponent: () =>
          import('./features/customers/customer-list/customer-list').then((m) => m.CustomerList),
      },
      {
        path: 'customers/new',
        loadComponent: () =>
          import('./features/customers/customer-form/customer-form').then((m) => m.CustomerForm),
      },
      {
        path: 'customers/:id',
        loadComponent: () =>
          import('./features/customers/customer-detail/customer-detail').then((m) => m.CustomerDetail),
      },
      {
        path: 'customers/:id/edit',
        loadComponent: () =>
          import('./features/customers/customer-form/customer-form').then((m) => m.CustomerForm),
      },
      {
        path: 'suppliers',
        loadComponent: () =>
          import('./features/suppliers/supplier-list/supplier-list').then((m) => m.SupplierList),
      },
      {
        path: 'sales-orders',
        loadComponent: () =>
          import('./features/sales-orders/sales-order-list/sales-order-list').then((m) => m.SalesOrderList),
      },
      {
        path: 'sales-orders/:id',
        loadComponent: () =>
          import('./features/sales-orders/sales-order-detail/sales-order-detail').then((m) => m.SalesOrderDetail),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
