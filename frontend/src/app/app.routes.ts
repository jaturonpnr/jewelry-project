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
      {
        path: 'work-orders',
        loadComponent: () =>
          import('./features/work-orders/work-order-list/work-order-list').then((m) => m.WorkOrderList),
      },
      {
        path: 'work-orders/:id',
        loadComponent: () =>
          import('./features/work-orders/work-order-detail/work-order-detail').then((m) => m.WorkOrderDetail),
      },
      {
        path: 'inventory',
        loadComponent: () =>
          import('./features/inventory/inventory-shell/inventory-shell').then((m) => m.InventoryShell),
        children: [
          { path: '', redirectTo: 'raw-materials', pathMatch: 'full' },
          {
            path: 'raw-materials',
            loadComponent: () =>
              import('./features/inventory/raw-materials/raw-material-list/raw-material-list').then((m) => m.RawMaterialList),
          },
          {
            path: 'raw-materials/receive',
            loadComponent: () =>
              import('./features/inventory/raw-materials/receive-raw-material/receive-raw-material').then((m) => m.ReceiveRawMaterial),
          },
          {
            path: 'stone-items',
            loadComponent: () =>
              import('./features/inventory/stone-items/stone-item-list/stone-item-list').then((m) => m.StoneItemList),
          },
          {
            path: 'stone-parcels',
            loadComponent: () =>
              import('./features/inventory/stone-parcels/stone-parcel-list/stone-parcel-list').then((m) => m.StoneParcelList),
          },
          {
            path: 'movements',
            loadComponent: () =>
              import('./features/inventory/stock-movements/stock-movement-list/stock-movement-list').then((m) => m.StockMovementList),
          },
        ],
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
