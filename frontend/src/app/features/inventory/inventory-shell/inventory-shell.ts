import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-inventory-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, MatTabsModule, MatIconModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-header">
      <h1>Inventory</h1>
      <p class="subtitle">Raw materials, stones (individual + parcels), and stock movement history</p>
    </div>

    <nav mat-tab-nav-bar [tabPanel]="tabPanel" mat-stretch-tabs="false" class="tabs">
      <a
        mat-tab-link
        routerLink="raw-materials"
        routerLinkActive
        #rawActive="routerLinkActive"
        [active]="rawActive.isActive"
      >
        <mat-icon>inventory_2</mat-icon>
        <span>Raw Materials</span>
      </a>
      <a
        mat-tab-link
        routerLink="stone-items"
        routerLinkActive
        #siActive="routerLinkActive"
        [active]="siActive.isActive"
      >
        <mat-icon>diamond</mat-icon>
        <span>Stone Items</span>
      </a>
      <a
        mat-tab-link
        routerLink="stone-parcels"
        routerLinkActive
        #spActive="routerLinkActive"
        [active]="spActive.isActive"
      >
        <mat-icon>view_module</mat-icon>
        <span>Stone Parcels</span>
      </a>
      <a
        mat-tab-link
        routerLink="movements"
        routerLinkActive
        #mvActive="routerLinkActive"
        [active]="mvActive.isActive"
      >
        <mat-icon>history</mat-icon>
        <span>Stock Movements</span>
      </a>
    </nav>
    <mat-tab-nav-panel #tabPanel>
      <div class="content">
        <router-outlet />
      </div>
    </mat-tab-nav-panel>
  `,
  styles: [`
    .page-header { margin-bottom: 12px; }
    .page-header h1 { margin: 0 0 4px; font-weight: 500; }
    .subtitle { margin: 0; color: #607d8b; font-size: 13px; }
    .tabs { background: #fff; border-radius: 8px 8px 0 0; }
    .tabs mat-icon { margin-right: 6px; vertical-align: middle; }
    .content { padding-top: 16px; }
  `],
})
export class InventoryShell {}
