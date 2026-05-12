import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-main-layout',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDividerModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-sidenav-container class="container">
      <mat-sidenav #drawer mode="side" opened class="sidenav">
        <div class="brand">
          <mat-icon>diamond</mat-icon>
          <span>JewelryFactory</span>
        </div>

        <mat-nav-list>
          @for (item of navItems; track item.route) {
            <a
              mat-list-item
              [routerLink]="item.route"
              routerLinkActive="active"
              [routerLinkActiveOptions]="{ exact: item.route === '/' }"
            >
              <mat-icon matListItemIcon>{{ item.icon }}</mat-icon>
              <span matListItemTitle>{{ item.label }}</span>
            </a>
          }
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content>
        <mat-toolbar color="primary" class="topbar">
          <button mat-icon-button (click)="drawer.toggle()">
            <mat-icon>menu</mat-icon>
          </button>
          <span class="spacer"></span>

          @if (auth.user(); as user) {
            <button mat-button [matMenuTriggerFor]="userMenu">
              <mat-icon>account_circle</mat-icon>
              <span class="user-label">{{ user.fullName }} ({{ user.role }})</span>
            </button>
            <mat-menu #userMenu="matMenu">
              <button mat-menu-item disabled>
                <mat-icon>email</mat-icon>
                <span>{{ user.email }}</span>
              </button>
              <mat-divider></mat-divider>
              <button mat-menu-item (click)="auth.logout()">
                <mat-icon>logout</mat-icon>
                <span>Sign out</span>
              </button>
            </mat-menu>
          }
        </mat-toolbar>

        <main class="content">
          <router-outlet />
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .container { height: 100vh; }
    .sidenav {
      width: 240px;
      background: #1a237e;
      color: #fff;
    }
    .brand {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 16px;
      font-size: 16px;
      font-weight: 500;
      border-bottom: 1px solid rgba(255,255,255,0.12);
    }
    .brand mat-icon { color: #ffd54f; }
    ::ng-deep .sidenav .mat-mdc-list-item {
      color: rgba(255,255,255,0.85);
      --mdc-list-list-item-label-text-color: rgba(255,255,255,0.85);
      --mdc-list-list-item-hover-label-text-color: #fff;
      --mdc-list-list-item-leading-icon-color: rgba(255,255,255,0.7);
    }
    ::ng-deep .sidenav .mat-mdc-list-item.active {
      background: rgba(255,255,255,0.12);
      --mdc-list-list-item-label-text-color: #fff;
      --mdc-list-list-item-leading-icon-color: #ffd54f;
    }
    .topbar { display: flex; align-items: center; }
    .spacer { flex: 1; }
    .user-label { margin-left: 6px; }
    .content { padding: 24px; min-height: calc(100vh - 64px); }
  `],
})
export class MainLayout {
  protected readonly auth = inject(AuthService);

  readonly navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'dashboard', route: '/' },
    { label: 'Customers', icon: 'business', route: '/customers' },
    { label: 'Suppliers', icon: 'local_shipping', route: '/suppliers' },
    { label: 'Inventory', icon: 'inventory_2', route: '/inventory' },
    { label: 'Sales Orders', icon: 'receipt_long', route: '/sales-orders' },
    { label: 'Work Orders', icon: 'precision_manufacturing', route: '/work-orders' },
    { label: 'BOM & Costing', icon: 'receipt', route: '/bom' },
    { label: 'Quality Control', icon: 'fact_check', route: '/qc' },
  ];
}
